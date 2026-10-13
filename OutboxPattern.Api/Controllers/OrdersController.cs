using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutboxPattern.Application.UseCases;

namespace OutboxPattern.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderUseCase _useCase;
    public OrdersController(PlaceOrderUseCase useCase) => _useCase = useCase;

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var orderId = await _useCase.ExecuteAsync(request.ProductId, request.Quantity);
        return Ok(new { orderId });
    }
}

public record PlaceOrderRequest(
    [Required, StringLength(100, MinimumLength = 1)] string ProductId,
    [Range(1, 10_000)] int Quantity);
