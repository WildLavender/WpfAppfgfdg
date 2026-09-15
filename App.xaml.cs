using System;
using System.Linq;
using System.Windows;

namespace CollegeGradeSystem
{
    public partial class App : Application
    {
        // 🔹 Свойство для хранения текущей темы
        public string CurrentTheme { get; set; } = "LightTheme";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Загружаем тему по умолчанию
            SwitchTheme("LightTheme");

            // Создаем и показываем окно входа
            var loginWindow = new Window
            {
                Title = "Вход в систему",
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new Views.LoginView()
            };

            loginWindow.Show();
        }

        /// <summary>
        /// Метод переключения темы
        /// </summary>
        public void SwitchTheme(string themeName)
        {
            // Находим текущую тему в словарях ресурсов
            var oldTheme = Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("Themes/") == true);

            // Удаляем старую тему
            if (oldTheme != null)
            {
                Resources.MergedDictionaries.Remove(oldTheme);
            }

            // Создаем и добавляем новую тему
            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"Themes/{themeName}.xaml", UriKind.Relative)
            };

            Resources.MergedDictionaries.Add(newTheme);

            // 🔹 Запоминаем текущую тему
            CurrentTheme = themeName;
        }
    }
}