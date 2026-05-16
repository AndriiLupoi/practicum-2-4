using System.Text;

namespace Nimble.Modulith.Reporting.Endpoints;

public static class CsvFormatter
{
    public static string ToCsv<T>(IEnumerable<T> items)
    {
        var sb = new StringBuilder();
        var props = typeof(T).GetProperties();

        sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.Name))));

        foreach (var item in items)
        {
            sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.GetValue(item)?.ToString() ?? string.Empty))));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}