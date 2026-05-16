using Ardalis.Result;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Nimble.Modulith.Customers.Domain.CustomerAggregate;
using Nimble.Modulith.Customers.Domain.Interfaces;
using Nimble.Modulith.Email.Contracts;
using Nimble.Modulith.Users.Contracts;
using Nimble.Modulith.Users.Infrastructure;

namespace Nimble.Modulith.Customers.UseCases.Customers.Commands;

public class CreateCustomerHandler(
    IRepository<Customer> repository,
    IMediator mediator)
    : ICommandHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async ValueTask<Result<CustomerDto>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        var customer = new Customer
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            Address = new Address { Street = command.Street, City = command.City, State = command.State, PostalCode = command.PostalCode, Country = command.Country }
        };

        await repository.AddAsync(customer, ct);
        await repository.SaveChangesAsync(ct);

        // Створити user account
        var temporaryPassword = PasswordGenerator.GeneratePassword();
        var createUserResult = await mediator.Send(new CreateUserCommand(command.Email, temporaryPassword), ct);

        string emailBody;
        string emailSubject;

        if (createUserResult.IsSuccess)
        {
            emailSubject = "Welcome - Your Account Has Been Created";
            emailBody = $"Your account has been created.\nEmail: {command.Email}\nTemporary Password: {temporaryPassword}\n\nPlease log in and change your password.";
        }
        else
        {
            emailSubject = "Customer Profile Created";
            emailBody = $"Welcome back!\nA customer profile has been created for your existing account.\nEmail: {command.Email}";
        }

        await mediator.Send(new SendEmailCommand(command.Email, emailSubject, emailBody), ct);

        return Result<CustomerDto>.Success(new CustomerDto(
            customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber,
            new AddressDto(customer.Address.Street, customer.Address.City, customer.Address.State, customer.Address.PostalCode, customer.Address.Country),
            customer.CreatedAt, customer.UpdatedAt));
    }
}