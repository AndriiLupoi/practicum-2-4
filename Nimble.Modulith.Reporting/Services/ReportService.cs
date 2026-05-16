using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Reporting.Data;

namespace Nimble.Modulith.Reporting.Services;

public class ReportService(ReportingDbContext db, ILogger<ReportService> logger) : IReportService
{
    private SqlConnection GetConnection()
        => new(db.Database.GetConnectionString());

    public async Task<OrdersReportResult> GetOrdersReportAsync(DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        using var conn = GetConnection();
        var startKey = startDate.Year * 10000 + startDate.Month * 100 + startDate.Day;
        var endKey = endDate.Year * 10000 + endDate.Month * 100 + endDate.Day;

        var sql = """
            SELECT 
                f.OrderId,
                f.OrderNumber,
                c.Email AS CustomerEmail,
                d.Year, d.Month, d.Day,
                COUNT(f.Id) AS ItemCount,
                MAX(f.OrderTotalAmount) AS TotalAmount
            FROM Reporting.FactOrders f
            JOIN Reporting.DimCustomer c ON f.CustomerId = c.CustomerId
            JOIN Reporting.DimDate d ON f.DateKey = d.DateKey
            WHERE f.DateKey BETWEEN @StartKey AND @EndKey
            GROUP BY f.OrderId, f.OrderNumber, c.Email, d.Year, d.Month, d.Day
            ORDER BY d.Year, d.Month, d.Day
            """;

        var rows = await conn.QueryAsync(sql, new { StartKey = startKey, EndKey = endKey });
        var orders = rows.Select(r => new OrderReportItem(
            (int)r.OrderId,
            (string)r.OrderNumber,
            (string)r.CustomerEmail,
            new DateOnly((int)r.Year, (int)r.Month, (int)r.Day),
            (int)r.ItemCount,
            (decimal)r.TotalAmount
        )).ToList();

        return new OrdersReportResult(
            orders,
            orders.Count,
            orders.Sum(o => o.TotalAmount),
            orders.Count > 0 ? orders.Average(o => o.TotalAmount) : 0
        );
    }

    public async Task<List<ProductSalesItem>> GetProductSalesReportAsync(DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        using var conn = GetConnection();
        var startKey = startDate.Year * 10000 + startDate.Month * 100 + startDate.Day;
        var endKey = endDate.Year * 10000 + endDate.Month * 100 + endDate.Day;

        var sql = """
            SELECT 
                p.ProductId,
                p.Name AS ProductName,
                SUM(f.Quantity) AS TotalQuantity,
                SUM(f.TotalPrice) AS TotalRevenue,
                COUNT(DISTINCT f.OrderId) AS OrderCount
            FROM Reporting.FactOrders f
            JOIN Reporting.DimProduct p ON f.ProductId = p.ProductId
            WHERE f.DateKey BETWEEN @StartKey AND @EndKey
            GROUP BY p.ProductId, p.Name
            ORDER BY TotalRevenue DESC
            """;

        var rows = await conn.QueryAsync(sql, new { StartKey = startKey, EndKey = endKey });
        return rows.Select(r => new ProductSalesItem(
            (int)r.ProductId,
            (string)r.ProductName,
            (int)r.TotalQuantity,
            (decimal)r.TotalRevenue,
            (int)r.OrderCount
        )).ToList();
    }

    public async Task<CustomerOrdersResult?> GetCustomerOrdersReportAsync(int customerId, CancellationToken ct = default)
    {
        using var conn = GetConnection();

        var customerSql = "SELECT CustomerId, Email FROM Reporting.DimCustomer WHERE CustomerId = @CustomerId";
        var customer = await conn.QueryFirstOrDefaultAsync(customerSql, new { CustomerId = customerId });
        if (customer is null) return null;

        var ordersSql = """
            SELECT 
                f.OrderId,
                f.OrderNumber,
                d.Year, d.Month, d.Day,
                MAX(f.OrderTotalAmount) AS TotalAmount,
                COUNT(f.Id) AS ItemCount
            FROM Reporting.FactOrders f
            JOIN Reporting.DimDate d ON f.DateKey = d.DateKey
            WHERE f.CustomerId = @CustomerId
            GROUP BY f.OrderId, f.OrderNumber, d.Year, d.Month, d.Day
            ORDER BY d.Year, d.Month, d.Day
            """;

        var rows = await conn.QueryAsync(ordersSql, new { CustomerId = customerId });
        var orders = rows.Select(r => new CustomerOrderItem(
            (int)r.OrderId,
            (string)r.OrderNumber,
            new DateOnly((int)r.Year, (int)r.Month, (int)r.Day),
            (decimal)r.TotalAmount,
            (int)r.ItemCount
        )).ToList();

        return new CustomerOrdersResult(
            customerId,
            (string)customer.Email,
            orders,
            orders.Sum(o => o.TotalAmount),
            orders.Count > 0 ? orders.Min(o => o.OrderDate) : null,
            orders.Count > 0 ? orders.Max(o => o.OrderDate) : null
        );
    }
}