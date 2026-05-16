using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Customers.Queries;

namespace Nimble.Modulith.Customers.Endpoints.Customers;

public class GetById(IMediator mediator) : EndpointWithoutRequest<CustomerResponse>
{
    public override void Configure() { Get("/customers/{id}"); AllowAnonymous(); Tags("customers"); }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await mediator.Send(new GetCustomerByIdQuery(id), ct);
        if (!result.IsSuccess) { await Send.NotFoundAsync(ct); return; }
        Response = new CustomerResponse(result.Value.Id, result.Value.FirstName, result.Value.LastName, result.Value.Email, result.Value.PhoneNumber,
            new AddressResponse(result.Value.Address.Street, result.Value.Address.City, result.Value.Address.State, result.Value.Address.PostalCode, result.Value.Address.Country));
    }
}