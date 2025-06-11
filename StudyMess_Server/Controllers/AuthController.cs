using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMess_Server.Data;
using StudyMess_Server.Models;

namespace StudyMess_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly StudyMessDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(StudyMessDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Данные не переданы");

                if (string.IsNullOrWhiteSpace(model.Username) ||
                    string.IsNullOrWhiteSpace(model.Password) ||
                    string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.FirstName) ||
                    string.IsNullOrWhiteSpace(model.LastName) ||
                    string.IsNullOrWhiteSpace(model.Role))
                    return BadRequest("Все поля обязательны для заполнения");

                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    return BadRequest("Такой пользователь уже существует");

                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                    return BadRequest("Пользователь с таким email уже существует");

                string? avatarPath = null;
                if (model.Avatar != null && model.Avatar.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.Avatar.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Avatar.CopyToAsync(stream);
                    }
                    avatarPath = "/avatars/" + fileName;
                }
                else
                {
                    avatarPath = "/avatars/logo.png";
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Role = model.Role,
                    GroupId = model.GroupId,
                    Avatar = avatarPath,
                    CreatedAt = DateTime.Now,
                    LastOnline = DateTime.Now,
                    IsOnline = true,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { token = _tokenService.GenerateToken(user) });
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            try
            {
                if (loginModel == null)
                    return BadRequest("Данные не переданы");

                if (string.IsNullOrWhiteSpace(loginModel.Login) || string.IsNullOrWhiteSpace(loginModel.Password))
                    return BadRequest("Логин и пароль обязательны");

                var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == loginModel.Login);
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password))
                    return Unauthorized("Неверный логин или пароль");

                return Ok(new { token = _tokenService.GenerateToken(user) });
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}
