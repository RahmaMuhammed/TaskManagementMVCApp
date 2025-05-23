namespace TaskManager.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public int TaskCount { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public List<DateTime> Deadlines { get; set; } = new();
    }
}