using FakeStore.Data;
using FakeStore.Model.Domain;
using Microsoft.EntityFrameworkCore;

namespace FakeStore.Services;

public class AttendantService(AppDbContext dbContext) : IAttendantService
{
    public async Task<Attendant?> GetNextAttendantIdForOrderDistributionAsync()
    {
        var result = await dbContext
            .Attendants.Include(x => x.Orders)
            .Select(a => new { a, OrdersCount = a.Orders!.Count })
            .OrderBy(r => r.OrdersCount)
            .FirstAsync();
        if (result != null)
        {
            return result.a;
        }
        return null;
    }
}
