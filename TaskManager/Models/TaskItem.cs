using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public enum Task_Status
    {
        NotStarted,
        InProgress,
        Completed
    }
    public class TaskItem
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime Deadline { get; set; }
        public Task_Status status { get; set; } = Task_Status.NotStarted;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }
        public AppUser User { get; set; }
        //[ForeignKey("Admin")]
        //[Required]
        //public string CreatedById { get; set; }
        //public AppUser Admin { get; set; }
        public DateTime? CompletedAt { get; set; }

    }
}
