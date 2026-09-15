using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CollegeGradeSystem.Services
{
    public static class EmailService
    {
        // 🔹 SMTP НАСТРОЙКИ (ЗАМЕНИТЕ НА СВОИ!)
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "top1pasanl2@gmail.com"; // 🔸 Ваш Gmail
        private const string SenderPassword = "ntrz ieky ylwm ssls"; // 🔸 Пароль приложения

        /// <summary>
        /// Отправка email пользователю по его UserId
        /// </summary>
        public static async Task SendAsync(int userId, string subject, string body)
        {
            string recipientEmail = await GetUserEmailAsync(userId);

            if (string.IsNullOrEmpty(recipientEmail))
            {
                System.Diagnostics.Debug.WriteLine($"[EmailService] Email для UserId={userId} не найден");
                return;
            }

            await SendEmailAsync(recipientEmail, subject, body);
        }

        /// <summary>
        /// Отправка email напрямую (когда email уже известен)
        /// </summary>
        public static async Task SendDirectAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(toEmail)) return;
            await SendEmailAsync(toEmail, subject, body);
        }

        /// <summary>
        /// Основной метод отправки
        /// </summary>
        private static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(SenderEmail, "College Grade System");
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = false;

                    using (var client = new SmtpClient(SmtpServer, SmtpPort))
                    {
                        client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                        client.EnableSsl = true;
                        await client.SendMailAsync(message);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Email отправлен на {toEmail}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка отправки email: {ex.Message}");
                LogEmailForReport(toEmail, subject, body);
            }
        }

        /// <summary>
        /// Получение email пользователя из БД
        /// </summary>
        private static async Task<string> GetUserEmailAsync(int userId)
        {
            string connectionString = @"Data Source=209-U\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT Email FROM Users WHERE Id = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    var result = await cmd.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
        }

        /// <summary>
        /// Логирование email в файл (если SMTP не настроен)
        /// </summary>
        private static void LogEmailForReport(string email, string subject, string body)
        {
            try
            {
                string logFile = "EmailNotifications_Log.txt";
                string logEntry = $@"
========================================
Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
Получатель: {email}
Тема: {subject}
----------------------------------------
Текст письма:
{body}
========================================
";
                System.IO.File.AppendAllText(logFile, logEntry);
            }
            catch { }
        }

        /// <summary>
        /// Проверка, настроены ли SMTP данные
        /// </summary>
        public static bool IsConfigured()
        {
            return !string.IsNullOrEmpty(SenderEmail) &&
                   !string.IsNullOrEmpty(SenderPassword) &&
                   SenderEmail != "your.email@gmail.com";
        }
    }
}