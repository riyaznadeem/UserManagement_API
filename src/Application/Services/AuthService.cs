using Application.Common.Interface;
using Application.DTOs.Requests;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtTokenService _jwt;
        private readonly ICurrentUserService _currentUserService;

        public AuthService(IApplicationDbContext context, PasswordHasher passwordHasher, JwtTokenService jwt, ICurrentUserService currentUserService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwt = jwt;
            _currentUserService = currentUserService;
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
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

                return "User registered";
            }
            catch (Exception ex)
            {
                Log.Error(ex,$"An error occurred while creating user '{request.Username}' Ip {_currentUserService.IPAddress}");
                throw;
            }
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.Include(r => r.Role)
                .FirstOrDefaultAsync(x => x.Username == request.Username);

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new Exception("Invalid credentials");

            return _jwt.Generate(user);
        }
    }

}
