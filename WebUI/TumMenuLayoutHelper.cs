namespace WebUI;

/// <summary>Shared checks for public layout (GA4 id, etc.).</summary>
public static class TumMenuLayoutHelper
{
	public static bool IsValidGa4MeasurementId(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		var s = value.Trim();
		if (s.Length < 10 || s.Length > 32)
		{
			return false;
		}

		if (!s.StartsWith("G-", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (s.Contains("OVERRIDE", StringComparison.OrdinalIgnoreCase)
		    || s.Contains("WEBCONFIG", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		for (var i = 2; i < s.Length; i++)
		{
			var c = s[i];
			if (!char.IsLetterOrDigit(c))
			{
				return false;
			}
		}

		return true;
	}
}
