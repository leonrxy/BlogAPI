using Microsoft.EntityFrameworkCore;
using BlogAPI.Models.DTO;

namespace BlogAPI.Helper;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public static class PaginationHelper
{
    public static async Task<PaginatedResult<T>> PaginateAsync<T>(IQueryable<T> query, PaginationParams param)
    {
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((param.Page - 1) * param.Limit)
            .Take(param.Limit)
            .ToListAsync();

        return new PaginatedResult<T>
        {
            Items = items,
            Page = param.Page,
            Limit = param.Limit,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)param.Limit)
        };
    }


    internal static async Task PaginateAsync<T>(List<T> items, int page, int limit)
    {
        throw new NotImplementedException();
    }
}
