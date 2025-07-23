namespace HodHod.Reports;

public static class PhoneReportLimitDefaults
{
    public const int MaxFileCount = 5;

    /// <summary>
    /// Maximum allowed file size for each uploaded file.
    /// </summary>
    public const long MaxFileSizeInBytes = 50 * 1024 * 1024; //50MB

    /// <summary>
    /// Maximum number of reports a phone number may submit within one hour.
    /// </summary>
    public const int MaxReportsPerHour = ReportRateLimitConsts.PermitLimit;
}