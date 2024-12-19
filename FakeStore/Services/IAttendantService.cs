using FakeStore.Model.Domain;
namespace FakeStore;

public interface IAttendantService
{
    Task<Attendant> GetAttendantForOrderAsync();
}
