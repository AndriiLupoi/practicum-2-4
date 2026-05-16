using Microsoft.EntityFrameworkCore;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data;

public class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<DimDate> DimDates => Set<DimDate>();
    public DbSet<DimCustomer> DimCustomers => Set<DimCustomer>();
    public DbSet<DimProduct> DimProducts => Set<DimProduct>();
    public DbSet<FactOrder> FactOrders => Set<FactOrder>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("Reporting");
        builder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);
    }
}