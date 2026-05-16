namespace Nimble.Modulith.Reporting.Services;

public record OrderReportItem(
    int OrderId,
    string OrderNumber,
    string CustomerEmail,
    DateOnly OrderDate,
    int ItemCount,
    decimal TotalAmount
);

public record OrdersReportResult(
    List<OrderReportItem> Orders,
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue
);

public record ProductSalesItem(
    int ProductId,
    string ProductName,
    int TotalQuantity,
    decimal TotalRevenue,
    int OrderCount
);

public record CustomerOrderItem(
    int OrderId,
    string OrderNumber,
    DateOnly OrderDate,
    decimal TotalAmount,
    int ItemCount
);

public record CustomerOrdersResult(
    int CustomerId,
    string CustomerEmail,
    List<CustomerOrderItem> Orders,
    decimal TotalSpent,
    DateOnly? FirstOrderDate,
    DateOnly? LastOrderDate
);