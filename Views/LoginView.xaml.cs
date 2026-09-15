using System.Windows;
using System.Windows.Controls;
using CollegeGradeSystem.ViewModels;

namespace CollegeGradeSystem.Views
{
    public partial class LoginView : UserControl
    {
        private LoginViewModel _viewModel;
        private bool _isRegisterMode = false;

        public LoginView()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();

            _viewModel.LoginSuccess += OnLoginSuccess;
            _viewModel.RegisterSuccess += OnRegisterSuccess;
            UpdateThemeIcon();
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;

            if (app.CurrentTheme == "LightTheme")
                app.SwitchTheme("DarkTheme");
            else
                app.SwitchTheme("LightTheme");

            UpdateThemeIcon();
        }

        private void UpdateThemeIcon()
        {
            var app = (App)Application.Current;
            ThemeToggleButton.Content = app.CurrentTheme == "DarkTheme" ? "☀️" : "🌙";
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;
            string confirm = TxtConfirmPassword.Password;

            if (_isRegisterMode)
                _viewModel.Register(login, password, confirm);
            else
                _viewModel.Login(login, password);
        }

        private void SwitchMode_Click(object sender, RoutedEventArgs e)
        {
            _isRegisterMode = !_isRegisterMode;

            if (_isRegisterMode)
            {
                TitleLabel.Text = "Регистрация";
                ConfirmPanel.Visibility = Visibility.Visible;
                ActionButton.Content = "Зарегистрироваться";
                ((Button)sender).Content = "Уже есть аккаунт? Войти";
            }
            else
            {
                TitleLabel.Text = "Вход в систему";
                ConfirmPanel.Visibility = Visibility.Collapsed;
                ActionButton.Content = "Войти";
                ((Button)sender).Content = "Нет аккаунта? Зарегистрироваться";
            }

            TxtLogin.Clear();
            TxtPassword.Clear();
            TxtConfirmPassword.Clear();
        }

        private void OnLoginSuccess()
        {
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Application.Current.Windows[0].Close();
        }

        private void OnRegisterSuccess()
        {
            SwitchMode_Click(new Button(), new RoutedEventArgs());
        }
    }
}