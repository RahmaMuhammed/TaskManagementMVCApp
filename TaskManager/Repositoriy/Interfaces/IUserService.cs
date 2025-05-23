using TaskManager.Models;

namespace TaskManager.Repositoriy.Interfaces
{
    public interface IUserService
    {
        public Task<List<AppUser>> GetAllUsersAsync();
        public Task<List<AppUser>> GetUsersHaveTasksAsync();
        public Task<AppUser> DeleteUserAsync(string id);
       // public Task<AppUser> GetUserByEmailAsync(string email);
       // public Task<AppUser> GetUserByIdAsync(string id);

    }
}
