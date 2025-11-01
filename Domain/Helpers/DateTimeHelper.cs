using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helpers
{
	public static class DateTimeHelper
	{
		public static TimeZoneInfo CurrentTimeZone => TimeZoneInfo.Local;
		public static DateTime ConvertLocalDateTime(this long date) => TimeZoneInfo.ConvertTimeFromUtc(FromLongDateToUTCDatime(date), CurrentTimeZone);
		public static DateTime ConvertLocalDateTime(this DateTimeOffset date) => TimeZoneInfo.ConvertTimeFromUtc(date.UtcDateTime, CurrentTimeZone);
		public static long ConvertFromLocalDate(this DateTime date) => new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second,
																				CurrentTimeZone.BaseUtcOffset).GetUtcTime();
		public static DateTime FromLongDateToUTCDatime(this long Datetime) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Datetime);
		public static DateTime GetUtcNowDate() => GetUtcNowTime().ConvertLocalDateTime();
		public static long GetUtcNowTime() => Convert.ToInt64((DateTime.UtcNow.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
		public static long GetUtcTime(this DateTime date) => Convert.ToInt64((date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
		public static long GetUtcTime(this DateTimeOffset date) => Convert.ToInt64((date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

	}
}
