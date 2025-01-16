using FakeStore.Model.Domain;
using FakeStore.Model.Enums;
using FakeStore.Repositories;
using FakeStore.Services;
using FakeStoreNunitTests.Utils;
using FluentAssertions;
using Moq;

namespace FakeStoreNunitTests.Tests.FakeStore.Services;

public class OrdersServiceTests
{
    public Mock<IOrdersRepository> ordersRepositoryMock;
    public Mock<OrdersService> ordersService;

    public Fakers fakers = new();

    public OrdersServiceTests()
    {
        ordersRepositoryMock = new Mock<IOrdersRepository>();
        ordersService = new Mock<OrdersService>(ordersRepositoryMock.Object);
    }

    [Test]
    [TestCase(60, 0, true)]
    [TestCase(60, 1, true)]
    [TestCase(60, 10, true)]
    [TestCase(60, -1, false)]
    [TestCase(60, -10, false)]
    public void IsOrderExpired(
        int OrderCancelationLimitInMinutes,
        int minutesOffset,
        bool expectedResult
    )
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = OrderCancelationLimitInMinutes;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + minutesOffset
        );

        // Act
        var result = ordersService.Object.IsOrderExpired(order);

        // Assert
        result
            .Should()
            .Be(
                expectedResult,
                $"order created at {order.CreatedAt} with offset {minutesOffset} minutes should be {(expectedResult ? "expired" : "not expired")} "
            );
    }

    [Test]
    public void IsOrderExpired_ShouldReturnTrue_WhenOrderIsExpired()
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );

        // Act
        var result = ordersService.Object.IsOrderExpired(order);

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
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );

        // Act
        var result = ordersService.Object.IsOrderExpired((Order)order);

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
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(order.Store.OrderCancelationLimitInMinutes);

        // Act
        var result = ordersService.Object.IsOrderExpired((Order)order);

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
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );

        // Act
        var result = await ordersService.Object.CancelOrderAsync((Order)order);

        // Assert
        result
            .Should()
            .Be("Order is too old to be cancelled", "an error message should be returned");
    }

    [Test]
    public async Task CancelOrder_GivenNotExpiredOrder_ShouldReturnNull_And_UpdateOrderStatusToCanceled()
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );
        ordersRepositoryMock
            .Setup(x => x.UpdateOrderAsync(It.Is<Order>(x => x.Status == OrderStatus.Cancelled)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ordersService.Object.CancelOrderAsync((Order)order);

        // Assert
        result.Should().BeNull("no error message should be returned");
    }
}
