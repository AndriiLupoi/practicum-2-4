using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Domain.CustomerAggregate;
using Nimble.Modulith.Customers.Domain.Interfaces;

namespace Nimble.Modulith.Customers.UseCases.Customers.Queries;

public class GetCustomerByIdHandler(IReadRepository<Customer> repository) : IQueryHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async ValueTask<Result<CustomerDto>> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await repository.GetByIdAsync(query.Id, ct);
        if (customer is null) return Result<CustomerDto>.NotFound();

        return Result<CustomerDto>.Success(new CustomerDto(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber,
            new AddressDto(customer.Address.Street, customer.Address.City, customer.Address.State, customer.Address.PostalCode, customer.Address.Country),
            customer.CreatedAt, customer.UpdatedAt));
    }
}