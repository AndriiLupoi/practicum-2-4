using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class AddOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class AddOrderItem(IMediator mediator) : Endpoint<AddOrderItemRequest, OrderResponse>
{
    public override void Configure()
    {
        Post("/orders/{id}/items");
        AllowAnonymous();
        Tags("orders");
        Summary(s => { s.Summary = "Add item to order"; });
    }

    public override async Task HandleAsync(AddOrderItemRequest req, CancellationToken ct)
    {
        var orderId = Route<int>("id");
        var result = await mediator.Send(new AddOrderItemCommand(orderId, req.ProductId, req.Quantity), ct);
        if (!result.IsSuccess) { AddError(result.Errors.FirstOrDefault() ?? "Failed"); await Send.ErrorsAsync(cancellation: ct); return; }

        Response = new OrderResponse(result.Value.Id, result.Value.CustomerId, result.Value.OrderNumber,
            result.Value.OrderDate, result.Value.Status, result.Value.TotalAmount,
            result.Value.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
    }
}