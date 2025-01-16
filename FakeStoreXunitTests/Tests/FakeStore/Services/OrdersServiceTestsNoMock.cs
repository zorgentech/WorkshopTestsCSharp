using FakeStore.Model.Enums;
using FakeStore.Services;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FakeStoreXunitTests.Tests.FakeStore.Services;

public class OrdersServiceTestsNoMock : TestBase
{
    public IOrdersService OrdersService => Scope.GetService<IOrdersService>();

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Theory]
    [InlineData(4)]
    [InlineData(10)]
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
            var attendantOrdersCountQuery = DbContext
                .Attendants.Include(x => x.Orders)
                .Select(x => new { OrdersCount = x.Orders!.Count })
                .OrderBy(x => x.OrdersCount);
            var attendantWithLeastOrders = await attendantOrdersCountQuery.FirstAsync();
            var attendantWithMoreOrders = await attendantOrdersCountQuery.LastAsync();
            var ordersCountDifference =
                attendantWithMoreOrders.OrdersCount - attendantWithLeastOrders.OrdersCount;
            ordersCountDifference
                .Should()
                .BeLessOrEqualTo(1, "orders count difference should be less than or equal 1");
        }

        var ordersCount = await DbContext.Orders.CountAsync();
        ordersCount.Should().Be(ordersQuantity, $"orders count should be {ordersQuantity}");
    }
}
