using CollegeGradeSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace CollegeGradeSystem.Repositories
{
    public class StudentRepository
    {
        private string connectionString = "Data Source=209-U\\SQLEXPRESS01;Initial Catalog=CollegeGradeDB;User ID=sa;Password=0000;Encrypt=False;";

        /// <summary>
        /// ЧТЕНИЕ: Получение всех студентов с группами
        /// </summary>
        public ObservableCollection<Student> GetAll()
        {
            var students = new ObservableCollection<Student>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT s.id_student, s.full_name, s.birth_date, 
                                s.id_group, s.email, g.name as group_name 
                                FROM Student s 
                                LEFT JOIN [Group] g ON s.id_group = g.id_group";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    students.Add(new Student
                    {
                        id_student = (int)reader["id_student"],
                        full_name = reader["full_name"].ToString(),
                        birth_date = (DateTime)reader["birth_date"],
                        id_group = (int)reader["id_group"],
                        group_name = reader["group_name"] != DBNull.Value ? reader["group_name"].ToString() : "",
                        email = reader["email"] != DBNull.Value ? reader["email"].ToString() : ""
                    });
                }
            }
            return students;
        }

        /// <summary>
        /// СОЗДАНИЕ/ОБНОВЛЕНИЕ: Сохранение студента
        /// </summary>
        public void Save(Student student)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (student.id_student == 0) // Новая запись
                {
                    string query = @"INSERT INTO Student (full_name, birth_date, id_group, email) 
                                    VALUES (@full_name, @birth_date, @id_group, @email)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@full_name", student.full_name);
                    cmd.Parameters.AddWithValue("@birth_date", student.birth_date);
                    cmd.Parameters.AddWithValue("@id_group", student.id_group);
                    cmd.Parameters.AddWithValue("@email", (object)student.email ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                else // Обновление существующей
                {
                    string query = @"UPDATE Student SET full_name=@full_name, birth_date=@birth_date, 
                                    id_group=@id_group, email=@email WHERE id_student=@id_student";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_student", student.id_student);
                    cmd.Parameters.AddWithValue("@full_name", student.full_name);
                    cmd.Parameters.AddWithValue("@birth_date", student.birth_date);
                    cmd.Parameters.AddWithValue("@id_group", student.id_group);
                    cmd.Parameters.AddWithValue("@email", (object)student.email ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// УДАЛЕНИЕ: Удаление студента по ID
        /// </summary>
        public void Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Student WHERE id_student = @id_student";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id_student", id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Получение списка групп для ComboBox
        /// </summary>
        public ObservableCollection<Group> GetGroups()
        {
            var groups = new ObservableCollection<Group>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM [Group]";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    groups.Add(new Group
                    {
                        id_group = (int)reader["id_group"],
                        name = reader["name"].ToString(),
                        course = (int)reader["course"],
                        curator_id = reader["curator_id"] != DBNull.Value ? (int)reader["curator_id"] : 0
                    });
                }
            }
            return groups;
        }
    }
}