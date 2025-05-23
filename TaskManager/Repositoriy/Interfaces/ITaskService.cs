using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Repositoriy.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetAllTasksAsync();
        Task<List<TaskItem>> GetAllTasksByUserIdAsync(string userId);
        Task<TaskItem> GetTaskById(int id);
        Task CreateTask(TaskViewModel task);
        Task UpdateTask(TaskViewModel task);
        Task DeleteTask(int id);
        Task<int> GetTaskCountByUserIdAsync(string userId);
        Task<int> GetCompletedTaskCountByUserIdAsync(string userId);
        Task<(bool success, string message)> UpdateTaskStatusAsync(int taskId, string userId, Task_Status newStatus);
    }
}
