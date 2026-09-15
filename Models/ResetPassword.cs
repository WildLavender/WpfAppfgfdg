namespace CollegeGradeSystem.Models
{
    public class ResetPassword
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}