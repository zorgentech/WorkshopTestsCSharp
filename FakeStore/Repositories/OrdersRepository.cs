using FakeStore.Data;
using FakeStore.Model.Domain;
using Microsoft.EntityFrameworkCore;

namespace FakeStore.Repositories;

public class OrdersRepository(AppDbContext dbContext) : IOrdersRepository
{
    public async Task<Order?> GetOrderByIdAsync(Guid orderId, bool includeStore)
    {
        var queryable = dbContext.Orders.AsQueryable();

        if (includeStore)
            queryable = queryable.Include(x => x.Store);

        return await queryable.FirstOrDefaultAsync(x => x.Id == orderId);
    }

    public Task UpdateOrderAsync(Order order)
    {
        dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }
}
