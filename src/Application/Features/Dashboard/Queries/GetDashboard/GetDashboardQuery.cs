using Application.Common.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery : IRequest<GetDashboardViewModel>;
public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, GetDashboardViewModel>
{
    private readonly IApplicationDbContext _context;
    public GetDashboardHandler(IApplicationDbContext context) => _context = context;
    public async Task<GetDashboardViewModel> Handle(GetDashboardQuery request,CancellationToken cancellationToken)
    {
        return new GetDashboardViewModel
        {
            ActiveUsers = await _context.Users.CountAsync(c => c.IsActive, cancellationToken),
            TotalUsers = await _context.Users.CountAsync(cancellationToken),
            DeleteUsers = await _context.Users.CountAsync(c => c.IsDeleted, cancellationToken)
        };
    }
}

