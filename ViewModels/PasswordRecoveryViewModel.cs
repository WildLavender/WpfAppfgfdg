using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CollegeGradeSystem.Models;
using CollegeGradeSystem.Services;
using CollegeGradeSystem.ViewModels.Commands;

namespace CollegeGradeSystem.ViewModels
{
    public class PasswordRecoveryViewModel : INotifyPropertyChanged
    {
        private readonly string _connectionString = @"Data Source=209-U\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False";

        private string _email;
        private string _token;
        private string _newPassword;
        private string _confirmPassword;
        private bool _isTokenSent;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Token
        {
            get => _token;
            set { _token = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public bool IsTokenSent
        {
            get => _isTokenSent;
            set { _isTokenSent = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand SendTokenCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand CancelCommand { get; }

        public PasswordRecoveryViewModel()
        {
            SendTokenCommand = new RelayCommand(async _ => await SendTokenAsync(), _ => !string.IsNullOrEmpty(Email));
            ResetPasswordCommand = new RelayCommand(async _ => await ResetPasswordAsync(), _ => IsTokenSent && !string.IsNullOrEmpty(NewPassword));
            CancelCommand = new RelayCommand(_ => CancelRecovery());
        }

        /// <summary>
        /// Отправка токена восстановления на email
        /// </summary>
        private async Task SendTokenAsync()
        {
            try
            {
                // Проверяем, существует ли преподаватель с таким email
                Teacher teacher = await GetTeacherByEmailAsync(Email);

                if (teacher == null)
                {
                    MessageBox.Show("Пользователь с указанным email не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Генерируем токен (простой пример - случайное число)
                string token = new Random().Next(100000, 999999).ToString();

                // Сохраняем токен в БД (в реальном проекте нужна отдельная таблица)
                // 🔹 ИСПРАВЛЕНО: teacher.id_teacher вместо teacher.id
                await SaveTokenToDatabaseAsync(teacher.id_teacher, token);

                // 🔹 ОТПРАВЛЯЕМ EMAIL через статический метод
                await EmailService.SendDirectAsync(
                    Email,
                    "🔐 Восстановление пароля | CollegeGradeSystem",
                    $"Здравствуйте, {teacher.full_name}!\n\n" +
                    $"Ваш код для сброса пароля: {token}\n\n" +
                    "Код действителен 15 минут.\n\n" +
                    "Если вы не запрашивали сброс пароля, проигнорируйте это письмо."
                );

                IsTokenSent = true;
                Token = token; // Для отладки (в реальном проекте не показывать!)

                MessageBox.Show($"Код восстановления отправлен на {Email}\n(Для тестирования: {token})",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Сброс пароля
        /// </summary>
        private async Task ResetPasswordAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPassword.Length < 6)
            {
                MessageBox.Show("Пароль должен быть не менее 6 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Хешируем новый пароль
                string salt = PasswordHelper.GenerateSalt();
                string hash = PasswordHelper.ComputeHash(NewPassword, salt);

                // Обновляем пароль в БД
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    // 🔹 Обновляем пароль в таблице Users по login преподавателя
                    string sql = @"UPDATE Users SET PasswordHash = @Hash, Salt = @Salt 
                                   WHERE Login = (SELECT login FROM Teacher WHERE email = @Email)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Hash", hash);
                        cmd.Parameters.AddWithValue("@Salt", salt);
                        cmd.Parameters.AddWithValue("@Email", Email);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Пароль успешно изменен! Теперь вы можете войти с новым паролем.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Сбрасываем форму
                Email = "";
                NewPassword = "";
                ConfirmPassword = "";
                IsTokenSent = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сброса пароля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Получение данных преподавателя по email
        /// </summary>
        private async Task<Teacher> GetTeacherByEmailAsync(string email)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "SELECT id_teacher, full_name, email FROM Teacher WHERE email = @Email";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // 🔹 ИСПРАВЛЕНО: id_teacher вместо id
                            return new Teacher
                            {
                                id_teacher = reader.GetInt32(0),
                                full_name = reader.GetString(1),
                                email = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Сохранение токена в БД (упрощенно - заглушка)
        /// </summary>
        private async Task SaveTokenToDatabaseAsync(int teacherId, string token)
        {
            // В реальном проекте здесь нужно:
            // 1. Создать таблицу PasswordResetTokens (id, teacher_id, token, expires_at, is_used)
            // 2. Вставить запись с токеном
            // 3. Вернуть задачу для await
            await Task.CompletedTask;
        }

        private void CancelRecovery()
        {
            // Закрытие окна восстановления
            // В реальном проекте: найти и закрыть окно PasswordRecoveryView
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}