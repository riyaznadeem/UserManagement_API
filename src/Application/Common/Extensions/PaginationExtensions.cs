using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Extensions
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
    public static class PaginationExtensions
    {
        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(cancellationToken);

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}
