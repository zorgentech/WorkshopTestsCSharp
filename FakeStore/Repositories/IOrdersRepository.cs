using FakeStore.Model.Domain;

namespace FakeStore.Repositories;

public interface IOrdersRepository
{
    Task<Order?> GetOrderByIdAsync(Guid orderId, bool includeStore = false);
    Task UpdateOrderAsync(Order order);
}
