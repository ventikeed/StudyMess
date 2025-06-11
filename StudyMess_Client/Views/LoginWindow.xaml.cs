using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using StudyMess_Client.Models;
using StudyMess_Client.Services;

namespace StudyMess_Client.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;

        public LoginWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            var loginModel = new LoginModel
            {
                Login = login,
                Password = password
            };

            string? token = await _authService.LoginAsync(loginModel);

            if (!string.IsNullOrWhiteSpace(token))
            {
                TokenStorage.Clear();
                TokenStorage.SetToken(token);

                MessageBox.Show("Вход выполнен успешно!", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);

                var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                this.Close();
            }
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var window = App.ServiceProvider.GetRequiredService<RegisterWindow>();
            window.Show();
            this.Close();

        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
