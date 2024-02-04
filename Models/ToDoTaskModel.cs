using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class ToDoTaskModel
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Notes { get; set; }
        public bool Completed { get; set; }
        [Required]
        public int UserID { get; set; }
    }
}
