using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data.Config;

public class DimProductConfig : IEntityTypeConfiguration<DimProduct>
{
    public void Configure(EntityTypeBuilder<DimProduct> builder)
    {
        builder.ToTable("DimProduct");
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).ValueGeneratedNever();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
    }
}