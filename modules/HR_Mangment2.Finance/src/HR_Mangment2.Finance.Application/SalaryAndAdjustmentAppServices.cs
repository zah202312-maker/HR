using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Finance;

[Authorize]
public class SalaryProfileAppService : FinanceAppServiceBase, ISalaryProfileAppService
{
    private readonly IRepository<SalaryProfile, Guid> _profiles;
    private readonly SalaryProfileReader _reader;
    private readonly ICompanyClock _companyClock;

    public SalaryProfileAppService(IRepository<SalaryProfile, Guid> profiles, SalaryProfileReader reader, ICompanyClock companyClock)
    {
        _profiles = profiles;
        _reader = reader;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.FinancePermissions.SalaryProfileRead)]
    public virtual async Task<SalaryProfileListItemPagedResultDto> GetListAsync(SalaryProfileListInput input)
    {
        var today = await _companyClock.GetTodayAsync();
        var filter = input.Filter?.Trim();
        var query = (await Employees.GetQueryableAsync())
            .Where(x => x.Status != EmployeeStatus.Ended)
            .WhereIf(input.DepartmentId != null, x => x.DepartmentId == input.DepartmentId)
            .WhereIf(!string.IsNullOrEmpty(filter), x => x.FullNameAr.Contains(filter!) || (x.FullNameEn != null && x.FullNameEn.Contains(filter!)));

        if (input.HasProfile != null)
        {
            var withProfile = (await AsyncExecuter.ToListAsync((await _profiles.GetQueryableAsync())
                .Where(x => x.EffectiveFrom <= today)
                .Select(x => x.EmployeeId)
                .Distinct()));
            query = input.HasProfile.Value ? query.Where(x => withProfile.Contains(x.Id)) : query.Where(x => !withProfile.Contains(x.Id));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.FullNameAr).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var references = await References.ToRefsAsync(rows);
        var profiles = await _reader.GetEffectiveAsync(rows.Select(x => x.Id).ToList(), today);

        await LogReadAsync(nameof(SalaryProfile));

        return new SalaryProfileListItemPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x =>
            {
                var profile = profiles.GetValueOrDefault(x.Id);
                return new SalaryProfileListItemDto
                {
                    Employee = references[x.Id],
                    HasProfile = profile != null,
                    BaseSalary = profile?.BaseSalary,
                    Currency = profile?.Currency,
                    EffectiveFrom = profile?.EffectiveFrom
                };
            }).ToList()
        };
    }

    [Authorize(HrPermissions.FinancePermissions.SalaryProfileRead)]
    public virtual async Task<SalaryProfileDto> GetAsync(Guid employeeId, SalaryProfileGetInput input)
    {
        var date = input.AsOf ?? await _companyClock.GetTodayAsync();
        var profiles = await _reader.GetEffectiveAsync(new[] { employeeId }, date);
        if (!profiles.TryGetValue(employeeId, out var profile))
        {
            throw new EntityNotFoundException(typeof(SalaryProfile), employeeId);
        }

        await LogReadAsync(nameof(SalaryProfile), profile.Id);
        return ToDto(profile, (await References.GetRefsAsync(new[] { employeeId }))[employeeId]);
    }

    [Authorize(HrPermissions.FinancePermissions.SalaryProfileUpdate)]
    public virtual async Task<SalaryProfileDto> UpdateAsync(Guid employeeId, SalaryProfileUpdateInputDto input)
    {
        var employee = await Employees.FindAsync(employeeId) ?? throw new EntityNotFoundException(typeof(Employee), employeeId);
        RefuseSelf(employee);

        var latest = await AsyncExecuter.FirstOrDefaultAsync((await _profiles.GetQueryableAsync())
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.EffectiveFrom));

        if (latest?.ConcurrencyStamp != input.ConcurrencyStamp)
        {
            throw new HrConflictException(L["Conflict:RecordChanged"]);
        }

        var effectiveFrom = input.EffectiveFrom!.Value;
        if (latest != null && effectiveFrom <= latest.EffectiveFrom)
        {
            throw new BusinessException(HrErrorCodes.Finance.EffectiveDateNotAfterLatest);
        }

        if (await Periods.IsDateLockedAsync(effectiveFrom))
        {
            throw new BusinessException(HrErrorCodes.Finance.EffectiveDateInLockedPeriod);
        }

        var names = input.Components.Select(x => x.Name.Trim()).ToList();
        if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count)
        {
            throw new BusinessException(HrErrorCodes.Finance.DuplicateComponentName);
        }

        var profile = new SalaryProfile(GuidGenerator.Create(), CurrentTenant.Id, employeeId, input.BaseSalary, input.Currency, effectiveFrom);
        foreach (var component in input.Components)
        {
            profile.Components.Add(new SalaryComponent(GuidGenerator.Create(), CurrentTenant.Id, profile.Id, component.Name.Trim(), component.Type!.Value, component.Amount));
        }

        await _profiles.InsertAsync(profile, autoSave: true);
        await Periods.MarkDraftsStaleFromAsync(effectiveFrom);

        return ToDto(profile, await References.ToRefAsync(employee));
    }

    [Authorize(HrPermissions.FinancePermissions.SalaryProfileRead)]
    public virtual async Task<SalaryProfilePagedResultDto> GetHistoryAsync(Guid employeeId, PagedAndSortedResultRequestDto input)
    {
        var query = (await _profiles.WithDetailsAsync(x => x.Components)).Where(x => x.EmployeeId == employeeId);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.EffectiveFrom).Skip(input.SkipCount).Take(input.MaxResultCount));
        var employee = (await References.GetRefsAsync(new[] { employeeId })).GetValueOrDefault(employeeId);

        await LogReadAsync(nameof(SalaryProfile));

        return new SalaryProfilePagedResultDto { TotalCount = total, Items = rows.Select(x => ToDto(x, employee!)).ToList() };
    }

    private static SalaryProfileDto ToDto(SalaryProfile profile, EmployeeRefDto employee) => new()
    {
        Id = profile.Id,
        Employee = employee,
        BaseSalary = profile.BaseSalary,
        Currency = profile.Currency,
        EffectiveFrom = profile.EffectiveFrom,
        Components = profile.Components.OrderBy(x => x.Name).Select(x => new SalaryComponentDto { Id = x.Id, Name = x.Name, Type = x.Type, Amount = x.Amount }).ToList(),
        ConcurrencyStamp = profile.ConcurrencyStamp,
        CreationTime = profile.CreationTime
    };
}

