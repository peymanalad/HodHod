using System;
using System.Globalization;

namespace HodHod;

public static class PersianDateTimeHelper
{
    public static string ToCompactPersianString(DateTime dateTime)
    {
        var pc = new PersianCalendar();
        return string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}",
            pc.GetYear(dateTime),
            pc.GetMonth(dateTime),
            pc.GetDayOfMonth(dateTime),
            dateTime.Hour,
            dateTime.Minute,
            dateTime.Second);
    }
}