using System;

namespace CollegeGradeSystem.Models
{
    public class Student
    {
        public int id_student { get; set; }
        public string full_name { get; set; }
        public DateTime birth_date { get; set; }
        public int id_group { get; set; }
        public string group_name { get; set; }
        public string email { get; set; }

        // 🔹 ЭТИ СВОЙСТВА ОБЯЗАТЕЛЬНЫ ДОБАВЬТЕ:
        public string Login { get; set; }
        public string Password { get; set; }
    }
}