using Microsoft.AspNetCore.SignalR.Client;
using StudyMess_Client.Models;

namespace StudyMess_Client.Services
{
    public class ChatHubService
    {
        private readonly HubConnection _connection;

        public event Action<Message>? OnMessageReceived;
        public event Action<int, bool>? OnUserStatusChanged;
        public event Action<Message>? OnMessageEdited;
        public event Action<int>? OnMessageDeleted;
        public event Action<int>? OnFileMessageDeleted;
        public event Action<FileMessage>? OnFileMessageReceived;
        public ChatHubService(string hubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(TokenStorage.Token);
                })
                .WithAutomaticReconnect()
                .Build();
            _connection.On<FileMessage>("ReceiveFileMessage", (fileMessage) =>
            {
                OnFileMessageReceived?.Invoke(fileMessage);
            });
            _connection.On<Message>("ReceiveMessage", (message) =>
            {
                OnMessageReceived?.Invoke(message);
            });
            _connection.On<int, bool>("UserStatusChanged", (userId, isOnline) =>
            {
                OnUserStatusChanged?.Invoke(userId, isOnline);
            });
            _connection.On<Message>("MessageEdited", (message) =>
            {
                OnMessageEdited?.Invoke(message);
            });
            _connection.On<int>("MessageDeleted", (messageId) =>
            {
                OnMessageDeleted?.Invoke(messageId);
            });
            _connection.On<int>("FileMessageDeleted", (fileMessageId) =>
            {
                OnFileMessageDeleted?.Invoke(fileMessageId);
            });
        }


        public async Task StartAsync() => await _connection.StartAsync();

        public async Task JoinChat(int chatId) =>
            await _connection.InvokeAsync("JoinChat", chatId);

        public async Task LeaveChat(int chatId) =>
            await _connection.InvokeAsync("LeaveChat", chatId);

        public async Task SendMessageToChat(int chatId, Message message) =>
            await _connection.InvokeAsync("SendMessageToChat", chatId, message);
        public async Task StopAsync() => await _connection.StopAsync();
        public async Task EditMessageAsync(int chatId, int messageId, string newContent)
        {
            await _connection.InvokeAsync("EditMessage", chatId, messageId, newContent);
        }

        public async Task DeleteMessageAsync(int chatId, int messageId)
        {
            await _connection.InvokeAsync("DeleteMessage", chatId, messageId);
        }
        public async Task DeleteFileMessageAsync(int chatId, int fileMessageId)
        {
            await _connection.InvokeAsync("DeleteFileMessage", chatId, fileMessageId);
        }
    }
}
