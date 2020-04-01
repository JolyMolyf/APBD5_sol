using System;
using System.ComponentModel.DataAnnotations;

namespace APBD5_sol_s18935.Models
{
    public class StdEnr
    {
        [Required]
        public string IndexNumber { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        [Required]
        public string Studies { get; set; }
    }
}