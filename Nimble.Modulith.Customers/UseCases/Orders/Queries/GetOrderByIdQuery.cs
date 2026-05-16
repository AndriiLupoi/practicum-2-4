using Ardalis.Result;
using Mediator;

namespace Nimble.Modulith.Customers.UseCases.Orders.Commands;

public record GetOrderByIdQuery(int Id) : IQuery<Result<OrderDto>>;