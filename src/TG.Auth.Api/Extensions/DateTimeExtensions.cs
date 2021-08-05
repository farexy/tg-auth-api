using System;

namespace TG.Auth.Api.Extensions
{
    // todo move to core
    public static class DateTimeExtensions
    {
        public static DateTime UnixStartTime =>
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UnixTimeToDateTime(this long unixTimeStamp)
        {
            return UnixStartTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        }

        public static DateTime? UnixTimeToDateTime(this long? unixTimeStamp)
        {
            return unixTimeStamp.HasValue
                ? UnixStartTime.AddSeconds(unixTimeStamp.Value).ToUniversalTime()
                : default(DateTime?);
        }

        public static DateTime MsUnixTimeToDateTime(this long unixTimeStamp)
        {
            return UnixStartTime.AddMilliseconds(unixTimeStamp).ToUniversalTime();
        }

        public static DateTime? MsUnixTimeToDateTime(this long? unixTimeStamp)
        {
            return unixTimeStamp.HasValue
                ? UnixStartTime.AddMilliseconds(unixTimeStamp.Value).ToUniversalTime()
                : default(DateTime?);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var timeSpan = new TimeSpan(UnixStartTime.Ticks);
            return (long)(new TimeSpan(date.ToUniversalTime().Ticks) - timeSpan).TotalSeconds;
        }

        public static long? ToUnixTime(this DateTime? date)
        {
            if (!date.HasValue)
            {
                return default;
            }
            var timeSpan = new TimeSpan(UnixStartTime.Ticks);
            return (long)(new TimeSpan(date.Value.ToUniversalTime().Ticks) - timeSpan).TotalSeconds;
        }

        public static double ToUnixMSTime(this DateTime date)
        {
            var timeSpan = new TimeSpan(UnixStartTime.Ticks);
            return (new TimeSpan(date.ToUniversalTime().Ticks) - timeSpan).TotalMilliseconds;
        }

        public static double? ToUnixMSTime(this DateTime? date)
        {
            return date?.ToUnixMSTime() ?? default;
        }
    }
}