using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Domain.CustomerAggregate;
using Nimble.Modulith.Customers.Domain.Interfaces;
using Nimble.Modulith.Customers.Domain.OrderAggregate;
using Nimble.Modulith.Products.Contracts;

namespace Nimble.Modulith.Customers.UseCases.Orders.Commands;

public class CreateOrderHandler(
    IRepository<Order> orderRepository,
    IReadRepository<Customer> customerRepository,
    IMediator mediator)
    : ICommandHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
            return Result<OrderDto>.NotFound($"Customer {command.CustomerId} not found");

        var order = new Order
        {
            CustomerId = command.CustomerId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            OrderDate = command.OrderDate,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in command.Items)
        {
            ProductDetailsResult productDetails;
            try
            {
                productDetails = await mediator.Send(new GetProductDetailsQuery(itemDto.ProductId), ct);
            }
            catch (InvalidOperationException ex)
            {
                return Result<OrderDto>.Error($"Failed to get product {itemDto.ProductId}: {ex.Message}");
            }

            order.AddItem(new OrderItem
            {
                ProductId = itemDto.ProductId,
                ProductName = productDetails.Name,
                Quantity = itemDto.Quantity,
                UnitPrice = productDetails.Price
            });
        }

        await orderRepository.AddAsync(order, ct);
        await orderRepository.SaveChangesAsync(ct);

        return Result<OrderDto>.Success(new OrderDto(
            order.Id, order.CustomerId, order.OrderNumber, order.OrderDate,
            order.Status.ToString(), order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList(),
            order.CreatedAt, order.UpdatedAt));
    }
}