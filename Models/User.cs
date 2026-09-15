namespace CollegeGradeSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
    }
}