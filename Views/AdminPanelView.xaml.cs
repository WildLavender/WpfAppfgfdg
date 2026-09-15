using System.Windows.Controls;

namespace CollegeGradeSystem.Views // ⭐ Должно быть точно так же
{
    public partial class AdminPanelView : UserControl // ⭐ public partial обязательно
    {
        public AdminPanelView()
        {
            InitializeComponent(); // Теперь найдётся
        }
    }
}