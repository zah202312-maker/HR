using System;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Attendance;

/// <summary>A fingerprint device or local relay. Only the hash of its key is stored.</summary>
[Audited]
public class AttendanceDevice : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxSerialNumberLength = 128;
    public const int MaxNameLength = 128;
    public const int KeyHashLength = 64;

    public Guid? TenantId { get; private set; }
    public string SerialNumber { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }

    [DisableAuditing]
    public string KeyHash { get; private set; } = null!;

    protected AttendanceDevice()
    {
    }

    public AttendanceDevice(Guid id, Guid? tenantId, string serialNumber, string name, string keyHash)
        : base(id)
    {
        TenantId = tenantId;
        SerialNumber = serialNumber;
        Name = name;
        KeyHash = keyHash;
        IsActive = true;
    }

    public void Set(string name, bool isActive)
    {
        Name = name;
        IsActive = isActive;
    }

    public void SetKeyHash(string keyHash) => KeyHash = keyHash;

    /// <summary>The device key is long and random, so a plain SHA-256 is enough and allows lookup by hash.</summary>
    public static string HashKey(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();

    public static string NewKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}

/// <summary>
/// The last contact of a device. It is a separate row so that receiving punches does not change the device's
/// concurrency stamp, which would make the administrator's edit fail with 409 while the device is sending.
/// </summary>
public class AttendanceDeviceActivity : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public DateTime? LastSeenAt { get; private set; }

    protected AttendanceDeviceActivity()
    {
    }

    public AttendanceDeviceActivity(Guid deviceId, Guid? tenantId)
        : base(deviceId)
    {
        TenantId = tenantId;
    }

    public void MarkSeen(DateTime at) => LastSeenAt = at;
}

/// <summary>Links a user number inside a device to an employee.</summary>
[Audited]
public class DeviceUserMap : CreationAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxDeviceUserIdLength = 64;

    public Guid? TenantId { get; private set; }
    public Guid DeviceId { get; private set; }
    public string DeviceUserId { get; private set; } = null!;
    public Guid EmployeeId { get; private set; }

    protected DeviceUserMap()
    {
    }

    public DeviceUserMap(Guid id, Guid? tenantId, Guid deviceId, string deviceUserId, Guid employeeId)
        : base(id)
    {
        TenantId = tenantId;
        DeviceId = deviceId;
        DeviceUserId = deviceUserId;
        EmployeeId = employeeId;
    }
}

/// <summary>A raw punch. Its time and source never change; only the employee link is filled in when a map is added.</summary>
public class AttendancePunch : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid DeviceId { get; private set; }
    public string DeviceUserId { get; private set; } = null!;
    public DateTime PunchTime { get; private set; }
    public AttendancePunchSource Source { get; private set; }
    public Guid? ImportId { get; private set; }
    public Guid? EmployeeId { get; private set; }

    protected AttendancePunch()
    {
    }

    public AttendancePunch(
        Guid id,
        Guid? tenantId,
        Guid deviceId,
        string deviceUserId,
        DateTime punchTime,
        AttendancePunchSource source,
        Guid? importId,
        Guid? employeeId)
        : base(id)
    {
        TenantId = tenantId;
        DeviceId = deviceId;
        DeviceUserId = deviceUserId;
        PunchTime = punchTime;
        Source = source;
        ImportId = importId;
        EmployeeId = employeeId;
    }

    public void LinkEmployee(Guid employeeId) => EmployeeId = employeeId;
}

