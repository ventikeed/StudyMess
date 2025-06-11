using System.Windows;
using StudyMess_Client.Services;

namespace StudyMess_Client.Utils
{
    public static class UnauthorizedHandler
    {
        public static void HandleUnauthorized(Window? currentWindow)
        {
            TokenStorage.Clear();
            MessageBox.Show("Ваша сессия истекла или вы не авторизованы. Пожалуйста, войдите в систему заново.", "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);

            var loginWindow = App.ServiceProvider.GetService(typeof(StudyMess_Client.Views.LoginWindow)) as Window;
            loginWindow?.Show();

            currentWindow?.Close();
        }
    }
}