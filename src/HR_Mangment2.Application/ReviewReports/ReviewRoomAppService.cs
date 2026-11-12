using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.ReviewReports;

/// <summary>
/// The reviewer's rooms for the daily report (decision 2026-11-11): one department manager and linked employees the
/// reviewer picks. Only the reviewer and their stand-in see or change them; the executive director sees rooms only
/// inside submitted reports.
/// </summary>
[Authorize]
public class ReviewRoomAppService : HR_Mangment2AppService, IReviewRoomAppService
{
    private readonly IRepository<ReviewRoom, Guid> _rooms;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IdentityUserManager _userManager;
    private readonly EmployeeReferenceReader _references;
    private readonly ReviewerAccess _access;

    public ReviewRoomAppService(
        IRepository<ReviewRoom, Guid> rooms,
        IRepository<Employee, Guid> employees,
        IdentityUserManager userManager,
        EmployeeReferenceReader references,
        ReviewerAccess access)
    {
        _rooms = rooms;
        _employees = employees;
        _userManager = userManager;
        _references = references;
        _access = access;
    }

    [Authorize]
    public virtual async Task<List<ReviewRoomCandidateDto>> GetCandidatesAsync()
    {
        var reviewers = await RequireReviewersAsync();
        var managers = await ReadManagersAsync(reviewers);
        if (managers.Count == 0)
        {
            return new List<ReviewRoomCandidateDto>();
        }

        var managerIds = managers.Select(x => x.Id).ToList();
        var members = await _employees.GetListAsync(x =>
            x.ManagerEmployeeId != null && managerIds.Contains(x.ManagerEmployeeId.Value) && x.Status == EmployeeStatus.Active);
        var refs = await _references.ToRefsAsync(managers.Concat(members).ToList());
        var departments = await _references.GetDepartmentsAsync(managers.Select(x => x.DepartmentId));

        return managers
            .OrderBy(x => refs[x.Id].FullName, StringComparer.CurrentCulture)
            .ThenBy(x => x.Id)
            .Select(manager => new ReviewRoomCandidateDto
            {
                Manager = refs[manager.Id],
                Department = departments.GetValueOrDefault(manager.DepartmentId),
                Members = members
                    .Where(x => x.ManagerEmployeeId == manager.Id)
                    .OrderBy(x => refs[x.Id].FullName, StringComparer.CurrentCulture)
                    .ThenBy(x => x.Id)
                    .Select(x => refs[x.Id])
                    .ToList()
            }).ToList();
    }

    [Authorize]
    public virtual async Task<ReviewRoomPagedResultDto> GetListAsync(ReviewRoomListInput input)
    {
        var reviewers = await RequireReviewersAsync();
        var rooms = await _rooms.GetListAsync(x => reviewers.Contains(x.ReviewerUserId));
        var dtos = await ToDtosAsync(rooms);

        return new ReviewRoomPagedResultDto
        {
            TotalCount = dtos.Count,
            Items = dtos.Skip(input.SkipCount).Take(input.MaxResultCount).ToList()
        };
    }

    [Authorize]
    public virtual async Task<ReviewRoomDto> CreateAsync(ReviewRoomCreateDto input)
    {
        var reviewer = await _access.ResolveReviewerAsync(input.OnBehalfOfUserId);
        var members = input.MemberEmployeeIds!.Distinct().ToList();
        await EnsureValidAsync(reviewer, input.ManagerEmployeeId!.Value, members, null);

        var room = new ReviewRoom(GuidGenerator.Create(), CurrentTenant.Id, reviewer, input.Name, input.ManagerEmployeeId.Value, members);
        await _rooms.InsertAsync(room, autoSave: true);
        return (await ToDtosAsync(new List<ReviewRoom> { room })).Single();
    }

    [Authorize]
    public virtual async Task<ReviewRoomDto> GetAsync(Guid id)
    {
        var room = await LoadOwnAsync(id);
        return (await ToDtosAsync(new List<ReviewRoom> { room })).Single();
    }

    [Authorize]
    public virtual async Task<ReviewRoomDto> UpdateAsync(Guid id, ReviewRoomUpdateDto input)
    {
        var room = await LoadOwnAsync(id);
        room.ConcurrencyStamp = input.ConcurrencyStamp;

        var members = input.MemberEmployeeIds!.Distinct().ToList();
        await EnsureValidAsync(room.ReviewerUserId, input.ManagerEmployeeId!.Value, members, room.Id);
        room.Set(input.Name, input.ManagerEmployeeId.Value, members);

        await _rooms.UpdateAsync(room, autoSave: true);
        return (await ToDtosAsync(new List<ReviewRoom> { room })).Single();
    }

    [Authorize]
    public virtual async Task DeleteAsync(Guid id)
    {
        var room = await LoadOwnAsync(id);
        await _rooms.DeleteAsync(room, autoSave: true);
    }

    private async Task<List<Guid>> RequireReviewersAsync()
    {
        var reviewers = await _access.ReviewersICanActForAsync();
        if (reviewers.Count == 0)
        {
            throw new AbpAuthorizationException();
        }

        return reviewers;
    }

