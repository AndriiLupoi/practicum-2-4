using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Domain.Interfaces;
using Nimble.Modulith.Customers.Domain.OrderAggregate;

namespace Nimble.Modulith.Customers.UseCases.Orders.Commands;

public class GetOrderByIdHandler(IReadRepository<Order> repository)
    : IQueryHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var spec = new OrderByIdSpec(query.Id);
        var order = await repository.FirstOrDefaultAsync(spec, ct);
        if (order is null) return Result<OrderDto>.NotFound();

        return Result<OrderDto>.Success(new OrderDto(
            order.Id, order.CustomerId, order.OrderNumber, order.OrderDate,
            order.Status.ToString(), order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList(),
            order.CreatedAt, order.UpdatedAt));
    }
}