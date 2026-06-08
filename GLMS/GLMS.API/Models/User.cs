using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; } 

        public string FullName { get; set; }
        public string Role { get; set; }
    }
}