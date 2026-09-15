namespace CollegeGradeSystem.Models
{
    public class Teacher
    {
        public int id_teacher { get; set; }
        public string full_name { get; set; }
        public string department { get; set; }
        public string position { get; set; }
        public string login { get; set; }
        public string password_hash { get; set; }
        public string email { get; set; }
    }
}