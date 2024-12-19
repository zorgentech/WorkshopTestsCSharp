using FakeStore.Data;
using FakeStore.Model.Domain;
using Microsoft.EntityFrameworkCore;

namespace FakeStore.Services;

public class AttendantService(AppDbContext dbContext) : IAttendantService
{
    public Task<Attendant> GetAttendantForOrderAsync()
    {
        // Annotate Orders quantity for each attendant
        var queryset = dbContext.Attendants
            .Include(x => x.Orders)
        throw new NotImplementedException();
        
    }
}
