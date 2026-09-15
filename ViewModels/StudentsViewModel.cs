using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using CollegeGradeSystem.Helpers;
using CollegeGradeSystem.Models;
using CollegeGradeSystem.Services;
using CollegeGradeSystem.ViewModels.Commands;

namespace CollegeGradeSystem.ViewModels
{
    public class StudentsViewModel : ViewModelBase
    {
        private readonly string _connectionString = @"Data Source=209-U\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False";

        #region Свойства

        private ObservableCollection<Student> _students;
        public ObservableCollection<Student> Students
        {
            get => _students;
            set => SetProperty(ref _students, value);
        }

        private ObservableCollection<Group> _groups;
        public ObservableCollection<Group> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    if (_selectedStudent != null)
                    {
                        CurrentEntity = new Student
                        {
                            id_student = _selectedStudent.id_student,
                            full_name = _selectedStudent.full_name,
                            birth_date = _selectedStudent.birth_date,
                            id_group = _selectedStudent.id_group,
                            email = _selectedStudent.email,
                            group_name = _selectedStudent.group_name,
                            Login = _selectedStudent.Login,
                            Password = _selectedStudent.Password
                        };
                    }
                    else
                    {
                        CurrentEntity = null;
                    }
                }
            }
        }

        private Student _currentEntity;
        public Student CurrentEntity
        {
            get => _currentEntity;
            set => SetProperty(ref _currentEntity, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterStudents();
                }
            }
        }

        private int? _selectedGroupId;
        public int? SelectedGroupId
        {
            get => _selectedGroupId;
            set
            {
                if (SetProperty(ref _selectedGroupId, value))
                {
                    FilterStudents();
                }
            }
        }

        #endregion

        #region Команды

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Роли

        public bool IsEditAllowed => Session.UserRole == "Администратор" || Session.UserRole == "Преподаватель";
        public bool CanDelete => Session.UserRole == "Администратор";

        #endregion

        #region Конструктор

        public StudentsViewModel()
        {
            LoadCommand = new RelayCommand(async _ => await LoadDataAsync(), _ => true);
            AddCommand = new RelayCommand(_ => AddNew(), _ => IsEditAllowed);
            SaveCommand = new RelayCommand(async _ => await SaveDataAsync(), _ => IsEditAllowed && CurrentEntity != null);
            DeleteCommand = new RelayCommand(async _ => await DeleteDataAsync(), _ => CanDelete && SelectedStudent != null);
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => CurrentEntity != null);
            SearchCommand = new RelayCommand(_ => FilterStudents(), _ => true);
            ClearFilterCommand = new RelayCommand(_ => ClearFilter(), _ => true);
            ExportCommand = new RelayCommand(_ => ExportData(), _ => IsEditAllowed);
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync(), _ => true);

            _ = LoadDataAsync();
            LoadGroups();
        }

        #endregion

        #region Методы работы с данными

        private async Task LoadDataAsync()
        {
            Students = new ObservableCollection<Student>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string sql = @"SELECT s.id_student, s.full_name, s.birth_date, s.id_group, 
                                          g.name as group_name, s.email, s.Login, s.Password
                                   FROM Student s 
                                   LEFT JOIN [Group] g ON s.id_group = g.id_group
                                   ORDER BY s.full_name";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Students.Add(new Student
                            {
                                id_student = reader.GetInt32(0),
                                full_name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                birth_date = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                                id_group = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                group_name = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                email = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                Login = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Password = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGroups()
        {
            Groups = new ObservableCollection<Group>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT id_group, name FROM [Group] ORDER BY name", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Groups.Add(new Group { id_group = reader.GetInt32(0), name = reader.GetString(1) });
                        }
                    }
                }
            }
            catch { }
        }

        private void FilterStudents()
        {
            if (string.IsNullOrWhiteSpace(SearchText) && (!SelectedGroupId.HasValue || SelectedGroupId == 0))
            {
                _ = LoadDataAsync();
                return;
            }

            var filtered = new ObservableCollection<Student>();
            foreach (var s in Students)
            {
                bool matchText = string.IsNullOrWhiteSpace(SearchText) ||
                    s.full_name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.email.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchGroup = !SelectedGroupId.HasValue || SelectedGroupId == 0 || s.id_group == SelectedGroupId.Value;

                if (matchText && matchGroup)
                    filtered.Add(s);
            }
            Students = filtered;
        }

        private void ClearFilter()
        {
            SearchText = "";
            SelectedGroupId = 0;
            _ = LoadDataAsync();
        }

        #endregion

        #region CRUD операции

        private async Task SaveDataAsync()
        {
            if (CurrentEntity == null) return;

            try
            {
                bool isNew = CurrentEntity.id_student == 0;
                string generatedLogin = CurrentEntity.Login;
                string generatedPassword = CurrentEntity.Password;

                // Автогенерация учетных данных для новых студентов
                if (isNew && string.IsNullOrWhiteSpace(generatedLogin))
                {
                    AuthHelper.GenerateCredentials(CurrentEntity.full_name, out generatedLogin, out generatedPassword);
                    CurrentEntity.Login = generatedLogin;
                    CurrentEntity.Password = generatedPassword;
                }

                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    if (isNew)
                    {
                        string sql = @"INSERT INTO Student (full_name, birth_date, id_group, email, Login, Password) 
                                       OUTPUT INSERTED.id_student
                                       VALUES (@FullName, @BirthDate, @IdGroup, @Email, @Login, @Password)";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@FullName", CurrentEntity.full_name);
                            cmd.Parameters.AddWithValue("@BirthDate", CurrentEntity.birth_date);
                            cmd.Parameters.AddWithValue("@IdGroup", CurrentEntity.id_group == 0 ? (object)DBNull.Value : CurrentEntity.id_group);
                            cmd.Parameters.AddWithValue("@Email", CurrentEntity.email ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Login", generatedLogin);
                            cmd.Parameters.AddWithValue("@Password", generatedPassword);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 🔔 УВЕДОМЛЕНИЕ О ДОБАВЛЕНИИ СТУДЕНТА (ВАШ ФОРМАТ)
                        try
                        {
                            string groupName = CurrentEntity.group_name ??
                                               Groups?.FirstOrDefault(g => g.id_group == CurrentEntity.id_group)?.name ?? "Не указана";
                            string emailInfo = string.IsNullOrWhiteSpace(CurrentEntity.email) ? "Не указан" : CurrentEntity.email;

                            string message = $"Здравствуйте!\n\n" +
                                             $"В систему добавлен новый студент:\n\n" +
                                             $"👤 ФИО: {CurrentEntity.full_name}\n" +
                                             $"🏫 Группа: {groupName}\n" +
                                             $"📧 Email: {emailInfo}\n" +
                                             $"📅 Дата рождения: {CurrentEntity.birth_date:dd.MM.yyyy}";

                            await SendNotificationToAllAdminsAsync(
                                title: "📚 Новый студент",
                                message: message,
                                type: "NewStudent"
                            );

                            AppEvents.RaiseNotification("📚 Новый студент", $"Добавлен: {CurrentEntity.full_name}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Ошибка уведомления: {ex.Message}");
                        }
                    }
                    else
                    {
                        string sql = @"UPDATE Student SET full_name = @FullName, birth_date = @BirthDate, 
                                       id_group = @IdGroup, email = @Email, Login = @Login, Password = @Password 
                                       WHERE id_student = @Id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@FullName", CurrentEntity.full_name);
                            cmd.Parameters.AddWithValue("@BirthDate", CurrentEntity.birth_date);
                            cmd.Parameters.AddWithValue("@IdGroup", CurrentEntity.id_group == 0 ? (object)DBNull.Value : CurrentEntity.id_group);
                            cmd.Parameters.AddWithValue("@Email", CurrentEntity.email ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Login", generatedLogin ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Password", generatedPassword ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Id", CurrentEntity.id_student);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                MessageBox.Show("Сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
                CurrentEntity = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteDataAsync()
        {
            if (SelectedStudent == null) return;

            var result = MessageBox.Show($"Удалить \"{SelectedStudent.full_name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 🔔 УВЕДОМЛЕНИЕ ОБ УДАЛЕНИИ СТУДЕНТА (ВАШ ФОРМАТ)
                    try
                    {
                        string groupName = SelectedStudent.group_name ?? "Не указана";
                        string emailInfo = string.IsNullOrWhiteSpace(SelectedStudent.email) ? "Не указан" : SelectedStudent.email;

                        string message = $"Здравствуйте!\n\n" +
                                         $"Из системы удален студент:\n\n" +
                                         $"👤 ФИО: {SelectedStudent.full_name}\n" +
                                         $"🏫 Группа: {groupName}\n" +
                                         $"📧 Email: {emailInfo}\n" +
                                         $"📅 Дата рождения: {SelectedStudent.birth_date:dd.MM.yyyy}";

                        await SendNotificationToAllAdminsAsync(
                            title: "🗑 Студент удален",
                            message: message,
                            type: "StudentDeleted"
                        );

                        AppEvents.RaiseNotification("🗑 Студент удален", $"Удален: {SelectedStudent.full_name}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Ошибка уведомления: {ex.Message}");
                    }

                    using (var conn = new SqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        using (var cmd = new SqlCommand("DELETE FROM Student WHERE id_student = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", SelectedStudent.id_student);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    MessageBox.Show("Удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                    CurrentEntity = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task SendNotificationToAllAdminsAsync(string title, string message, string type)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"SELECT u.Id FROM Users u 
                               INNER JOIN Roles r ON u.RoleId = r.Id 
                               WHERE r.Name = 'Администратор'";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int adminUserId = reader.GetInt32(0);
                        await NotificationService.CreateNotificationAsync(adminUserId, title, message, type);
                    }
                }
            }
        }

        private void CancelEdit()
        {
            if (SelectedStudent != null)
            {
                CurrentEntity = new Student
                {
                    id_student = SelectedStudent.id_student,
                    full_name = SelectedStudent.full_name,
                    birth_date = SelectedStudent.birth_date,
                    id_group = SelectedStudent.id_group,
                    email = SelectedStudent.email,
                    Login = SelectedStudent.Login,
                    Password = SelectedStudent.Password
                };
            }
            else
            {
                CurrentEntity = null;
            }
        }

        private void AddNew()
        {
            SelectedStudent = null;
            CurrentEntity = new Student { birth_date = DateTime.Now };
        }

        #endregion

        #region Экспорт в файл

        private void ExportData()
        {
            if (Students == null || Students.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Экспорт списка студентов",
                    FileName = $"Students_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    DefaultExt = ".txt",
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    string filePath = dialog.FileName;
                    var sb = new StringBuilder();

                    // Заголовок
                    sb.AppendLine("ID;ФИО;Дата рождения;Группа;Email;Логин");
                    sb.AppendLine("----------------------------------------");

                    // Данные
                    foreach (var student in Students)
                    {
                        string groupInfo = string.IsNullOrEmpty(student.group_name) ? "Не указана" : student.group_name;
                        string loginInfo = string.IsNullOrEmpty(student.Login) ? "Нет" : student.Login;

                        sb.AppendLine($"{student.id_student};" +
                                    $"\"{student.full_name}\";" +
                                    $"{student.birth_date:dd.MM.yyyy};" +
                                    $"\"{groupInfo}\";" +
                                    $"\"{student.email}\";" +
                                    $"\"{loginInfo}\"");
                    }

                    // Сохранение в файл UTF-8
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{filePath}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}