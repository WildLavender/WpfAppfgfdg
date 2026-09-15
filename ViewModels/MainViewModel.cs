using System.ComponentModel;
using System.Runtime.CompilerServices;
using CollegeGradeSystem.Services;

namespace CollegeGradeSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _userDisplay;
        private int _unreadCount;

        public string UserDisplay
        {
            get => _userDisplay;
            set { _userDisplay = value; OnPropertyChanged(); }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                _unreadCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasUnread));
            }
        }

        public bool HasUnread => UnreadCount > 0;

        public MainViewModel()
        {
            UpdateSessionInfo();
        }

        public void UpdateSessionInfo()
        {
            UserDisplay = $"{Session.UserLogin} | {Session.UserRole}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}