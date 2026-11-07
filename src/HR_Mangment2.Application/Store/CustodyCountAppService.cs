using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.Store;

[Authorize]
public class CustodyCountAppService : HR_Mangment2AppService, ICustodyCountAppService
{
    private const string CreateOperation = "CustodyCount_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["countDate"] = nameof(CustodyCount.CountDate),
        ["result"] = nameof(CustodyCount.Result),
        ["creationTime"] = nameof(CustodyCount.CreationTime)
    };

    private readonly IRepository<CustodyCount, Guid> _counts;
    private readonly IRepository<Custody, Guid> _custodies;
    private readonly CustodyDepartmentScope _scope;
    private readonly ItemReferenceReader _itemRefs;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly IIdentityUserRepository _users;
    private readonly IdempotencyStore _idempotency;

    public CustodyCountAppService(
        IRepository<CustodyCount, Guid> counts,
        IRepository<Custody, Guid> custodies,
        CustodyDepartmentScope scope,
        ItemReferenceReader itemRefs,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        IIdentityUserRepository users,
        IdempotencyStore idempotency)
    {
        _counts = counts;
        _custodies = custodies;
        _scope = scope;
        _itemRefs = itemRefs;
        _references = references;
        _notifications = notifications;
        _taskLog = taskLog;
        _users = users;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.CustodyPermissions.Count)]
    public virtual async Task<CustodyCountDto> CreateAsync(CustodyCountCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await ToDtoAsync(await LoadAsync(earlier.Value));
        }

        var me = CurrentUser.GetId();
        var departmentId = input.DepartmentId!.Value;
        if (!(await _scope.GetManagedDepartmentIdsAsync(me)).Contains(departmentId))
        {
            throw new BusinessException(HrErrorCodes.Custody.DepartmentNotInScope);
        }

        var open = (await _custodies.GetListAsync(x => x.DepartmentId == departmentId && x.Status != CustodyStatus.Returned)).ToDictionary(x => x.Id);
        var countedIds = input.Lines.Select(x => x.CustodyId!.Value).ToList();

        if (countedIds.Distinct().Count() != countedIds.Count || countedIds.Any(x => !open.ContainsKey(x)))
        {
            throw new BusinessException(HrErrorCodes.Custody.CountLineInvalid);
        }

        if (open.Keys.Any(x => !countedIds.Contains(x)))
        {
            throw new BusinessException(HrErrorCodes.Custody.CountLinesIncomplete);
        }

        var result = input.Lines.Any(x => x.CountedQuantity != open[x.CustodyId!.Value].Quantity)
            ? CustodyCountResult.Discrepancy
            : CustodyCountResult.Matched;

        var count = new CustodyCount(GuidGenerator.Create(), CurrentTenant.Id, departmentId, input.CountDate!.Value, result, input.Note, me);
        foreach (var line in input.Lines)
        {
            var custody = open[line.CustodyId!.Value];
            count.Lines.Add(new CustodyCountLine(GuidGenerator.Create(), CurrentTenant.Id, count.Id, custody.Id, custody.ItemId, custody.Quantity, line.CountedQuantity!.Value, line.Note));
        }

        await _counts.InsertAsync(count, autoSave: true);
        await _taskLog.LogAsync(me, AdministrativeTaskType.CustodyCountRecorded, count.Id);

        if (result == CustodyCountResult.Discrepancy)
        {
            foreach (var officer in await _scope.GetStoreOfficerUserIdsAsync())
            {
                await _notifications.PublishAsync(officer, NotificationTemplateCode.CustodyCountDifference, NotificationResourceType.Custody, count.Id);
            }
        }

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, count.Id);
        return await ToDtoAsync(count);
    }

    [Authorize(HrPermissions.CustodyPermissions.ViewCount)]
    public virtual async Task<CustodyCountPagedResultDto> GetListAsync(CustodyCountListInput input)
    {
        var userId = CurrentUser.GetId();
        var roles = await _users.GetRoleNamesAsync(userId);
        var query = (await _counts.WithDetailsAsync(x => x.Lines))
            .WhereIf(input.DepartmentId != null, x => x.DepartmentId == input.DepartmentId)
            .WhereIf(input.Result != null, x => x.Result == input.Result)
            .WhereIf(input.FromDate != null, x => x.CountDate >= input.FromDate)
            .WhereIf(input.ToDate != null, x => x.CountDate <= input.ToDate);

        if (!roles.Contains(HrRoles.StoreOfficer) && !roles.Contains(HrRoles.HrManager))
        {
            var departments = await _scope.GetManagedDepartmentIdsAsync(userId);
            query = query.Where(x => departments.Contains(x.DepartmentId));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(CustodyCount.CountDate) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new CustodyCountPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    private async Task<CustodyCount> LoadAsync(Guid id)
    {
        var query = (await _counts.WithDetailsAsync(x => x.Lines)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstAsync(query);
    }

    private async Task<CustodyCountDto> ToDtoAsync(CustodyCount count) => (await ToDtosAsync(new[] { count })).Single();

    private async Task<List<CustodyCountDto>> ToDtosAsync(IReadOnlyCollection<CustodyCount> rows)
    {
        var items = await _itemRefs.GetRefsAsync(rows.SelectMany(x => x.Lines).Select(x => x.ItemId));
        var departments = await _references.GetDepartmentsAsync(rows.Select(x => x.DepartmentId));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.CountedByUserId));

        return rows.Select(x => new CustodyCountDto
        {
            Id = x.Id,
            Department = departments[x.DepartmentId],
            CountDate = x.CountDate,
            Result = x.Result,
            Note = x.Note,
            CountedBy = users[x.CountedByUserId],
            CreationTime = x.CreationTime,
            Lines = x.Lines.Select(line => new CustodyCountLineDto
            {
                CustodyId = line.CustodyId,
                Item = items[line.ItemId],
                ExpectedQuantity = line.ExpectedQuantity,
                CountedQuantity = line.CountedQuantity,
                Note = line.Note
            }).ToList()
        }).ToList();
    }
}

public class CustodyCountCreateInputDtoValidator : AbstractValidator<CustodyCountCreateInputDto>
{
    public CustodyCountCreateInputDtoValidator()
    {
        RuleFor(x => x.DepartmentId).NotNull();
        RuleFor(x => x.CountDate).NotNull();
        RuleFor(x => x.Note).MaximumLength(StoreConsts.MaxNoteLength);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.CustodyId).NotNull();
            line.RuleFor(x => x.CountedQuantity).NotNull().GreaterThanOrEqualTo(0).PrecisionScale(StoreConsts.QuantityPrecision, StoreConsts.QuantityScale, true);
            line.RuleFor(x => x.Note).MaximumLength(StoreConsts.MaxNoteLength);
        });
    }
}
