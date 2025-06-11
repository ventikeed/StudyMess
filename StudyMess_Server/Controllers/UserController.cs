using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMess_Server.Data;
using StudyMess_Server.Models;
using StudyMess_Server.Services;

namespace StudyMess_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly StudyMessDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(StudyMessDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByGroupId(int groupId)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.GroupId == groupId)
                    .ToListAsync();

                if (users == null || users.Count == 0)
                {
                    return NotFound($"Пользователи с GroupId {groupId} не найдены.");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении пользователей по GroupId {groupId}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpGet("chat/{chatId}/users")]
        public async Task<ActionResult<IEnumerable<User>>> GetChatUsers(int chatId)
        {
            try
            {
                var userIds = await _context.ChatMembers
                    .Where(cm => cm.ChatId == chatId)
                    .Select(cm => cm.UserId)
                    .ToListAsync();

                if (userIds == null || userIds.Count == 0)
                {
                    return NotFound($"В чате с Id {chatId} нет участников.");
                }

                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении участников чата {chatId}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPost("online-statuses")]
        public IActionResult GetOnlineStatuses([FromBody] List<int> userIds)
        {
            var statuses = userIds.Select(id => new
            {
                Id = id,
                IsOnline = ChatHub.OnlineUsers.TryGetValue(id, out var online) && online
            }).ToList();
            return Ok(statuses);
        }
        [HttpGet("online-status/{userId}")]
        public IActionResult GetOnlineStatus(int userId)
        {
            var isOnline = ChatHub.OnlineUsers.TryGetValue(userId, out var online) && online;
            return Ok(new { Id = userId, IsOnline = isOnline });
        }
    }
}