using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Warnings;

/// <summary>A recommendation until HR decides. Never deleted: correction is a documented decision.</summary>
[Audited]
public class Warning : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxDescriptionLength = 4000;
    public const int MaxNoteLength = 2000;
    public const int MaxAttachments = 10;
    public const int ResponseWorkingDays = 3;
    public const int ObjectionWorkingDays = 5;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public WarningLevel RecommendedLevel { get; private set; }
    public DateOnly IncidentDate { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid RecommendedByUserId { get; private set; }
    public WarningRecommenderRole RecommenderRole { get; private set; }
    public WarningStatus Status { get; private set; }
    public ICollection<WarningAttachment> Attachments { get; private set; } = new List<WarningAttachment>();

    public DateTime? NotifiedAt { get; private set; }
    public DateOnly? ResponseDueDate { get; private set; }

    public string? ResponseContent { get; private set; }
    public DateTime? ResponseSubmittedAt { get; private set; }

    public WarningDecisionOutcome? DecisionOutcome { get; private set; }
    public WarningLevel? IssuedLevel { get; private set; }
    public string? DecisionNote { get; private set; }
    public Guid? DecidedByUserId { get; private set; }
    public DateTime? DecidedAt { get; private set; }
    public DateOnly? ObjectionDueDate { get; private set; }

    public string? ObjectionContent { get; private set; }
    public DateTime? ObjectionSubmittedAt { get; private set; }
    public WarningObjectionOutcome? ObjectionOutcome { get; private set; }
    public string? ObjectionResolutionNote { get; private set; }
    public Guid? ObjectionResolvedByUserId { get; private set; }
    public DateTime? ObjectionResolvedAt { get; private set; }

    protected Warning()
    {
    }

    public Warning(
        Guid id,
        Guid? tenantId,
        Guid employeeId,
        WarningLevel recommendedLevel,
        DateOnly incidentDate,
        string description,
        Guid recommendedByUserId,
        WarningRecommenderRole recommenderRole)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        RecommendedLevel = recommendedLevel;
        IncidentDate = incidentDate;
        Description = description;
        RecommendedByUserId = recommendedByUserId;
        RecommenderRole = recommenderRole;
        Status = WarningStatus.Recommended;
    }

    public void Notify(DateTime now, DateOnly responseDueDate)
    {
        NotifiedAt = now;
        ResponseDueDate = responseDueDate;
        Status = WarningStatus.Notified;
    }

    public void Respond(string content, DateTime now)
    {
        ResponseContent = content;
        ResponseSubmittedAt = now;
        Status = WarningStatus.Responded;
    }

    public void ExpireResponseWindow() => Status = WarningStatus.ResponseExpired;

    public void Decide(WarningDecisionOutcome outcome, WarningLevel? issuedLevel, string note, Guid userId, DateTime now, DateOnly? objectionDueDate)
    {
        DecisionOutcome = outcome;
        IssuedLevel = issuedLevel;
        DecisionNote = note;
        DecidedByUserId = userId;
        DecidedAt = now;
        ObjectionDueDate = objectionDueDate;
        Status = outcome == WarningDecisionOutcome.Issue ? WarningStatus.Issued : WarningStatus.Dismissed;
    }

    public void Object(string content, DateTime now)
    {
        ObjectionContent = content;
        ObjectionSubmittedAt = now;
        Status = WarningStatus.Objected;
    }

    public void ResolveObjection(WarningObjectionOutcome outcome, string note, Guid userId, DateTime now)
    {
        ObjectionOutcome = outcome;
        ObjectionResolutionNote = note;
        ObjectionResolvedByUserId = userId;
        ObjectionResolvedAt = now;
        Status = outcome == WarningObjectionOutcome.Uphold ? WarningStatus.ObjectionUpheld : WarningStatus.ObjectionOverturned;
    }
}

public class WarningAttachment : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid WarningId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string BlobName { get; private set; } = null!;
    public DateTime CreationTime { get; private set; }

    protected WarningAttachment()
    {
    }

    public WarningAttachment(Guid id, Guid? tenantId, Guid warningId, string fileName, string contentType, long sizeBytes, string blobName, DateTime creationTime)
        : base(id)
    {
        TenantId = tenantId;
        WarningId = warningId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        BlobName = blobName;
        CreationTime = creationTime;
    }
}

/// <summary>Moves warnings whose reply window ended. Safe to repeat: only rows still in notified are touched.</summary>
public class WarningDeadlineService : ITransientDependency
{
    private readonly IRepository<Warning, Guid> _warnings;
    private readonly ICompanyClock _companyClock;
    private readonly IdentityUserManager _userManager;
    private readonly HrNotificationPublisher _notifications;

    public WarningDeadlineService(
        IRepository<Warning, Guid> warnings,
        ICompanyClock companyClock,
        IdentityUserManager userManager,
        HrNotificationPublisher notifications)
    {
        _warnings = warnings;
        _companyClock = companyClock;
        _userManager = userManager;
        _notifications = notifications;
    }

    public virtual async Task ProcessAsync()
    {
        var today = await _companyClock.GetTodayAsync();
        var expired = await _warnings.GetListAsync(x => x.Status == WarningStatus.Notified && x.ResponseDueDate < today);
        if (expired.Count == 0)
        {
            return;
        }

        var hrUsers = (await _userManager.GetUsersInRoleAsync(HrRoles.HrManager)).Where(x => x.IsActive).Select(x => x.Id).ToList();
        foreach (var warning in expired)
        {
            warning.ExpireResponseWindow();
            await _warnings.UpdateAsync(warning);
            foreach (var userId in hrUsers)
            {
                await _notifications.PublishAsync(userId, NotificationTemplateCode.WarningReadyForDecision, NotificationResourceType.Warning, warning.Id);
            }
        }
    }
}
