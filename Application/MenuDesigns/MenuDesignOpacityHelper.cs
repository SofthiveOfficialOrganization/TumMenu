namespace Application.MenuDesigns;

public static class MenuDesignOpacityHelper
{
    public static string ToPercent(string? value, string fallbackPercent)
    {
        if (string.IsNullOrWhiteSpace(value))
            return EnsurePercent(fallbackPercent);

        var trimmed = value.Trim().TrimEnd('%');
        if (!int.TryParse(trimmed, out var n))
            return EnsurePercent(fallbackPercent);

        n = Math.Clamp(n, 0, 100);
        return $"{n}%";
    }

    private static string EnsurePercent(string value) =>
        value.EndsWith('%') ? value : $"{value}%";
}
