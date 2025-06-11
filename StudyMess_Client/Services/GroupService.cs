using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using StudyMess_Client.Models;
using StudyMess_Client.Utils;
using static StudyMess_Client.Services.UrlStorage;

namespace StudyMess_Client.Services
{
    public class GroupService
    {
        private readonly HttpClient _client = new();
        private readonly string _url = $"{BaseUrl}/api/";

        public GroupService()
        {
        }

        public async Task<List<Group>?> GetGroupsAsync(Window? ownerWindow = null)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Token);
                var response = await _client.GetAsync($"{_url}group");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    UnauthorizedHandler.HandleUnauthorized(ownerWindow);
                    return null;
                }
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Group>>();
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении групп: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении групп: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
    }
}
