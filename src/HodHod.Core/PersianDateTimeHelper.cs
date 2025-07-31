using System;
using System.Globalization;
using TimeZoneConverter;

namespace HodHod;

public static class PersianDateTimeHelper
{
    public static string ToCompactPersianString(DateTime dateTime)
    {
        var tz = TZConvert.GetTimeZoneInfo("Iran Standard Time");
        var local = TimeZoneInfo.ConvertTime(dateTime, tz);
        var pc = new PersianCalendar();
        return string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}",
            pc.GetYear(local),
            pc.GetMonth(local),
            pc.GetDayOfMonth(local),
            local.Hour,
            local.Minute,
            local.Second);
    }
    public static long ToCompactPersianNumber(DateTime dateTime)
    {
        var formatted = ToCompactPersianString(dateTime);
        return long.Parse(formatted);
    }
}