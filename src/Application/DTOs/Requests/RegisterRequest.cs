using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(4)]
        public string Username { get; set; }

        [Required]
        [MinLength(4)]
        public string DisplayName { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }
    public class UpdateUserRequest
    {
        [Required]
        public string DisplayName { get; set; }

        public Guid? RoleId { get; set; } // Admin only can update this
    }

    public class UpdateOwnProfileRequest
    {
        [Required]
        public string DisplayName { get; set; }
    }
}
