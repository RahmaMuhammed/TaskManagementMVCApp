namespace TaskManager.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int TotalUsers { get; set; }

        public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
    }
}
