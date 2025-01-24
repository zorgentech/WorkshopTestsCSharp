using FakeStore.Model.Domain;
using FakeStore.Model.Enums;
using FakeStore.Repositories;
using FakeStore.Services;
using FakeStoreXunitTests.Utils;
using FluentAssertions;
using Moq;

namespace FakeStoreXunitTests.Tests.FakeStore.Services;

public class OrdersServiceTests
{
    public Mock<IOrdersRepository> ordersRepositoryMock;
    public OrdersService ordersService;
    public Fakers fakers = new();
    public static readonly IEnumerable<object[]> TestCases = TestDataProvider.GetTestData<TestData>(
        "TestData/TestData.json"
    );

    public class TestData
    {
        public int CancelationLimit { get; set; }
        public int OffSet { get; set; }
        public bool Expected { get; set; }
    }

    public OrdersServiceTests()
    {
        ordersRepositoryMock = new Mock<IOrdersRepository>();
        ordersService = new OrdersService(ordersRepositoryMock.Object);
    }

    [Theory]
    [InlineData(60, 0, true)]
    [InlineData(60, 1, true)]
    [InlineData(60, 10, true)]
    [InlineData(60, -1, false)]
    [InlineData(60, -10, false)]
    public void IsOrderExpiredInlineTestCase(
        int orderCancelationLimitInMinutes,
        int minutesOffset,
        bool expectedResult
    )
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = orderCancelationLimitInMinutes;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + minutesOffset
        );

        // Act
        var result = ordersService.IsOrderExpired(order);

        // Assert
        result
            .Should()
            .Be(
                expectedResult,
                $"order created at {order.CreatedAt} with offset {minutesOffset} minutes should be {(expectedResult ? "expired" : "not expired")} "
            );
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void IsOrderExpiredMemberDataTestCase(TestData testData)
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = testData.CancelationLimit;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + testData.OffSet
        );

        // Act
        var result = ordersService.IsOrderExpired(order);

        // Assert
        result
            .Should()
            .Be(
                testData.Expected,
                $"order created at {order.CreatedAt} with offset {testData.OffSet} minutes should be {(testData.Expected ? "expired" : "not expired")} "
            );
    }

    [Fact]
    public void IsOrderExpired_WhenOrderIsExpired_ShouldReturnTrue()
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
    public void IsOrderExpired_WhenOrderIsNotExpired_ShouldReturnFalse()
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
    public void IsOrderExpired_WhenOrderIsAtLimit_ShouldReturnTrue()
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
    public async Task CancelOrder_WhenOrderIsExpired_ShouldReturnErrorMessage()
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes + 1
        );

        // Act
        var result = await ordersService.CancelOrderAsync(order);

        // Assert
        result
            .Should()
            .Be("Order is too old to be cancelled", "an error message should be returned");
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsNotExpired_ShouldUpdateOrderStatusToCancelled_AndSaveToRepository()
    {
        // Arrange
        var order = fakers.order.Generate();
        order.Store.OrderCancelationLimitInMinutes = 60;
        order.CreatedAt = DateTime.UtcNow.AddMinutes(
            order.Store.OrderCancelationLimitInMinutes - 1
        );

        // Act
        var result = await ordersService.CancelOrderAsync(order);

        // Assert
        ordersRepositoryMock.Verify(
            x =>
                x.UpdateOrderAsync(
                    It.Is<Order>(o => o == order && o.Status == OrderStatus.Cancelled)
                ),
            Times.Once,
            "repository updated should be called with same order with status cancelled"
        );
        result.Should().BeNull("no error message should be returned");
        order.Status.Should().Be(OrderStatus.Cancelled, "order status should be changed");
    }
}
