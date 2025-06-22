using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using StudyMess_Client.Models;
using StudyMess_Client.Services;
using Group = StudyMess_Client.Models.Group;

namespace StudyMess_Client.Views
{
    public partial class RegisterWindow : Window
    {
        private string? _avatarFilePath;
        private readonly AuthService _authService;
        private readonly GroupService _groupService;
        private ObservableCollection<Group> _groups = new();

        public RegisterWindow(
            AuthService authService,
            GroupService groupService)
        {
            InitializeComponent();
            _authService = authService;
            _groupService = groupService;
            LoadGroups();
        }
        private async void LoadGroups()
        {
            var groups = await _groupService.GetGroupsAsync();
            if (groups != null)
            {
                _groups = new ObservableCollection<Group>(groups);
                GroupListBox.ItemsSource = _groups;
            }
            else
            {
                MessageBox.Show("Не удалось загрузить список групп", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void SelectAvatar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _avatarFilePath = openFileDialog.FileName;

                var bitmap = new BitmapImage();
                using (var stream = new FileStream(_avatarFilePath, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                AvatarPreview.Source = bitmap;
            }
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = GroupListBox.SelectedItem as Group;
            string[] fullName = FullNameTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fullName.Length > 2 || fullName.Length < 2)
            {
                MessageBox.Show("Введите имя и фамилию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (UsernameTextBox.Text.Length < 3 || UsernameTextBox.Text.Length > 20)
            {
                MessageBox.Show("Длина логина должна быть от 3 до 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (PasswordBox.Password.Length < 6 || PasswordBox.Password.Length > 20)
            {
                MessageBox.Show("Длина пароля должна быть от 6 до 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Regex regex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
            if (!regex.IsMatch(EmailTextBox.Text))
            {
                MessageBox.Show("Введите корректную почту.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }    
            if (EmailTextBox.Text.Length < 5 || EmailTextBox.Text.Length > 50)
            {
                MessageBox.Show("Длина email должна быть от 5 до 50 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (selectedGroup == null)
            {
                MessageBox.Show("Выберите группу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var registerModel = new RegisterModel
            {
                Username = UsernameTextBox.Text,
                Password = PasswordBox.Password,
                FirstName = fullName.Length > 0 ? fullName[0] : "",
                LastName = fullName.Length > 1 ? fullName[1] : "",
                Email = EmailTextBox.Text,
                Role = "Student",
                GroupId = selectedGroup?.Id ?? 0
            };

            var token = await _authService.RegisterAsync(registerModel, _avatarFilePath);

            if (!string.IsNullOrEmpty(token))
            {
                TokenStorage.Clear();
                TokenStorage.SetToken(token);
                MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                this.Close();
            }
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

        private void EmailTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
