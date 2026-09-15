using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CollegeGradeSystem.Services;

namespace CollegeGradeSystem.Converters
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Visibility.Visible;

            string allowedRoles = parameter.ToString();
            string currentRole = Session.UserRole ?? "";

            return allowedRoles.Contains(currentRole) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}