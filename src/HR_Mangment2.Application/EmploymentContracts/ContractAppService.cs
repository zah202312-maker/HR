using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.EmploymentContracts;

[Authorize]
public class ContractAppService : HR_Mangment2AppService, IContractAppService
{
    private const string CreateOperation = "Contract_Create";
    private const int DecisionWorkingDays = 5;

    private static readonly Dictionary<string, string> Sort = new()
    {
        ["contractType"] = nameof(EmploymentContract.ContractType),
        ["startDate"] = nameof(EmploymentContract.StartDate),
        ["endDate"] = nameof(EmploymentContract.EndDate),
        ["status"] = nameof(EmploymentContract.Status),
        ["creationTime"] = nameof(EmploymentContract.CreationTime)
    };

    private readonly IRepository<EmploymentContract, Guid> _contracts;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly EmployeeReferenceReader _references;
    private readonly HrSettingsStore _settings;
    private readonly ICompanyClock _companyClock;
    private readonly WorkCalendarService _calendar;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdempotencyStore _idempotency;

    public ContractAppService(
        IRepository<EmploymentContract, Guid> contracts,
        IRepository<Employee, Guid> employees,
        EmployeeReferenceReader references,
        HrSettingsStore settings,
        ICompanyClock companyClock,
        WorkCalendarService calendar,
        HrNotificationPublisher notifications,
        IdempotencyStore idempotency)
    {
        _contracts = contracts;
        _employees = employees;
        _references = references;
        _settings = settings;
        _companyClock = companyClock;
        _calendar = calendar;
        _notifications = notifications;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.ContractPermissions.Read)]
    public virtual async Task<ContractPagedResultDto> GetListAsync(ContractListInput input)
    {
        var query = (await ScopedAsync(input.DepartmentId))
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.EndDateFrom != null, x => x.EndDate >= input.EndDateFrom)
            .WhereIf(input.EndDateTo != null, x => x.EndDate <= input.EndDateTo);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(EmploymentContract.CreationTime) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        return new ContractPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    [Authorize(HrPermissions.ContractPermissions.Create)]
    public virtual async Task<ContractDto> CreateAsync(ContractCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await ToDtoAsync(await _contracts.GetAsync(earlier.Value));
        }

        var employeeId = input.EmployeeId!.Value;
        var start = input.StartDate!.Value;
        var end = input.EndDate!.Value;

        var employee = await _employees.FindAsync(employeeId);
        if (employee == null || employee.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Contract.EmployeeNotFound);
        }

        if (end <= start)
        {
            throw new BusinessException(HrErrorCodes.Contract.InvalidDateRange);
        }

        if (input.PreviousContractId != null)
        {
            var previous = await _contracts.FindAsync(input.PreviousContractId.Value);
            if (previous == null || previous.EmployeeId != employeeId || previous.Status != ContractStatus.Renewed)
            {
                throw new BusinessException(HrErrorCodes.Contract.InvalidPreviousContract);
            }
        }

        if (await _contracts.AnyAsync(x => x.EmployeeId == employeeId && x.StartDate <= end && x.EndDate >= start))
        {
            throw new BusinessException(HrErrorCodes.Contract.OverlappingContract);
        }

        var today = await _companyClock.GetTodayAsync();
        var alertDays = (await _settings.PeekValueAsync<ContractSettings>()).AlertDays;
        var status = end <= today.AddDays(alertDays) ? ContractStatus.PendingRecommendation : ContractStatus.Active;

        var contract = new EmploymentContract(GuidGenerator.Create(), CurrentTenant.Id, employeeId, input.ContractType.Trim(), start, end, input.PreviousContractId, status);
        await _contracts.InsertAsync(contract, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, contract.Id);

        return await ToDtoAsync(contract);
    }

    [Authorize(HrPermissions.ContractPermissions.Read)]
    public virtual async Task<ContractExpiringPagedResultDto> GetExpiringAsync(ContractExpiringListInput input)
    {
        var alertDays = input.WithinDays ?? (await _settings.PeekValueAsync<ContractSettings>()).AlertDays;
        var horizon = (await _companyClock.GetTodayAsync()).AddDays(alertDays);

        var query = (await ScopedAsync(input.DepartmentId))
            .Where(x => x.Decision == null && x.EndDate <= horizon);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(EmploymentContract.EndDate), Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        return new ContractExpiringPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows), AlertDays = alertDays };
    }

    [Authorize(HrPermissions.ContractPermissions.Read)]
    public virtual async Task<ContractDto> GetAsync(Guid id) => await ToDtoAsync(await _contracts.GetAsync(id));

    [Authorize(HrPermissions.ContractPermissions.Recommend)]
    public virtual async Task<ContractDto> RecommendAsync(Guid id, ContractRecommendInputDto input)
    {
        var contract = await _contracts.GetAsync(id);
        await RefuseOwnContractAsync(contract);

        if (contract.Status != ContractStatus.PendingRecommendation)
        {
            throw new BusinessException(HrErrorCodes.Contract.InvalidStatus);
        }

        var today = await _companyClock.GetTodayAsync();
        var dueDate = await _calendar.AddWorkingDaysAsync(today, DecisionWorkingDays) ?? today;

        contract.Recommend(input.EmployeePosition.Trim(), input.Recommendation!.Value, input.Note, CurrentUser.GetId(), Clock.Now, dueDate);
        await _contracts.UpdateAsync(contract, autoSave: true);

        return await ToDtoAsync(contract);
    }

    [Authorize(HrPermissions.ContractPermissions.Decide)]
    public virtual async Task<ContractDto> DecideAsync(Guid id, ContractDecideInputDto input)
    {
        var contract = await _contracts.GetAsync(id);
        var employee = await RefuseOwnContractAsync(contract);

        if (contract.Status != ContractStatus.PendingDecision)
        {
            throw new BusinessException(HrErrorCodes.Contract.InvalidStatus);
        }

        contract.Decide(input.Decision!.Value, input.Note, input.EmployeeMessage, CurrentUser.GetId(), Clock.Now);
        await _contracts.UpdateAsync(contract, autoSave: true);

        if (employee.UserId != null)
        {
            await _notifications.PublishAsync(employee.UserId.Value, NotificationTemplateCode.ContractDecided, NotificationResourceType.Contract, contract.Id);
        }

        return await ToDtoAsync(contract);
    }

    [Authorize(HrPermissions.ContractPermissions.ReadOwn)]
    public virtual async Task<ContractEmployeePagedResultDto> GetMyListAsync(PagedAndSortedResultRequestDto input)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (me == null)
        {
            return new ContractEmployeePagedResultDto();
        }

        var query = (await _contracts.GetQueryableAsync()).Where(x => x.EmployeeId == me.Id);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(EmploymentContract.StartDate) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));
        var today = await _companyClock.GetTodayAsync();

        return new ContractEmployeePagedResultDto { TotalCount = total, Items = rows.Select(x => ToEmployeeDto(x, today)).ToList() };
    }

    [Authorize(HrPermissions.ContractPermissions.ReadOwn)]
    public virtual async Task<ContractEmployeeDto> GetMyAsync(Guid id)
    {
        var contract = await _contracts.FindAsync(id);
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (contract == null || me == null || contract.EmployeeId != me.Id)
        {
            throw new EntityNotFoundException(typeof(EmploymentContract), id);
        }

        return ToEmployeeDto(contract, await _companyClock.GetTodayAsync());
    }

    /// <summary>QUESTIONS.md item 48 gives this case to the executive director, but the contract's roles do not reach them yet: see D34.</summary>
    private async Task<Employee> RefuseOwnContractAsync(EmploymentContract contract)
    {
        var employee = await _employees.GetAsync(contract.EmployeeId);
        if (employee.UserId == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Contract.SelfActionNotAllowed);
        }

        return employee;
    }

    private async Task<IQueryable<EmploymentContract>> ScopedAsync(Guid? departmentId)
    {
        var query = await _contracts.GetQueryableAsync();
        if (departmentId == null)
        {
            return query;
        }

        var employeeIds = (await _employees.GetQueryableAsync()).Where(x => x.DepartmentId == departmentId).Select(x => x.Id);
        return query.Where(x => employeeIds.Contains(x.EmployeeId));
    }

    private async Task<ContractDto> ToDtoAsync(EmploymentContract contract) => (await ToDtosAsync(new[] { contract }))[0];

    private async Task<List<ContractDto>> ToDtosAsync(IReadOnlyCollection<EmploymentContract> contracts)
    {
        var employees = await _references.GetRefsAsync(contracts.Select(x => x.EmployeeId));
        var users = await _references.GetUserRefsAsync(contracts
            .SelectMany(x => new[] { x.RecommendedByUserId, x.DecidedByUserId })
            .Where(x => x != null)
            .Select(x => x!.Value));
        var today = await _companyClock.GetTodayAsync();

        return contracts.Select(x => new ContractDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            ContractType = x.ContractType,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            PreviousContractId = x.PreviousContractId,
            Status = x.Status,
            DaysRemaining = x.EndDate.DayNumber - today.DayNumber,
            IsExpired = x.EndDate < today,
            Recommendation = x.Recommendation == null ? null : new ContractRecommendationDto
            {
                EmployeePosition = x.EmployeePosition!,
                Recommendation = x.Recommendation.Value,
                Note = x.RecommendationNote,
                RecommendedBy = users[x.RecommendedByUserId!.Value],
                RecommendedAt = x.RecommendedAt!.Value
            },
            Decision = x.Decision == null ? null : new ContractDecisionDto
            {
                Decision = x.Decision.Value,
                Note = x.DecisionNote,
                EmployeeMessage = x.EmployeeMessage,
                DecidedBy = users[x.DecidedByUserId!.Value],
                DecidedAt = x.DecidedAt!.Value
            },
            DecisionDueDate = x.DecisionDueDate,
            IsDecisionOverdue = x.Decision == null && x.DecisionDueDate != null && x.DecisionDueDate < today,
            EmployeeNotifiedAt = x.EmployeeNotifiedAt,
            CreationTime = x.CreationTime,
            LastModificationTime = x.LastModificationTime
        }).ToList();
    }

    private static ContractEmployeeDto ToEmployeeDto(EmploymentContract contract, DateOnly today) => new()
    {
        Id = contract.Id,
        ContractType = contract.ContractType,
        StartDate = contract.StartDate,
        EndDate = contract.EndDate,
        Status = contract.Status switch
        {
            ContractStatus.Renewed => ContractEmployeeStatus.Renewed,
            ContractStatus.NotRenewed => ContractEmployeeStatus.NotRenewed,
            _ => ContractEmployeeStatus.Active
        },
        IsExpired = contract.EndDate < today,
        EmployeeMessage = contract.EmployeeMessage,
        DecidedAt = contract.DecidedAt
    };
}

public class ContractCreateInputDtoValidator : AbstractValidator<ContractCreateInputDto>
{
    public ContractCreateInputDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.ContractType).NotNull().Length(1, EmploymentContract.MaxContractTypeLength);
        RuleFor(x => x.StartDate).NotNull();
        RuleFor(x => x.EndDate).NotNull();
    }
}

public class ContractRecommendInputDtoValidator : AbstractValidator<ContractRecommendInputDto>
{
    public ContractRecommendInputDtoValidator()
    {
        RuleFor(x => x.EmployeePosition).NotNull().Length(1, EmploymentContract.MaxTextLength);
        RuleFor(x => x.Recommendation).NotNull();
        RuleFor(x => x.Note).MaximumLength(EmploymentContract.MaxTextLength);
    }
}

public class ContractDecideInputDtoValidator : AbstractValidator<ContractDecideInputDto>
{
    public ContractDecideInputDtoValidator()
    {
        RuleFor(x => x.Decision).NotNull();
        RuleFor(x => x.Note).MaximumLength(EmploymentContract.MaxTextLength);
        RuleFor(x => x.EmployeeMessage).MaximumLength(EmploymentContract.MaxTextLength);
    }
}
