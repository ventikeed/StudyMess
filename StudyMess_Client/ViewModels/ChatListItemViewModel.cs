using System.ComponentModel;

public class ChatListItemViewModel : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsGroupChat { get; set; }
    public string? AvatarUrl { get; set; }
    public int? InterlocutorUserId { get; set; }

    private bool _isOnline;
    public bool IsOnline
    {
        get => _isOnline;
        set
        {
            if (_isOnline != value)
            {
                _isOnline = value;
                OnPropertyChanged(nameof(IsOnline));
            }
        }
    }

    private DateTime? _lastOnline;
    public DateTime? LastOnline
    {
        get => _lastOnline;
        set
        {
            if (_lastOnline != value)
            {
                _lastOnline = value;
                OnPropertyChanged(nameof(LastOnline));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
