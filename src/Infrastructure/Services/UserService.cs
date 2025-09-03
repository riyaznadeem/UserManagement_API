using Application.Common;
using Application.Common.Interface;
using Application.DTOs.Requests;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly PasswordHasher _passwordHasher;
        public UserService(IApplicationDbContext context, ILogger<UserService> logger, ICurrentUserService currentUserService, PasswordHasher passwordHasher)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto> CreateUserAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new Exception("Username already exists");

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
            // Password hashing logic here (omitted for brevity)

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId}", user.Id);

            return new UserDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, bool isAdmin)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            user.DisplayName = request.DisplayName;

            if (isAdmin && request.RoleId.HasValue)
            {
                _logger.LogInformation("Role changed for User {UserId} from {OldRole} to {NewRole}",
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
            user.UpdatedDate  = DateTime.UtcNow;
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

            _logger.LogError("User {UserId} logged in", id);
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
