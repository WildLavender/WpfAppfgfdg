using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CollegeGradeSystem.Models;
using CollegeGradeSystem.Services;
using CollegeGradeSystem.ViewModels.Commands;

namespace CollegeGradeSystem.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        private ObservableCollection<Notification> _allNotifications;
        private ObservableCollection<Notification> _unreadNotifications;
        private ObservableCollection<Notification> _readNotifications;
        private int _unreadCount;

        // 🔹 Все уведомления (для XAML)
        public ObservableCollection<Notification> AllNotifications
        {
            get => _allNotifications;
            set => SetProperty(ref _allNotifications, value);
        }

        public ObservableCollection<Notification> UnreadNotifications
        {
            get => _unreadNotifications;
            set => SetProperty(ref _unreadNotifications, value);
        }

        public ObservableCollection<Notification> ReadNotifications
        {
            get => _readNotifications;
            set => SetProperty(ref _readNotifications, value);
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set => SetProperty(ref _unreadCount, value);
        }

        public bool HasUnread => UnreadCount > 0;

        public ICommand LoadCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }

        public event EventHandler<int> UnreadCountChanged;

        public NotificationsViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadNotificationsAsync());
            MarkAsReadCommand = new RelayCommand(async p => await MarkAsReadAsync((int)p));
            MarkAllAsReadCommand = new RelayCommand(async _ => await MarkAllAsReadAsync());

            // 🔹 Инициализируем коллекции
            AllNotifications = new ObservableCollection<Notification>();
            UnreadNotifications = new ObservableCollection<Notification>();
            ReadNotifications = new ObservableCollection<Notification>();

            _ = LoadNotificationsAsync();
        }

        public async Task LoadNotificationsAsync()
        {
            try
            {
                string login = Services.Session.UserLogin;
                if (string.IsNullOrEmpty(login)) return;

                int userId = await NotificationService.GetUserIdByLoginAsync(login);
                if (userId == 0) return;

                var all = await NotificationService.GetNotificationsAsync(userId, false);

                // 🔹 Обновляем все коллекции
                UpdateCollection(AllNotifications, all);

                var unread = all.Where(n => !n.IsRead).OrderByDescending(n => n.CreatedAt).ToList();
                var read = all.Where(n => n.IsRead).OrderByDescending(n => n.CreatedAt).ToList();

                UpdateCollection(UnreadNotifications, unread);
                UpdateCollection(ReadNotifications, read);

                UnreadCount = unread.Count;
                OnPropertyChanged(nameof(HasUnread));

                UnreadCountChanged?.Invoke(this, UnreadCount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки уведомлений: {ex.Message}");
            }
        }

        private void UpdateCollection<T>(ObservableCollection<T> target, System.Collections.Generic.List<T> source)
        {
            target.Clear();
            foreach (var item in source) target.Add(item);
        }

        private async Task MarkAsReadAsync(int notificationId)
        {
            await NotificationService.MarkAsReadAsync(notificationId);
            await LoadNotificationsAsync();
        }

        private async Task MarkAllAsReadAsync()
        {
            var ids = UnreadNotifications.Select(n => n.Id).ToList();
            foreach (var id in ids)
            {
                await NotificationService.MarkAsReadAsync(id);
            }
            await LoadNotificationsAsync();
        }
    }
}