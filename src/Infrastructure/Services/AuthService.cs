using Application.Common.Interface;
using Application.DTOs.Requests;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtTokenService _jwt;

        public AuthService(ApplicationDbContext context, PasswordHasher passwordHasher, JwtTokenService jwt)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwt = jwt;
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
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
                RoleId = request.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "User registered";
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
