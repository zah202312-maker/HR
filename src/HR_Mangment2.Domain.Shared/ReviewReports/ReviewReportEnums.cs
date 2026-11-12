namespace HR_Mangment2.ReviewReports;

public enum ReviewReportStatus
{
    Draft,
    Submitted
}

public static class ReviewReportConsts
{
    public const int MaxContentLength = 10000;
    public const int MaxNotesLength = 2000;
    public const int MaxTeamsPerUpdate = 500;
    public const int MaxRoomNameLength = 128;
    public const int MaxRoomMembers = 200;
}
