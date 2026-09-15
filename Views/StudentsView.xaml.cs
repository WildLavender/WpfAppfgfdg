using System.Windows.Controls;
using CollegeGradeSystem.ViewModels;

namespace CollegeGradeSystem.Views
{
    public partial class StudentsView : UserControl
    {
        public StudentsView()
        {
            InitializeComponent();
            // ⭐ ЭТО ОБЯЗАТЕЛЬНО: привязываем ViewModel к представлению
            this.DataContext = new StudentsViewModel();
        }
    }
}