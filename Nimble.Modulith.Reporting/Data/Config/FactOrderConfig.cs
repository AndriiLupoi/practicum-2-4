using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data.Config;

public class FactOrderConfig : IEntityTypeConfiguration<FactOrder>
{
    public void Configure(EntityTypeBuilder<FactOrder> builder)
    {
        builder.ToTable("FactOrders");
        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.OrderId, f.OrderItemId }).IsUnique();

        builder.Property(f => f.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(f => f.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(f => f.OrderTotalAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(f => f.Date).WithMany().HasForeignKey(f => f.DateKey);
        builder.HasOne(f => f.Customer).WithMany().HasForeignKey(f => f.CustomerId);
        builder.HasOne(f => f.Product).WithMany().HasForeignKey(f => f.ProductId);
    }
}