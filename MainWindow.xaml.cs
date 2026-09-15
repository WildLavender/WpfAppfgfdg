using System.Windows;
using System.Windows.Controls;
using CollegeGradeSystem.Services;
using CollegeGradeSystem.ViewModels;
using CollegeGradeSystem.Views;

namespace CollegeGradeSystem
{
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;
        private NotificationsViewModel _notifViewModel;

        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel();
            _notifViewModel = new NotificationsViewModel();

            // 🔹 1. Синхронизация при старте и при открытии вкладки уведомлений
            _notifViewModel.UnreadCountChanged += (s, count) =>
            {
                Dispatcher.Invoke(() => _mainViewModel.UnreadCount = count);
            };

            // 🔹 2. МГНОВЕННОЕ обновление счетчика при добавлении студента (БЕЗ всплывающего окна)
            AppEvents.NewNotificationArrived += (title, message) =>
            {
                Dispatcher.Invoke(() => _mainViewModel.UnreadCount++);
            };

            this.DataContext = _mainViewModel;

            // Запускаем первоначальную загрузку (обновит счетчик при старте)
            _notifViewModel.LoadCommand.Execute(null);

            MainContent.Content = new StudentsView();
        }

        private void btnStudents_Click(object sender, RoutedEventArgs e) => MainContent.Content = new StudentsView();
        private void btnGroups_Click(object sender, RoutedEventArgs e) => MainContent.Content = new GroupsView();
        private void btnAdmin_Click(object sender, RoutedEventArgs e) => MessageBox.Show($"Роль: {Session.UserRole}", "Инфо", MessageBoxButton.OK);

        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем список и синхронизируем счетчик перед открытием
            _notifViewModel.LoadCommand.Execute(null);
            MainContent.Content = new NotificationView { DataContext = _notifViewModel };
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) => MainContent.Content = new NotificationSettingsView();

        private void cmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTheme.SelectedItem is ComboBoxItem item)
            {
                string themeName = item.Tag?.ToString();
                if (!string.IsNullOrEmpty(themeName))
                    ((App)Application.Current).SwitchTheme(themeName);
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Session.Clear();
                var loginWindow = new Window { Title = "Вход", Width = 500, Height = 600, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = new LoginView() };
                loginWindow.Show();
                this.Close();
            }
        }
    }
}