[Authorize]
public class AdjustmentAppService : FinanceAppServiceBase, IAdjustmentAppService
{
    private const string CreateOperation = "Adjustment_Create";

    private static readonly Dictionary<string, string> Sort = new()
    {
        ["period"] = nameof(Adjustment.Period),
        ["amount"] = nameof(Adjustment.Amount),
        ["creationTime"] = nameof(Adjustment.CreationTime)
    };

    private readonly IRepository<Adjustment, Guid> _adjustments;
    private readonly IRepository<AdjustmentSuggestion, Guid> _suggestions;
    private readonly SalaryProfileReader _profiles;
    private readonly IdempotencyStore _idempotency;

    public AdjustmentAppService(
        IRepository<Adjustment, Guid> adjustments,
        IRepository<AdjustmentSuggestion, Guid> suggestions,
        SalaryProfileReader profiles,
        IdempotencyStore idempotency)
    {
        _adjustments = adjustments;
        _suggestions = suggestions;
        _profiles = profiles;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.FinancePermissions.AdjustmentRead)]
    public virtual async Task<AdjustmentPagedResultDto> GetListAsync(AdjustmentListInput input)
    {
        var departmentIds = await DepartmentEmployeeIdsAsync(input.DepartmentId);
        var query = (await _adjustments.GetQueryableAsync())
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(departmentIds != null, x => departmentIds!.Contains(x.EmployeeId))
            .WhereIf(!string.IsNullOrEmpty(input.Period), x => x.Period == input.Period)
            .WhereIf(input.Type != null, x => x.Type == input.Type)
            .WhereIf(input.Source != null, x => x.Source == input.Source)
            .WhereIf(input.Status != null, x => x.Status == input.Status);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(Adjustment.CreationTime) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        await LogReadAsync(nameof(Adjustment));
        return new AdjustmentPagedResultDto { TotalCount = total, Items = await AdjustmentMapper.ToDtosAsync(References, rows) };
    }

    [Authorize(HrPermissions.FinancePermissions.AdjustmentCreate)]
    public virtual async Task<AdjustmentDto> CreateAsync(AdjustmentCreateInput input, Guid? idempotencyKey)
    {
        var fingerprint = new { input.EmployeeId, input.Period, input.Type, input.Amount, input.Reason, input.SourceRequestId, input.Attachment.FileName, input.Attachment.Content.Length };
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return await ToDtoAsync(await _adjustments.GetAsync(earlier.Value));
        }

        var employee = await LoadOtherEmployeeAsync(input.EmployeeId!.Value);
        var currency = await RequireCurrencyAsync(employee.Id, input.Period);
        await RefuseLockedPeriodAsync(input.Period);
        await CheckRequestAtAccountantStepAsync(input.SourceRequestId);

        var file = await StoreAttachmentAsync(input.Attachment);
        var adjustment = new Adjustment(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            employee.Id,
            input.Period,
            input.Type!.Value,
            input.Amount,
            currency,
            input.Reason.Trim(),
            AdjustmentSource.Manual,
            sourceRequestId: input.SourceRequestId);
        adjustment.AttachFile(file.Id, file.FileName, input.Attachment.ContentType, input.Attachment.Content.LongLength, file.BlobName);

        await _adjustments.InsertAsync(adjustment, autoSave: true);
        await Periods.MarkDraftsStaleAsync(input.Period);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, fingerprint, adjustment.Id);

        return await ToDtoAsync(adjustment);
    }

    [Authorize(HrPermissions.FinancePermissions.AdjustmentCreate)]
    public virtual async Task<AdjustmentDto> ReverseAsync(Guid id, AdjustmentReverseInputDto input)
    {
        var original = await _adjustments.GetAsync(id);
        var employee = await Employees.GetAsync(original.EmployeeId);
        RefuseSelf(employee);

        if (original.Status == AdjustmentStatus.Reversed)
        {
            throw new BusinessException(HrErrorCodes.Finance.AdjustmentAlreadyReversed);
        }

        await RefuseLockedPeriodAsync(input.Period);

        // A movement no run has paid cancels with its reversal, so neither enters a run. Otherwise the amount would be taken back without ever being paid: see Q118.
        var neverPaid = original.Status == AdjustmentStatus.Open;
        var counterType = original.Type == AdjustmentType.Addition ? AdjustmentType.Deduction : AdjustmentType.Addition;

        var reversal = new Adjustment(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            original.EmployeeId,
            input.Period,
            counterType,
            original.Amount,
            original.Currency,
            input.Reason.Trim(),
            AdjustmentSource.Reversal,
            neverPaid ? AdjustmentStatus.Reversed : AdjustmentStatus.Open,
            reversalOfAdjustmentId: original.Id);

        original.MarkReversed();
        await _adjustments.UpdateAsync(original, autoSave: true);
        await _adjustments.InsertAsync(reversal, autoSave: true);

        await Periods.MarkDraftsStaleAsync(original.Period);
        await Periods.MarkDraftsStaleAsync(input.Period);

        return await ToDtoAsync(reversal);
    }

    [Authorize(HrPermissions.FinancePermissions.AdjustmentRead)]
    public virtual async Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id)
    {
        var adjustment = await _adjustments.GetAsync(id);
        if (adjustment.AttachmentId == null)
        {
            throw new EntityNotFoundException(typeof(Adjustment), id);
        }

        await LogReadAsync(nameof(Adjustment), id);
        var link = await Links.IssueAsync(adjustment.AttachmentId.Value, adjustment.AttachmentBlobName!, adjustment.AttachmentFileName!, adjustment.AttachmentContentType!);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }

    [Authorize(HrPermissions.FinancePermissions.AdjustmentReview)]
    public virtual async Task<AdjustmentSuggestionPagedResultDto> GetSuggestionsAsync(AdjustmentSuggestionListInput input)
    {
        var departmentIds = await DepartmentEmployeeIdsAsync(input.DepartmentId);
        var query = (await _suggestions.GetQueryableAsync())
            .WhereIf(!string.IsNullOrEmpty(input.Period), x => x.Period == input.Period)
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(departmentIds != null, x => departmentIds!.Contains(x.EmployeeId))
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.Basis != null, x => x.Basis == input.Basis);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.Period).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var employees = await References.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var users = await References.GetUserRefsAsync(rows.Where(x => x.ReviewedByUserId != null).Select(x => x.ReviewedByUserId!.Value));

        await LogReadAsync(nameof(AdjustmentSuggestion));

        return new AdjustmentSuggestionPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => ToSuggestionDto(x, employees[x.EmployeeId], x.ReviewedByUserId == null ? null : users[x.ReviewedByUserId.Value])).ToList()
        };
    }

    [Authorize(HrPermissions.FinancePermissions.AdjustmentReview)]
    public virtual async Task<AdjustmentDto> ApproveSuggestionAsync(Guid id, AdjustmentSuggestionApproveInputDto input)
    {
        var suggestion = await LoadPendingSuggestionAsync(id);
        var amount = input.Amount ?? suggestion.SuggestedAmount ?? throw new BusinessException(HrErrorCodes.Finance.SuggestionAmountRequired);
        var currency = await RequireCurrencyAsync(suggestion.EmployeeId, suggestion.Period);
        await RefuseLockedPeriodAsync(suggestion.Period);

        var reason = string.IsNullOrWhiteSpace(input.Reason)
            ? (string)L["Finance:SuggestionReason", suggestion.Period, suggestion.Quantity, L["Finance:SuggestionBasis:" + EnumName(suggestion.Basis)]]
            : input.Reason.Trim();

        var adjustment = new Adjustment(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            suggestion.EmployeeId,
            suggestion.Period,
            suggestion.Type,
            amount,
            currency,
            reason,
            AdjustmentSource.AttendanceSuggestion,
            suggestionId: suggestion.Id);
        await _adjustments.InsertAsync(adjustment, autoSave: true);

        suggestion.Approve(adjustment.Id, CurrentUser.GetId(), Clock.Now, input.Reason);
        await _suggestions.UpdateAsync(suggestion, autoSave: true);
        await Periods.MarkDraftsStaleAsync(suggestion.Period);

        return await ToDtoAsync(adjustment);
    }

    [Authorize(HrPermissions.FinancePermissions.AdjustmentReview)]
    public virtual async Task<AdjustmentSuggestionDto> RejectSuggestionAsync(Guid id, ReasonInputDto input)
    {
        var suggestion = await LoadPendingSuggestionAsync(id);
        suggestion.Reject(CurrentUser.GetId(), Clock.Now, input.Reason.Trim());
        await _suggestions.UpdateAsync(suggestion, autoSave: true);

        var employee = (await References.GetRefsAsync(new[] { suggestion.EmployeeId }))[suggestion.EmployeeId];
        var reviewer = (await References.GetUserRefsAsync(new[] { suggestion.ReviewedByUserId!.Value }))[suggestion.ReviewedByUserId.Value];
        return ToSuggestionDto(suggestion, employee, reviewer);
    }

    private async Task<AdjustmentSuggestion> LoadPendingSuggestionAsync(Guid id)
    {
        var suggestion = await _suggestions.GetAsync(id);
        RefuseSelf(await Employees.GetAsync(suggestion.EmployeeId));

        if (suggestion.Status != AdjustmentSuggestionStatus.PendingReview)
        {
            throw new BusinessException(HrErrorCodes.Finance.SuggestionAlreadyReviewed);
        }

        return suggestion;
    }

    private async Task<string> RequireCurrencyAsync(Guid employeeId, string period)
    {
        var profiles = await _profiles.GetEffectiveAsync(new[] { employeeId }, FinancePeriod.LastDay(period));
        return profiles.TryGetValue(employeeId, out var profile)
            ? profile.Currency
            : throw new BusinessException(HrErrorCodes.Finance.SalaryProfileMissing);
    }

    private async Task RefuseLockedPeriodAsync(string period)
    {
        if (await Periods.IsLockedAsync(period))
        {
            throw new BusinessException(HrErrorCodes.Finance.PeriodLocked);
        }
    }

    private async Task<AdjustmentDto> ToDtoAsync(Adjustment adjustment) => (await AdjustmentMapper.ToDtosAsync(References, new[] { adjustment }))[0];

    private static AdjustmentSuggestionDto ToSuggestionDto(AdjustmentSuggestion x, EmployeeRefDto employee, HR_Mangment2.Identity.UserRefDto? reviewer) => new()
    {
        Id = x.Id,
        Employee = employee,
        Period = x.Period,
        Type = x.Type,
        Basis = x.Basis,
        Quantity = x.Quantity,
        QuantityUnit = x.QuantityUnit,
        SuggestedAmount = x.SuggestedAmount,
        Currency = x.Currency,
        SettlementForPeriod = x.SettlementForPeriod,
        Status = x.Status,
        AdjustmentId = x.AdjustmentId,
        ReviewedBy = reviewer,
        ReviewedAt = x.ReviewedAt,
        ReviewNote = x.ReviewNote,
        CreationTime = x.CreationTime
    };
}

