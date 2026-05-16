using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public DateOnly OrderDate { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public record CreateOrderItemRequest(int ProductId, int Quantity);

public class CreateOrder(IMediator mediator) : Endpoint<CreateOrderRequest, OrderResponse>
{
    public override void Configure()
    {
        Post("/orders");
        AllowAnonymous();
        Tags("orders");
        Summary(s => { s.Summary = "Create a new order"; });
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            req.CustomerId,
            req.OrderDate,
            req.Items.Select(i => new CreateOrderItemDto(i.ProductId, i.Quantity)).ToList());

        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess) { AddError(result.Errors.FirstOrDefault() ?? "Failed"); await Send.ErrorsAsync(cancellation: ct); return; }

        Response = new OrderResponse(result.Value.Id, result.Value.CustomerId, result.Value.OrderNumber,
            result.Value.OrderDate, result.Value.Status, result.Value.TotalAmount,
            result.Value.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
        await Send.CreatedAtAsync("/orders/" + result.Value.Id, Response, cancellation: ct);
    }
}