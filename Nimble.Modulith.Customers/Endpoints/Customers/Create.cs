using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Customers.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Customers;

public class CreateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public AddressRequest Address { get; set; } = new();
}
public class AddressRequest { public string Street { get; set; } = string.Empty; public string City { get; set; } = string.Empty; public string State { get; set; } = string.Empty; public string PostalCode { get; set; } = string.Empty; public string Country { get; set; } = string.Empty; }

public class Create(IMediator mediator) : Endpoint<CreateCustomerRequest, CustomerResponse>
{
    public override void Configure() { Post("/customers"); AllowAnonymous(); Tags("customers"); }

    public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
    {
        var command = new CreateCustomerCommand(req.FirstName, req.LastName, req.Email, req.PhoneNumber, req.Address.Street, req.Address.City, req.Address.State, req.Address.PostalCode, req.Address.Country);
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess) { await Send.ErrorsAsync(cancellation: ct); return; }
        Response = new CustomerResponse(result.Value.Id, result.Value.FirstName, result.Value.LastName, result.Value.Email, result.Value.PhoneNumber,
            new AddressResponse(result.Value.Address.Street, result.Value.Address.City, result.Value.Address.State, result.Value.Address.PostalCode, result.Value.Address.Country));
        await Send.CreatedAtAsync<GetById>(new { id = result.Value.Id }, generateAbsoluteUrl: false, cancellation: ct);
    }
}