public static class AdjustmentMapper
{
    public static async Task<List<AdjustmentDto>> ToDtosAsync(EmployeeReferenceReader references, IReadOnlyCollection<Adjustment> rows)
    {
        var employees = await references.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var users = await references.GetUserRefsAsync(rows.Where(x => x.CreatorId != null).Select(x => x.CreatorId!.Value));

        return rows.Select(x => new AdjustmentDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            Period = x.Period,
            Type = x.Type,
            Amount = x.Amount,
            Currency = x.Currency,
            Reason = x.Reason,
            Source = x.Source,
            Status = x.Status,
            Attachment = x.AttachmentId == null ? null : new AttachmentDto
            {
                Id = x.AttachmentId.Value,
                FileName = x.AttachmentFileName!,
                ContentType = x.AttachmentContentType!,
                SizeBytes = x.AttachmentSizeBytes!.Value,
                CreationTime = x.CreationTime
            },
            SourceRequestId = x.SourceRequestId,
            SuggestionId = x.SuggestionId,
            ReversalOfAdjustmentId = x.ReversalOfAdjustmentId,
            CreatedBy = x.CreatorId == null ? null! : users[x.CreatorId.Value],
            CreationTime = x.CreationTime
        }).ToList();
    }
}

public class SalaryProfileUpdateInputDtoValidator : AbstractValidator<SalaryProfileUpdateInputDto>
{
    public SalaryProfileUpdateInputDtoValidator()
    {
        RuleFor(x => x.BaseSalary).GreaterThanOrEqualTo(0.01m);
        RuleFor(x => x.Currency).NotNull().Matches(FinanceConsts.CurrencyPattern);
        RuleFor(x => x.EffectiveFrom).NotNull();
        RuleFor(x => x.Components).NotNull();
        RuleForEach(x => x.Components).ChildRules(c =>
        {
            c.RuleFor(x => x.Name).NotNull().Length(1, FinanceConsts.MaxNameLength);
            c.RuleFor(x => x.Type).NotNull();
            c.RuleFor(x => x.Amount).GreaterThanOrEqualTo(0.01m);
        });
    }
}

public class AdjustmentCreateInputValidator : AbstractValidator<AdjustmentCreateInput>
{
    public AdjustmentCreateInputValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.Period).NotNull().Matches(FinanceConsts.PeriodPattern);
        RuleFor(x => x.Type).NotNull();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0.01m);
        RuleFor(x => x.Reason).NotNull().Length(1, FinanceConsts.MaxTextLength);
        RuleFor(x => x.Attachment).NotNull();
    }
}

public class AdjustmentReverseInputDtoValidator : AbstractValidator<AdjustmentReverseInputDto>
{
    public AdjustmentReverseInputDtoValidator()
    {
        RuleFor(x => x.Reason).NotNull().Length(1, FinanceConsts.MaxTextLength);
        RuleFor(x => x.Period).NotNull().Matches(FinanceConsts.PeriodPattern);
    }
}

public class AdjustmentSuggestionApproveInputDtoValidator : AbstractValidator<AdjustmentSuggestionApproveInputDto>
{
    public AdjustmentSuggestionApproveInputDtoValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0.01m).When(x => x.Amount != null);
        RuleFor(x => x.Reason).MaximumLength(FinanceConsts.MaxTextLength);
    }
}
