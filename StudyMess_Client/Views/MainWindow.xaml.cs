using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using StudyMess_Client.Models;
using StudyMess_Client.Services;
using StudyMess_Client.Utils;
using StudyMess_Client.ViewModels;
using static StudyMess_Client.Services.UrlStorage;

namespace StudyMess_Client.Views
{
    public partial class MainWindow : Window
    {
        private readonly ChatService _chatService;
        private readonly UserService _userService;
        private readonly ChatHubService _chatHubService;
        private ObservableCollection<MessageViewModel> _messages = new();
        private ObservableCollection<ChatListItemViewModel> _chatItems = new();
        private ChatListItemViewModel? _selectedChat;
        private List<ChatMember>? _currentChatMembers;
        private List<User>? _currentChatUsers;
        private int? _myUserId;
        private int? _myChatMemberId;

        public MainWindow(
            ChatService chatService,
            UserService userService,
            ChatHubService chatHubService)
        {
            InitializeComponent();
            _chatService = chatService;
            _userService = userService;
            _chatHubService = chatHubService;
            _chatHubService.OnMessageReceived += ChatHubService_OnMessageReceived;
            _chatHubService.OnUserStatusChanged += ChatHubService_OnUserStatusChanged;
            _chatHubService.OnMessageEdited += ChatHubService_OnMessageEdited;
            _chatHubService.OnMessageDeleted += ChatHubService_OnMessageDeleted;
            _chatHubService.OnFileMessageDeleted += ChatHubService_OnFileMessageDeleted;
            _chatHubService.OnFileMessageReceived += ChatHubService_OnFileMessageReceived;
            _ = _chatHubService.StartAsync();

            _myUserId = TokenDecoder.GetUserId(TokenStorage.Token);
            ProfileName.Text = TokenDecoder.GetFullName(TokenStorage.Token) ?? "Неизвестный пользователь";
            ProfileEmail.Text = TokenDecoder.GetEmail(TokenStorage.Token) ?? "";
            string? avatarFileName = TokenDecoder.GetAvatar(TokenStorage.Token);
            if (!string.IsNullOrEmpty(avatarFileName))
            {
                string avatarUrl = $"{BaseUrl}{avatarFileName}";
                try
                {
                    ProfileAvatar.Source = new BitmapImage(new Uri(avatarUrl));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки аватара: " + ex.Message);
                }
            }
            LoadChat();
            SetAdminPanelButtonVisibility();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await _chatHubService.StopAsync();
            Properties.Settings.Default.Save();
        }

        public async void LoadChat()
        {
            await LoadChatsAsync();
        }

        private async Task LoadChatsAsync()
        {
            var chats = await _chatService.GetMyChatsAsync(TokenStorage.Token ?? "", this);
            _chatItems.Clear();

            if (chats != null)
            {
                foreach (var chat in chats)
                {
                    string? avatarUrl = null;
                    string displayName = chat.Name;
                    DateTime? lastOnline = null;
                    int? interlocutorUserId = null;

                    if (!chat.IsGroupChat)
                    {
                        var members = await _userService.GetChatUsersAsync(chat.Id, this);
                        User? interlocutor = null;
                        if (members != null)
                            interlocutor = members.FirstOrDefault(u => u.Id != _myUserId);

                        if (interlocutor != null)
                        {
                            displayName = interlocutor.Username;
                            interlocutorUserId = interlocutor.Id;
                            if (!string.IsNullOrEmpty(interlocutor.Avatar))
                                avatarUrl = $"{BaseUrl}{interlocutor.Avatar}";
                            lastOnline = interlocutor.LastOnline;
                        }
                        else
                        {
                            displayName = chat.Name;
                            avatarUrl = null;
                            lastOnline = null;
                        }
                    }

                    var chatVm = new ChatListItemViewModel
                    {
                        Id = chat.Id,
                        Name = displayName,
                        IsGroupChat = chat.IsGroupChat,
                        AvatarUrl = avatarUrl,
                        IsOnline = false,
                        LastOnline = lastOnline,
                        InterlocutorUserId = interlocutorUserId
                    };
                    _chatItems.Add(chatVm);
                }

                var allInterlocutorIds = _chatItems
                    .Where(c => !c.IsGroupChat && c.InterlocutorUserId.HasValue)
                    .Select(c => c.InterlocutorUserId.Value)
                    .Distinct()
                    .ToList();

                if (allInterlocutorIds.Count > 0)
                {
                    var statuses = await _userService.GetUsersOnlineStatusesAsync(allInterlocutorIds);
                    foreach (var chat in _chatItems.Where(c => !c.IsGroupChat && c.InterlocutorUserId.HasValue))
                    {
                        var status = statuses.FirstOrDefault(s => s.Id == chat.InterlocutorUserId.Value);
                        if (status != default)
                            chat.IsOnline = status.IsOnline;
                    }
                }

                ChatsListBox.ItemsSource = _chatItems;
            }
        }

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            var createChatWindow = App.ServiceProvider.GetRequiredService<CreateChatWindow>();
            createChatWindow.ShowDialog();
            _ = LoadChatsAsync();
        }

