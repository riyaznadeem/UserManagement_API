using Azure.Core;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;

namespace Infrastructure.Seeding
{
    public class Seeder
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _passwordHasher;

        public Seeder(ApplicationDbContext context, PasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync()
        {
            #region Roles
            if (!_context.Roles.Any())
            {
                var roles = new List<Role>
            {
                new Role { Id = Guid.Parse("9B481398-E2CF-4263-9D43-5AC45FE76CE2"), Name = "Admin", Description = "System administrator" ,IsActive = true, IsDeleted = false},
                new Role { Id = Guid.Parse("57C8A23E-93BD-4C52-9C1E-B529A7835B4C"), Name = "User", Description = "Regular user" ,IsActive = true, IsDeleted = false },
                new Role { Id = Guid.Parse("1EABBA0E-A41A-4D8F-8813-EFF86FDAC744"), Name = "ReadOnlyUser", Description = "User Can Only View"  ,IsActive = true, IsDeleted = false}
            };
                _context.Roles.AddRange(roles);
            }
           
            #endregion

            #region Users
            _passwordHasher.CreateHash("#itadmin", out var hash, out var salt);
            if (!_context.Users.Any())
            {
                var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "admin", DisplayName = "ADMIN" ,PasswordHash = hash,PasswordSalt = salt ,RoleId = Guid.Parse("9B481398-E2CF-4263-9D43-5AC45FE76CE2"),CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false},
                new User { Id = Guid.NewGuid(), Username = "user", DisplayName = "User" ,PasswordHash = hash,PasswordSalt = salt ,RoleId = Guid.Parse("57C8A23E-93BD-4C52-9C1E-B529A7835B4C"),CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false},
                new User { Id = Guid.NewGuid(), Username = "read", DisplayName = "Read Only User" ,PasswordHash = hash,PasswordSalt = salt ,RoleId = Guid.Parse("1EABBA0E-A41A-4D8F-8813-EFF86FDAC744"),CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false},
            };

            _context.Users.AddRange(users);
            }
            #endregion

            await _context.SaveChangesAsync();
        }
    }

}
