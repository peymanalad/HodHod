namespace HodHod.Reports;

public static class PhoneReportLimitDefaults
{
    public const int MaxFileCount = 5;
    public const long MaxFileSizeInBytes = 10 * 1024 * 1024; //10MB
    public const int MaxReportsPerHour = ReportRateLimitConsts.PermitLimit;
}