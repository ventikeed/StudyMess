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
    public class UserService
    {
        private readonly HttpClient _client = new();
        private readonly string _url = $"{BaseUrl}/api/";

        public UserService()
        {
        }

        public async Task<List<User>?> GetUsersAsync(string token, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _client.GetAsync($"{_url}user");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<User>>();
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении пользователей: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public async Task<List<User>?> GetUsersByGroupIdAsync(int groupId, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.GetAsync($"{_url}user/group/{groupId}");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<User>>();
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении пользователей группы: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении пользователей группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public async Task<List<User>?> GetChatUsersAsync(int chatId, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.GetAsync($"{_url}chat/{chatId}/users");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<User>>();
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении пользователей чата: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении пользователей чата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public async Task<List<(int Id, bool IsOnline)>> GetUsersOnlineStatusesAsync(List<int> userIds, Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.PostAsJsonAsync($"{_url}user/online-statuses", userIds);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return new List<(int, bool)>();
                }
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var statuses = JsonSerializer.Deserialize<List<UserStatusModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return statuses?.Select(s => (s.Id, s.IsOnline)).ToList() ?? new List<(int, bool)>();
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении статусов онлайн: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении статусов онлайн: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new List<(int, bool)>();
        }
    }
}
