using Microsoft.EntityFrameworkCore;
using Nimble.Modulith.Products.Models;

namespace Nimble.Modulith.Products.Data;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("Products");
        builder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);
    }
}