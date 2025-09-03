using Application.Common.Interface;
using Application.DTOs.Requests;
using Application.Features.Users.Queries.GetRoleLookup;
using Application.Features.Users.Queries.GetUserList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly IMediator _mediator;

        public UsersController(IUserService userService, ILogger<UsersController> logger, IMediator mediator)
        {
            _userService = userService;
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost("GetUserList")]
        [Authorize()]
        public async Task<IActionResult> GetUsers(GetUsersListQuery query)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return StatusCode(403, new
            //    {
            //        status = 403,
            //        title = "Forbidden",
            //        message = "Only administrators can access this resource."
            //    });
            //}
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id) => Ok(await _userService.GetUserByIdAsync(id));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] RegisterRequest request)
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPost("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.UpdateUserAsync(id, request, isAdmin: true);
            return Ok(user);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        // User updates own profile (no role change allowed)
        [HttpPost("me")]
        [Authorize]
        public async Task<IActionResult> UpdateOwnProfile([FromBody] UpdateOwnProfileRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.UpdateOwnProfileAsync(userId, request);
            return Ok(user);
        }
       

    }

}
