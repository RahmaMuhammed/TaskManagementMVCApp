using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using TaskManager.Models;
using TaskManager.Repositoriy.Interfaces;

namespace TaskManager.Repositoriy.Implemintation
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AppUser> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return user;
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<List<AppUser>> GetUsersHaveTasksAsync()
        {
            var userEmailsWithTasks = await _context.Tasks
                .Where(t => t.User != null)
                .Select(t => t.User.Email)
                .Distinct()
                .ToListAsync();

            return await _context.Users
                .Where(u => userEmailsWithTasks.Contains(u.Email))
                .ToListAsync();
        }

       
        //public async Task<AppUser> GetUserByEmailAsync(string email)
        //{
        //    return await _context.Users
        //        .FirstOrDefaultAsync(u => u.Email == email);
        //}

        //public async Task<AppUser> GetUserByIdAsync(string id)
        //{
        //    return await _context.Users
        //        .FirstOrDefaultAsync(u => u.Id == id);
        //}
    }
}
