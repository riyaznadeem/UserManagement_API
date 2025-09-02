using Domain.Entities;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System administrator" },
                new Role { Id = Guid.NewGuid(), Name = "User", Description = "Regular user" },
                new Role { Id = Guid.NewGuid(), Name = "ReadOnlyUser", Description = "User Can Only View" }
            };

                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();
            }
        }
    }

}
