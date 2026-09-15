using System;

namespace CollegeGradeSystem.Models
{
    public class Group
    {
        // Основные поля
        public int id_group { get; set; }
        public string name { get; set; }

        
        public int course { get; set; }
        public int curator_id { get; set; }

        // ✅ ПУЛЕНЕПРОБИВАЕМЫЙ ToString()
        public override string ToString()
        {
            return name;
        }
    }
}