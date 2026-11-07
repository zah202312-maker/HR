using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Settings;

public class SystemSettingsSectionDto
{
    public SystemSettingsSectionCode Code { get; set; }
    public AppRole OwnerRole { get; set; }
    public bool IsEditableByCaller { get; set; }
}

public class SystemSettingsSectionListDto
{
    public List<SystemSettingsSectionDto> Items { get; set; } = new();
}

public class SystemSettingsRemindersDto
{
    public TimeOnly MonthEndOpenTime { get; set; }
    public int MonthlySubmissionWorkingDays { get; set; }
    public int MonthlyReminderIntervalWorkingDays { get; set; }
    public int MonthlyEscalationWorkingDay { get; set; }
    public int? ApprovalReminderIntervalWorkingDays { get; set; }
    public int? EvaluationMinWorkedDays { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime? LastModificationTime { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class SystemSettingsRemindersUpdateInputDto
{
    [Required]
    public TimeOnly? MonthEndOpenTime { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int? MonthlySubmissionWorkingDays { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int? MonthlyReminderIntervalWorkingDays { get; set; }

    [Required, Range(2, int.MaxValue)]
    public int? MonthlyEscalationWorkingDay { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int? ApprovalReminderIntervalWorkingDays { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int? EvaluationMinWorkedDays { get; set; }

    [Required]
    public string ConcurrencyStamp { get; set; } = null!;
}

public class SystemSettingsContractsDto
{
    public int AlertDays { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime? LastModificationTime { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class SystemSettingsContractsUpdateInputDto
{
    [Required, Range(1, 365)]
    public int? AlertDays { get; set; }

    [Required]
    public string ConcurrencyStamp { get; set; } = null!;
}

public class SystemSettingsComplaintsDto
{
    public int? DelayDeadlineWorkingDays { get; set; }
    public int? OverdueReminderIntervalWorkingDays { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime? LastModificationTime { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class SystemSettingsComplaintsUpdateInputDto
{
    [Required, Range(1, int.MaxValue)]
    public int? DelayDeadlineWorkingDays { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int? OverdueReminderIntervalWorkingDays { get; set; }

    [Required]
    public string ConcurrencyStamp { get; set; } = null!;
}

public class SystemSettingsHourlyLeaveDto
{
    public decimal? MonthlyAllowedHours { get; set; }
    public decimal? HoursPerDay { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime? LastModificationTime { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class SystemSettingsHourlyLeaveUpdateInputDto
{
    [Required, Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal? MonthlyAllowedHours { get; set; }

    [Required, Range(typeof(decimal), "0.01", "24")]
    public decimal? HoursPerDay { get; set; }

    [Required]
    public string ConcurrencyStamp { get; set; } = null!;
}

public class SystemSettingsFinanceDto
{
    public PayrollApproverRole? PayrollApproverRole { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime? LastModificationTime { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class SystemSettingsFinanceUpdateInputDto
{
    [Required]
    public PayrollApproverRole? PayrollApproverRole { get; set; }

    [Required]
    public string ConcurrencyStamp { get; set; } = null!;
}

public interface ISystemSettingsAppService : IApplicationService
{
    Task<SystemSettingsSectionListDto> ListSectionsAsync();
    Task<SystemSettingsRemindersDto> GetRemindersAsync();
    Task<SystemSettingsRemindersDto> UpdateRemindersAsync(SystemSettingsRemindersUpdateInputDto input);
    Task<SystemSettingsContractsDto> GetContractsAsync();
    Task<SystemSettingsContractsDto> UpdateContractsAsync(SystemSettingsContractsUpdateInputDto input);
    Task<SystemSettingsComplaintsDto> GetComplaintsAsync();
    Task<SystemSettingsComplaintsDto> UpdateComplaintsAsync(SystemSettingsComplaintsUpdateInputDto input);
    Task<SystemSettingsHourlyLeaveDto> GetHourlyLeaveAsync();
    Task<SystemSettingsHourlyLeaveDto> UpdateHourlyLeaveAsync(SystemSettingsHourlyLeaveUpdateInputDto input);
    Task<SystemSettingsFinanceDto> GetFinanceAsync();
    Task<SystemSettingsFinanceDto> UpdateFinanceAsync(SystemSettingsFinanceUpdateInputDto input);
}
