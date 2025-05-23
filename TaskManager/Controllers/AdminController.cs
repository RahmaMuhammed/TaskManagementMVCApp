using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TaskManager.Repositoriy.Interfaces;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        /// ///////////////////////////////////////////////////// Tasks //////////////////////////////////////////////
      
        public AdminController(ITaskService taskService, IUserService userService, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
        {
            _taskService = taskService;
            _userService = userService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string search)
        {
            var tasks = await _taskService.GetAllTasksAsync();
            var taskVMs = tasks.Select(t => new TaskViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Deadline = t.Deadline,
                Status = t.status,
                UserEmail = t.User?.Email
            }).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                taskVMs = taskVMs
                    .Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || t.UserEmail.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var users = await _userService.GetUsersHaveTasksAsync();

            var userViewModels = users.Select(u =>
            {
                var userTasks = taskVMs.Where(t => t.UserEmail == u.Email).ToList();

                return new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    TaskCount = userTasks.Count,
                    CompletedTasks = userTasks.Count(t => t.Status == Task_Status.Completed),
                    InProgressTasks = userTasks.Count(t => t.Status == Task_Status.InProgress),
                    Deadlines = userTasks.Select(t => t.Deadline).ToList()
                };
            }).ToList();

            var dashboardVM = new AdminDashboardViewModel
            {
                TotalTasks = taskVMs.Count,
                CompletedTasks = taskVMs.Count(t => t.Status == Task_Status.Completed),
                PendingTasks = taskVMs.Count(t => t.Status == Task_Status.InProgress),
                TotalUsers = users.Count,
                Tasks = taskVMs ?? new List<TaskViewModel>(),
                Users = userViewModels
            };

            return View(dashboardVM);

        }
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _taskService.GetTaskById(id);
            if (task == null)
            {
                return NotFound();
            }

            var taskVM = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                UserEmail = task.User?.Email
            };

            return View(taskVM);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _taskService.DeleteTask(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
        public async Task<IActionResult> EditTask(int id)
        {
            var task = await _taskService.GetTaskById(id);

            if (task == null)
                return NotFound();

            var taskVM = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                UserEmail = task.User?.Email
            };

            var users = await _userService.GetAllUsersAsync();
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var regularUsers = users.Except(adminUsers).ToList();
            ViewBag.UserEmails = new SelectList(regularUsers, "Email", "Email");

            return View(taskVM);
        }
        [HttpPost]
        public async Task<IActionResult> EditTask(TaskViewModel taskVM)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _taskService.UpdateTask(taskVM);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(taskVM);
        }
        public async Task<IActionResult> TaskDetails(int id)
        {
            var task = await _taskService.GetTaskById(id);
            if (task == null)
            {
                return NotFound();
            }
            var taskVM = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Status = task.status,
                UserEmail = task.User?.Email
            };
            return View(taskVM);
        }
        public async Task<IActionResult> CreateTask()
        {
            var users = await _userService.GetAllUsersAsync();
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var regularUsers = users.Except(adminUsers).ToList();
            ViewBag.UserEmails = new SelectList(regularUsers, "Email", "Email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskViewModel taskVM)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(taskVM.UserEmail);

                if (user == null)
                {
                    ModelState.AddModelError("UserEmail", "Selected user email does not exist.");
                    var users = await _userService.GetAllUsersAsync();
                    ViewBag.UserEmails = new SelectList(users, "Email", "Email");
                    return View(taskVM);
                }

               
                await _taskService.CreateTask(taskVM);
                return RedirectToAction("Index");
            }

            var allUsers = await _userService.GetAllUsersAsync();
            ViewBag.UserEmails = new SelectList(allUsers, "Email", "Email");
            return View(taskVM);
        }

        //////////////////////////////////////////// Users ////////////////////////////////////////////
        
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var users = await _userManager.FindByIdAsync(Id);
            if (users == null)
            {
                return NotFound();
            }
            var userVM = new UserViewModel
            {
                Id = users.Id,
                Email = users.Email,
                TaskCount = await _taskService.GetTaskCountByUserIdAsync(users.Id),
                CompletedTasks = await _taskService.GetCompletedTaskCountByUserIdAsync(users.Id),
            };
            return View(userVM);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUserConfirmed(string Id)
        {
            try
            {
                if(await _userService.DeleteUserAsync(Id) == null)
                {
                    return NotFound();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

    }
}
