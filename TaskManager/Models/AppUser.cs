using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateOnly  DOB { get; set; }
    }
}
