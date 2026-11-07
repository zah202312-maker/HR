namespace HR_Mangment2.Attendance;

public enum AttendanceCaptureMethod
{
    Device,
    Gps,
    Both
}

public enum AttendanceDayStatus
{
    Present,
    Late,
    Absent,
    Incomplete,
    OnLeave,
    Holiday,
    WeeklyOff,
    NoShift
}

public enum AttendancePunchSource
{
    Device,
    File,
    Gps
}

public enum AttendancePunchType
{
    CheckIn,
    CheckOut
}

public enum AttendancePunchResultStatus
{
    Accepted,
    Duplicate,
    UnmappedDeviceUser,
    Invalid
}

public enum AttendanceImportStatus
{
    Queued,
    Processing,
    Completed,
    CompletedWithErrors,
    Failed
}

public enum AttendanceImportErrorCode
{
    MissingColumn,
    InvalidDateTime,
    EmptyDeviceUserId,
    InvalidDeviceUserId,
    FuturePunch
}
