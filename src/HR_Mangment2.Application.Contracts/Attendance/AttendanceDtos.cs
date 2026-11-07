using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.WorkShifts;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Attendance;

public class AttendanceDayDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public DateOnly Date { get; set; }
    public WorkShiftRefDto? Shift { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public AttendanceDayStatus Status { get; set; }
    public int LateMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public int? HourlyLeaveMinutes { get; set; }
    public Guid? LeaveRequestId { get; set; }
    public bool IsCorrected { get; set; }
}

public class AttendancePeriodSummaryDto
{
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int TotalLateMinutes { get; set; }
    public int AbsentDays { get; set; }
    public int IncompleteDays { get; set; }
    public decimal LeaveDays { get; set; }
    public long WorkedMinutes { get; set; }
    public long OvertimeMinutes { get; set; }
}

public class AttendanceDayPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AttendanceDayDto> Items { get; set; } = new();
    public AttendancePeriodSummaryDto Summary { get; set; } = new();
}

public class AttendanceRawPunchDto
{
    public Guid Id { get; set; }
    public DateTime PunchTime { get; set; }
    public AttendancePunchSource Source { get; set; }
    public string? DeviceName { get; set; }
    public Guid? ImportId { get; set; }
}

public class AttendanceCorrectionHistoryEntryDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public AttendancePunchType PunchType { get; set; }
    public DateTime? OldValue { get; set; }
    public DateTime NewValue { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public UserRefDto RequestedBy { get; set; } = null!;
    public UserRefDto? ApprovedBy { get; set; }
    public bool AffectsClosedMonth { get; set; }
    public string? SettlementPeriod { get; set; }
}

public class AttendanceDayDetailDto
{
    public AttendanceDayDto Day { get; set; } = null!;
    public DateTime? OriginalCheckIn { get; set; }
    public DateTime? OriginalCheckOut { get; set; }
    public List<AttendanceRawPunchDto> RawPunches { get; set; } = new();
    public List<AttendanceCorrectionHistoryEntryDto> Corrections { get; set; } = new();
}

public class AttendanceMyListInput : PagedAndSortedResultRequestDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public AttendanceDayStatus? Status { get; set; }
}

public class AttendanceTeamListInput : AttendanceMyListInput
{
    public Guid? EmployeeId { get; set; }
}

public interface IAttendanceAppService : IApplicationService
{
    Task<AttendanceDayPagedResultDto> GetMyAsync(AttendanceMyListInput input);
    Task<AttendanceDayPagedResultDto> GetTeamAsync(AttendanceTeamListInput input);
    Task<AttendanceDayPagedResultDto> GetByEmployeeAsync(Guid employeeId, AttendanceMyListInput input);
    Task<AttendanceDayDetailDto> GetDayDetailAsync(Guid id);
}

