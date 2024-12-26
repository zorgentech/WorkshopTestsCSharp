using FakeStore.Model.Domain;

namespace FakeStore.Services;

public interface IAttendantService
{
    Task<Attendant?> GetNextAttendantIdForOrderDistributionAsync();
}
