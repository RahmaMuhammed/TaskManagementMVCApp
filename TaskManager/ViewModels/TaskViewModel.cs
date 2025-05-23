using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        // public string UserId { get; set; }
        public string UserEmail { get; set; } = "";
        public Task_Status Status { get; set; }
        public DateTime Deadline { get; set; }
    }
}