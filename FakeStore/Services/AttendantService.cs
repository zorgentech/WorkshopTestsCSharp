using FakeStore.Data;
using FakeStore.Model.Domain;
using Microsoft.EntityFrameworkCore;

namespace FakeStore.Services;

public class AttendantService(AppDbContext dbContext) : IAttendantService
{
    public async Task<Attendant?> GetNextAttendantIdForOrderDistributionAsync()
    {
        var result = await dbContext
            .Attendants.Include(x => x.Orders!)
            .ThenInclude(o => o.Store)
            .Select(a => new
            {
                a,
                LastOrderCreatedAt = a.Orders!.OrderBy(o => o.CreatedAt).LastOrDefault(),
            })
            .OrderBy(r =>
                r.LastOrderCreatedAt == null ? DateTime.MinValue : r.LastOrderCreatedAt.CreatedAt
            )
            .ToListAsync();
        if (result != null)
        {
            return result.First().a;
        }
        return null;
    }
}
