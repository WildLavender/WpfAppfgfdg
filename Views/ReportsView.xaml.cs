using CollegeGradeSystem.Repositories;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CollegeGradeSystem.Views
{
    public partial class ReportsView : UserControl
    {
        private StudentRepository _studentRepository = new StudentRepository();

        public ReportsView()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                var students = _studentRepository.GetAll();
                var groups = _studentRepository.GetGroups();

                txtTotalStudents.Text = students.Count.ToString();
                txtTotalGroups.Text = groups.Count.ToString();
                txtTotalTeachers.Text = "15";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            MessageBox.Show("Статистика обновлена!", "Обновление",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}