using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using CollegeGradeSystem.Models;
using CollegeGradeSystem.Services;
using CollegeGradeSystem.ViewModels.Commands;

namespace CollegeGradeSystem.ViewModels
{
    public class GroupsViewModel : ViewModelBase
    {
        private readonly string _connectionString = @"Data Source=209-U\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False";

        private ObservableCollection<Group> _groups;
        public ObservableCollection<Group> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value) && _selectedGroup != null)
                {
                    EditGroup = new Group
                    {
                        id_group = _selectedGroup.id_group,
                        name = _selectedGroup.name,
                        course = _selectedGroup.course,
                        curator_id = _selectedGroup.curator_id
                    };
                }
            }
        }

        private Group _editGroup;
        public Group EditGroup
        {
            get => _editGroup;
            set => SetProperty(ref _editGroup, value);
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public bool IsEditAllowed => Session.UserRole == "Администратор" ||
                                     Session.UserRole == "Преподаватель";

        public bool CanDelete => Session.UserRole == "Администратор";

        public GroupsViewModel()
        {
            LoadCommand = new RelayCommand(_ => LoadData(), _ => true);
            AddCommand = new RelayCommand(_ => AddNew(), _ => IsEditAllowed);
            SaveCommand = new RelayCommand(_ => SaveData(), _ => IsEditAllowed && EditGroup != null);
            DeleteCommand = new RelayCommand(_ => DeleteData(), _ => CanDelete && SelectedGroup != null);
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => EditGroup != null);

            LoadData();
        }

        private void LoadData()
        {
            Groups = new ObservableCollection<Group>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "SELECT id_group, name, course, curator_id FROM [Group] ORDER BY name";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Groups.Add(new Group
                            {
                                id_group = reader.GetInt32(0),
                                name = reader.GetString(1),
                                course = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                curator_id = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNew()
        {
            EditGroup = new Group();
            SelectedGroup = null;
        }

        private void SaveData()
        {
            if (EditGroup == null) return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    if (EditGroup.id_group == 0)
                    {
                        string sql = @"INSERT INTO [Group] (name, course, curator_id) 
                                       VALUES (@Name, @Course, @CuratorId)";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", EditGroup.name);
                            cmd.Parameters.AddWithValue("@Course", EditGroup.course);
                            cmd.Parameters.AddWithValue("@CuratorId", EditGroup.curator_id == 0 ? (object)DBNull.Value : EditGroup.curator_id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string sql = @"UPDATE [Group] SET name = @Name, course = @Course, 
                                       curator_id = @CuratorId WHERE id_group = @Id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", EditGroup.name);
                            cmd.Parameters.AddWithValue("@Course", EditGroup.course);
                            cmd.Parameters.AddWithValue("@CuratorId", EditGroup.curator_id == 0 ? (object)DBNull.Value : EditGroup.curator_id);
                            cmd.Parameters.AddWithValue("@Id", EditGroup.id_group);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Данные успешно сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
                EditGroup = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteData()
        {
            if (SelectedGroup == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить группу \"{SelectedGroup.name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM [Group] WHERE id_group = @Id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", SelectedGroup.id_group);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Группа успешно удалена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                    EditGroup = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelEdit()
        {
            if (SelectedGroup != null)
            {
                EditGroup = new Group
                {
                    id_group = SelectedGroup.id_group,
                    name = SelectedGroup.name,
                    course = SelectedGroup.course,
                    curator_id = SelectedGroup.curator_id
                };
            }
            else
            {
                EditGroup = null;
            }
        }
    }
}