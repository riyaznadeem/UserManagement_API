using Application.Common.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Queries.GetRoleLookup;

public class GetRoleLookupQuery : IRequest<List<GetRoleLookupViewModel>>;
public class GetRoleLookupHandler : IRequestHandler<GetRoleLookupQuery, List<GetRoleLookupViewModel>>
{
    private readonly IApplicationDbContext _context;
    public GetRoleLookupHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<GetRoleLookupViewModel>> Handle(GetRoleLookupQuery request,CancellationToken cancellationToken)
    {
        var query = _context.Roles.AsNoTracking().Where(w => w.IsActive).AsQueryable();

        return await query.Select(x => new GetRoleLookupViewModel{
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
        }).ToListAsync(cancellationToken);
    }
}
