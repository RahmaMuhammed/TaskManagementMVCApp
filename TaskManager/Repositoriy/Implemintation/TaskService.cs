using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TaskManager.Repositoriy.Interfaces;
using TaskManager.ViewModels;

namespace TaskManager.Services.Implemintation
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public TaskService(AppDbContext context, IUserService userService, UserManager<AppUser> userManager)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
        }

        public async Task CreateTask(TaskViewModel taskVM)
        {
            var user = await _userManager.FindByEmailAsync(taskVM.UserEmail);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var task = new TaskItem
            {
                Title = taskVM.Title,
                Description = taskVM.Description,
                UserId = user.Id,
                Deadline = taskVM.Deadline,
                status = Task_Status.NotStarted
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();  
        }

        public async Task DeleteTask(int id)
        {
            var task =
                await _context.Tasks.SingleOrDefaultAsync(task => task.Id == id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Task not found");
            }
        }

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            return await _context.Tasks.Include(t => t.User).ToListAsync();
        }

        public Task<List<TaskItem>> GetAllTasksByUserIdAsync(string userId)
        {
            return _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetCompletedTaskCountByUserIdAsync(string userId)
        {
            return await _context.Tasks.CountAsync(t => t.UserId == userId && t.status == Task_Status.Completed);
        }

        public async Task<TaskItem> GetTaskById(int id)
        {
            return await _context.Tasks.Include(U => U.User).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<int> GetTaskCountByUserIdAsync(string userId)
        {
            return await _context.Tasks.CountAsync(t => t.UserId == userId);
        }

        public async Task UpdateTask(TaskViewModel taskVM)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskVM.Id);

            if (task != null)
            {
                task.Title = taskVM.Title;
                task.Description = taskVM.Description;
                task.Deadline = taskVM.Deadline;

                var newUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == taskVM.UserEmail);

                if (newUser != null)
                {
                    task.UserId = newUser.Id; 
                }
                else
                {
                    throw new Exception("Selected user not found.");
                }

                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Task not found.");
            }
        }

        public async Task<(bool success, string message)> UpdateTaskStatusAsync(int taskId, string userId, Task_Status newStatus)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null)
                return (false, "Task not found.");

            if (DateTime.Now > task.Deadline && task.status != Task_Status.Completed)
                return (false, "Status cannot be modified after Deadline.");

            task.status = newStatus;
            await _context.SaveChangesAsync();

            return (true, "The Task status has been modified.");
        }

    }
}
