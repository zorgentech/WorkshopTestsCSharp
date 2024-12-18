using AutoBogus;
using FakeStore.Model.Domain;
using FakeStore.Model.Enums;
using FakeStore.Repositories;
using FakeStore.Services;
using FluentAssertions;
using Moq;

namespace FakeStoreXunitTests.Tests;

public class OrdersServiceTests
{
    public Mock<IOrdersRepository> ordersRepositoryMock;
    public Mock<OrdersService> ordersService;

    public OrdersServiceTests()
    {
        ordersRepositoryMock = new Mock<IOrdersRepository>();
        ordersService = new Mock<OrdersService>(ordersRepositoryMock.Object);
    }

    [Fact]
    public void IsOrderExpired_ShouldReturnTrue_WhenOrderIsExpired()
    {
        // Arrange
        var order = AutoFaker.Generate<Order>();
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

    [Fact]
    public void IsOrderExpired_ShouldReturnFalse_WhenOrderIsNotExpired()
    {
        // Arrange
        var order = AutoFaker.Generate<Order>();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );

        // Act
        var result = ordersService.Object.IsOrderExpired(order);

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
        var order = AutoFaker.Generate<Order>();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(order.Store.OrderCancelationLimitInMinutes);

        // Act
        var result = ordersService.Object.IsOrderExpired(order);

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
        var order = AutoFaker.Generate<Order>();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );

        // Act
        var result = await ordersService.Object.CancelOrderAsync(order);

        // Assert
        result
            .Should()
            .Be("Order is too old to be cancelled", "an error message should be returned");
    }

    [Fact]
    public async Task CancelOrder_GivenNotExpiredOrder_ShouldReturnNull_And_UpdateOrderStatusToCanceled()
    {
        // Arrange
        var order = AutoFaker.Generate<Order>();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );
        ordersRepositoryMock
            .Setup(x => x.UpdateOrderAsync(It.Is<Order>(x => x.Status == OrderStatus.Cancelled)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ordersService.Object.CancelOrderAsync(order);

        // Assert
        result.Should().BeNull("no error message should be returned");
    }
}
