using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Seeding
{
    public class RoleSeeder
    {
        private readonly ApplicationDbContext _context;

        public RoleSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.Roles.Any())
            {
                var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System administrator" ,IsActive = true, IsDeleted = false},
                new Role { Id = Guid.NewGuid(), Name = "User", Description = "Regular user" ,IsActive = true, IsDeleted = false },
                new Role { Id = Guid.NewGuid(), Name = "ReadOnlyUser", Description = "User Can Only View"  ,IsActive = true, IsDeleted = false}
            };

                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();
            }
        }
    }

}
