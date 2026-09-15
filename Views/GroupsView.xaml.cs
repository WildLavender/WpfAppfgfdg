using CollegeGradeSystem.ViewModels;
using System.Windows.Controls;

namespace CollegeGradeSystem.Views
{
    public partial class GroupsView : UserControl
    {
        public GroupsView()
        {
            InitializeComponent();
            DataContext = new GroupsViewModel();
        }
    }
}