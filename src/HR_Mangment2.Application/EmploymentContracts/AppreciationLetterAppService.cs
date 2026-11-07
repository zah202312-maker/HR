using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Files;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Appreciation;

[Authorize]
public class AppreciationLetterAppService : HR_Mangment2AppService, IAppreciationLetterAppService
{
    private const string CreateOperation = "AppreciationLetter_Create";

    private static readonly Dictionary<string, string> Sort = new()
    {
        ["title"] = nameof(AppreciationLetter.Title),
        ["issuedAt"] = nameof(AppreciationLetter.IssuedAt)
    };

    private readonly IRepository<AppreciationLetter, Guid> _letters;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdempotencyStore _idempotency;
    private readonly UploadedFileGuard _fileGuard;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly FileDownloadLinkManager _links;

    public AppreciationLetterAppService(
        IRepository<AppreciationLetter, Guid> letters,
        IRepository<Employee, Guid> employees,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications,
        IdempotencyStore idempotency,
        UploadedFileGuard fileGuard,
        IBlobContainer<HrFilesContainer> files,
        FileDownloadLinkManager links)
    {
        _letters = letters;
        _employees = employees;
        _references = references;
        _notifications = notifications;
        _idempotency = idempotency;
        _fileGuard = fileGuard;
        _files = files;
        _links = links;
    }

    [Authorize(HrPermissions.AppreciationLetterPermissions.Issue)]
    public virtual async Task<AppreciationLetterDto> CreateAsync(AppreciationLetterCreateInput input, Guid? idempotencyKey)
    {
        var fingerprint = new { input.EmployeeId, input.Title, input.Note, input.File.FileName, input.File.ContentType, input.File.Content.Length };
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return await ToDtoAsync(await _letters.GetAsync(earlier.Value));
        }

