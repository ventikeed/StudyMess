using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StudyMess_Client.Models;
using StudyMess_Client.Services;

namespace StudyMess_Client.Views
{
    public partial class AdminWindow : Window
    {
        private readonly AdminService _adminService;
        private ObservableCollection<User> _users = new();
        private ObservableCollection<Chat> _chats = new();
        private ObservableCollection<Message> _messages = new();
        private ObservableCollection<Group> _groups = new();

        public AdminWindow()
        {
            InitializeComponent();
            _adminService = App.ServiceProvider.GetRequiredService<AdminService>();
            LoadAllData();
        }

        private async void LoadAllData()
        {
            try
            {
                var users = await _adminService.GetUsersAsync();
                var chats = await _adminService.GetChatsAsync();
                var messages = await _adminService.GetMessagesAsync();
                var groups = await _adminService.GetGroupsAsync();

                _users = new ObservableCollection<User>(users ?? []);
                _chats = new ObservableCollection<Chat>(chats ?? []);
                _messages = new ObservableCollection<Message>(messages ?? []);
                _groups = new ObservableCollection<Group>(groups ?? []);

                UsersGrid.ItemsSource = _users;
                ChatsGrid.ItemsSource = _chats;
                MessagesGrid.ItemsSource = _messages;
                GroupsGrid.ItemsSource = _groups;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not User user)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"Удалить пользователя {user.Username}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = await _adminService.DeleteUserAsync(user.Id);
                if (success)
                {
                    _users.Remove(user);
                    MessageBox.Show($"Пользователь {user.Username} удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not User user)
            {
                MessageBox.Show("Выберите пользователя для изменения роли.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string newRole = NewRoleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newRole))
            {
                MessageBox.Show("Введите новую роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                bool success = await _adminService.ChangeUserRoleAsync(user.Id, newRole);
                if (success)
                {
                    user.Role = newRole;
                    UsersGrid.Items.Refresh();
                    MessageBox.Show($"Роль пользователя {user.Username} изменена на {newRole}.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteChat_Click(object sender, RoutedEventArgs e)
        {
            if (ChatsGrid.SelectedItem is not Chat chat)
            {
                MessageBox.Show("Выберите чат для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"Удалить чат \"{chat.Name}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = await _adminService.DeleteChatAsync(chat.Id);
                if (success)
                {
                    _chats.Remove(chat);
                    MessageBox.Show($"Чат \"{chat.Name}\" удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении чата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteMessage_Click(object sender, RoutedEventArgs e)
        {
            if (MessagesGrid.SelectedItem is not Message message)
            {
                MessageBox.Show("Выберите сообщение для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show("Удалить сообщение?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = await _adminService.DeleteMessageAsync(message.Id);
                if (success)
                {
                    _messages.Remove(message);
                    MessageBox.Show("Сообщение удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении сообщения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            string groupName = NewGroupNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(groupName))
            {
                MessageBox.Show("Введите название группы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var group = await _adminService.AddGroupAsync(groupName);
                if (group != null)
                {
                    _groups.Add(group);
                    MessageBox.Show($"Группа \"{groupName}\" добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NewGroupNameTextBox.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem is not Group group)
            {
                MessageBox.Show("Выберите группу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"Удалить группу \"{group.GroupName}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = await _adminService.DeleteGroupAsync(group.Id);
                if (success)
                {
                    _groups.Remove(group);
                    MessageBox.Show($"Группа \"{group.GroupName}\" удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