        private async void ChatsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedChat != null)
                await _chatHubService.LeaveChat(_selectedChat.Id);

            var selectedItem = ChatsListBox.SelectedItem as ChatListItemViewModel;
            _selectedChat = selectedItem;
            if (_selectedChat != null)
            {
                await _chatHubService.JoinChat(_selectedChat.Id);
                await LoadMessagesAsync(_selectedChat.Id);
            }
            else
            {
                _messages.Clear();
            }
        }

        private async Task LoadMessagesAsync(int chatId)
        {
            var messages = await _chatService.GetMessagesAsync(chatId, this);
            var fileMessages = await _chatService.GetFileMessagesAsync(chatId, this);

            _currentChatMembers = await _chatService.GetChatMembersAsync(chatId, this);
            _currentChatUsers = await _userService.GetChatUsersAsync(chatId, this);
            if (_currentChatUsers != null && _currentChatUsers.Count > 0)
            {
                var ids = _currentChatUsers.Select(u => u.Id).ToList();
                var statuses = await _userService.GetUsersOnlineStatusesAsync(ids);
                foreach (var user in _currentChatUsers)
                {
                    var status = statuses.FirstOrDefault(s => s.Id == user.Id);
                    user.IsOnline = status.IsOnline;
                }
            }
            _myChatMemberId = _currentChatMembers?.FirstOrDefault(cm => cm.UserId == _myUserId)?.Id;

            _messages.Clear();

            if (messages != null)
            {
                foreach (var m in messages)
                {
                    var member = _currentChatMembers?.FirstOrDefault(cm => cm.Id == m.SenderId);
                    var user = _currentChatUsers?.FirstOrDefault(u => u.Id == member?.UserId);

                    _messages.Add(new MessageViewModel
                    {
                        Id = m.Id,
                        ChatId = m.ChatId,
                        SenderId = m.SenderId,
                        SenderUsername = user?.Username ?? "Unknown",
                        Text = string.IsNullOrWhiteSpace(m.Content) ? null : m.Content,
                        SentAt = m.SentAt,
                        IsMine = m.SenderId == _myChatMemberId,
                        AttachmentFileName = null,
                        AttachmentFileUrl = null,
                        IsEdited = m.IsEdited
                    });
                }
            }

            if (fileMessages != null)
            {
                foreach (var fm in fileMessages)
                {
                    var member = _currentChatMembers?.FirstOrDefault(cm => cm.Id == fm.SenderId);
                    var user = _currentChatUsers?.FirstOrDefault(u => u.Id == member?.UserId);

                    _messages.Add(new MessageViewModel
                    {
                        Id = fm.Id,
                        ChatId = fm.ChatId,
                        SenderId = fm.SenderId,
                        SenderUsername = user?.Username ?? "Unknown",
                        Text = null,
                        SentAt = fm.SentAt,
                        IsMine = fm.SenderId == _myChatMemberId,
                        AttachmentFileName = string.IsNullOrWhiteSpace(fm.filepath) ? null : Path.GetFileName(fm.filepath),
                        AttachmentFileUrl = string.IsNullOrWhiteSpace(fm.filepath) ? null : $"{BaseUrl}{fm.filepath}"
                    });
                }
            }

            var sorted = _messages.OrderBy(m => m.SentAt).ToList();
            _messages.Clear();
            foreach (var m in sorted)
                _messages.Add(m);
            MessagesListBox.ItemsSource = _messages;
            if (_messages.Count > 0)
                MessagesListBox.ScrollIntoView(_messages[_messages.Count - 1]);
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChat == null || _myUserId == null)
                return;

            if (_myChatMemberId == null)
            {
                MessageBox.Show("Не удалось определить участника чата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string text = MessageTextBox.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var message = new Message
            {
                ChatId = _selectedChat.Id,
                SenderId = _myChatMemberId.Value,
                Content = text
            };

            var sentMessage = await _chatService.SendMessageAsync(message);
            MessageTextBox.Clear();
            await LoadMessagesAsync(_selectedChat.Id);
        }

        private async void AttachFile_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChat == null || _myUserId == null)
                return;

            if (_myChatMemberId == null)
            {
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                MessageBoxResult result = MessageBox.Show($"Точно хотите отправить {dialog.FileName}?", "Отправка файла", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
                try
                {
                    var sentFileMessage = await _chatService.SendFileMessageAsync(_selectedChat.Id, _myChatMemberId.Value, dialog.FileName);
                    if (sentFileMessage == null)
                    {
                        return;
                    }
                    await LoadMessagesAsync(_selectedChat.Id);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void ChatHubService_OnMessageReceived(Message message)
        {
            if (_selectedChat != null && message.ChatId == _selectedChat.Id)
            {
                var member = _currentChatMembers?.FirstOrDefault(cm => cm.Id == message.SenderId);
                var user = _currentChatUsers?.FirstOrDefault(u => u.Id == member?.UserId);

                bool isMine = message.SenderId == _myChatMemberId;
                Dispatcher.Invoke(() =>
                {
                    _messages.Add(new MessageViewModel
                    {
                        Id = message.Id,
                        ChatId = message.ChatId,
                        SenderId = message.SenderId,
                        SenderUsername = user?.Username ?? "Unknown",
                        Text = string.IsNullOrWhiteSpace(message.Content) ? null : message.Content,
                        SentAt = message.SentAt,
                        IsMine = isMine,
                        AttachmentFileName = null,
                        AttachmentFileUrl = null
                    });
                    if (_messages.Count > 0)
                        MessagesListBox.ScrollIntoView(_messages[_messages.Count - 1]);
                });

            }
        }

        private void ChatHubService_OnFileMessageReceived(FileMessage fileMessage)
        {
            if (_selectedChat != null && fileMessage.ChatId == _selectedChat.Id)
            {
                var member = _currentChatMembers?.FirstOrDefault(cm => cm.Id == fileMessage.SenderId);
                var user = _currentChatUsers?.FirstOrDefault(u => u.Id == member?.UserId);

                bool isMine = fileMessage.SenderId == _myChatMemberId;
                Dispatcher.Invoke(() =>
                {
                    _messages.Add(new MessageViewModel
                    {
                        Id = fileMessage.Id,
                        ChatId = fileMessage.ChatId,
                        SenderId = fileMessage.SenderId,
                        SenderUsername = user?.Username ?? "Unknown",
                        Text = null,
                        SentAt = fileMessage.SentAt,
                        IsMine = isMine,
                        AttachmentFileName = string.IsNullOrWhiteSpace(fileMessage.filepath) ? null : System.IO.Path.GetFileName(fileMessage.filepath),
                        AttachmentFileUrl = string.IsNullOrWhiteSpace(fileMessage.filepath) ? null : $"{BaseUrl}{fileMessage.filepath}"
                    });
                    if (_messages.Count > 0)
                        MessagesListBox.ScrollIntoView(_messages[_messages.Count - 1]);
                });
            }
        }

        private void ChatHubService_OnUserStatusChanged(int userId, bool isOnline)
        {
            Dispatcher.Invoke(() =>
            {
                if (_chatItems != null)
                {
                    foreach (var chat in _chatItems)
                    {
                        if (!chat.IsGroupChat && chat.InterlocutorUserId == userId)
                        {
                            chat.IsOnline = isOnline;
                            chat.LastOnline = isOnline ? null : DateTime.Now;
                        }
                    }
                }
            });
        }
        private async void ChatHubService_OnMessageEdited(Message editedMessage)
        {
            if (_selectedChat != null && editedMessage.ChatId == _selectedChat.Id)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await LoadMessagesAsync(_selectedChat.Id);
                });
            }
        }

        private async void ChatHubService_OnMessageDeleted(int messageId)
        {
            if (_selectedChat != null)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await LoadMessagesAsync(_selectedChat.Id);
                });
            }
        }
        private async void ChatHubService_OnFileMessageDeleted(int fileMessageId)
        {
            if (_selectedChat != null)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await LoadMessagesAsync(_selectedChat.Id);
                });
            }
        }
        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Вы точно хотите выйти из аккаунта?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (MessageBoxResult.No == messageBoxResult) return;
            await _chatHubService.StopAsync();
            TokenStorage.Clear();
            var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                if (e?.Uri == null || string.IsNullOrWhiteSpace(e.Uri.OriginalString))
                {
                    MessageBox.Show("Ссылка отсутствует или повреждена.");
                    return;
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии ссылки: {ex.Message}");
            }
        }

        private void EditMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is MessageViewModel message)
            {
                string? newText = Microsoft.VisualBasic.Interaction.InputBox("Изменить сообщение:", "Редактирование", message.Text ?? "");
                if (!string.IsNullOrWhiteSpace(newText) && newText != message.Text)
                {
                    _ = _chatHubService.EditMessageAsync(message.ChatId, message.Id, newText);
                }
            }
        }

        private void DeleteMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is MessageViewModel message)
            {
                if (MessageBox.Show("Удалить сообщение?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (!string.IsNullOrEmpty(message.AttachmentFileUrl))
                    {
                        _ = _chatHubService.DeleteFileMessageAsync(message.ChatId, message.Id);
                    }
                    else
                    {
                        _ = _chatHubService.DeleteMessageAsync(message.ChatId, message.Id);
                    }
                }
            }
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void SetAdminPanelButtonVisibility()
        {
            var role = TokenDecoder.GetRole(TokenStorage.Token);
            AdminPanelButton.Visibility = role == "Admin" ? Visibility.Visible : Visibility.Collapsed;
        }
        private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.ShowDialog();
        }

        private void UpdateChats_Click(object sender, RoutedEventArgs e)
        {
            LoadChat();
        }
        private async void AddMembers_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChat == null || !_selectedChat.IsGroupChat)
            {
                MessageBox.Show("Выберите групповой чат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectionWindow = new UserAddWindow(_selectedChat.Id) { Owner = this };
            if (selectionWindow.ShowDialog() == true)
            {
                var usersToAdd = selectionWindow.SelectedUsers;
                if (usersToAdd.Count > 0)
                {
                    var result = await _chatService.AddUsersToChatAsync(_selectedChat.Id, usersToAdd.Select(u => u.Id).ToList(), this);
                    if (result)
                    {
                        MessageBox.Show("Пользователи успешно добавлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMessagesAsync(_selectedChat.Id);
                    }
                }
            }
        }
    }
}