using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Nimble.Modulith.Reporting.Endpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints.Reports;

public class ProductSalesReportRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Format { get; set; }
}

public class ProductSalesReport(IReportService reportService) : Endpoint<ProductSalesReportRequest>
{
    public override void Configure()
    {
        Get("/reports/product-sales");
        AllowAnonymous();
        Tags("reports");
        Summary(s => { s.Summary = "Get product sales report ranked by revenue"; });
    }

    public override async Task HandleAsync(ProductSalesReportRequest req, CancellationToken ct)
    {
        var result = await reportService.GetProductSalesReportAsync(req.StartDate, req.EndDate, ct);

        var wantsCsv = req.Format?.Equals("csv", StringComparison.OrdinalIgnoreCase) == true
                    || HttpContext.Request.Headers.Accept.ToString().Contains("text/csv");

        if (wantsCsv)
        {
            await HttpContext.Response.WriteAsync(CsvFormatter.ToCsv(result), ct);
            HttpContext.Response.ContentType = "text/csv";
            return;
        }

        await Send.OkAsync(result, ct);
    }
}