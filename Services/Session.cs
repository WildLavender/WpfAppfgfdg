namespace CollegeGradeSystem.Services
{
    public static class Session
    {
        public static string UserLogin { get; set; }
        public static string UserRole { get; set; }
        public static bool IsLoggedIn => UserLogin != null;

        public static void Clear()
        {
            UserLogin = null;
            UserRole = null;
        }
    }
}