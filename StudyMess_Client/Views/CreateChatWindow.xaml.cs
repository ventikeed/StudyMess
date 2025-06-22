using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StudyMess_Client.Models;
using StudyMess_Client.Services;

namespace StudyMess_Client.Views
{
    public partial class CreateChatWindow : Window
    {
        private readonly GroupService _groupService;
        private readonly UserService _userService;
        private readonly ChatService _chatService;

        private ObservableCollection<Group> _groups = new();
        private ObservableCollection<User> _users = new();

        public CreateChatWindow(
            GroupService groupService,
            UserService userService,
            ChatService chatService)
        {
            InitializeComponent();
            _groupService = groupService;
            _userService = userService;
            _chatService = chatService;

            LoadGroups();
        }

        private async void LoadGroups()
        {
            var groups = await _groupService.GetGroupsAsync(this);
            if (groups != null)
            {
                _groups = new ObservableCollection<Group>(groups);
                GroupComboBox.ItemsSource = _groups;
            }
        }

        private async void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UsersListBox.ItemsSource = null;
            if (GroupComboBox.SelectedItem is Group group)
            {
                var users = await _userService.GetUsersByGroupIdAsync(group.Id, this);
                if (users != null)
                {
                    _users = new ObservableCollection<User>(users);
                    foreach (var user in _users.ToList())
                    {
                        if (user.Id == StudyMess_Client.Utils.TokenDecoder.GetUserId(StudyMess_Client.Services.TokenStorage.Token))
                        {
                            _users.Remove(user);
                        }
                    }
                    UsersListBox.ItemsSource = _users;
                }
            }
        }

        private void IsGroupChatCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UsersListBox.SelectionMode = IsGroupChatCheckBox.IsChecked == true ? SelectionMode.Extended : SelectionMode.Single;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string chatName;
            if (IsGroupChatCheckBox.IsChecked == false)
            {
                Random r = new Random();
                chatName = r.Next(1,10000000).ToString();
            }
            else
            {
                chatName = GroupComboBox.Text.Trim();
            }
            if (string.IsNullOrWhiteSpace(chatName))
            {
                MessageBox.Show("Введите название чата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedUsers = UsersListBox.SelectedItems.Cast<User>().ToList();
            if (selectedUsers.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одного участника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int? currentUserId = StudyMess_Client.Utils.TokenDecoder.GetUserId(StudyMess_Client.Services.TokenStorage.Token);
            if (currentUserId == null)
            {
                MessageBox.Show("Ошибка идентификации пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var selectedUserIds = selectedUsers.Select(u => u.Id).ToList();
            selectedUserIds.Add(currentUserId.Value);
            var chatModel = new CreateChatModel
            {
                Name = chatName,
                IsGroupChat = IsGroupChatCheckBox.IsChecked == true,
                UserIds = selectedUserIds
            };

            var chat = await _chatService.CreateChatAsync(chatModel, this);
            if (chat != null)
            {
                MessageBox.Show("Чат успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
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
