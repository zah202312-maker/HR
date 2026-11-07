using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Evaluations;

/// <summary>
/// The only way to an internal note. Its author and the HR manager read it, and the evaluated employee gets a 404 whatever
/// their other roles. It sends no notification, by design.
/// </summary>
[Authorize]
public class EvaluationInternalNoteAppService : HR_Mangment2AppService, IEvaluationInternalNoteAppService
{
    private const string AddOperation = "EvaluationInternalNote_Add";

    private readonly IRepository<EvaluationInternalNote, Guid> _notes;
    private readonly IRepository<EvaluationInternalNoteDraft, Guid> _drafts;
    private readonly IRepository<Evaluation, Guid> _evaluations;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;

    public EvaluationInternalNoteAppService(
        IRepository<EvaluationInternalNote, Guid> notes,
        IRepository<EvaluationInternalNoteDraft, Guid> drafts,
        IRepository<Evaluation, Guid> evaluations,
        IRepository<Employee, Guid> employees,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency)
    {
        _notes = notes;
        _drafts = drafts;
        _evaluations = evaluations;
        _employees = employees;
        _references = references;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.EvaluationsPermissions.InternalNoteRead)]
    public virtual async Task<EvaluationInternalNotePagedResultDto> GetListAsync(Guid evaluationId, PagedAndSortedResultRequestDto input)
    {
        await LoadNotOwnAsync(evaluationId);

        var me = CurrentUser.GetId();
        var isHr = await AuthorizationService.IsGrantedAsync(HrPermissions.EvaluationsPermissions.DecideObjection);

        // Someone who wrote nothing gets an empty list, so a new manager cannot tell that notes exist.
        var fixedNotes = await _notes.GetListAsync(x => x.EvaluationId == evaluationId && (isHr || x.AuthorUserId == me));
        var ownDraft = await _drafts.FirstOrDefaultAsync(x => x.EvaluationId == evaluationId && x.AuthorUserId == me);

        var rows = fixedNotes
            .Select(x => new Row(x.Id, x.AuthorUserId, x.Content, false, x.CreationTime))
            .Concat(ownDraft == null ? Enumerable.Empty<Row>() : new[] { new Row(ownDraft.Id, me, ownDraft.Content, true, ownDraft.LastModificationTime ?? ownDraft.CreationTime) })
            .OrderBy(x => x.CreationTime)
            .ToList();

        var page = rows.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        var users = await _references.GetUserRefsAsync(page.Select(x => x.AuthorUserId));

        return new EvaluationInternalNotePagedResultDto
        {
            TotalCount = rows.Count,
            Items = page.Select(x => new EvaluationInternalNoteDto
            {
                Id = x.Id,
                Author = users[x.AuthorUserId],
                Content = x.Content,
                IsDraft = x.IsDraft,
                CreationTime = x.CreationTime
            }).ToList()
        };
    }

    [Authorize(HrPermissions.EvaluationsPermissions.InternalNoteWrite)]
    public virtual async Task<EvaluationInternalNoteDto> AddAsync(Guid evaluationId, EvaluationInternalNoteCreateDto input, Guid? idempotencyKey)
    {
        var fingerprint = new { evaluationId, input.Content };
        var me = CurrentUser.GetId();
        var evaluation = await LoadNotOwnAsync(evaluationId);

        var earlier = await _idempotency.FindResourceAsync(AddOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return await ToDtoAsync(await _notes.GetAsync(earlier.Value));
        }

        if (evaluation.Status == EvaluationStatus.Draft)
        {
            if (!await IsCurrentManagerAsync(evaluation, me))
            {
                throw new EntityNotFoundException(typeof(Evaluation), evaluationId);
            }

            throw new BusinessException(HrErrorCodes.Evaluations.EvaluationNotSubmitted);
        }

        if (evaluation.SubmittedByUserId != me)
        {
            throw new EntityNotFoundException(typeof(Evaluation), evaluationId);
        }

        var note = await _notes.InsertAsync(new EvaluationInternalNote(GuidGenerator.Create(), CurrentTenant.Id, evaluationId, me, input.Content), autoSave: true);
        await _idempotency.SaveAsync(AddOperation, idempotencyKey, fingerprint, note.Id);
        return await ToDtoAsync(note);
    }

    [Authorize(HrPermissions.EvaluationsPermissions.InternalNoteWrite)]
    public virtual async Task<EvaluationInternalNoteDto> SaveDraftAsync(Guid evaluationId, EvaluationInternalNoteDraftDto input)
    {
        var me = CurrentUser.GetId();
        var evaluation = await LoadNotOwnAsync(evaluationId);
        if (!await IsCurrentManagerAsync(evaluation, me))
        {
            throw new EntityNotFoundException(typeof(Evaluation), evaluationId);
        }

        if (evaluation.Status != EvaluationStatus.Draft)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.NotEditable);
        }

        var draft = await _drafts.FirstOrDefaultAsync(x => x.EvaluationId == evaluationId && x.AuthorUserId == me);
        var users = await _references.GetUserRefsAsync(new[] { me });

        if (string.IsNullOrEmpty(input.Content))
        {
            var removedId = draft?.Id ?? Guid.Empty;
            if (draft != null)
            {
                await _drafts.DeleteAsync(draft, autoSave: true);
            }

            return new EvaluationInternalNoteDto { Id = removedId, Author = users[me], Content = string.Empty, IsDraft = true, CreationTime = Clock.Now };
        }

        if (draft == null)
        {
            draft = await _drafts.InsertAsync(new EvaluationInternalNoteDraft(GuidGenerator.Create(), CurrentTenant.Id, evaluationId, me, input.Content), autoSave: true);
        }
        else
        {
            draft.SetContent(input.Content);
            await _drafts.UpdateAsync(draft, autoSave: true);
        }

        return new EvaluationInternalNoteDto { Id = draft.Id, Author = users[me], Content = draft.Content, IsDraft = true, CreationTime = draft.CreationTime };
    }

    /// <summary>The evaluated employee never reaches these routes, even holding the HR role.</summary>
    private async Task<Evaluation> LoadNotOwnAsync(Guid id)
    {
        var evaluation = await _evaluations.GetAsync(id);
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        if (owner.UserId == CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Evaluation), id);
        }

        return evaluation;
    }

    private async Task<bool> IsCurrentManagerAsync(Evaluation evaluation, Guid userId)
    {
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        return owner.ManagerEmployeeId != null && await _employees.AnyAsync(x => x.Id == owner.ManagerEmployeeId && x.UserId == userId);
    }

    private async Task<EvaluationInternalNoteDto> ToDtoAsync(EvaluationInternalNote note)
    {
        var users = await _references.GetUserRefsAsync(new[] { note.AuthorUserId });
        return new EvaluationInternalNoteDto { Id = note.Id, Author = users[note.AuthorUserId], Content = note.Content, IsDraft = false, CreationTime = note.CreationTime };
    }

    private sealed record Row(Guid Id, Guid AuthorUserId, string Content, bool IsDraft, DateTime CreationTime);
}

public class EvaluationInternalNoteCreateDtoValidator : AbstractValidator<EvaluationInternalNoteCreateDto>
{
    public EvaluationInternalNoteCreateDtoValidator()
    {
        RuleFor(x => x.Content).NotNull().Length(1, EvaluationConsts.MaxInternalNoteLength);
    }
}

public class EvaluationInternalNoteDraftDtoValidator : AbstractValidator<EvaluationInternalNoteDraftDto>
{
    public EvaluationInternalNoteDraftDtoValidator()
    {
        RuleFor(x => x.Content).MaximumLength(EvaluationConsts.MaxInternalNoteLength);
    }
}
