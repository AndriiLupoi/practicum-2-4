namespace Nimble.Modulith.Customers.Contracts;

public record OrderItemDetails(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);