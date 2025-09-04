using Application.Features.Dashboard.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize()]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("GetDashboard")]
    [Authorize]
    [ProducesResponseType(typeof(GetDashboardViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _mediator.Send(new GetDashboardQuery());
        return Ok(result);
    }
}
