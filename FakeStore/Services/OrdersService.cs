using FakeStore.Model.Domain;
using FakeStore.Model.Enums;
using FakeStore.Repositories;

namespace FakeStore.Services;

public class OrdersService(IOrdersRepository ordersRepository) : IOrdersService
{
    public async Task<Order?> GetOrderByIdAsync(Guid orderId, bool inludeStore)
    {
        return await ordersRepository.GetOrderByIdAsync(orderId, inludeStore);
    }

    public bool IsOrderExpired(Order order)
    {
        var now = DateTime.UtcNow;

        return Convert.ToInt64(now.Subtract(order.CreatedAt).TotalMinutes)
            <= -order.Store.OrderCancelationLimitInMinutes;
    }

    public async Task UpdateOrderAsync(Order order)
    {
        await ordersRepository.UpdateOrderAsync(order);
    }

    public async Task<string?> CancelOrderAsync(Order order)
    {
        if (IsOrderExpired(order))
            return "Order is too old to be cancelled";
        order.Status = OrderStatus.Cancelled;
        await UpdateOrderAsync(order);
        return null;
    }
}
