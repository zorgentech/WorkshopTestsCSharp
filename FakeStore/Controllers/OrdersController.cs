using FakeStore.Data;
using FakeStore.Model.Domain;
using FakeStore.Model.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FakeStore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(AppDbContext dbContext) : ControllerBase
{
    public async Task<IActionResult> CancelOrder(Guid orderId)
    {
        if (await dbContext.Orders.FindAsync(orderId) is not Order order)
            return NotFound();

        var now = DateTime.Now;

        if (now.Subtract(order.CreatedAt).TotalMinutes < order.Store.OrderCancelationLimitInMinutes)
        {
            order.Status = OrderStatus.Cancelled;
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest(new { error = "Order is too new to be cancelled" });
    }
}
