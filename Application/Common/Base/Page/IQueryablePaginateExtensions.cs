using Microsoft.EntityFrameworkCore;

namespace Application.Common.Base.Page
{
    public static class IQueryablePaginateExtensions
    {
        public static async Task<IPaginate<T>> ToPaginateAsync<T>(
            this IQueryable<T> source,
            CancellationToken ct,
            int index = 0,
            int size = 10,
            int from = 0)
        {
            int count = await source.CountAsync(ct);
            var items = await source
                .Skip((index - from) * size)
                .Take(size)
                .ToListAsync(ct);

            return new Paginate<T>(items, count, index, size, from);
        }
    }
}
