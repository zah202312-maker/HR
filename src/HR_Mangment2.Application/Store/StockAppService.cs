using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace HR_Mangment2.Store;

[Authorize]
public class StockAppService : HR_Mangment2AppService, IStockAppService
{
    private const string ReceiveOperation = "Stock_Receive";
    private const string AdjustOperation = "Stock_Adjust";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["creationTime"] = nameof(StockMovement.CreationTime),
        ["type"] = nameof(StockMovement.Type),
        ["quantity"] = nameof(StockMovement.Quantity)
    };

    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<StockMovement, Guid> _movements;
    private readonly IRepository<StockIssue, Guid> _issues;
    private readonly StockLedger _ledger;
    private readonly IStockExpenseGateway _expenses;
    private readonly ItemReferenceReader _itemRefs;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;
    private readonly ICompanyClock _companyClock;

    public StockAppService(
        IRepository<Item, Guid> items,
        IRepository<StockMovement, Guid> movements,
        IRepository<StockIssue, Guid> issues,
        StockLedger ledger,
        IStockExpenseGateway expenses,
        ItemReferenceReader itemRefs,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency,
        ICompanyClock companyClock)
    {
        _items = items;
        _movements = movements;
        _issues = issues;
        _ledger = ledger;
        _expenses = expenses;
        _itemRefs = itemRefs;
        _references = references;
        _idempotency = idempotency;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.StorePermissions.StockReceive)]
    public virtual async Task<StockMovementDto> ReceiveAsync(StockReceiveInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(ReceiveOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return (await ToDtosAsync(new[] { await _movements.GetAsync(earlier.Value) })).Single();
        }

        var item = await _items.GetAsync(input.ItemId!.Value);
        if (!item.IsActive)
        {
            throw new BusinessException(HrErrorCodes.Stock.ItemInactive);
        }

        Guid? requestId = null;
        Guid? employeeId = null;
        if (input.StockIssueId != null)
        {
            if (input.TotalCost == null)
            {
                throw new BusinessException(HrErrorCodes.Stock.PurchaseCostRequired);
            }

            var issue = await _issues.GetAsync(input.StockIssueId.Value);
            if (issue.Status != StockIssueStatus.Purchasing)
            {
                throw new BusinessException(HrErrorCodes.Stock.IssueNotPurchasing);
            }

            if (issue.ItemId != item.Id)
            {
                throw new BusinessException(HrErrorCodes.Stock.IssueItemMismatch);
            }

            requestId = issue.RequestId;
            employeeId = issue.EmployeeId;
        }

        var movement = await _ledger.RecordAsync(item, new StockMovementDraft(
            StockMovementType.Receive,
            input.Quantity!.Value,
            CurrentUser.GetId(),
            input.Note,
            RequestId: requestId,
            StockIssueId: input.StockIssueId,
            EmployeeId: employeeId,
            TotalCost: input.TotalCost));

        if (movement.TotalCost != null)
        {
            await _expenses.OnPurchaseRecordedAsync(movement);
        }

        await _idempotency.SaveAsync(ReceiveOperation, idempotencyKey, input, movement.Id);
        return (await ToDtosAsync(new[] { movement })).Single();
    }

    [Authorize(HrPermissions.StorePermissions.StockAdjust)]
    public virtual async Task<StockMovementDto> AdjustAsync(StockAdjustInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(AdjustOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return (await ToDtosAsync(new[] { await _movements.GetAsync(earlier.Value) })).Single();
        }

        if (input.Quantity == 0)
        {
            throw new BusinessException(HrErrorCodes.Stock.ZeroQuantity);
        }

        var item = await _items.GetAsync(input.ItemId!.Value);

        StockMovement? original = null;
        if (input.ReversesMovementId != null)
        {
            original = await _movements.FindAsync(input.ReversesMovementId.Value);

            // The contract gives no code for a reference that this movement cannot reverse.
            if (original == null || original.ItemId != item.Id || original.Quantity != -input.Quantity)
            {
                throw new AbpValidationException(
                    "Invalid reversal.",
                    new List<ValidationResult> { new("The movement must be of this item, with the opposite quantity.", new[] { "reversesMovementId" }) });
            }

            var originalId = original.Id;
            if (await _movements.AnyAsync(x => x.ReversesMovementId == originalId))
            {
                throw new BusinessException(HrErrorCodes.Stock.MovementAlreadyReversed);
            }
        }

        var movement = await _ledger.RecordAsync(item, new StockMovementDraft(
            StockMovementType.Adjustment,
            input.Quantity!.Value,
            CurrentUser.GetId(),
            input.Reason,
            ReversesMovementId: original?.Id));

        if (original?.TotalCost != null)
        {
            await _expenses.OnPurchaseReversedAsync(original, movement);
        }

        await _idempotency.SaveAsync(AdjustOperation, idempotencyKey, input, movement.Id);
        return (await ToDtosAsync(new[] { movement })).Single();
    }

    [Authorize(HrPermissions.StorePermissions.StockView)]
    public virtual async Task<StockMovementPagedResultDto> GetMovementsAsync(StockMovementListInput input)
    {
        var query = (await _movements.GetQueryableAsync())
            .WhereIf(input.ItemId != null, x => x.ItemId == input.ItemId)
            .WhereIf(input.Type != null, x => x.Type == input.Type);

        if (input.FromDate != null)
        {
            var from = await _companyClock.StartOfDayUtcAsync(input.FromDate.Value);
            query = query.Where(x => x.CreationTime >= from);
        }

        if (input.ToDate != null)
        {
            var before = await _companyClock.StartOfDayUtcAsync(input.ToDate.Value.AddDays(1));
            query = query.Where(x => x.CreationTime < before);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(StockMovement.CreationTime) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new StockMovementPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    /// <summary>The cost is deliberately absent: QUESTIONS.md item 29, the store officer enters it and does not see it again.</summary>
    private async Task<List<StockMovementDto>> ToDtosAsync(IReadOnlyCollection<StockMovement> rows)
    {
        var items = await _itemRefs.GetRefsAsync(rows.Select(x => x.ItemId));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.PerformedByUserId));
        var employees = await _references.GetRefsAsync(rows.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value));

        return rows.Select(x => new StockMovementDto
        {
            Id = x.Id,
            Item = items[x.ItemId],
            Type = x.Type,
            Quantity = x.Quantity,
            BalanceAfter = x.BalanceAfter,
            RequestId = x.RequestId,
            StockIssueId = x.StockIssueId,
            CustodyId = x.CustodyId,
            ReversesMovementId = x.ReversesMovementId,
            Employee = x.EmployeeId == null ? null : employees.GetValueOrDefault(x.EmployeeId.Value),
            PerformedBy = users[x.PerformedByUserId],
            Note = x.Note,
            CreationTime = x.CreationTime
        }).ToList();
    }
}

public class StockReceiveInputDtoValidator : AbstractValidator<StockReceiveInputDto>
{
    public StockReceiveInputDtoValidator()
    {
        RuleFor(x => x.ItemId).NotNull();
        RuleFor(x => x.Quantity).NotNull().GreaterThan(0).PrecisionScale(StoreConsts.QuantityPrecision, StoreConsts.QuantityScale, true);
        RuleFor(x => x.TotalCost).GreaterThanOrEqualTo(0).PrecisionScale(StoreConsts.CostPrecision, StoreConsts.CostScale, true);
        RuleFor(x => x.Note).MaximumLength(StoreConsts.MaxNoteLength);
    }
}

public class StockAdjustInputDtoValidator : AbstractValidator<StockAdjustInputDto>
{
    public StockAdjustInputDtoValidator()
    {
        RuleFor(x => x.ItemId).NotNull();
        RuleFor(x => x.Quantity).NotNull().PrecisionScale(StoreConsts.QuantityPrecision, StoreConsts.QuantityScale, true);
        RuleFor(x => x.Reason).NotNull().Length(1, StoreConsts.MaxNoteLength);
    }
}
