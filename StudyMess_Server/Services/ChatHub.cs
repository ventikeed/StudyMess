using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using StudyMess_Server.Data;
using StudyMess_Server.Models;

namespace StudyMess_Server.Services
{
    public class ChatHub : Hub
    {
        public static readonly ConcurrentDictionary<int, bool> OnlineUsers = new();
        private readonly StudyMessDbContext _context;

        public ChatHub(StudyMessDbContext context)
        {
            _context = context;
        }

        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task SendMessageToChat(int chatId, string content)
        {
            int userId = GetUserId();
            var message = new Message
            {
                ChatId = chatId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.Now,
                IsEdited = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", message);
        }

        public async Task EditMessage(int chatId, int messageId, string newContent)
        {
            int userId = GetUserId();
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.ChatId != chatId)
                return;

            message.Content = newContent;
            message.IsEdited = true;
            await _context.SaveChangesAsync();

            await Clients.Group($"chat_{chatId}").SendAsync("MessageEdited", message);
        }

        public async Task DeleteMessage(int chatId, int messageId)
        {
            int userId = GetUserId();
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.ChatId != chatId)
                return;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await Clients.Group($"chat_{chatId}").SendAsync("MessageDeleted", messageId);
        }
        public async Task DeleteFileMessage(int chatId, int fileMessageId)
        {
            int userId = GetUserId();
            var fileMessage = await _context.FileMessages.FindAsync(fileMessageId);
            if (fileMessage == null || fileMessage.ChatId != chatId)
                return;
            if (!string.IsNullOrEmpty(fileMessage.filepath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileMessage.filepath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            _context.FileMessages.Remove(fileMessage);
            await _context.SaveChangesAsync();

            await Clients.Group($"chat_{chatId}").SendAsync("FileMessageDeleted", fileMessageId);
        }
        public override async Task OnConnectedAsync()
        {
            int userId = GetUserId();
            OnlineUsers[userId] = true;
            await Clients.All.SendAsync("UserStatusChanged", userId, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            int userId = GetUserId();
            OnlineUsers[userId] = false;

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastOnline = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            await Clients.All.SendAsync("UserStatusChanged", userId, false);
            await base.OnDisconnectedAsync(exception);
        }

        private int GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        public Task<bool> IsUserOnline(int userId)
        {
            return Task.FromResult(OnlineUsers.TryGetValue(userId, out var isOnline) && isOnline);
        }
    }
}
