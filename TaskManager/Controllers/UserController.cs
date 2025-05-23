using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Repositoriy.Interfaces;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITaskService _taskService;

        public UserController(UserManager<AppUser> userManager, ITaskService taskService)
        {
            _userManager = userManager;
            _taskService = taskService;
        }

        // Show All Tasks for the logged-in user
        public async Task<IActionResult> Index()
        {
            // Get the logged-in user (by default the userManager deal with logged-in user by token or Cookie)
            var user = await _userManager.GetUserAsync(User);
            var tasks = await _taskService.GetAllTasksByUserIdAsync(user.Id);
            var taskViewModels = tasks.Select(t => new TaskViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Deadline = t.Deadline,
                Status = t.status
            }).ToList();

            return View(taskViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, Task_Status newStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            var (success, message) = await _taskService.UpdateTaskStatusAsync(id, user.Id, newStatus);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = message;
            return RedirectToAction("Index");
        }
    }
}
