using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data.Config;

public class DimCustomerConfig : IEntityTypeConfiguration<DimCustomer>
{
    public void Configure(EntityTypeBuilder<DimCustomer> builder)
    {
        builder.ToTable("DimCustomer");
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerId).ValueGeneratedNever();
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.FirstName).HasMaxLength(100);
        builder.Property(c => c.LastName).HasMaxLength(100);
    }
}