using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudyMess_Server.Data;
using StudyMess_Server.Models;
using StudyMess_Server.Services;

namespace StudyMess_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly StudyMessDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(StudyMessDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat(CreateChatModel chatModel)
        {
            try
            {
                if (chatModel == null || string.IsNullOrWhiteSpace(chatModel.Name) || chatModel.UserIds == null || !chatModel.UserIds.Any())
                    return BadRequest("Некорректные данные для создания чата");
                var chat = new Chat
                {
                    Name = chatModel.Name,
                    IsGroupChat = chatModel.IsGroupChat,
                    CreatedAt = DateTime.Now,
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                var chatMembers = chatModel.UserIds.Select(userId => new ChatMember
                {
                    ChatId = chat.Id,
                    UserId = userId,
                    Role = "Member",
                    JoinedAt = DateTime.Now
                }).ToList();
                _context.ChatMembers.AddRange(chatMembers);
                await _context.SaveChangesAsync();

                return Ok(chat);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Chat>>> GetMyChats()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Пользователь не авторизован");

                var userId = int.Parse(userIdClaim.Value);


                var chatIds = await _context.ChatMembers
                    .Where(cm => cm.UserId == userId)
                    .Select(cm => cm.ChatId)
                    .ToListAsync();

                var chats = await _context.Chats
                    .Where(c => chatIds.Contains(c.Id))
                    .ToListAsync();

                return Ok(chats);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message SendedMessage)
        {
            try
            {
                var message = new Message
                {
                    ChatId = SendedMessage.ChatId,
                    SenderId = SendedMessage.SenderId,
                    Content = SendedMessage.Content,
                    SentAt = DateTime.Now,
                    IsEdited = false
                };
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"chat_{message.ChatId}")
                    .SendAsync("ReceiveMessage", message);

                return Ok(message);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("send-file")]
        public async Task<IActionResult> SendFileMessage([FromForm] FileMessage fileMessage)
        {
            try
            {
                if (fileMessage.File == null || fileMessage.File.Length == 0)
                    return BadRequest("Файл не выбран");

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileMessage.File.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileMessage.File.CopyToAsync(stream);
                }

                var dbFileMessage = new FileMessage
                {
                    ChatId = fileMessage.ChatId,
                    SenderId = fileMessage.SenderId,
                    SentAt = DateTime.Now,
                    IsEdited = false,
                    filepath = $"/uploads/{fileName}"
                };

                _context.FileMessages.Add(dbFileMessage);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group($"chat_{dbFileMessage.ChatId}")
                    .SendAsync("ReceiveFileMessage", dbFileMessage);

                return Ok(dbFileMessage);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                return Ok(messages);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("{chatId}/file-messages")]
        public async Task<IActionResult> GetFileMessages(int chatId)
        {
            try
            {
                var fileMessages = await _context.FileMessages
                    .Where(fm => fm.ChatId == chatId)
                    .OrderBy(fm => fm.SentAt)
                    .ToListAsync();

                return Ok(fileMessages);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("{chatId}/members")]
        public async Task<ActionResult<IEnumerable<ChatMember>>> GetChatMembers(int chatId)
        {
            try
            {
                var members = await _context.ChatMembers
                    .Where(cm => cm.ChatId == chatId)
                    .ToListAsync();

                if (members == null || members.Count == 0)
                {
                    return NotFound($"В чате с Id {chatId} нет участников.");
                }

                return Ok(members);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("{chatId}/users")]
        public async Task<ActionResult<IEnumerable<User>>> GetChatUsers(int chatId)
        {
            try
            {
                var userIds = await _context.ChatMembers
                    .Where(cm => cm.ChatId == chatId)
                    .Select(cm => cm.UserId)
                    .ToListAsync();

                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                if (users == null || users.Count == 0)
                    return NotFound($"В чате с Id {chatId} нет пользователей.");

                return Ok(users);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var message = await _context.Messages.FindAsync(messageId);
                if (message == null)
                    return NotFound();

                int chatId = message.ChatId;
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"chat_{chatId}")
                    .SendAsync("MessageDeleted", messageId);

                return Ok();
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("file-message/{fileMessageId}")]
        public async Task<IActionResult> DeleteFileMessage(int fileMessageId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var fileMessage = await _context.FileMessages.FindAsync(fileMessageId);
                if (fileMessage == null)
                    return NotFound();

                if (fileMessage.SenderId != userId)
                    return Forbid();
                if (!string.IsNullOrEmpty(fileMessage.filepath))
                {
                    try
                    {
                        var filePath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot", fileMessage.filepath);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch
                    {
                    }
                }

                int chatId = fileMessage.ChatId;
                _context.FileMessages.Remove(fileMessage);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"chat_{chatId}")
                    .SendAsync("FileMessageDeleted", fileMessageId);

                return Ok();
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPut("message/{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] string newContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newContent))
                    return BadRequest("Новое содержимое не может быть пустым");

                var message = await _context.Messages.FindAsync(messageId);
                if (message == null)
                    return NotFound();

                message.Content = newContent;
                message.IsEdited = true;

                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"chat_{message.ChatId}")
                    .SendAsync("MessageEdited", message);

                return Ok(message);
            }
            catch
            {
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("{chatId}/add-users")]
        public async Task<IActionResult> AddUsersToChat(int chatId, [FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return BadRequest("Список пользователей пуст.");

            var chat = await _context.Chats.FindAsync(chatId);
            if (chat == null)
                return NotFound("Чат не найден.");

            var existingUserIds = await _context.ChatMembers
                .Where(cm => cm.ChatId == chatId)
                .Select(cm => cm.UserId)
                .ToListAsync();

            var newUserIds = userIds.Except(existingUserIds).ToList();
            if (newUserIds.Count == 0)
                return BadRequest("Нет новых пользователей для добавления.");

            var newMembers = newUserIds.Select(userId => new ChatMember
            {
                ChatId = chatId,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.Now
            }).ToList();

            _context.ChatMembers.AddRange(newMembers);
            await _context.SaveChangesAsync();

            return Ok(newMembers.Count);
        }
    }
}