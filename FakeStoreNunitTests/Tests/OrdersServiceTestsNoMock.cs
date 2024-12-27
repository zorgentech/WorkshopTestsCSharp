using FakeStore.Model.Domain;
using FakeStore.Model.Enums;
using FakeStore.Services;
using FakeStoreNunitTests.Extensions;
using FakeStoreNunitTests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FakeStoreNunitTests.Tests;

public class OrdersServiceTestsNoMock : TestBase
{
    public IOrdersService OrdersService => Scope.GetService<IOrdersService>();

    [Test]
    public void IsOrderExpired_ShouldReturnTrue_WhenOrderIsExpired()
    {
        // Arrange

        var order = Fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );

        // Act
        var result = OrdersService.IsOrderExpired(order);

        // Assert
        result
            .Should()
            .Be(
                true,
                $"order created at is {order.Store.OrderCancelationLimitInMinutes + 1} minutes older than configured at store"
            );
    }

    [Test]
    public void IsOrderExpired_ShouldReturnFalse_WhenOrderIsNotExpired()
    {
        // Arrange
        var order = Fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );

        // Act
        var result = OrdersService.IsOrderExpired(order);

        // Assert
        result
            .Should()
            .Be(
                false,
                $"order created at is {order.Store.OrderCancelationLimitInMinutes} minutes newer than configured at store"
            );
    }

    [Test]
    public void IsOrderExpired_ShouldReturnTrue_WhenOrderIsAtLimit()
    {
        // Arrange
        var order = Fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(order.Store.OrderCancelationLimitInMinutes);

        // Act
        var result = OrdersService.IsOrderExpired(order);

        // Assert
        result
            .Should()
            .Be(
                true,
                $"order created at is at limit of cancelation time {order.Store.OrderCancelationLimitInMinutes} minutes"
            );
    }

    [Test]
    public async Task CancelOrder_GivenExpiredOrder_ShouldReturnErrorMessage()
    {
        // Arrange
        var order = Fakers.order.Generate();
        order.Status = OrderStatus.Pending;
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );
        await DbContext.Orders.AddAsync(order);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await OrdersService.CancelOrderAsync(order);

        // Assert
        result
            .Should()
            .Be("Order is too old to be cancelled", "an error message should be returned");
        order.Status.Should().Be(OrderStatus.Pending, "order status should not be changed");
    }

    [Test]
    public async Task CancelOrder_GivenNotExpiredOrder_ShouldReturnNull_And_UpdateOrderStatusToCanceled()
    {
        // Arrange
        var order = Fakers.order.Generate();
        order.Status = OrderStatus.Pending;
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );
        await DbContext.Orders.AddAsync(order);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await OrdersService.CancelOrderAsync(order);

        // Assert
        result.Should().BeNull("no error message should be returned");
        order.Status.Should().Be(OrderStatus.Cancelled, "order status should be changed");
    }

    [TestCase(50)]
    [TestCase(100)]
    public async Task GetNextAttendantForOrderDistributionAsync_ShouldReturnAttendantWithFewerOrders2(
        int ordersQuantity
    )
    {
        // Arrange
        var attendant1 = Fakers.attendant.Generate();
        var attendant2 = Fakers.attendant.Generate();
        var attendant3 = Fakers.attendant.Generate();
        await DbContext.AddRangeAsync(attendant1, attendant2, attendant3);
        await DbContext.SaveChangesAsync();
        var attendantService = Scope.GetService<IAttendantService>();

        for (int i = 0; i < ordersQuantity; i++)
        {
            // Act
            var order = Fakers.order.Generate();
            order.Attendant = await attendantService.GetNextAttendantIdForOrderDistributionAsync();
            await DbContext.Orders.AddAsync(order);
            await DbContext.SaveChangesAsync();

            // Assert
            var attendantWithLeastOrders = await DbContext
                .Attendants.Include(x => x.Orders)
                .Select(x => new { x.Id, OrdersCount = x.Orders!.Count })
                .OrderBy(x => x.OrdersCount)
                .FirstAsync();
            var attendantWithMoreOrders = await DbContext
                .Attendants.Include(x => x.Orders)
                .Select(x => new { x.Id, OrdersCount = x.Orders!.Count })
                .OrderBy(x => x.OrdersCount)
                .LastAsync();
            var ordersCountDifference =
                attendantWithMoreOrders.OrdersCount - attendantWithLeastOrders.OrdersCount;
            ordersCountDifference
                .Should()
                .BeLessOrEqualTo(1, "orders count difference should be less than or equal 1");
        }

        var ordersCount = await DbContext.Orders.CountAsync();
        ordersCount.Should().Be(ordersQuantity, $"orders count should be {ordersQuantity}");
    }

    private async Task CreateOrdersForAttendantAndStore(Attendant attendant, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            var order = Fakers.order.Generate();
            order.Attendant = attendant;
            await DbContext.Orders.AddAsync(order);
            await DbContext.SaveChangesAsync();
        }
    }
}
