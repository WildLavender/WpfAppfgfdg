using System;

namespace CollegeGradeSystem.Models
{
    public class Grade
    {
        public int id_grade { get; set; }
        public int id_student { get; set; }
        public int id_discipline { get; set; }
        public int id_teacher { get; set; }
        public string value { get; set; }
        public DateTime grade_date { get; set; }
    }
}