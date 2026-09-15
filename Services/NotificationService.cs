using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CollegeGradeSystem.Models;

namespace CollegeGradeSystem.Services
{
    public static class NotificationService
    {
        private static readonly string _connectionString =
            @"Data Source=209-U\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False";

        public static async Task CreateNotificationAsync(int targetUserId, string title, string message, string type)
        {
            var settings = await GetUserSettingsAsync(targetUserId);

            // 1. Внутрисистемное уведомление
            if (settings.ReceiveInApp)
            {
                await SaveToDatabaseAsync(targetUserId, title, message, type);
            }

            // 2. Email уведомление
            if (settings.ReceiveEmail)
            {
                bool shouldSend = false;

                // Определяем, нужно ли отправлять email для этого типа
                if (type == "NewStudent")
                {
                    shouldSend = settings.EmailForNewStudent;
                }
                else if (type == "System")
                {
                    shouldSend = settings.EmailForSystemAlerts;
                }
                else
                {
                    shouldSend = true;
                }

                if (shouldSend)
                {
                    await EmailService.SendAsync(targetUserId, title, message);
                }
            }
        }

        private static async Task SaveToDatabaseAsync(int userId, string title, string message, string type)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"INSERT INTO Notifications (UserId, Title, Message, Type) 
                               VALUES (@UserId, @Title, @Message, @Type)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@Type", type);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task<List<Notification>> GetNotificationsAsync(int userId, bool unreadOnly = false)
        {
            var list = new List<Notification>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = unreadOnly
                    ? "SELECT * FROM Notifications WHERE UserId = @UserId AND IsRead = 0 ORDER BY CreatedAt DESC"
                    : "SELECT * FROM Notifications WHERE UserId = @UserId ORDER BY CreatedAt DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Notification
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Title = reader.GetString(2),
                                Message = reader.GetString(3),
                                Type = reader.GetString(4),
                                IsRead = reader.GetBoolean(5),
                                CreatedAt = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public static async Task MarkAsReadAsync(int notificationId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("UPDATE Notifications SET IsRead = 1 WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", notificationId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static async Task<UserNotificationSettings> GetUserSettingsAsync(int userId)
        {
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
                            return new UserNotificationSettings
                            {
                                UserId = reader.GetInt32(0),
                                ReceiveEmail = reader.GetBoolean(1),
                                ReceiveInApp = reader.GetBoolean(2),
                                EmailForNewStudent = reader.GetBoolean(3),
                                EmailForSystemAlerts = reader.GetBoolean(4)
                            };
                        }
                    }
                }
            }
            return new UserNotificationSettings
            {
                UserId = userId,
                ReceiveInApp = true,
                ReceiveEmail = true,
                EmailForNewStudent = true,
                EmailForSystemAlerts = true
            };
        }

        public static async Task<int> GetUserIdByLoginAsync(string login)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT Id FROM Users WHERE Login = @Login", conn))
                {
                    cmd.Parameters.AddWithValue("@Login", login);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? (int)result : 0;
                }
            }
        }
    }
}