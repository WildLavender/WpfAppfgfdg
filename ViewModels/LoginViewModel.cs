using System;
using System.Data.SqlClient;
using System.Windows;
using CollegeGradeSystem.Services;

namespace CollegeGradeSystem.ViewModels
{
    public class LoginViewModel
    {
        private readonly string _connectionString = @"Data Source=209-U\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False";

        public event Action LoginSuccess;
        public event Action RegisterSuccess;

        public void Login(string login, string password)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT u.PasswordHash, u.Salt, r.Name as RoleName 
                                   FROM Users u 
                                   LEFT JOIN Roles r ON u.RoleId = r.Id 
                                   WHERE u.Login = @Login";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        var reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            string dbHash = reader.GetString(0);
                            string dbSalt = reader.GetString(1);
                            string roleName = reader.IsDBNull(2) ? "Студент" : reader.GetString(2);

                            string inputHash = PasswordHelper.ComputeHash(password, dbSalt);

                            if (inputHash == dbHash)
                            {
                                Session.UserLogin = login;
                                Session.UserRole = roleName;

                                MessageBox.Show($"Вход выполнен успешно!\nРоль: {roleName}", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                LoginSuccess?.Invoke();
                                return;
                            }
                        }
                    }
                }
                MessageBox.Show("Неверный логин или пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Register(string login, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string salt = PasswordHelper.GenerateSalt();
                string hash = PasswordHelper.ComputeHash(password, salt);

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Users (Login, PasswordHash, Salt, RoleId) VALUES (@Login, @Hash, @Salt, 3)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@Hash", hash);
                        cmd.Parameters.AddWithValue("@Salt", salt);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                RegisterSuccess?.Invoke();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                    MessageBox.Show("Такой логин уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}