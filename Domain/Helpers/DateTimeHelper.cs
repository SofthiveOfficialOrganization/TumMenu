namespace Domain.Helpers
{
    public static class DateTimeHelper
    {
        // Şimdi (UTC) epoch saniye
        public static long UtcNowSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public static long UtcNowMilliseconds() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Dönüşümler
        public static DateTimeOffset FromUnixSeconds(long seconds) => DateTimeOffset.FromUnixTimeSeconds(seconds);
        public static long ToUnixSeconds(this DateTimeOffset dto) => dto.ToUnixTimeSeconds();

        // Yerel zamanı epoch'a çevir (DST doğru)
        public static long ToUnixSeconds(DateTime localTime, TimeZoneInfo tz)
        {
            var offset = tz.GetUtcOffset(localTime); // DST-aware
            var dto = new DateTimeOffset(localTime, offset);
            return dto.ToUnixTimeSeconds();
        }

        // Epoch -> yerel DateTime (DST doğru)
        public static DateTime ToLocalTime(long seconds, TimeZoneInfo tz)
            => TimeZoneInfo.ConvertTime(FromUnixSeconds(seconds), tz).DateTime;

    }
}
