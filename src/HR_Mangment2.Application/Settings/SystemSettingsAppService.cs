using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;

namespace HR_Mangment2.Settings;

[Authorize]
public class SystemSettingsAppService : HR_Mangment2AppService, ISystemSettingsAppService
{
    /// <summary>Owners from document 5.18. Work shifts are also editable by the system admin (QUESTIONS.md item 11).</summary>
    private static readonly (SystemSettingsSectionCode Code, AppRole Owner)[] Sections =
    {
        (SystemSettingsSectionCode.Reminders, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.Contracts, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.Complaints, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.Attendance, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.HourlyLeave, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.Finance, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.RolesAndPermissions, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.RequestTypes, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.OtherLeaveTypes, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.AttendanceDevices, AppRole.SystemAdmin),
        (SystemSettingsSectionCode.EmployeeLeaveBalances, AppRole.HrManager),
        (SystemSettingsSectionCode.WorkShifts, AppRole.HrManager),
        (SystemSettingsSectionCode.Holidays, AppRole.HrManager),
        (SystemSettingsSectionCode.DepartmentsAndManagerLinks, AppRole.HrManager),
        (SystemSettingsSectionCode.AnnualLeave, AppRole.HrManager),
        (SystemSettingsSectionCode.HrEvaluationAlert, AppRole.HrManager)
    };

    private readonly HrSettingsStore _store;

    public SystemSettingsAppService(HrSettingsStore store)
    {
        _store = store;
    }

    [Authorize(HrPermissions.SettingsPermissions.Read)]
    public virtual Task<SystemSettingsSectionListDto> ListSectionsAsync()
    {
        var isHr = CurrentUser.IsInRole(HrRoles.HrManager);
        var isAdmin = CurrentUser.IsInRole(HrRoles.SystemAdmin);

        return Task.FromResult(new SystemSettingsSectionListDto
        {
            Items = Sections.Select(s => new SystemSettingsSectionDto
            {
                Code = s.Code,
                OwnerRole = s.Owner,
                IsEditableByCaller = s.Owner == AppRole.HrManager
                    ? isHr || (isAdmin && s.Code == SystemSettingsSectionCode.WorkShifts)
                    : isAdmin
            }).ToList()
        });
    }

    [Authorize(HrPermissions.SettingsPermissions.Reminders)]
    public virtual async Task<SystemSettingsRemindersDto> GetRemindersAsync() => ToDto(await _store.GetAsync<ReminderSettings>());

    [Authorize(HrPermissions.SettingsPermissions.Reminders)]
    public virtual async Task<SystemSettingsRemindersDto> UpdateRemindersAsync(SystemSettingsRemindersUpdateInputDto input)
    {
        if (input.MonthlyEscalationWorkingDay <= input.MonthlySubmissionWorkingDays)
        {
            throw new BusinessException(HrErrorCodes.Settings.InvalidEscalationDay);
        }

        return ToDto(await _store.SetAsync(new ReminderSettings
        {
            MonthEndOpenTime = input.MonthEndOpenTime!.Value,
            MonthlySubmissionWorkingDays = input.MonthlySubmissionWorkingDays!.Value,
            MonthlyReminderIntervalWorkingDays = input.MonthlyReminderIntervalWorkingDays!.Value,
            MonthlyEscalationWorkingDay = input.MonthlyEscalationWorkingDay!.Value,
            ApprovalReminderIntervalWorkingDays = input.ApprovalReminderIntervalWorkingDays,
            EvaluationMinWorkedDays = input.EvaluationMinWorkedDays
        }, input.ConcurrencyStamp));
    }

    [Authorize(HrPermissions.SettingsPermissions.Contracts)]
    public virtual async Task<SystemSettingsContractsDto> GetContractsAsync() => ToDto(await _store.GetAsync<ContractSettings>());

    [Authorize(HrPermissions.SettingsPermissions.Contracts)]
    public virtual async Task<SystemSettingsContractsDto> UpdateContractsAsync(SystemSettingsContractsUpdateInputDto input) =>
        ToDto(await _store.SetAsync(new ContractSettings { AlertDays = input.AlertDays!.Value }, input.ConcurrencyStamp));

    [Authorize(HrPermissions.SettingsPermissions.Complaints)]
    public virtual async Task<SystemSettingsComplaintsDto> GetComplaintsAsync() => ToDto(await _store.GetAsync<ComplaintSettings>());

    [Authorize(HrPermissions.SettingsPermissions.Complaints)]
    public virtual async Task<SystemSettingsComplaintsDto> UpdateComplaintsAsync(SystemSettingsComplaintsUpdateInputDto input) =>
        ToDto(await _store.SetAsync(new ComplaintSettings
        {
            DelayDeadlineWorkingDays = input.DelayDeadlineWorkingDays,
            OverdueReminderIntervalWorkingDays = input.OverdueReminderIntervalWorkingDays
        }, input.ConcurrencyStamp));

    [Authorize(HrPermissions.SettingsPermissions.HourlyLeave)]
    public virtual async Task<SystemSettingsHourlyLeaveDto> GetHourlyLeaveAsync() => ToDto(await _store.GetAsync<HourlyLeaveSettings>());

    [Authorize(HrPermissions.SettingsPermissions.HourlyLeave)]
    public virtual async Task<SystemSettingsHourlyLeaveDto> UpdateHourlyLeaveAsync(SystemSettingsHourlyLeaveUpdateInputDto input) =>
        ToDto(await _store.SetAsync(new HourlyLeaveSettings
        {
            MonthlyAllowedHours = input.MonthlyAllowedHours,
            HoursPerDay = input.HoursPerDay
        }, input.ConcurrencyStamp));

