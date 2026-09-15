using System.Windows.Controls;
using CollegeGradeSystem.ViewModels; // Важно!

namespace CollegeGradeSystem.Views
{
    public partial class NotificationSettingsView : UserControl
    {
        public NotificationSettingsView()
        {
            InitializeComponent();
            // Назначаем DataContext
            this.DataContext = new NotificationSettingsViewModel();
        }
    }
}