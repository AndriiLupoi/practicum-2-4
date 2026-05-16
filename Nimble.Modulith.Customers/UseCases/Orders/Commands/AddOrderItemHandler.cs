using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Domain.Interfaces;
using Nimble.Modulith.Customers.Domain.OrderAggregate;
using Nimble.Modulith.Products.Contracts;

namespace Nimble.Modulith.Customers.UseCases.Orders.Commands;

public class AddOrderItemHandler(IRepository<Order> repository, IMediator mediator)
    : ICommandHandler<AddOrderItemCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(AddOrderItemCommand command, CancellationToken ct)
    {
        var spec = new OrderByIdSpec(command.OrderId);
        var order = await repository.FirstOrDefaultAsync(spec, ct);
        if (order is null) return Result<OrderDto>.NotFound();

        ProductDetailsResult productDetails;
        try { productDetails = await mediator.Send(new GetProductDetailsQuery(command.ProductId), ct); }
        catch (InvalidOperationException ex) { return Result<OrderDto>.Error(ex.Message); }

        try { order.AddItem(new OrderItem { ProductId = command.ProductId, ProductName = productDetails.Name, Quantity = command.Quantity, UnitPrice = productDetails.Price }); }
        catch (InvalidOperationException ex) { return Result<OrderDto>.Error(ex.Message); }

        order.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(order, ct);
        await repository.SaveChangesAsync(ct);

        return Result<OrderDto>.Success(new OrderDto(
            order.Id, order.CustomerId, order.OrderNumber, order.OrderDate,
            order.Status.ToString(), order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList(),
            order.CreatedAt, order.UpdatedAt));
    }
}