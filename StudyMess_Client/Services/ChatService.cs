using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using StudyMess_Client.Models;
using StudyMess_Client.Utils;
using static StudyMess_Client.Services.UrlStorage;

namespace StudyMess_Client.Services
{
    public class ChatService
    {
        private readonly HttpClient _client = new();
        private readonly string _url = $"{BaseUrl}/api/";

        public ChatService()
        {
        }

        public async Task<List<Chat>?> GetMyChatsAsync(string token, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _client.GetAsync($"{_url}chat/my");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    var chats = JsonSerializer.Deserialize<List<Chat>>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return chats;
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении чатов: Код: {response.StatusCode}, Ответ: {responseBody}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения при получении чатов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public async Task<Chat?> CreateChatAsync(CreateChatModel chatModel, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.PostAsJsonAsync($"{_url}chat/create", chatModel);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    var chat = JsonSerializer.Deserialize<Chat>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return chat;
                }
                else
                {
                    MessageBox.Show($"Ошибка при создании чата: Код: {response.StatusCode}, Ответ: {responseBody}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения при создании чата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public async Task<List<Message>?> GetMessagesAsync(int chatId, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.GetAsync($"{_url}chat/{chatId}/messages");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var messages = JsonSerializer.Deserialize<List<Message>>(responseBody, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return messages;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка десериализации сообщений. Ответ сервера: {responseBody}\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении сообщений: Код: {response.StatusCode}, Ответ: {responseBody}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении сообщений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public async Task<List<FileMessage>?> GetFileMessagesAsync(int chatId, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.GetAsync($"{_url}chat/{chatId}/file-messages");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var fileMessages = JsonSerializer.Deserialize<List<FileMessage>>(responseBody, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return fileMessages;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка десериализации файловых сообщений. Ответ сервера: {responseBody}\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении файловых сообщений: Код: {response.StatusCode}, Ответ: {responseBody}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении файловых сообщений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public async Task<Message?> SendMessageAsync(Message message, Window? ownerWindow = null)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                MessageBox.Show("Попытка отправить пустое сообщение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.PostAsJsonAsync($"{_url}chat/send", message);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Ошибка при отправке сообщения: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<Message>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Исключение при отправке сообщения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<FileMessage?> SendFileMessageAsync(int chatId, int senderId, string filePath, Window? ownerWindow = null)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Файл не выбран или не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(chatId.ToString()), "ChatId");
                form.Add(new StringContent(senderId.ToString()), "SenderId");
                form.Add(new StringContent(DateTime.UtcNow.ToString("o")), "SentAt");
                form.Add(new StringContent("false"), "IsEdited");
                form.Add(new StringContent(Path.GetFileName(filePath)), "filepath");
                var fileStream = File.OpenRead(filePath);
                form.Add(new StreamContent(fileStream), "File", Path.GetFileName(filePath));
                var response = await _client.PostAsync(_url + "chat/send-file", form);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Ошибка при отправке файла: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<FileMessage>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Исключение при отправке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<List<ChatMember>?> GetChatMembersAsync(int chatId, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.GetAsync($"{_url}chat/{chatId}/members");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    var members = await response.Content.ReadFromJsonAsync<List<ChatMember>>();
                    return members;
                }
                else
                {
                    MessageBox.Show($"Не удалось получить участников чата {chatId}: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении участников чата {chatId}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}