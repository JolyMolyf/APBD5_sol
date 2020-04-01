using System;

namespace APBD5_sol_s18935.Models
{
    public class Enrollment
    {
        public int IdEnrollment { get; set; }
        public int Semester { get; set; }
        public DateTime StartDate { get; set; }
        public Study Study { get; set; }
    }
}