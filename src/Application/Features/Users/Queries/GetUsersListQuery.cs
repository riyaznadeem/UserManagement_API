using Application.Common.Extensions;
using Application.Common.Interface;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries
{
    public class GetUsersListQuery : Pagination, IRequest<PaginatedList<GetUserListViewModel>>
    {
        public string? userNameOrDisplayName {  get; set; }
        public Guid? Role { get; set; }
    }

    public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, PaginatedList<GetUserListViewModel>>
    {
        private readonly IApplicationDbContext _context;

        public GetUsersListQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<GetUserListViewModel>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            // ✅ Apply optional filters
            if (!string.IsNullOrWhiteSpace(request.userNameOrDisplayName))
            {
                var filter = request.userNameOrDisplayName.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(filter) ||
                    u.DisplayName.ToLower().Contains(filter));
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.RoleId == request.Role.Value);
            }
            query = query.OrderBy(u => u.Username);

            var pagedResult = await query
                .Select(u => new GetUserListViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    RoleName = u.Role != null ? u.Role.Name : ""
                })
                .ToPaginatedListAsync(request.PageNumber,request.PageSize,cancellationToken);

            return pagedResult;
        }
    }
}
