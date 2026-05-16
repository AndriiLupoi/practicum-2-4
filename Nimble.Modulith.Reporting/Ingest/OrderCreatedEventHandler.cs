using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Customers.Contracts;
using Nimble.Modulith.Reporting.Data;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Ingest;

public class OrderCreatedEventHandler(
    ReportingDbContext db,
    ILogger<OrderCreatedEventHandler> logger)
    : INotificationHandler<OrderCreatedEvent>
{
    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Ingesting OrderCreatedEvent for order {OrderId}", notification.OrderId);

        var dateKey = ConvertToDateKey(notification.OrderDate);

        // Ensure DimCustomer exists
        var customer = await db.DimCustomers.FindAsync([notification.CustomerId], cancellationToken);
        if (customer is null)
        {
            var nameParts = notification.CustomerEmail.Split('@')[0].Split('.');
            db.DimCustomers.Add(new DimCustomer
            {
                CustomerId = notification.CustomerId,
                Email = notification.CustomerEmail,
                FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty
            });
        }

        // Ensure DimDate exists (fallback if date not seeded)
        var date = await db.DimDates.FindAsync([dateKey], cancellationToken);
        if (date is null)
        {
            var d = notification.OrderDate.ToDateTime(TimeOnly.MinValue);
            db.DimDates.Add(new DimDate
            {
                DateKey = dateKey,
                Date = d,
                Year = d.Year,
                Quarter = (d.Month - 1) / 3 + 1,
                Month = d.Month,
                Day = d.Day,
                DayOfWeek = (int)d.DayOfWeek,
                DayName = d.DayOfWeek.ToString(),
                MonthName = d.ToString("MMMM")
            });
        }

        foreach (var item in notification.Items)
        {
            // Idempotency check
            var exists = await db.FactOrders
                .AnyAsync(f => f.OrderId == notification.OrderId && f.OrderItemId == item.Id, cancellationToken);
            if (exists)
            {
                logger.LogWarning("FactOrder for OrderId={OrderId} ItemId={ItemId} already exists, skipping", notification.OrderId, item.Id);
                continue;
            }

            // Ensure DimProduct exists
            var product = await db.DimProducts.FindAsync([item.ProductId], cancellationToken);
            if (product is null)
            {
                db.DimProducts.Add(new DimProduct
                {
                    ProductId = item.ProductId,
                    Name = item.ProductName
                });
            }

            db.FactOrders.Add(new FactOrder
            {
                OrderId = notification.OrderId,
                OrderItemId = item.Id,
                OrderNumber = notification.OrderNumber,
                DateKey = dateKey,
                CustomerId = notification.CustomerId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                OrderTotalAmount = notification.TotalAmount
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully ingested order {OrderId} with {ItemCount} items", notification.OrderId, notification.Items.Count);
    }

    private static int ConvertToDateKey(DateOnly date)
        => date.Year * 10000 + date.Month * 100 + date.Day;
}