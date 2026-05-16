using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Reporting.Data;
using Nimble.Modulith.Reporting.Services;
using Serilog;

namespace Nimble.Modulith.Reporting;

public static class ReportingModuleExtensions
{
    public static IHostApplicationBuilder AddReportingModuleServices(
        this IHostApplicationBuilder builder, Serilog.ILogger logger)
    {
        builder.AddSqlServerDbContext<ReportingDbContext>("reportingdb");
        builder.Services.AddScoped<IReportService, ReportService>();
        logger.Information("{Module} module services registered", "Reporting");
        return builder;
    }

    public static async Task<WebApplication> EnsureReportingModuleDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportingDbContext>>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            logger.LogInformation("Dev: dropping and recreating reporting database...");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Reporting database created with seed data");
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        return app;
    }
}