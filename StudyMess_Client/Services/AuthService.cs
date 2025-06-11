using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using StudyMess_Client.Models;
using static StudyMess_Client.Services.UrlStorage;

namespace StudyMess_Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _client = new();
        private readonly string _url = $"{BaseUrl}/api/";

        public AuthService()
        {
        }

        public async Task<string?> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _client.PostAsJsonAsync($"{_url}Auth/login", model);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenObj = System.Text.Json.JsonDocument.Parse(json);
                    return tokenObj.RootElement.GetProperty("token").GetString();
                }
                else
                {
                    MessageBox.Show("Ошибка входа. Проверьте логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }


        public async Task<string?> RegisterAsync(RegisterModel model, string? avatarFilePath)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(model.Username), "Username");
                form.Add(new StringContent(model.Password), "Password");
                form.Add(new StringContent(model.FirstName), "FirstName");
                form.Add(new StringContent(model.LastName), "LastName");
                form.Add(new StringContent(model.Email), "Email");
                form.Add(new StringContent(model.Role), "Role");
                form.Add(new StringContent(model.GroupId.ToString()), "GroupId");

                if (!string.IsNullOrEmpty(avatarFilePath) && System.IO.File.Exists(avatarFilePath))
                {
                    var fileStream = System.IO.File.OpenRead(avatarFilePath);
                    var fileName = System.IO.Path.GetFileName(avatarFilePath);
                    form.Add(new StreamContent(fileStream), "Avatar", fileName);
                }

                var response = await _client.PostAsync($"{_url}auth/register", form);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenObj = System.Text.Json.JsonDocument.Parse(json);
                    return tokenObj.RootElement.GetProperty("token").GetString();
                }
                else
                {
                    MessageBox.Show("Ошибка регистрации. Проверьте введённые данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
    }
}
