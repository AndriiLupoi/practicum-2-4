using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Nimble.Modulith.Reporting.Endpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints.Reports;

public class CustomerOrdersReportRequest
{
    public int CustomerId { get; set; }
    public string? Format { get; set; }
}

public class CustomerOrdersReport(IReportService reportService) : Endpoint<CustomerOrdersReportRequest>
{
    public override void Configure()
    {
        Get("/reports/customers/{customerId}/orders");
        AllowAnonymous();
        Tags("reports");
        Summary(s => { s.Summary = "Get orders report for a specific customer"; });
    }

    public override async Task HandleAsync(CustomerOrdersReportRequest req, CancellationToken ct)
    {
        var result = await reportService.GetCustomerOrdersReportAsync(req.CustomerId, ct);
        if (result is null) { await Send.NotFoundAsync(ct); return; }

        var wantsCsv = req.Format?.Equals("csv", StringComparison.OrdinalIgnoreCase) == true
                    || HttpContext.Request.Headers.Accept.ToString().Contains("text/csv");

        if (wantsCsv)
        {
            await HttpContext.Response.WriteAsync(CsvFormatter.ToCsv(result.Orders), ct);
            HttpContext.Response.ContentType = "text/csv";
            return;
        }

        await Send.OkAsync(result, ct);
    }
}