/// <summary>
/// One employee's computed day. Everything is derived from the punches, shift, holidays and leave, except the
/// corrected times, which only an approved correction sets and which survive every recalculation.
/// </summary>
public class AttendanceDay : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateOnly Date { get; private set; }
    public Guid? WorkShiftId { get; private set; }
    public DateTime? ScheduledStart { get; private set; }
    public DateTime? ScheduledEnd { get; private set; }
    public DateTime? OriginalCheckIn { get; private set; }
    public DateTime? OriginalCheckOut { get; private set; }
    public DateTime? CorrectedCheckIn { get; private set; }
    public DateTime? CorrectedCheckOut { get; private set; }
    public AttendanceDayStatus Status { get; private set; }
    public int LateMinutes { get; private set; }
    public int WorkedMinutes { get; private set; }
    public int OvertimeMinutes { get; private set; }
    public int? HourlyLeaveMinutes { get; private set; }
    public Guid? LeaveRequestId { get; private set; }
    public decimal LeaveDays { get; private set; }

    /// <summary>The span of punches that belong to this day, so the raw punches can be shown without changing them.</summary>
    public DateTime WindowStart { get; private set; }
    public DateTime WindowEnd { get; private set; }

    public DateTime? CheckIn => CorrectedCheckIn ?? OriginalCheckIn;
    public DateTime? CheckOut => CorrectedCheckOut ?? OriginalCheckOut;
    public bool IsCorrected => CorrectedCheckIn != null || CorrectedCheckOut != null;

    protected AttendanceDay()
    {
    }

    public AttendanceDay(Guid id, Guid? tenantId, Guid employeeId, DateOnly date)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        Date = date;
    }

    public void ApplyCorrection(AttendancePunchType punchType, DateTime value)
    {
        if (punchType == AttendancePunchType.CheckIn)
        {
            CorrectedCheckIn = value;
        }
        else
        {
            CorrectedCheckOut = value;
        }
    }

    /// <summary>For a row created by a correction, before the calculation fills it in.</summary>
    public void SetWindow(DateTime start, DateTime end)
    {
        WindowStart = start;
        WindowEnd = end;
    }

    public void Apply(AttendanceDayResult result)
    {
        WorkShiftId = result.WorkShiftId;
        ScheduledStart = result.ScheduledStart;
        ScheduledEnd = result.ScheduledEnd;
        OriginalCheckIn = result.OriginalCheckIn;
        OriginalCheckOut = result.OriginalCheckOut;
        Status = result.Status;
        LateMinutes = result.LateMinutes;
        WorkedMinutes = result.WorkedMinutes;
        OvertimeMinutes = result.OvertimeMinutes;
        HourlyLeaveMinutes = result.HourlyLeaveMinutes;
        LeaveRequestId = result.LeaveRequestId;
        LeaveDays = result.LeaveDays;
        WindowStart = result.WindowStart;
        WindowEnd = result.WindowEnd;
    }
}

/// <summary>A fallback file upload. Its rows are processed in the background.</summary>
public class AttendanceImport : CreationAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxFileNameLength = 256;
    public const int MaxBlobNameLength = 64;

    public Guid? TenantId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string BlobName { get; private set; } = null!;
    public Guid DeviceId { get; private set; }
    public AttendanceImportStatus Status { get; private set; }
    public int TotalRows { get; private set; }
    public int AcceptedCount { get; private set; }
    public int DuplicateCount { get; private set; }
    public int UnmappedCount { get; private set; }
    public int ErrorCount { get; private set; }

    /// <summary>Row errors as JSON, without their text: the message is localized when read.</summary>
    public string ErrorsJson { get; private set; } = "[]";

    public DateTime? CompletedAt { get; private set; }

    protected AttendanceImport()
    {
    }

    public AttendanceImport(Guid id, Guid? tenantId, string fileName, string blobName, Guid deviceId)
        : base(id)
    {
        TenantId = tenantId;
        FileName = fileName;
        BlobName = blobName;
        DeviceId = deviceId;
        Status = AttendanceImportStatus.Queued;
    }

    public void MarkProcessing() => Status = AttendanceImportStatus.Processing;

    public void Complete(
        AttendanceImportStatus status,
        int totalRows,
        int acceptedCount,
        int duplicateCount,
        int unmappedCount,
        int errorCount,
        string errorsJson,
        DateTime completedAt)
    {
        Status = status;
        TotalRows = totalRows;
        AcceptedCount = acceptedCount;
        DuplicateCount = duplicateCount;
        UnmappedCount = unmappedCount;
        ErrorCount = errorCount;
        ErrorsJson = errorsJson;
        CompletedAt = completedAt;
    }
}
