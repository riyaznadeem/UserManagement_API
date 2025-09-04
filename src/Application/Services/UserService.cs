using Application.Common;
using Application.Common.Interface;
using Application.DTOs.Requests;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly PasswordHasher _passwordHasher;
        public UserService(IApplicationDbContext context, ICurrentUserService currentUserService, PasswordHasher passwordHasher)
        {
            _context = context;
            _currentUserService = currentUserService;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto> CreateUserAsync(RegisterRequest request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == request.Username && !u.IsDeleted))
                {
                    Log.Error("Username already exists");
                    throw new Exception("Username already exists");
                }

                _passwordHasher.CreateHash(request.Password, out var hash, out var salt);
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    DisplayName = request.DisplayName,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    RoleId = request.RoleId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = _currentUserService.UserId,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                Log.Information($"User {user.Username}");

                return new UserDto(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating user '{Username}'", request.Username);
                throw;
            }

        }

        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, bool isAdmin)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            user.DisplayName = request.DisplayName;
            user.Username = request.UserName;

            if (isAdmin && request.RoleId.HasValue)
            {
                Log.Information("Role changed for User {UserId} from {OldRole} to {NewRole}",
                    userId, user.RoleId, request.RoleId.Value);
                user.RoleId = request.RoleId.Value;
            }

            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            await _context.SaveChangesAsync();

            return new UserDto(user);
        }

        public async Task<UserDto> UpdateOwnProfileAsync(Guid userId, UpdateOwnProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            user.DisplayName = request.DisplayName;
            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = userId.ToString();

            await _context.SaveChangesAsync();

            return new UserDto(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(f => f.Id == id);
            if (user == null) throw new NotFoundException("User not found");

            user.IsDeleted = true;
            user.IsActive = false;

            await _context.SaveChangesAsync();

            Log.Error("User {UserId} logged in", id);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new NotFoundException("User not found");
            return new UserDto(user);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return users.Select(u => new UserDto(u)).ToList();
        }
    }

}
