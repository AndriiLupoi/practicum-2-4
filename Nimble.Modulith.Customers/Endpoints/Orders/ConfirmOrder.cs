using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.Contracts;
using Nimble.Modulith.Customers.Infrastructure;
using Nimble.Modulith.Customers.UseCases.Customers.Queries;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;
using Nimble.Modulith.Customers.Endpoints.Orders;
using Nimble.Modulith.Email.Contracts;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class ConfirmOrder(IMediator mediator, ICustomerAuthorizationService authService)
    : EndpointWithoutRequest<OrderResponse>
{
    public override void Configure()
    {
        Post("/orders/{id}/confirm");
        Tags("orders");
        Summary(s => { s.Summary = "Confirm an order"; s.Description = "Changes order status to Processing and sends confirmation email"; });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<int>("id");

        // Get order first to find customer
        var orderResult = await mediator.Send(new GetOrderByIdQuery(orderId), ct);
        if (!orderResult.IsSuccess) { await Send.NotFoundAsync(ct); return; }

        var customerResult = await mediator.Send(new GetCustomerByIdQuery(orderResult.Value.CustomerId), ct);
        if (!customerResult.IsSuccess) { AddError("Customer not found"); await Send.ErrorsAsync(statusCode: 404, cancellation: ct); return; }

        if (!authService.IsAdminOrOwner(User, customerResult.Value.Email))
        {
            AddError("You can only confirm your own orders");
            await Send.ErrorsAsync(statusCode: 403, cancellation: ct);
            return;
        }

        var result = await mediator.Send(new ConfirmOrderCommand(orderId), ct);
        if (!result.IsSuccess) { AddError("Failed to confirm order"); await Send.ErrorsAsync(cancellation: ct); return; }

        // Publish event
        await mediator.Publish(new OrderCreatedEvent(
            result.Value.Id, result.Value.CustomerId, customerResult.Value.Email,
            result.Value.OrderNumber, result.Value.OrderDate, result.Value.TotalAmount,
            result.Value.Items.Select(i => new OrderItemDetails(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()), ct);

        // Send confirmation email
        var emailBody = $"Your order {result.Value.OrderNumber} has been confirmed!\nTotal: ${result.Value.TotalAmount:F2}";
        await mediator.Send(new SendEmailCommand(customerResult.Value.Email, $"Order Confirmation - {result.Value.OrderNumber}", emailBody), ct);

        Response = new OrderResponse(result.Value.Id, result.Value.CustomerId, result.Value.OrderNumber,
            result.Value.OrderDate, result.Value.Status, result.Value.TotalAmount,
            result.Value.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
    }
}