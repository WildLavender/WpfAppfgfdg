using CollegeGradeSystem.ViewModels;
using System.Windows;

namespace CollegeGradeSystem.Views
{
    public partial class PasswordRecoveryView : Window
    {
        public PasswordRecoveryView()
        {
            InitializeComponent();
            DataContext = new PasswordRecoveryViewModel();

            var vm = (PasswordRecoveryViewModel)DataContext;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PasswordRecoveryViewModel.IsTokenSent))
                {
                    RequestPanel.Visibility = vm.IsTokenSent ? Visibility.Collapsed : Visibility.Visible;
                    ResetPanel.Visibility = vm.IsTokenSent ? Visibility.Visible : Visibility.Collapsed;
                }
            };
        }
    }
}