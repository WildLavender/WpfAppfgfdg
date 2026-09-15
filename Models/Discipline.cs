namespace CollegeGradeSystem.Models
{
    public class Discipline
    {
        public int id_discipline { get; set; }
        public string name { get; set; }
        public int total_hours { get; set; }
        public string control_type { get; set; }
        public int id_teacher { get; set; }
    }
}