using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests
{
    public class UserDto
    {
        public Guid Id { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public string RoleName { get; }

        public UserDto(User user)
        {
            Id = user.Id;
            Username = user.Username;
            DisplayName = user.DisplayName;
            RoleName = user.Role?.Name ?? "";
        }
    }

}
