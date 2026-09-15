namespace CollegeGradeSystem.Models
{
    public class UserNotificationSettings
    {
        public int UserId { get; set; }
        public bool ReceiveEmail { get; set; }
        public bool ReceiveInApp { get; set; }
        public bool EmailForNewStudent { get; set; }
        public bool EmailForSystemAlerts { get; set; }
    }
}