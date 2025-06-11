using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMess_Server.Data;
using StudyMess_Server.Models;

namespace StudyMess_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly StudyMessDbContext _context;

        public AdminController(StudyMessDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound("Пользователь не найден");
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPut("user/{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
                return BadRequest("Роль не может быть пустой");

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound("Пользователь не найден");
                user.Role = newRole;
                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetAllChats()
        {
            try
            {
                var chats = await _context.Chats.ToListAsync();
                return Ok(chats);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("chat/{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            try
            {
                var chat = await _context.Chats.FindAsync(id);
                if (chat == null) return NotFound("Чат не найден");
                _context.Chats.Remove(chat);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetAllMessages()
        {
            try
            {
                var messages = await _context.Messages.ToListAsync();
                return Ok(messages);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("message/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null) return NotFound("Сообщение не найдено");
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            try
            {
                var groups = await _context.Groups.ToListAsync();
                return Ok(groups);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroup([FromBody] Group group)
        {
            if (group == null || string.IsNullOrWhiteSpace(group.GroupName))
                return BadRequest("Некорректные данные группы");

            try
            {
                if (await _context.Groups.AnyAsync(g => g.GroupName == group.GroupName))
                    return Conflict("Группа с таким названием уже существует");

                _context.Groups.Add(group);
                await _context.SaveChangesAsync();
                return Ok(group);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("group/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            try
            {
                var group = await _context.Groups.FindAsync(id);
                if (group == null) return NotFound("Группа не найдена");
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}
