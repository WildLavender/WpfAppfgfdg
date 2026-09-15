using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CollegeGradeSystem.Models;
using CollegeGradeSystem.Services;
using CollegeGradeSystem.ViewModels.Commands;

namespace CollegeGradeSystem.ViewModels
{
    public class NotificationSettingsViewModel : ViewModelBase
    {
        private UserNotificationSettings _settings;
        private readonly string _connectionString = @"Data Source=209-U\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False";

        public UserNotificationSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }

        public NotificationSettingsViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadSettings());
            SaveCommand = new RelayCommand(async _ => await SaveSettings());
            _ = LoadSettings();
        }

        private async Task LoadSettings()
        {
            string login = Services.Session.UserLogin;
            if (string.IsNullOrEmpty(login)) return;

            int userId = await NotificationService.GetUserIdByLoginAsync(login);

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT * FROM UserNotificationSettings WHERE UserId = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Settings = new UserNotificationSettings
                            {
                                UserId = userId,
                                ReceiveEmail = reader.GetBoolean(1),
                                ReceiveInApp = reader.GetBoolean(2),
                                EmailForNewStudent = reader.GetBoolean(3),
                                EmailForSystemAlerts = reader.GetBoolean(4)
                            };
                        }
                        else
                        {
                            Settings = new UserNotificationSettings { UserId = userId, ReceiveEmail = true, ReceiveInApp = true, EmailForNewStudent = true, EmailForSystemAlerts = true };
                        }
                    }
                }
            }
        }

        private async Task SaveSettings()
        {
            if (Settings == null) return;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string sql = @"MERGE UserNotificationSettings AS target
                                   USING (VALUES (@UserId, @Email, @InApp, @NewStudent, @System)) 
                                   AS source (UserId, Email, InApp, NewStudent, System)
                                   ON target.UserId = source.UserId
                                   WHEN MATCHED THEN 
                                       UPDATE SET ReceiveEmail = source.Email, ReceiveInApp = source.InApp, 
                                                  EmailForNewStudent = source.NewStudent, EmailForSystemAlerts = source.System
                                   WHEN NOT MATCHED THEN 
                                       INSERT (UserId, ReceiveEmail, ReceiveInApp, EmailForNewStudent, EmailForSystemAlerts)
                                       VALUES (source.UserId, source.Email, source.InApp, source.NewStudent, source.System);";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", Settings.UserId);
                        cmd.Parameters.AddWithValue("@Email", Settings.ReceiveEmail);
                        cmd.Parameters.AddWithValue("@InApp", Settings.ReceiveInApp);
                        cmd.Parameters.AddWithValue("@NewStudent", Settings.EmailForNewStudent);
                        cmd.Parameters.AddWithValue("@System", Settings.EmailForSystemAlerts);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                MessageBox.Show("Настройки сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}