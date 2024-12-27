using FakeStore.Model.Domain;
using FakeStore.Model.Enums;
using FakeStore.Services;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Fixtures;
using FakeStoreXunitTests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FakeStoreXunitTests.Tests;

public class OrdersServiceTestsNoMock(MySetUpFixture mySetUpFixture) : TestBase(mySetUpFixture)
{
    public IOrdersService ordersService => Scope.GetService<IOrdersService>();
    public Fakers fakers = new();

    [Fact]
    public void IsOrderExpired_ShouldReturnTrue_WhenOrderIsExpired()
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );

        // Act
        var result = ordersService.IsOrderExpired(order);

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
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );

        // Act
        var result = ordersService.IsOrderExpired(order);

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
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(order.Store.OrderCancelationLimitInMinutes);

        // Act
        var result = ordersService.IsOrderExpired(order);

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
        var order = fakers.order.Generate();
        order.Status = OrderStatus.Pending;
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );
        await DbContext.Orders.AddAsync(order);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await ordersService.CancelOrderAsync(order);

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
        var order = fakers.order.Generate();
        order.Status = OrderStatus.Pending;
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );
        await DbContext.Orders.AddAsync(order);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await ordersService.CancelOrderAsync(order);

        // Assert
        result.Should().BeNull("no error message should be returned");
        order.Status.Should().Be(OrderStatus.Cancelled, "order status should be changed");
    }

    [Theory]
    [InlineData(10, 11, 11, 0)]
    [InlineData(11, 10, 11, 1)]
    [InlineData(11, 11, 10, 2)]
    public async Task GetNextAttendantForOrderDistributionAsync_ShouldReturnAttendantWithFewerOrders(
        int attendant1OrdersQuantity,
        int attendant2OrdersQuantity,
        int attendant3OrdersQuantity,
        int expected
    )
    {
        // Arrange
        var attendant1 = fakers.attendant.Generate();
        var attendant2 = fakers.attendant.Generate();
        var attendant3 = fakers.attendant.Generate();
        var attendants = new List<Attendant> { attendant1, attendant2, attendant3 };
        await DbContext.AddRangeAsync(attendants);
        await DbContext.SaveChangesAsync();
        await CreateOrdersForAttendantAndStore(attendant1, attendant1OrdersQuantity);
        await CreateOrdersForAttendantAndStore(attendant2, attendant2OrdersQuantity);
        await CreateOrdersForAttendantAndStore(attendant3, attendant3OrdersQuantity);
        var attendantService = Scope.GetService<IAttendantService>();

        // Act
        var result = await attendantService.GetNextAttendantIdForOrderDistributionAsync();

        // Assert
        result
            .Should()
            .Be(attendants[expected], "the attendant with the least orders should be returned");
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetNextAttendantForOrderDistributionAsync_ShouldReturnAttendantWithFewerOrders2(
        int ordersQuantity
    )
    {
        // Arrange
        var attendant1 = fakers.attendant.Generate();
        var attendant2 = fakers.attendant.Generate();
        var attendant3 = fakers.attendant.Generate();
        await DbContext.AddRangeAsync(attendant1, attendant2, attendant3);
        await DbContext.SaveChangesAsync();
        var attendantService = Scope.GetService<IAttendantService>();

        for (int i = 0; i < ordersQuantity; i++)
        {
            // Act
            var order = fakers.order.Generate();
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
        ordersCount.Should().Be(ordersQuantity, "orders count should be 100");
    }

    private async Task CreateOrdersForAttendantAndStore(Attendant attendant, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            var order = fakers.order.Generate();
            order.Attendant = attendant;
            await DbContext.Orders.AddAsync(order);
            await DbContext.SaveChangesAsync();
        }
    }
}
