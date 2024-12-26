using FakeStore.Data;
using Microsoft.EntityFrameworkCore;

namespace FakeStore.Services;

public class AttendantService(AppDbContext dbContext) : IAttendantService
{
    public async Task<Guid?> GetNextAttendantIdForOrderDistributionAsync()
    {
        var result = await dbContext
            .Attendants.Include(x => x.Orders)
            .Select(a => new { a.Id, OrdersCount = a.Orders!.Count })
            .OrderBy(r => r.OrdersCount)
            .FirstAsync();
        if (result != null)
        {
            return result.Id;
        }
        return null;
    }
}
