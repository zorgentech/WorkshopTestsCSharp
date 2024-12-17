using AutoBogus;
using FakeStore.Model.Domain;
using FakeStore.Repositories;
using FakeStore.Services;
using FluentAssertions;
using Moq;

namespace FakeStoreXunitTests.Tests;

public class OrdersServiceTests
{
    public Mock<OrdersService> ordersService;

    public OrdersServiceTests()
    {
        var _ordersRespositoryMock = new Mock<IOrdersRepository>();
        ordersService = new Mock<OrdersService>(_ordersRespositoryMock.Object);
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
}
