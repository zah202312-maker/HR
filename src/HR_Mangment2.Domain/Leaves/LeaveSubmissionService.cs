using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Requests;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace HR_Mangment2.Leaves;

/// <param name="ManagerStepApprovedByUserId">Set when a memo is converted: its conversion counts as the manager's approval.</param>
public record LeaveSubmission(
    Employee Owner,
    LeavePlan Plan,
    Guid SubmittedByUserId,
    bool IsOnBehalf,
    string? Notes,
    IReadOnlyCollection<LeaveAttachment> Attachments,
    Guid? SourceMemoId = null,
    Guid? ManagerStepApprovedByUserId = null);

/// <summary>Turns a checked plan into a leave request with its held balance, for direct submission and memo conversion alike.</summary>
public class LeaveSubmissionService : ITransientDependency
{
    private readonly IRepository<LeaveRequest, Guid> _leaves;
    private readonly IRepository<LeaveAttachment, Guid> _attachments;
    private readonly IRepository<RequestType, Guid> _requestTypes;
    private readonly RequestWorkflowService _workflow;
    private readonly LeaveBalanceManager _balances;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public LeaveSubmissionService(
        IRepository<LeaveRequest, Guid> leaves,
        IRepository<LeaveAttachment, Guid> attachments,
        IRepository<RequestType, Guid> requestTypes,
        RequestWorkflowService workflow,
        LeaveBalanceManager balances,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IClock clock)
    {
        _leaves = leaves;
        _attachments = attachments;
        _requestTypes = requestTypes;
        _workflow = workflow;
        _balances = balances;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _clock = clock;
    }

    public virtual async Task<LeaveRequest> SubmitAsync(LeaveSubmission submission)
    {
        var plan = submission.Plan;
        var type = await _requestTypes.FirstAsync(x => x.Code == RequestTypeCodes.Leave);
        var (summaryAr, summaryEn) = BuildSummary(plan);

        var request = await _workflow.SubmitAsync(new RequestSubmission(
            type,
            submission.Owner,
            submission.SubmittedByUserId,
            submission.IsOnBehalf,
            null,
            summaryAr,
            summaryEn,
            SourceMemoId: submission.SourceMemoId,
            ManagerStepApprovedByUserId: submission.ManagerStepApprovedByUserId));

        var leave = new LeaveRequest(request.Id, _currentTenant.Id, submission.Owner.Id, plan, submission.Notes, _clock.Now);
        foreach (var segment in plan.Segments)
        {
            leave.Segments.Add(new LeaveRequestSegment(
                _guidGenerator.Create(),
                _currentTenant.Id,
                leave.Id,
                segment.YearStart,
                segment.YearEnd,
                segment.StartDate,
                segment.EndDate,
                segment.WorkingDays,
                segment.FromCarriedDays,
                segment.FromEntitlementDays));
        }

        await _leaves.InsertAsync(leave, autoSave: true);

        foreach (var attachment in submission.Attachments)
        {
            attachment.AttachTo(leave.Id);
            await _attachments.UpdateAsync(attachment, autoSave: true);
        }

        await _balances.HoldAsync(submission.Owner, plan, leave.Id);
        return leave;
    }

    private static (string Ar, string En) BuildSummary(LeavePlan plan)
    {
        var typeAr = plan.OtherLeaveType?.NameAr;
        var typeEn = plan.OtherLeaveType?.NameEn;

        return plan.Kind switch
        {
            LeaveKind.Hourly => (
                $"إجازة زمنية يوم {plan.Date:yyyy-MM-dd} من {plan.FromTime:HH:mm} إلى {plan.ToTime:HH:mm}",
                $"Hourly leave on {plan.Date:yyyy-MM-dd} from {plan.FromTime:HH:mm} to {plan.ToTime:HH:mm}"),
            LeaveKind.Annual => (
                $"إجازة من الرصيد من {plan.StartDate:yyyy-MM-dd} إلى {plan.EndDate:yyyy-MM-dd} ({plan.WorkingDays:0.##} يوم)",
                $"Annual leave from {plan.StartDate:yyyy-MM-dd} to {plan.EndDate:yyyy-MM-dd} ({plan.WorkingDays:0.##} days)"),
            _ => (
                $"{typeAr} من {plan.StartDate:yyyy-MM-dd} إلى {plan.EndDate:yyyy-MM-dd} ({plan.WorkingDays:0.##} يوم)",
                $"{typeEn} from {plan.StartDate:yyyy-MM-dd} to {plan.EndDate:yyyy-MM-dd} ({plan.WorkingDays:0.##} days)")
        };
    }
}
