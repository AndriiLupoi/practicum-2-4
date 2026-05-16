using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data.Config;

public class DimDateConfig : IEntityTypeConfiguration<DimDate>
{
    public void Configure(EntityTypeBuilder<DimDate> builder)
    {
        builder.ToTable("DimDate");
        builder.HasKey(d => d.DateKey);
        builder.Property(d => d.DateKey).ValueGeneratedNever();
        builder.Property(d => d.DayName).HasMaxLength(20);
        builder.Property(d => d.MonthName).HasMaxLength(20);

        builder.HasData(GenerateDateDimension(2025));
        builder.HasData(GenerateDateDimension(2026));
    }

    private static List<DimDate> GenerateDateDimension(int year)
    {
        var dates = new List<DimDate>();
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31);
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            dates.Add(new DimDate
            {
                DateKey = int.Parse(d.ToString("yyyyMMdd")),
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
        return dates;
    }
}