        var employee = await _employees.FindAsync(input.EmployeeId!.Value);
        if (employee == null || employee.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Appreciation.EmployeeNotFound);
        }

        if (employee.UserId == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Appreciation.SelfIssueNotAllowed);
        }

        await _fileGuard.EnsureAcceptedAsync(input.File.Content, input.File.ContentType, HrErrorCodes.Appreciation.FileRejected);

        var fileId = GuidGenerator.Create();
        var blobName = fileId.ToString("N");
        await _files.SaveAsync(blobName, input.File.Content);

        var letter = new AppreciationLetter(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            employee.Id,
            input.Title.Trim(),
            input.Note,
            CurrentUser.GetId(),
            Clock.Now,
            fileId,
            Path.GetFileName(input.File.FileName),
            input.File.ContentType,
            input.File.Content.LongLength,
            blobName);
        await _letters.InsertAsync(letter, autoSave: true);

        if (employee.UserId != null)
        {
            await _notifications.PublishAsync(employee.UserId.Value, NotificationTemplateCode.AppreciationLetterIssued, NotificationResourceType.AppreciationLetter, letter.Id);
        }

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, fingerprint, letter.Id);
        return await ToDtoAsync(letter);
    }

    [Authorize(HrPermissions.AppreciationLetterPermissions.ReadAll)]
    public virtual async Task<AppreciationLetterPagedResultDto> GetListAsync(AppreciationLetterListInput input)
    {
        var query = (await _letters.GetQueryableAsync())
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.IsAcknowledged == true, x => x.AcknowledgedAt != null)
            .WhereIf(input.IsAcknowledged == false, x => x.AcknowledgedAt == null);

        return await PageAsync(query, input);
    }

    [Authorize(HrPermissions.AppreciationLetterPermissions.ReadOwn)]
    public virtual async Task<AppreciationLetterPagedResultDto> GetMyListAsync(PagedAndSortedResultRequestDto input)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (me == null)
        {
            return new AppreciationLetterPagedResultDto();
        }

        return await PageAsync((await _letters.GetQueryableAsync()).Where(x => x.EmployeeId == me.Id), input);
    }

    [Authorize(HrPermissions.AppreciationLetterPermissions.ReadEmployeeFile)]
    public virtual async Task<AppreciationLetterPagedResultDto> GetEmployeeFileAsync(Guid employeeId, PagedAndSortedResultRequestDto input)
    {
        var employee = await _employees.FindAsync(employeeId) ?? throw new EntityNotFoundException(typeof(Employee), employeeId);
        if (!IsCompanyWideReader() && !await IsMyTeamMemberAsync(employee))
        {
            throw new EntityNotFoundException(typeof(Employee), employeeId);
        }

        return await PageAsync((await _letters.GetQueryableAsync()).Where(x => x.EmployeeId == employeeId), input);
    }

    [Authorize(HrPermissions.AppreciationLetterPermissions.Read)]
    public virtual async Task<AppreciationLetterDto> GetAsync(Guid id) => await ToDtoAsync(await LoadVisibleAsync(id));

    [Authorize(HrPermissions.AppreciationLetterPermissions.Read)]
    public virtual async Task<FileDownloadUrlDto> GetDownloadUrlAsync(Guid id)
    {
        var letter = await LoadVisibleAsync(id);
        var link = await _links.IssueAsync(letter.FileId, letter.BlobName, letter.FileName, letter.ContentType);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }

    [Authorize(HrPermissions.AppreciationLetterPermissions.Acknowledge)]
    public virtual async Task<AppreciationLetterDto> AcknowledgeAsync(Guid id)
    {
        var letter = await _letters.FindAsync(id);
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (letter == null || me == null || letter.EmployeeId != me.Id)
        {
            throw new EntityNotFoundException(typeof(AppreciationLetter), id);
        }

        if (letter.AcknowledgedAt == null)
        {
            letter.Acknowledge(Clock.Now);
            await _letters.UpdateAsync(letter, autoSave: true);
        }

        return await ToDtoAsync(letter);
    }

    private bool IsCompanyWideReader() => CurrentUser.IsInRole(HrRoles.HrManager) || CurrentUser.IsInRole(HrRoles.ExecutiveDirector);

    private async Task<bool> IsMyTeamMemberAsync(Employee employee)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        return me != null && employee.ManagerEmployeeId == me.Id;
    }

    private async Task<AppreciationLetter> LoadVisibleAsync(Guid id)
    {
        var letter = await _letters.FindAsync(id) ?? throw new EntityNotFoundException(typeof(AppreciationLetter), id);
        if (IsCompanyWideReader())
        {
            return letter;
        }

        var employee = await _employees.GetAsync(letter.EmployeeId);
        if (employee.UserId == CurrentUser.GetId() || await IsMyTeamMemberAsync(employee))
        {
            return letter;
        }

        throw new EntityNotFoundException(typeof(AppreciationLetter), id);
    }

    private async Task<AppreciationLetterPagedResultDto> PageAsync(IQueryable<AppreciationLetter> query, PagedAndSortedResultRequestDto input)
    {
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(AppreciationLetter.IssuedAt) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.IssuedByUserId));

        return new AppreciationLetterPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => ToDto(x, employees[x.EmployeeId], users[x.IssuedByUserId])).ToList()
        };
    }

    private async Task<AppreciationLetterDto> ToDtoAsync(AppreciationLetter letter)
    {
        var employees = await _references.GetRefsAsync(new[] { letter.EmployeeId });
        var users = await _references.GetUserRefsAsync(new[] { letter.IssuedByUserId });
        return ToDto(letter, employees[letter.EmployeeId], users[letter.IssuedByUserId]);
    }

    private static AppreciationLetterDto ToDto(AppreciationLetter letter, EmployeeRefDto employee, UserRefDto issuedBy) => new()
    {
        Id = letter.Id,
        Employee = employee,
        Title = letter.Title,
        Note = letter.Note,
        IssuedBy = issuedBy,
        IssuedAt = letter.IssuedAt,
        File = new AttachmentDto
        {
            Id = letter.FileId,
            FileName = letter.FileName,
            ContentType = letter.ContentType,
            SizeBytes = letter.SizeBytes,
            CreationTime = letter.IssuedAt
        },
        IsAcknowledged = letter.AcknowledgedAt != null,
        AcknowledgedAt = letter.AcknowledgedAt
    };
}

public class AppreciationLetterCreateInputValidator : AbstractValidator<AppreciationLetterCreateInput>
{
    public AppreciationLetterCreateInputValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.Title).NotNull().Length(1, AppreciationLetter.MaxTitleLength);
        RuleFor(x => x.Note).MaximumLength(AppreciationLetter.MaxNoteLength);
        RuleFor(x => x.File).NotNull();
    }
}
