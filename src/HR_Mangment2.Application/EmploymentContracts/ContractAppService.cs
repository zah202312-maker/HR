using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Files;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Leaves;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
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
    private readonly IRepository<ContractAttachment, Guid> _attachments;
    private readonly UploadedFileGuard _fileGuard;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly FileDownloadLinkManager _links;

    public ContractAppService(
        IRepository<EmploymentContract, Guid> contracts,
        IRepository<Employee, Guid> employees,
        EmployeeReferenceReader references,
        HrSettingsStore settings,
        ICompanyClock companyClock,
        WorkCalendarService calendar,
        HrNotificationPublisher notifications,
        IdempotencyStore idempotency,
        IRepository<ContractAttachment, Guid> attachments,
        UploadedFileGuard fileGuard,
        IBlobContainer<HrFilesContainer> files,
        FileDownloadLinkManager links)
    {
        _contracts = contracts;
        _employees = employees;
        _references = references;
        _settings = settings;
        _companyClock = companyClock;
        _calendar = calendar;
        _notifications = notifications;
        _idempotency = idempotency;
        _attachments = attachments;
        _fileGuard = fileGuard;
        _files = files;
        _links = links;
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
    public virtual async Task<ContractDto> CreateAsync(ContractCreateInput input, Guid? idempotencyKey)
    {
        // The file bytes are not part of the fingerprint: their names, types and sizes are enough to tell a retry apart.
        var fingerprint = new
        {
            input.EmployeeId,
            input.ContractType,
            input.StartDate,
            input.EndDate,
            input.PreviousContractId,
            Files = input.Attachments.Select(x => new { x.FileName, x.ContentType, x.Content.Length }).ToList()
        };

        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, fingerprint);
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

        foreach (var file in input.Attachments)
        {
            await _fileGuard.EnsureAcceptedAsync(file.Content, file.ContentType, HrErrorCodes.Contract.AttachmentRejected);
        }

        var contract = new EmploymentContract(GuidGenerator.Create(), CurrentTenant.Id, employeeId, input.ContractType.Trim(), start, end, input.PreviousContractId, status);
        await _contracts.InsertAsync(contract, autoSave: true);

        foreach (var file in input.Attachments)
        {
            var attachmentId = GuidGenerator.Create();
            var blobName = attachmentId.ToString("N");
            await _files.SaveAsync(blobName, file.Content);
            await _attachments.InsertAsync(new ContractAttachment(attachmentId, CurrentTenant.Id, contract.Id, Path.GetFileName(file.FileName), file.ContentType, file.Content.LongLength, blobName, Clock.Now), autoSave: true);
        }

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, fingerprint, contract.Id);

        return await ToDtoAsync(contract);
    }

    [Authorize(HrPermissions.ContractPermissions.Read)]
    public virtual async Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId) =>
        await IssueLinkAsync(await FindAttachmentAsync(id, attachmentId));

    [Authorize(HrPermissions.ContractPermissions.ReadOwn)]
    public virtual async Task<FileDownloadUrlDto> GetMyAttachmentDownloadUrlAsync(Guid id, Guid attachmentId)
    {
        var contract = await _contracts.FindAsync(id);
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (contract == null || me == null || contract.EmployeeId != me.Id)
        {
            throw new EntityNotFoundException(typeof(EmploymentContract), id);
        }

        return await IssueLinkAsync(await FindAttachmentAsync(id, attachmentId));
    }

    /// <summary>Soft delete: hidden from views and downloads, row and blob kept. The entity is audited.</summary>
    [Authorize(HrPermissions.ContractPermissions.Create)]
    public virtual async Task DeleteAttachmentAsync(Guid id, Guid attachmentId)
    {
        var attachment = await FindAttachmentAsync(id, attachmentId);
        attachment.MarkDeleted(CurrentUser.GetId(), Clock.Now);
        await _attachments.UpdateAsync(attachment, autoSave: true);
    }

    /// <summary>The attachment must belong to this contract and not be deleted (the soft-delete filter hides deleted rows).</summary>
    private async Task<ContractAttachment> FindAttachmentAsync(Guid contractId, Guid attachmentId) =>
        await _attachments.FirstOrDefaultAsync(x => x.Id == attachmentId && x.ContractId == contractId)
        ?? throw new EntityNotFoundException(typeof(ContractAttachment), attachmentId);

    private async Task<FileDownloadUrlDto> IssueLinkAsync(ContractAttachment attachment)
    {
        var link = await _links.IssueAsync(attachment.Id, attachment.BlobName, attachment.FileName, attachment.ContentType);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }

    private async Task<Dictionary<Guid, List<AttachmentDto>>> GetAttachmentsAsync(IEnumerable<Guid> contractIds)
    {
        var ids = contractIds.Distinct().ToList();
        var rows = await _attachments.GetListAsync(x => ids.Contains(x.ContractId));
        return ids.ToDictionary(id => id, id => rows
            .Where(x => x.ContractId == id)
            .OrderBy(x => x.CreationTime).ThenBy(x => x.Id)
            .Select(x => new AttachmentDto { Id = x.Id, FileName = x.FileName, ContentType = x.ContentType, SizeBytes = x.SizeBytes, CreationTime = x.CreationTime })
            .ToList());
    }

    /// <summary>
    /// The executive director's employee contracts page (decision 2026-11-11): every active employee with the details of
    /// their current (latest) contract, or nulls when they have none. No attachments, recommendation or decision.
    /// </summary>
    [Authorize(HrPermissions.ContractPermissions.ReadOverview)]
    public virtual async Task<EmployeeContractOverviewPagedResultDto> GetOverviewAsync(ContractOverviewListInput input)
    {
        var search = input.Search?.Trim();
        var query = (await _employees.GetQueryableAsync())
            .Where(x => x.Status == EmployeeStatus.Active)
            .WhereIf(!string.IsNullOrEmpty(search), x =>
                x.FullNameAr.Contains(search!) ||
                (x.FullNameEn != null && x.FullNameEn.Contains(search!)) ||
                (x.EmployeeNumber != null && x.EmployeeNumber.Contains(search!)));

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.FullNameAr).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var references = await _references.ToRefsAsync(rows);
        var ids = rows.Select(x => x.Id).ToList();
        var current = (await _contracts.GetListAsync(x => ids.Contains(x.EmployeeId)))
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(c => c.StartDate).ThenByDescending(c => c.CreationTime).First());

        return new EmployeeContractOverviewPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x =>
            {
                var contract = current.GetValueOrDefault(x.Id);
                return new EmployeeContractOverviewDto
                {
                    Employee = references[x.Id],
                    ContractType = contract?.ContractType,
                    StartDate = contract?.StartDate,
                    EndDate = contract?.EndDate,
                    Status = contract?.Status
                };
            }).ToList()
        };
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
        var attachments = await GetAttachmentsAsync(rows.Select(x => x.Id));

        return new ContractEmployeePagedResultDto { TotalCount = total, Items = rows.Select(x => ToEmployeeDto(x, today, attachments[x.Id])).ToList() };
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

        var attachments = await GetAttachmentsAsync(new[] { contract.Id });
        return ToEmployeeDto(contract, await _companyClock.GetTodayAsync(), attachments[contract.Id]);
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
        var attachments = await GetAttachmentsAsync(contracts.Select(x => x.Id));

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
            LastModificationTime = x.LastModificationTime,
            Attachments = attachments[x.Id]
        }).ToList();
    }

    private static ContractEmployeeDto ToEmployeeDto(EmploymentContract contract, DateOnly today, List<AttachmentDto> attachments) => new()
    {
        Attachments = attachments,
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

public class ContractCreateInputValidator : AbstractValidator<ContractCreateInput>
{
    public ContractCreateInputValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.ContractType).NotNull().Length(1, EmploymentContract.MaxContractTypeLength);
        RuleFor(x => x.StartDate).NotNull();
        RuleFor(x => x.EndDate).NotNull();
        RuleFor(x => x.Attachments).Must(x => x.Count <= ContractAttachment.MaxPerContract);
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
