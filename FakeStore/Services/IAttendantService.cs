namespace FakeStore.Services;

public interface IAttendantService
{
    Task<Guid?> GetNextAttendantIdForOrderDistributionAsync();
}
