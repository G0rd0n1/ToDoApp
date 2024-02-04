using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class UserModel
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        public ICollection<ToDoTaskModel> Tasks { get; set; }
    }
}
