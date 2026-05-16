namespace Nimble.Modulith.Customers.UseCases.Customers;

public record CustomerDto(int Id, string FirstName, string LastName, string Email, string PhoneNumber, AddressDto Address, DateTime CreatedAt, DateTime? UpdatedAt);
public record AddressDto(string Street, string City, string State, string PostalCode, string Country);