using CollegeGradeSystem.Models;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CollegeGradeSystem.Repositories
{
    public class TeacherRepository
    {
        private string connectionString = "Data Source=209-U\\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False;";

        /// <summary>
        /// Проверка логина и пароля при входе
        /// </summary>
        public async Task<Teacher> ValidateCredentialsAsync(string login, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string passwordHash = HashPassword(password);

                string query = @"SELECT * FROM Teacher 
                                WHERE login = @login AND password_hash = @password_hash";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password_hash", passwordHash);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                {
                    return new Teacher
                    {
                        id_teacher = (int)reader["id_teacher"],
                        full_name = reader["full_name"].ToString(),
                        email = reader["email"] != DBNull.Value ? reader["email"].ToString() : ""
                    };
                }

                return null;
            }
        }

        /// <summary>
        /// Получить преподавателя по email
        /// </summary>
        public async Task<Teacher> GetByEmailAsync(string email)
        {
            Teacher teacher = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM Teacher WHERE email = @email";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", email);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    teacher = new Teacher
                    {
                        id_teacher = (int)reader["id_teacher"],
                        full_name = reader["full_name"].ToString(),
                        email = reader["email"] != DBNull.Value ? reader["email"].ToString() : ""
                    };
                }
            }
            return teacher;
        }

        /// <summary>
        /// Сохранить токен сброса пароля
        /// </summary>
        public async Task SaveResetTokenAsync(int userId, string token, DateTime expiresAt)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO PasswordResetTokens (UserId, Token, TokenExpires) 
                                VALUES (@UserId, @Token, @TokenExpires)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@TokenExpires", expiresAt);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Проверить валидность токена
        /// </summary>
        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = @"SELECT COUNT(*) FROM PasswordResetTokens 
                                WHERE Token = @Token AND TokenExpires > GETDATE() AND IsUsed = 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Token", token);
                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        /// <summary>
        /// Сбросить пароль по токену
        /// </summary>
        public async Task ResetPasswordByTokenAsync(string token, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string getUserIdQuery = "SELECT UserId FROM PasswordResetTokens WHERE Token = @Token";
                SqlCommand getUserIdCmd = new SqlCommand(getUserIdQuery, conn);
                getUserIdCmd.Parameters.AddWithValue("@Token", token);
                int userId = (int)await getUserIdCmd.ExecuteScalarAsync();

                string updateQuery = "UPDATE Teacher SET password_hash = @Password WHERE id_teacher = @Id";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@Password", HashPassword(newPassword));
                updateCmd.Parameters.AddWithValue("@Id", userId);
                await updateCmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Аннулировать токен
        /// </summary>
        public async Task InvalidateTokenAsync(string token)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE PasswordResetTokens SET IsUsed = 1 WHERE Token = @Token";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Token", token);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Хеширование пароля (SHA256) - HEX формат
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}