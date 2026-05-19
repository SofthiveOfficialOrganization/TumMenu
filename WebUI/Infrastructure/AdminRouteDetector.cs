namespace WebUI.Infrastructure;

public static class AdminRouteDetector
{
	public const string ErrorOriginalAreaKey = "ErrorOriginalArea";
	public const string ErrorOriginalPathKey = "ErrorOriginalPath";

	public static bool IsAdminRequest(string? area, string? path)
	{
		if(area?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
			return true;

		var p = path ?? string.Empty;
		return p.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ||
		       p.StartsWith("/owner", StringComparison.OrdinalIgnoreCase);
	}

	public static void StashOriginalRequest(HttpContext context, string? area, string? path)
	{
		if(!string.IsNullOrEmpty(area))
			context.Items[ErrorOriginalAreaKey] = area;

		if(!string.IsNullOrEmpty(path))
			context.Items[ErrorOriginalPathKey] = path;
	}

	public static (string? area, string? path) GetStashed(HttpContext context)
	{
		context.Items.TryGetValue(ErrorOriginalAreaKey, out var area);
		context.Items.TryGetValue(ErrorOriginalPathKey, out var path);
		return (area as string, path as string);
	}
}
