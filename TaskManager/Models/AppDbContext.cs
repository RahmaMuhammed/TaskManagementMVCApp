using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TaskManager.ViewModels;

namespace TaskManager.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Call the base OnModelCreating method to ensure that all Identity-related tables 
            // (like AspNetUsers, AspNetRoles, etc.) are configured correctly before applying custom configurations.
            base.OnModelCreating(builder);

            builder.Entity<TaskItem>()
           .HasOne(t => t.User)
           .WithMany()
           .HasForeignKey(t => t.UserId)
           .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>()
           .Property(t => t.status)
           .HasConversion<string>();


            //  builder.Entity<TaskItem>()
            //.HasOne(t => t.Admin)
            //.WithMany()
            //.HasForeignKey(t => t.CreatedById)
            //.OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskManager.ViewModels.TaskViewModel> TaskViewModel { get; set; } = default!;
        public DbSet<TaskManager.ViewModels.UserViewModel> UserViewModel { get; set; } = default!;


    }
}
