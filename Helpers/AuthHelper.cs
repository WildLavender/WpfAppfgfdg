using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CollegeGradeSystem.Helpers
{
    public static class AuthHelper
    {
        private static readonly Dictionary<char, string> TranslitMap = new Dictionary<char, string>
        {
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"}, {'е', "e"}, {'ё', "yo"},
            {'ж', "zh"}, {'з', "z"}, {'и', "i"}, {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"},
            {'н', "n"}, {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"}, {'у', "u"},
            {'ф', "f"}, {'х', "h"}, {'ц', "ts"}, {'ч', "ch"}, {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""},
            {'ы', "y"}, {'ь', ""}, {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

        public static string ToTranslit(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text.ToLower())
                sb.Append(TranslitMap.ContainsKey(c) ? TranslitMap[c] : c.ToString());
            return sb.ToString();
        }

        public static void GenerateCredentials(string fullName, out string login, out string password)
        {
            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string surname = parts.Length > 0 ? parts[0] : "student";
            string firstname = parts.Length > 1 ? parts[1] : "user";

            string tSurname = ToTranslit(surname);
            string tFirstname = ToTranslit(firstname);

            Random rnd = new Random();
            string loginDigits, passDigits;

            do
            {
                loginDigits = rnd.Next(100, 999).ToString();
                passDigits = rnd.Next(10, 99).ToString();
            } while (loginDigits.Contains(passDigits[0]) || loginDigits.Contains(passDigits[1]));

            login = $"{tSurname}{char.ToUpper(tFirstname[0])}{loginDigits}";
            password = $"{tSurname}{tFirstname}{passDigits}";
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}