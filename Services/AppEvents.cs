using System;

namespace CollegeGradeSystem.Services
{
    public static class AppEvents
    {
        // Событие: "Появилось новое уведомление"
        public static event Action<string, string> NewNotificationArrived;

        public static void RaiseNotification(string title, string message)
        {
            NewNotificationArrived?.Invoke(title, message);
        }
    }
}