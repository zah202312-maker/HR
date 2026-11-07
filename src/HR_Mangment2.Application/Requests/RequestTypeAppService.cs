using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Errors;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Requests;

[Authorize]
public class RequestTypeAppService : HR_Mangment2AppService, IRequestTypeAppService
{
    private readonly IRepository<RequestType, Guid> _types;

    public RequestTypeAppService(IRepository<RequestType, Guid> types)
    {
        _types = types;
    }

    private static string NameField => LocalizedName.IsEnglish ? nameof(RequestType.NameEn) : nameof(RequestType.NameAr);

    private static Dictionary<string, string> SortFields => new()
    {
        ["name"] = NameField,
        ["nameAr"] = nameof(RequestType.NameAr),
        ["nameEn"] = nameof(RequestType.NameEn),
        ["code"] = nameof(RequestType.Code),
        ["destination"] = nameof(RequestType.Destination),
        ["isActive"] = nameof(RequestType.IsActive),
        ["creationTime"] = nameof(RequestType.CreationTime)
    };

    [Authorize(HrPermissions.RequestPermissions.TypeView)]
    public virtual async Task<RequestTypePagedResultDto> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = (await _types.GetQueryableAsync()).Where(x => x.IsActive);
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, NameField, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new RequestTypePagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new RequestTypeDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = LocalizedName.Pick(x.NameAr, x.NameEn),
                Destination = x.Destination,
                UsesDedicatedEndpoint = RequestTypeCodes.UsesDedicatedEndpoint(x.Code),
                RequiresItem = x.Destination == RequestDestination.Store
            }).ToList()
        };
    }

    [Authorize(HrPermissions.RequestPermissions.TypeManage)]
    public virtual async Task<RequestTypeAdminDto> CreateAsync(RequestTypeCreateInputDto input)
    {
        if (await _types.AnyAsync(x => x.Code == input.Code))
        {
            throw new BusinessException(HrErrorCodes.Request.TypeCodeAlreadyExists);
        }

        var type = new RequestType(GuidGenerator.Create(), CurrentTenant.Id, input.Code, input.NameAr, input.NameEn, input.Destination!.Value, input.IsActive!.Value, false);
        await _types.InsertAsync(type, autoSave: true);
        return ToAdminDto(type);
    }

    [Authorize(HrPermissions.RequestPermissions.TypeManage)]
    public virtual async Task<RequestTypeAdminPagedResultDto> GetAdminListAsync(RequestTypeAdminListInput input)
    {
        var query = await _types.GetQueryableAsync();
        if (input.IsActive != null)
        {
            query = query.Where(x => x.IsActive == input.IsActive);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, NameField, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new RequestTypeAdminPagedResultDto { TotalCount = total, Items = items.Select(ToAdminDto).ToList() };
    }

    [Authorize(HrPermissions.RequestPermissions.TypeManage)]
    public virtual async Task<RequestTypeAdminDto> GetAsync(Guid id) => ToAdminDto(await _types.GetAsync(id));

    [Authorize(HrPermissions.RequestPermissions.TypeManage)]
    public virtual async Task<RequestTypeAdminDto> UpdateAsync(Guid id, RequestTypeUpdateInputDto input)
    {
        var type = await _types.GetAsync(id);
        if (type.IsSystem && input.Destination != type.Destination)
        {
            throw new BusinessException(HrErrorCodes.Request.SystemTypeDestinationLocked);
        }

        type.ConcurrencyStamp = input.ConcurrencyStamp;
        type.Set(input.NameAr, input.NameEn, input.Destination!.Value, input.IsActive!.Value);
        await _types.UpdateAsync(type, autoSave: true);
        return ToAdminDto(type);
    }

    private static RequestTypeAdminDto ToAdminDto(RequestType type) => new()
    {
        Id = type.Id,
        Code = type.Code,
        NameAr = type.NameAr,
        NameEn = type.NameEn,
        Destination = type.Destination,
        IsActive = type.IsActive,
        IsSystem = type.IsSystem,
        ConcurrencyStamp = type.ConcurrencyStamp,
        CreationTime = type.CreationTime,
        LastModificationTime = type.LastModificationTime
    };
}

public class RequestTypeCreateInputDtoValidator : AbstractValidator<RequestTypeCreateInputDto>
{
    public RequestTypeCreateInputDtoValidator()
    {
        RuleFor(x => x.Code).NotNull().Matches("^[a-z][A-Za-z0-9]{1,63}$");
        RuleFor(x => x.NameAr).NotNull().Length(1, RequestType.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, RequestType.MaxNameLength);
        RuleFor(x => x.Destination).NotNull();
        RuleFor(x => x.IsActive).NotNull();
    }
}

public class RequestTypeUpdateInputDtoValidator : AbstractValidator<RequestTypeUpdateInputDto>
{
    public RequestTypeUpdateInputDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, RequestType.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, RequestType.MaxNameLength);
        RuleFor(x => x.Destination).NotNull();
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