    [Authorize(HrPermissions.SettingsPermissions.AnnualLeave)]
    public virtual async Task<SystemSettingsAnnualLeaveDto> GetAnnualLeaveAsync() => ToDto(await _store.GetAsync<AnnualLeaveSettings>());

    /// <summary>Applies to every leave year that starts afterwards; no current year changes (decision 2026-11-10).</summary>
    [Authorize(HrPermissions.SettingsPermissions.AnnualLeave)]
    public virtual async Task<SystemSettingsAnnualLeaveDto> UpdateAnnualLeaveAsync(SystemSettingsAnnualLeaveUpdateInputDto input) =>
        ToDto(await _store.SetAsync(new AnnualLeaveSettings { DefaultEntitlementDays = input.DefaultEntitlementDays!.Value }, input.ConcurrencyStamp));

    [Authorize(HrPermissions.SettingsPermissions.HrEvaluationAlert)]
    public virtual async Task<SystemSettingsHrEvaluationAlertDto> GetHrEvaluationAlertAsync() => ToDto(await _store.GetAsync<HrEvaluationAlertSettings>());

    /// <summary>Applies from the next cycle; computed cycles are not recomputed (decision 2026-11-11).</summary>
    [Authorize(HrPermissions.SettingsPermissions.HrEvaluationAlert)]
    public virtual async Task<SystemSettingsHrEvaluationAlertDto> UpdateHrEvaluationAlertAsync(SystemSettingsHrEvaluationAlertUpdateInputDto input) =>
        ToDto(await _store.SetAsync(new HrEvaluationAlertSettings
        {
            Threshold = input.Threshold!.Value,
            WindowWeeks = input.WindowWeeks!.Value,
            MinEvaluations = input.MinEvaluations!.Value
        }, input.ConcurrencyStamp));

    [Authorize(HrPermissions.SettingsPermissions.Finance)]
    public virtual async Task<SystemSettingsFinanceDto> GetFinanceAsync() => ToDto(await _store.GetAsync<FinanceSettings>());

    [Authorize(HrPermissions.SettingsPermissions.Finance)]
    public virtual async Task<SystemSettingsFinanceDto> UpdateFinanceAsync(SystemSettingsFinanceUpdateInputDto input) =>
        ToDto(await _store.SetAsync(new FinanceSettings { PayrollApproverRole = input.PayrollApproverRole }, input.ConcurrencyStamp));

    private static SystemSettingsRemindersDto ToDto(SettingsSnapshot<ReminderSettings> s) => new()
    {
        MonthEndOpenTime = s.Value.MonthEndOpenTime,
        MonthlySubmissionWorkingDays = s.Value.MonthlySubmissionWorkingDays,
        MonthlyReminderIntervalWorkingDays = s.Value.MonthlyReminderIntervalWorkingDays,
        MonthlyEscalationWorkingDay = s.Value.MonthlyEscalationWorkingDay,
        ApprovalReminderIntervalWorkingDays = s.Value.ApprovalReminderIntervalWorkingDays,
        EvaluationMinWorkedDays = s.Value.EvaluationMinWorkedDays,
        ConcurrencyStamp = s.ConcurrencyStamp,
        LastModificationTime = s.LastModificationTime
    };

    private static SystemSettingsContractsDto ToDto(SettingsSnapshot<ContractSettings> s) => new()
    {
        AlertDays = s.Value.AlertDays,
        ConcurrencyStamp = s.ConcurrencyStamp,
        LastModificationTime = s.LastModificationTime
    };

    private static SystemSettingsComplaintsDto ToDto(SettingsSnapshot<ComplaintSettings> s) => new()
    {
        DelayDeadlineWorkingDays = s.Value.DelayDeadlineWorkingDays,
        OverdueReminderIntervalWorkingDays = s.Value.OverdueReminderIntervalWorkingDays,
        ConcurrencyStamp = s.ConcurrencyStamp,
        LastModificationTime = s.LastModificationTime
    };

    private static SystemSettingsHourlyLeaveDto ToDto(SettingsSnapshot<HourlyLeaveSettings> s) => new()
    {
        MonthlyAllowedHours = s.Value.MonthlyAllowedHours,
        HoursPerDay = s.Value.HoursPerDay,
        ConcurrencyStamp = s.ConcurrencyStamp,
        LastModificationTime = s.LastModificationTime
    };

    private static SystemSettingsAnnualLeaveDto ToDto(SettingsSnapshot<AnnualLeaveSettings> s) => new()
    {
        DefaultEntitlementDays = s.Value.DefaultEntitlementDays,
        ConcurrencyStamp = s.ConcurrencyStamp,
        LastModificationTime = s.LastModificationTime
    };

    private static SystemSettingsHrEvaluationAlertDto ToDto(SettingsSnapshot<HrEvaluationAlertSettings> s) => new()
    {
        Threshold = s.Value.Threshold,
        WindowWeeks = s.Value.WindowWeeks,
        MinEvaluations = s.Value.MinEvaluations,
        ConcurrencyStamp = s.ConcurrencyStamp,
        LastModificationTime = s.LastModificationTime
    };

    private static SystemSettingsFinanceDto ToDto(SettingsSnapshot<FinanceSettings> s) => new()
    {
        PayrollApproverRole = s.Value.PayrollApproverRole,
        ConcurrencyStamp = s.ConcurrencyStamp,
        LastModificationTime = s.LastModificationTime
    };
}
