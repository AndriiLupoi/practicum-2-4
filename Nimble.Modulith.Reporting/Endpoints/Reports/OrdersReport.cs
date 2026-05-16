using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Nimble.Modulith.Reporting.Endpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints.Reports;

public class OrdersReportRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Format { get; set; }
}

public class OrdersReport(IReportService reportService) : Endpoint<OrdersReportRequest>
{
    public override void Configure()
    {
        Get("/reports/orders");
        AllowAnonymous();
        Tags("reports");
        Summary(s => { s.Summary = "Get orders report"; s.Description = "Supports JSON and CSV output"; });
    }

    public override async Task HandleAsync(OrdersReportRequest req, CancellationToken ct)
    {
        var result = await reportService.GetOrdersReportAsync(req.StartDate, req.EndDate, ct);

        var wantsCsv = req.Format?.Equals("csv", StringComparison.OrdinalIgnoreCase) == true
                    || HttpContext.Request.Headers.Accept.ToString().Contains("text/csv");

        if (wantsCsv)
        {
            var csv = CsvFormatter.ToCsv(result.Orders);
            await HttpContext.Response.WriteAsync(csv, ct);
            HttpContext.Response.ContentType = "text/csv";
            return;
        }

        await Send.OkAsync(result, ct);
    }
}