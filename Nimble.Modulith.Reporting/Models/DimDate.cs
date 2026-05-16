namespace Nimble.Modulith.Reporting.Models;

public class DimDate
{
    public int DateKey { get; set; }   // YYYYMMDD
    public DateTime Date { get; set; }
    public int Year { get; set; }
    public int Quarter { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string MonthName { get; set; } = string.Empty;
}