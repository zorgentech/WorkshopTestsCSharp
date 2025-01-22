using System;
using FakeStore.Controllers;
using FakeStore.Model.Domain;
using FakeStore.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FakeStoreXunitTests.Tests.FakeStore.Controllers;

public class OrdersControllerV2Tests
{
    [Fact]
    public async Task CancelOrderAsync_WhenOrderDoesNotExit_ShouldReturnNotFound()
    {
        // Given
        var ordersServiceMock = new Mock<IOrdersService>();
        var ordersController = new OrdersControllerV2(ordersServiceMock.Object);
        var testId = Guid.NewGuid();

        // setup
        ordersServiceMock
            .Setup(s => s.GetOrderByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((Order)null!);

        // When
        var result = await ordersController.CancelOrderAsync(testId);

        // Then
        result.Should().BeOfType<NotFoundResult>().Which.StatusCode.Should().Be(404);
        ordersServiceMock.Verify(
            s => s.GetOrderByIdAsync(It.Is<Guid>(v => v == testId), It.Is<bool>(v => v == true)),
            Times.Once
        );
    }
}
