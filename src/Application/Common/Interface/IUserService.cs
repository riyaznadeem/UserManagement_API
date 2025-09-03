using Application.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interface
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(RegisterRequest request);
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, bool isAdmin);
        Task<UserDto> UpdateOwnProfileAsync(Guid userId, UpdateOwnProfileRequest request);
        Task DeleteUserAsync(Guid id);
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<List<UserDto>> GetAllUsersAsync();
    }
}
