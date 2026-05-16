namespace Nimble.Modulith.Customers.Endpoints.Customers;

public record CustomerResponse(int Id, string FirstName, string LastName, string Email, string PhoneNumber, AddressResponse Address);
public record AddressResponse(string Street, string City, string State, string PostalCode, string Country);