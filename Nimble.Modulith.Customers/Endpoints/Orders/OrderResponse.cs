namespace Nimble.Modulith.Customers.Endpoints.Orders;

public record OrderResponse(
    int Id,
    int CustomerId,
    string OrderNumber,
    DateOnly OrderDate,
    string Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);