using System;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Attendance;

public class AttendanceCorrectionCreateDto
{
    public Guid? EmployeeId { get; set; }
    public DateOnly? Date { get; set; }
    public AttendancePunchType? PunchType { get; set; }
    public TimeOnly? CorrectTime { get; set; }
    public string Reason { get; set; } = null!;
}

public class AttendanceCorrectionDto
{
    public Guid RequestId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto SubmittedBy { get; set; } = null!;
    public bool IsOnBehalf { get; set; }
    public DateOnly Date { get; set; }
    public AttendancePunchType PunchType { get; set; }
    public TimeOnly CorrectTime { get; set; }
    public string Reason { get; set; } = null!;
    public RequestStatus Status { get; set; }
    public DateTime? OriginalValue { get; set; }
    public DateTime? AppliedValue { get; set; }
    public DateTime? AppliedAt { get; set; }
    public bool? AffectsClosedMonth { get; set; }
    public string? SettlementPeriod { get; set; }
    public DateTime CreationTime { get; set; }
}

public interface IAttendanceCorrectionAppService : IApplicationService
{
    Task<AttendanceCorrectionDto> CreateAsync(AttendanceCorrectionCreateDto input, Guid? idempotencyKey);
    Task<AttendanceCorrectionDto> GetAsync(Guid id);
}
