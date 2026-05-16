namespace Nimble.Modulith.Reporting.Services;

public interface IReportService
{
    Task<OrdersReportResult> GetOrdersReportAsync(DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
    Task<List<ProductSalesItem>> GetProductSalesReportAsync(DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
    Task<CustomerOrdersResult?> GetCustomerOrdersReportAsync(int customerId, CancellationToken ct = default);
}