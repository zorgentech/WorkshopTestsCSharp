using FakeStore.Model.Domain;

namespace FakeStore.Services;

public interface IOrdersService
{
    bool IsOrderExpired(Order order);

    Task<Order?> GetOrderByIdAsync(Guid orderId, bool includeStore);

    Task UpdateOrderAsync(Order order);

    Task CancelOrderAsync(Order order);
}
