using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Store;

[Authorize]
public class StockIssueAppService : HR_Mangment2AppService, IStockIssueAppService
{
    private const string DeliverOperation = "StockIssue_Deliver";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["creationTime"] = nameof(StockIssue.CreationTime),
        ["status"] = nameof(StockIssue.Status),
        ["quantity"] = nameof(StockIssue.Quantity)
    };

    private readonly IRepository<StockIssue, Guid> _issues;
    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<Custody, Guid> _custodies;
    private readonly StockLedger _ledger;
    private readonly RequestWorkflowService _workflow;
    private readonly CustodyDepartmentScope _scope;
    private readonly ItemReferenceReader _itemRefs;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdempotencyStore _idempotency;

    public StockIssueAppService(
        IRepository<StockIssue, Guid> issues,
        IRepository<Item, Guid> items,
        IRepository<Request, Guid> requests,
        IRepository<Employee, Guid> employees,
        IRepository<Custody, Guid> custodies,
        StockLedger ledger,
        RequestWorkflowService workflow,
        CustodyDepartmentScope scope,
        ItemReferenceReader itemRefs,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications,
        IdempotencyStore idempotency)
    {
        _issues = issues;
        _items = items;
        _requests = requests;
        _employees = employees;
        _custodies = custodies;
        _ledger = ledger;
        _workflow = workflow;
        _scope = scope;
        _itemRefs = itemRefs;
        _references = references;
        _notifications = notifications;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.StorePermissions.IssueView)]
    public virtual async Task<StockIssuePagedResultDto> GetListAsync(StockIssueListInput input)
    {
        var query = (await _issues.GetQueryableAsync())
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.ItemId != null, x => x.ItemId == input.ItemId)
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(StockIssue.CreationTime) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new StockIssuePagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    [Authorize(HrPermissions.StorePermissions.IssueView)]
    public virtual async Task<StockIssueDto> GetAsync(Guid id) => await ToDtoAsync(await _issues.GetAsync(id));

    [Authorize(HrPermissions.StorePermissions.IssueProcess)]
    public virtual async Task<StockIssueDto> AcknowledgeAsync(Guid id)
    {
        var issue = await _issues.GetAsync(id);
        CheckStatus(issue, StockIssueStatus.PendingAcknowledgement);

        issue.Acknowledge(CurrentUser.GetId(), Clock.Now);
        await _issues.UpdateAsync(issue, autoSave: true);

        await _workflow.RecordStoreProgressAsync(await LoadRequestAsync(issue.RequestId), RequestTimelineActions.StockIssueAcknowledged, CurrentUser.GetId(), null);
        return await ToDtoAsync(issue);
    }

    [Authorize(HrPermissions.StorePermissions.IssueProcess)]
    public virtual async Task<StockIssueDto> MarkPurchasingAsync(Guid id, NoteInputDto input)
    {
        var issue = await _issues.GetAsync(id);
        CheckStatus(issue, StockIssueStatus.Acknowledged);

        issue.MarkPurchasing(Clock.Now);
        await _issues.UpdateAsync(issue, autoSave: true);

        await _workflow.RecordStoreProgressAsync(await LoadRequestAsync(issue.RequestId), RequestTimelineActions.StockIssuePurchasing, CurrentUser.GetId(), input.Note);
        return await ToDtoAsync(issue);
    }

    [Authorize(HrPermissions.StorePermissions.IssueDeliver)]
    public virtual async Task<StockIssueDto> DeliverAsync(Guid id, StockIssueDeliverInputDto input, Guid? idempotencyKey)
    {
        var fingerprint = new { id, input.CustodyKind, input.Note };
        var earlier = await _idempotency.FindResourceAsync(DeliverOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return await ToDtoAsync(await _issues.GetAsync(earlier.Value));
        }

        var issue = await _issues.GetAsync(id);
        CheckStatus(issue, StockIssueStatus.Acknowledged, StockIssueStatus.Purchasing);

        var me = CurrentUser.GetId();
        var now = Clock.Now;
        var item = await _items.GetAsync(issue.ItemId);
        var owner = await _employees.GetAsync(issue.EmployeeId);
        var request = await LoadRequestAsync(issue.RequestId);

        // Checked first, because a custody row must never exist for a delivery that fails.
        if (item.QuantityOnHand < issue.Quantity)
        {
            throw new BusinessException(HrErrorCodes.Stock.InsufficientStock);
        }

        Custody? custody = null;
        if (item.Kind == ItemKind.Returnable)
        {
            var shared = input.CustodyKind == CustodyKind.Shared;
            custody = await _custodies.InsertAsync(
                new Custody(
                    GuidGenerator.Create(),
                    CurrentTenant.Id,
                    item.Id,
                    shared ? CustodyKind.Shared : CustodyKind.Personal,
                    shared ? null : owner.Id,
                    shared ? owner.DepartmentId : null,
                    issue.Quantity,
                    now,
                    me,
                    request.Id,
                    issue.Id,
                    input.Note),
                autoSave: true);
        }

        var movement = await _ledger.RecordAsync(item, new StockMovementDraft(
            StockMovementType.Issue,
            -issue.Quantity,
            me,
            input.Note,
            RequestId: request.Id,
            StockIssueId: issue.Id,
            CustodyId: custody?.Id,
            EmployeeId: owner.Id));

        issue.Deliver(me, now, movement.Id, custody?.Id);
        await _issues.UpdateAsync(issue, autoSave: true);
        await _workflow.CompleteAtStoreAsync(request, me, input.Note);

        if (custody != null)
        {
            var holders = custody.Kind == CustodyKind.Personal
                ? (owner.UserId == null ? new List<Guid>() : new List<Guid> { owner.UserId.Value })
                : await _scope.GetDepartmentManagerUserIdsAsync(custody.DepartmentId!.Value);

            foreach (var holder in holders)
            {
                await _notifications.PublishAsync(holder, NotificationTemplateCode.CustodyAssigned, NotificationResourceType.Custody, custody.Id);
            }
        }

        await _idempotency.SaveAsync(DeliverOperation, idempotencyKey, fingerprint, issue.Id);
        return await ToDtoAsync(issue);
    }

    private static void CheckStatus(StockIssue issue, params StockIssueStatus[] allowed)
    {
        if (!allowed.Contains(issue.Status))
        {
            throw new BusinessException(HrErrorCodes.Stock.IssueInvalidState);
        }
    }

    private async Task<Request> LoadRequestAsync(Guid id)
    {
        var query = (await _requests.WithDetailsAsync(x => x.Steps, x => x.Timeline)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstOrDefaultAsync(query) ?? throw new EntityNotFoundException(typeof(Request), id);
    }

    private async Task<StockIssueDto> ToDtoAsync(StockIssue issue) => (await ToDtosAsync(new[] { issue })).Single();

    private async Task<List<StockIssueDto>> ToDtosAsync(IReadOnlyCollection<StockIssue> rows)
    {
        var items = await _itemRefs.GetAsync(rows.Select(x => x.ItemId));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var users = await _references.GetUserRefsAsync(rows
            .SelectMany(x => new[] { x.AcknowledgedByUserId, x.DeliveredByUserId })
            .Where(x => x != null)
            .Select(x => x!.Value));

        return rows.Select(x => new StockIssueDto
        {
            Id = x.Id,
            RequestId = x.RequestId,
            Item = ItemReferenceReader.ToRef(items[x.ItemId]),
            Quantity = x.Quantity,
            QuantityOnHand = items[x.ItemId].QuantityOnHand,
            IsAvailable = items[x.ItemId].QuantityOnHand >= x.Quantity,
            Status = x.Status,
            StatusLabel = L[StoreLabels.IssueStatus(x.Status)],
            Employee = employees[x.EmployeeId],
            AcknowledgedAt = x.AcknowledgedAt,
            AcknowledgedBy = x.AcknowledgedByUserId == null ? null : users[x.AcknowledgedByUserId.Value],
            PurchasingAt = x.PurchasingAt,
            DeliveredAt = x.DeliveredAt,
            DeliveredBy = x.DeliveredByUserId == null ? null : users[x.DeliveredByUserId.Value],
            CustodyId = x.CustodyId,
            StockMovementId = x.StockMovementId,
            CreationTime = x.CreationTime
        }).ToList();
    }
}

public class StockIssueDeliverInputDtoValidator : AbstractValidator<StockIssueDeliverInputDto>
{
    public StockIssueDeliverInputDtoValidator()
    {
        RuleFor(x => x.Note).MaximumLength(StoreConsts.MaxNoteLength);
    }
}
