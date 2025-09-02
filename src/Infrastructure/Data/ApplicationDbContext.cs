using Application.Common.Interface;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext , IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)                  // Each User has one Role
                .WithMany(r => r.Users)               // Each Role has many Users
                .HasForeignKey(u => u.RoleId)         // Foreign key in User
                .OnDelete(DeleteBehavior.Restrict);   // Optional: prevent cascade delete
        }
    }
}
