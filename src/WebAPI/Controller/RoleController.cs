using Application.Common.Interface;
using Application.Features.Roles.Queries.GetRoleLookup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize()]
    public class RoleController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RoleController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetRoleLookup")]
        [Authorize]
        public async Task<IActionResult> GetRoleLookup()
        {
            var result = await _mediator.Send(new GetRoleLookupQuery());
            return Ok(result);
        }
    }
}
