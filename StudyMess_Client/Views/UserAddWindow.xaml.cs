using Microsoft.Extensions.DependencyInjection;
using StudyMess_Client.Models;
using StudyMess_Client.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StudyMess_Client.Views
{
    public partial class UserAddWindow : Window
    {
        private readonly GroupService _groupService;
        private readonly UserService _userService;
        public int currentChatId;
        public ObservableCollection<User> AvailableUsers { get; set; } = new();
        public ObservableCollection<Group> Groups { get; set; } = new();
        public List<User> SelectedUsers { get; private set; } = new();

        public UserAddWindow(int ChatId)
        {
            InitializeComponent();
            _groupService = App.ServiceProvider.GetRequiredService<GroupService>();
            _userService = App.ServiceProvider.GetRequiredService<UserService>();
            currentChatId = ChatId;
            LoadGroups();
        }

        private async void LoadGroups()
        {
            var groups = await _groupService.GetGroupsAsync(this);
            if (groups != null)
            {
                Groups = new ObservableCollection<Group>(groups);
                GroupComboBox.ItemsSource = Groups;
            }
        }

        private async void GroupComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UsersListBox.ItemsSource = null;
            if (GroupComboBox.SelectedItem is Group group)
            {
                var groupUsers = await _userService.GetUsersByGroupIdAsync(group.Id, this);
                var chatUsers = await _userService.GetChatUsersAsync(currentChatId, this);

                if (groupUsers != null)
                {
                    var available = groupUsers
                        .Where(u => chatUsers == null || !chatUsers.Any(cu => cu.Id == u.Id))
                        .ToList();

                    AvailableUsers = new ObservableCollection<User>(available);
                    UsersListBox.ItemsSource = AvailableUsers;
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedUsers = UsersListBox.SelectedItems.Cast<User>().ToList();
            if (SelectedUsers.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одного пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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
    }
}