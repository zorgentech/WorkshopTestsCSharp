using FakeStore.Model.Domain;
using FakeStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace FakeStore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersControllerV2(IOrdersService service) : ControllerBase
{
    public async Task<IActionResult> CancelOrder(Guid orderId)
    {
        if (await service.GetOrderByIdAsync(orderId, includeStore: true) is not Order order)
            return NotFound();

        if (await service.CancelOrderAsync(order) is string error)
            return BadRequest(new { error });

        return Ok();
    }
}
