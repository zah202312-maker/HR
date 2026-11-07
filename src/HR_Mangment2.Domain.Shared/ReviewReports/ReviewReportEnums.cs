namespace HR_Mangment2.ReviewReports;

public enum ReviewReportPeriodType
{
    Daily,
    Weekly,
    Monthly
}

public enum ReviewReportStatus
{
    Draft,
    Submitted
}

public static class ReviewReportConsts
{
    public const int MaxSummaryLength = 4000;
    public const int MaxNotesLength = 2000;
    public const int MaxTeamsPerUpdate = 500;
    public const int MinCompletionPercent = 0;
    public const int MaxCompletionPercent = 100;
}