    private async Task<ReviewRoom> LoadOwnAsync(Guid id)
    {
        var reviewers = await RequireReviewersAsync();
        var room = await _rooms.FindAsync(id);
        if (room == null || !reviewers.Contains(room.ReviewerUserId))
        {
            throw new EntityNotFoundException(typeof(ReviewRoom), id);
        }

        return room;
    }

    /// <summary>Active department managers with an active file, never one of the given reviewers.</summary>
    private async Task<List<Employee>> ReadManagersAsync(ICollection<Guid> excludedUserIds)
    {
        var managerUserIds = (await _userManager.GetUsersInRoleAsync(HrRoles.DepartmentManager))
            .Where(x => x.IsActive && !excludedUserIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();
        if (managerUserIds.Count == 0)
        {
            return new List<Employee>();
        }

        return await _employees.GetListAsync(x => x.UserId != null && managerUserIds.Contains(x.UserId.Value) && x.Status == EmployeeStatus.Active);
    }

    private async Task EnsureValidAsync(Guid reviewer, Guid managerEmployeeId, List<Guid> memberIds, Guid? exceptRoomId)
    {
        if (!(await ReadManagersAsync(new[] { reviewer })).Any(x => x.Id == managerEmployeeId))
        {
            throw new BusinessException(HrErrorCodes.ReviewRooms.NotADepartmentManager);
        }

        var linked = await _employees.CountAsync(x =>
            memberIds.Contains(x.Id) && x.ManagerEmployeeId == managerEmployeeId && x.Status == EmployeeStatus.Active);
        if (linked != memberIds.Count)
        {
            throw new BusinessException(HrErrorCodes.ReviewRooms.MemberNotLinked);
        }

        if (await _rooms.AnyAsync(x => x.ReviewerUserId == reviewer && x.ManagerEmployeeId == managerEmployeeId && x.Id != exceptRoomId))
        {
            throw new BusinessException(HrErrorCodes.ReviewRooms.ManagerAlreadyHasRoom);
        }
    }

    /// <summary>Rooms follow their lead's name as the reader sees it; members follow their own names.</summary>
    private async Task<List<ReviewRoomDto>> ToDtosAsync(List<ReviewRoom> rooms)
    {
        var employeeIds = rooms.Select(x => x.ManagerEmployeeId).Concat(rooms.SelectMany(x => x.MemberEmployeeIds)).Distinct().ToList();
        var employees = await _employees.GetListAsync(x => employeeIds.Contains(x.Id));
        var byId = employees.ToDictionary(x => x.Id);
        var refs = await _references.ToRefsAsync(employees);
        var departments = await _references.GetDepartmentsAsync(employees.Select(x => x.DepartmentId));
        var users = await _references.GetUserRefsAsync(rooms.Select(x => x.ReviewerUserId));

        return rooms
            .Where(x => byId.ContainsKey(x.ManagerEmployeeId))
            .OrderBy(x => refs[x.ManagerEmployeeId].FullName, StringComparer.CurrentCulture)
            .ThenBy(x => x.Id)
            .Select(room => new ReviewRoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Reviewer = users[room.ReviewerUserId],
                Manager = refs[room.ManagerEmployeeId],
                Department = departments.GetValueOrDefault(byId[room.ManagerEmployeeId].DepartmentId),
                Members = room.MemberEmployeeIds
                    .Where(byId.ContainsKey)
                    .OrderBy(id => refs[id].FullName, StringComparer.CurrentCulture)
                    .ThenBy(id => id)
                    .Select(id => refs[id])
                    .ToList(),
                CreationTime = room.CreationTime,
                ConcurrencyStamp = room.ConcurrencyStamp
            }).ToList();
    }
}

public class ReviewRoomCreateDtoValidator : AbstractValidator<ReviewRoomCreateDto>
{
    public ReviewRoomCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ReviewReportConsts.MaxRoomNameLength);
        RuleFor(x => x.ManagerEmployeeId).NotNull();
        RuleFor(x => x.MemberEmployeeIds).NotNull();
        RuleFor(x => x.MemberEmployeeIds)
            .Must(ids => ids!.Count >= 1 && ids.Count <= ReviewReportConsts.MaxRoomMembers)
            .When(x => x.MemberEmployeeIds != null)
            .WithMessage($"Between 1 and {ReviewReportConsts.MaxRoomMembers} members.");
        RuleFor(x => x.MemberEmployeeIds)
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
            .When(x => x.MemberEmployeeIds != null)
            .WithMessage("Each member may appear once.");
    }
}

public class ReviewRoomUpdateDtoValidator : AbstractValidator<ReviewRoomUpdateDto>
{
    public ReviewRoomUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ReviewReportConsts.MaxRoomNameLength);
        RuleFor(x => x.ManagerEmployeeId).NotNull();
        RuleFor(x => x.MemberEmployeeIds).NotNull();
        RuleFor(x => x.MemberEmployeeIds)
            .Must(ids => ids!.Count >= 1 && ids.Count <= ReviewReportConsts.MaxRoomMembers)
            .When(x => x.MemberEmployeeIds != null)
            .WithMessage($"Between 1 and {ReviewReportConsts.MaxRoomMembers} members.");
        RuleFor(x => x.MemberEmployeeIds)
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
            .When(x => x.MemberEmployeeIds != null)
            .WithMessage("Each member may appear once.");
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
