using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Leaves;

[Authorize]
public class OtherLeaveTypeAppService : HR_Mangment2AppService, IOtherLeaveTypeAppService
{
    private const string CreateOperation = "OtherLeaveType_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["nameAr"] = nameof(OtherLeaveType.NameAr),
        ["nameEn"] = nameof(OtherLeaveType.NameEn),
        ["creationTime"] = nameof(OtherLeaveType.CreationTime)
    };

    private readonly IRepository<OtherLeaveType, Guid> _types;
    private readonly DedicatedRequestOwnerResolver _owners;
    private readonly IdempotencyStore _idempotency;

    public OtherLeaveTypeAppService(IRepository<OtherLeaveType, Guid> types, DedicatedRequestOwnerResolver owners, IdempotencyStore idempotency)
    {
        _types = types;
        _owners = owners;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.LeavePermissions.OtherTypeManage)]
    public virtual async Task<OtherLeaveTypePagedResultDto> GetListAsync(OtherLeaveTypeListInput input)
    {
        var query = await _types.GetQueryableAsync();
        if (input.IsActive != null)
        {
            query = query.Where(x => x.IsActive == input.IsActive);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(OtherLeaveType.CreationTime) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new OtherLeaveTypePagedResultDto { TotalCount = total, Items = items.Select(ToDto).ToList() };
    }

    [Authorize(HrPermissions.LeavePermissions.OtherTypeManage)]
    public virtual async Task<OtherLeaveTypeDto> CreateAsync(OtherLeaveTypeCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return ToDto(await _types.GetAsync(earlier.Value));
        }

        var type = new OtherLeaveType(GuidGenerator.Create(), CurrentTenant.Id);
        type.Set(input.NameAr, input.NameEn, input.IsPaid!.Value, input.MaxDays, input.RequiresAttachment!.Value, input.EligibleCategory!.Value, input.DeductsFromAnnualBalance, input.IsActive!.Value);
        await _types.InsertAsync(type, autoSave: true);

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, type.Id);
        return ToDto(type);
    }

    [Authorize(HrPermissions.LeavePermissions.OtherTypeManage)]
    public virtual async Task<OtherLeaveTypeDto> UpdateAsync(Guid id, OtherLeaveTypeUpdateDto input)
    {
        var type = await _types.GetAsync(id);
        type.ConcurrencyStamp = input.ConcurrencyStamp;
        type.Set(input.NameAr, input.NameEn, input.IsPaid!.Value, input.MaxDays, input.RequiresAttachment!.Value, input.EligibleCategory!.Value, input.DeductsFromAnnualBalance!.Value, input.IsActive!.Value);
        await _types.UpdateAsync(type, autoSave: true);
        return ToDto(type);
    }

    [Authorize(HrPermissions.LeavePermissions.RequestCreate)]
    public virtual async Task<OtherLeaveTypeOptionListDto> GetAvailableAsync(Guid? employeeId)
    {
        var (owner, _) = await _owners.ResolveAsync(CurrentUser.GetId(), employeeId, HrErrorCodes.Leave.ExecutiveDirectorNotEligible, HrErrorCodes.Leave.EmployeeNotActive);
        var active = await _types.GetListAsync(x => x.IsActive);

        return new OtherLeaveTypeOptionListDto
        {
            Items = active.Where(x => LeavePlanner.IsEligible(x, owner.Gender)).OrderBy(x => LocalizedName.Pick(x.NameAr, x.NameEn)).Select(ToOption).ToList()
        };
    }

    public static OtherLeaveTypeOptionDto ToOption(OtherLeaveType type) => new()
    {
        Id = type.Id,
        Name = LocalizedName.Pick(type.NameAr, type.NameEn),
        IsPaid = type.IsPaid,
        MaxDays = type.MaxDays,
        RequiresAttachment = type.RequiresAttachment,
        DeductsFromAnnualBalance = type.DeductsFromAnnualBalance
    };

    private static OtherLeaveTypeDto ToDto(OtherLeaveType type) => new()
    {
        Id = type.Id,
        Name = LocalizedName.Pick(type.NameAr, type.NameEn),
        NameAr = type.NameAr,
        NameEn = type.NameEn,
        IsPaid = type.IsPaid,
        MaxDays = type.MaxDays,
        RequiresAttachment = type.RequiresAttachment,
        EligibleCategory = type.EligibleCategory,
        DeductsFromAnnualBalance = type.DeductsFromAnnualBalance,
        IsActive = type.IsActive,
        CreationTime = type.CreationTime,
        ConcurrencyStamp = type.ConcurrencyStamp
    };
}

public class OtherLeaveTypeCreateDtoValidator : AbstractValidator<OtherLeaveTypeCreateDto>
{
    public OtherLeaveTypeCreateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, OtherLeaveType.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, OtherLeaveType.MaxNameLength);
        RuleFor(x => x.IsPaid).NotNull();
        RuleFor(x => x.MaxDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequiresAttachment).NotNull();
        RuleFor(x => x.EligibleCategory).NotNull();
        RuleFor(x => x.IsActive).NotNull();
    }
}

public class OtherLeaveTypeUpdateDtoValidator : AbstractValidator<OtherLeaveTypeUpdateDto>
{
    public OtherLeaveTypeUpdateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, OtherLeaveType.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, OtherLeaveType.MaxNameLength);
        RuleFor(x => x.IsPaid).NotNull();
        RuleFor(x => x.MaxDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequiresAttachment).NotNull();
        RuleFor(x => x.EligibleCategory).NotNull();
        RuleFor(x => x.DeductsFromAnnualBalance).NotNull();
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