public class AttendanceSettingsDto
{
    public AttendanceCaptureMethod Method { get; set; }
    public bool GpsAvailable { get; set; }
    public string TimeZone { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class AttendanceSettingsUpdateDto
{
    public AttendanceCaptureMethod? Method { get; set; }
    public string TimeZone { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public interface IAttendanceSettingsAppService : IApplicationService
{
    Task<AttendanceSettingsDto> GetAsync();
    Task<AttendanceSettingsDto> UpdateAsync(AttendanceSettingsUpdateDto input);
}

public class AttendanceDeviceDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public int MappedUserCount { get; set; }
    public int UnmappedUserCount { get; set; }
    public DateTime CreationTime { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class AttendanceDeviceKeyDto
{
    public AttendanceDeviceDto Device { get; set; } = null!;
    public string DeviceKey { get; set; } = null!;
}

public class AttendanceDevicePagedResultDto
{
    public long TotalCount { get; set; }
    public List<AttendanceDeviceDto> Items { get; set; } = new();
}

public class AttendanceDeviceCreateDto
{
    public string SerialNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class AttendanceDeviceUpdateDto
{
    public string Name { get; set; } = null!;
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class AttendanceDeviceListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}

public class DeviceUserMapCreateDto
{
    public string DeviceUserId { get; set; } = null!;
    public Guid? EmployeeId { get; set; }
}

public class DeviceUserMapDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string DeviceUserId { get; set; } = null!;
    public EmployeeRefDto Employee { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class DeviceUserMapPagedResultDto
{
    public long TotalCount { get; set; }
    public List<DeviceUserMapDto> Items { get; set; } = new();
}

public class DeviceUserMapListInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
}

public class UnmappedDeviceUserDto
{
    public string DeviceUserId { get; set; } = null!;
    public int PunchCount { get; set; }
    public DateTime FirstPunchTime { get; set; }
    public DateTime LastPunchTime { get; set; }
}

public class UnmappedDeviceUserPagedResultDto
{
    public long TotalCount { get; set; }
    public List<UnmappedDeviceUserDto> Items { get; set; } = new();
}

public interface IAttendanceDeviceAppService : IApplicationService
{
    Task<AttendanceDevicePagedResultDto> GetListAsync(AttendanceDeviceListInput input);
    Task<AttendanceDeviceKeyDto> CreateAsync(AttendanceDeviceCreateDto input, Guid? idempotencyKey);
    Task<AttendanceDeviceDto> UpdateAsync(Guid id, AttendanceDeviceUpdateDto input);
    Task<AttendanceDeviceKeyDto> RotateKeyAsync(Guid id);
    Task<DeviceUserMapPagedResultDto> GetUserMapAsync(Guid id, DeviceUserMapListInput input);
    Task<DeviceUserMapDto> CreateUserMapAsync(Guid id, DeviceUserMapCreateDto input);
    Task DeleteUserMapAsync(Guid id, Guid mapId);
    Task<UnmappedDeviceUserPagedResultDto> GetUnmappedUsersAsync(Guid id, PagedAndSortedResultRequestDto input);
}

public class AttendancePunchInputDto
{
    public string? DeviceUserId { get; set; }
    public DateTime? PunchTime { get; set; }
}

public class AttendancePunchBatchDto
{
    public List<AttendancePunchInputDto> Punches { get; set; } = null!;
}

public class AttendancePunchIssueDto
{
    public int Index { get; set; }
    public string DeviceUserId { get; set; } = null!;
    public DateTime? PunchTime { get; set; }
    public AttendancePunchResultStatus Status { get; set; }
}

public class AttendancePunchBatchResultDto
{
    public int ReceivedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int UnmappedCount { get; set; }
    public int InvalidCount { get; set; }
    public List<AttendancePunchIssueDto> Issues { get; set; } = new();
}

public interface IAttendancePunchAppService : IApplicationService
{
    /// <param name="deviceKey">The X-Device-Key header. It is checked first, so a caller without a key learns nothing about the body rules.</param>
    Task<AttendancePunchBatchResultDto> IngestAsync(string? deviceKey, AttendancePunchBatchDto? input);
}

public class AttendanceImportErrorDto
{
    public int RowNumber { get; set; }
    public string? ColumnName { get; set; }
    public AttendanceImportErrorCode Code { get; set; }
    public string Message { get; set; } = null!;
}

public class AttendanceImportDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public Guid DeviceId { get; set; }
    public AttendanceImportStatus Status { get; set; }
    public int TotalRows { get; set; }
    public int AcceptedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int UnmappedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<AttendanceImportErrorDto> Errors { get; set; } = new();
    public UserRefDto UploadedBy { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class AttendanceImportPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AttendanceImportDto> Items { get; set; } = new();
}

public class AttendanceImportListInput : PagedAndSortedResultRequestDto
{
    public AttendanceImportStatus? Status { get; set; }
}

public class AttendanceImportUploadInput
{
    public byte[] Content { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public Guid DeviceId { get; set; }
}

public interface IAttendanceImportAppService : IApplicationService
{
    Task<AttendanceImportDto> CreateAsync(AttendanceImportUploadInput input);
    Task<AttendanceImportPagedResultDto> GetListAsync(AttendanceImportListInput input);
    Task<AttendanceImportDto> GetAsync(Guid id);
}
