using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Requests;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Store;

public record StockMovementDraft(
    StockMovementType Type,
    decimal SignedQuantity,
    Guid PerformedByUserId,
    string? Note = null,
    Guid? RequestId = null,
    Guid? StockIssueId = null,
    Guid? CustodyId = null,
    Guid? ReversesMovementId = null,
    Guid? EmployeeId = null,
    decimal? TotalCost = null,
    decimal? UnitCost = null,
    Guid? PurchaseId = null,
    Guid? IssueVoucherId = null,
    Guid? TeamId = null,
    string? DocumentNumber = null);

/// <summary>
/// The only writer of an item's balance and average cost. The balance and its movement change together, and it never
/// goes negative. A movement is valued at <see cref="StockMovementDraft.UnitCost"/>, or at TotalCost / quantity, or else
/// at the item's average cost, so an outflow leaves the average where it was.
/// </summary>
public class StockLedger : ITransientDependency
{
    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<StockMovement, Guid> _movements;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public StockLedger(IRepository<Item, Guid> items, IRepository<StockMovement, Guid> movements, IGuidGenerator guidGenerator, ICurrentTenant currentTenant)
    {
        _items = items;
        _movements = movements;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public virtual async Task<StockMovement> RecordAsync(Item item, StockMovementDraft draft)
    {
        if (item.QuantityOnHand + draft.SignedQuantity < 0)
        {
            throw new BusinessException(HrErrorCodes.Stock.InsufficientStock);
        }

        var unitCost = draft.UnitCost
            ?? (draft.TotalCost != null && draft.SignedQuantity != 0 ? Math.Round(draft.TotalCost.Value / Math.Abs(draft.SignedQuantity), StoreConsts.CostScale) : item.AverageUnitCost);
        // An entered total stays exact, so the accountant's expense matches the paper receipt to the last unit.
        var totalCost = draft.TotalCost != null
            ? Math.Sign(draft.SignedQuantity) * Math.Abs(draft.TotalCost.Value)
            : Math.Round(draft.SignedQuantity * unitCost, StoreConsts.CostScale);

        item.ApplyMovement(draft.SignedQuantity, unitCost);
        await _items.UpdateAsync(item, autoSave: true);

        return await _movements.InsertAsync(
            new StockMovement(
                _guidGenerator.Create(),
                _currentTenant.Id,
                item.Id,
                draft.Type,
                draft.SignedQuantity,
                item.QuantityOnHand,
                draft.PerformedByUserId,
                draft.Note,
                draft.RequestId,
                draft.StockIssueId,
                draft.CustodyId,
                draft.ReversesMovementId,
                draft.EmployeeId,
                totalCost,
                unitCost,
                draft.PurchaseId,
                draft.IssueVoucherId,
                draft.TeamId,
                draft.DocumentNumber),
            autoSave: true);
    }
}

/// <summary>Where a purchase cost goes. The finance module implements this in release 3 by creating the accountant's expense.</summary>
public interface IStockExpenseGateway
{
    /// <summary>One expense for this movement's cost. <paramref name="description"/> names the receipt, if any.</summary>
    Task OnPurchaseRecordedAsync(StockMovement movement, string? description = null);

    Task OnPurchaseReversedAsync(StockMovement original, StockMovement reversal);
}

public class NullStockExpenseGateway : IStockExpenseGateway, ITransientDependency
{
    public Task OnPurchaseRecordedAsync(StockMovement movement, string? description = null) => Task.CompletedTask;

    public Task OnPurchaseReversedAsync(StockMovement original, StockMovement reversal) => Task.CompletedTask;
}

/// <summary>QUESTIONS.md item 31: which managers answer for a department's shared custody. Any department manager with staff there.</summary>
public class CustodyDepartmentScope : ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _users;
    private readonly IAsyncQueryableExecuter _executer;

    public CustodyDepartmentScope(IRepository<Employee, Guid> employees, IdentityUserManager userManager, IIdentityUserRepository users, IAsyncQueryableExecuter executer)
    {
        _employees = employees;
        _userManager = userManager;
        _users = users;
        _executer = executer;
    }

    /// <summary>The departments of the user's direct reports, and none unless the user holds the department manager role.</summary>
    public virtual async Task<List<Guid>> GetManagedDepartmentIdsAsync(Guid userId)
    {
        if (!(await _users.GetRoleNamesAsync(userId)).Contains(HrRoles.DepartmentManager))
        {
            return new List<Guid>();
        }

        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        if (me == null)
        {
            return new List<Guid>();
        }

        return await _executer.ToListAsync((await _employees.GetQueryableAsync())
            .Where(x => x.ManagerEmployeeId == me.Id && x.Status != EmployeeStatus.Ended)
            .Select(x => x.DepartmentId)
            .Distinct());
    }

    public virtual async Task<List<Guid>> GetDepartmentManagerUserIdsAsync(Guid departmentId)
    {
        var employees = await _employees.GetQueryableAsync();
        var managerUserIds = await _executer.ToListAsync(
            from staff in employees
            join manager in employees on staff.ManagerEmployeeId equals manager.Id
            where staff.DepartmentId == departmentId && staff.Status != EmployeeStatus.Ended && manager.UserId != null
            select manager.UserId!.Value);

        var result = new List<Guid>();
        foreach (var userId in managerUserIds.Distinct())
        {
            if ((await _users.GetRoleNamesAsync(userId)).Contains(HrRoles.DepartmentManager))
            {
                result.Add(userId);
            }
        }

        return result;
    }

    public virtual async Task<List<Guid>> GetStoreOfficerUserIdsAsync() =>
        (await _userManager.GetUsersInRoleAsync(HrRoles.StoreOfficer)).Where(x => x.IsActive).Select(x => x.Id).ToList();
}

/// <summary>The side of the store that the requests module calls when a store request moves.</summary>
public class StoreRequestStockGateway : IRequestStockGateway, ITransientDependency
{
    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<StockIssue, Guid> _issues;
    private readonly CustodyDepartmentScope _scope;
    private readonly HrNotificationPublisher _notifications;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public StoreRequestStockGateway(
        IRepository<Item, Guid> items,
        IRepository<StockIssue, Guid> issues,
        CustodyDepartmentScope scope,
        HrNotificationPublisher notifications,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _items = items;
        _issues = issues;
        _scope = scope;
        _notifications = notifications;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public virtual Task<bool> IsItemActiveAsync(Guid itemId) => _items.AnyAsync(x => x.Id == itemId && x.IsActive);

    public virtual async Task CreateIssueAsync(Request request)
    {
        var issue = await _issues.InsertAsync(
            new StockIssue(_guidGenerator.Create(), _currentTenant.Id, request.Id, request.ItemId!.Value, request.EmployeeId, request.Quantity!.Value),
            autoSave: true);

        foreach (var officer in await _scope.GetStoreOfficerUserIdsAsync())
        {
            await _notifications.PublishAsync(officer, NotificationTemplateCode.StockIssuePending, NotificationResourceType.StockIssue, issue.Id);
        }
    }

    public virtual async Task<bool> CanCancelIssueAsync(Request request)
    {
        var issue = await _issues.FirstOrDefaultAsync(x => x.RequestId == request.Id);
        return issue is null or { Status: StockIssueStatus.PendingAcknowledgement };
    }

    public virtual async Task CancelIssueAsync(Request request)
    {
        var issue = await _issues.FirstOrDefaultAsync(x => x.RequestId == request.Id);
        if (issue != null)
        {
            issue.Cancel();
            await _issues.UpdateAsync(issue, autoSave: true);
        }
    }
}
