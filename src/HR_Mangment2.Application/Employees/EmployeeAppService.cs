using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Departments;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using HR_Mangment2.WorkShifts;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace HR_Mangment2.Employees;

[Authorize]
public class EmployeeAppService : HR_Mangment2AppService, IEmployeeAppService
{
    /// <summary>Roles that see every active employee in the lookup. A reviewer without one of them sees department managers only.</summary>
    private static readonly string[] FullLookupRoles =
    {
        HrRoles.StoreOfficer, HrRoles.HrManager, HrRoles.ExecutiveDirector, HrRoles.Accountant, HrRoles.SystemAdmin
    };

    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeeManagerLink, Guid> _links;
    private readonly IRepository<EmployeeShiftAssignment, Guid> _assignments;
    private readonly IRepository<AdministrativeTaskLogEntry, Guid> _taskLog;
    private readonly IRepository<Department, Guid> _departments;
    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _users;
    private readonly IIdentityRoleRepository _roles;
    private readonly AccountProvisioner _provisioner;
    private readonly AccountAccessEvaluator _accessEvaluator;
    private readonly UserTokenRevoker _tokenRevoker;
    private readonly ManagerLinkService _managerLinks;
    private readonly EmployeeReferenceReader _references;
    private readonly HR_Mangment2.EmploymentContracts.CurrentContractReader _currentContracts;
    private readonly ILeaveLedgerReader _leaveLedger;
    private readonly ICompanyClock _companyClock;

    public EmployeeAppService(
        IRepository<Employee, Guid> employees,
        IRepository<EmployeeManagerLink, Guid> links,
        IRepository<EmployeeShiftAssignment, Guid> assignments,
        IRepository<AdministrativeTaskLogEntry, Guid> taskLog,
        IRepository<Department, Guid> departments,
        IRepository<WorkShift, Guid> shifts,
        IdentityUserManager userManager,
        IIdentityUserRepository users,
        IIdentityRoleRepository roles,
        AccountProvisioner provisioner,
        AccountAccessEvaluator accessEvaluator,
        UserTokenRevoker tokenRevoker,
        ManagerLinkService managerLinks,
        EmployeeReferenceReader references,
        HR_Mangment2.EmploymentContracts.CurrentContractReader currentContracts,
        ILeaveLedgerReader leaveLedger,
        ICompanyClock companyClock)
    {
        _employees = employees;
        _links = links;
        _assignments = assignments;
        _taskLog = taskLog;
        _departments = departments;
        _shifts = shifts;
        _userManager = userManager;
        _users = users;
        _roles = roles;
        _provisioner = provisioner;
        _accessEvaluator = accessEvaluator;
        _tokenRevoker = tokenRevoker;
        _managerLinks = managerLinks;
        _references = references;
        _currentContracts = currentContracts;
        _leaveLedger = leaveLedger;
        _companyClock = companyClock;
    }

    private static string DefaultNameSorting => LocalizedName.IsEnglish ? nameof(Employee.FullNameEn) : nameof(Employee.FullNameAr);

    private static Dictionary<string, string> SortFields => new()
    {
        ["fullName"] = DefaultNameSorting,
        ["jobTitle"] = nameof(Employee.JobTitle),
        ["hireDate"] = nameof(Employee.HireDate),
        ["status"] = nameof(Employee.Status),
        ["creationTime"] = nameof(Employee.CreationTime)
    };

    [Authorize(HrPermissions.EmployeePermissions.Read)]
    public virtual async Task<EmployeeListItemPagedResultDto> GetListAsync(EmployeeListInput input)
    {
        var query = ApplySearch(await _employees.GetQueryableAsync(), input.Search);

        if (input.DepartmentId != null)
        {
            query = query.Where(x => x.DepartmentId == input.DepartmentId);
        }

        if (input.ManagerEmployeeId != null)
        {
            query = query.Where(x => x.ManagerEmployeeId == input.ManagerEmployeeId);
        }

        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.RoleName))
        {
            var holders = await GetRoleHolderIdsAsync(input.RoleName);
            query = query.Where(x => x.UserId != null && holders.Contains(x.UserId.Value));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, DefaultNameSorting, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var departments = await _references.GetDepartmentsAsync(items.Select(x => x.DepartmentId));
        var managers = await _references.GetRefsAsync(items.Where(x => x.ManagerEmployeeId != null).Select(x => x.ManagerEmployeeId!.Value));

        var result = new List<EmployeeListItemDto>();
        foreach (var item in items)
        {
            result.Add(new EmployeeListItemDto
            {
                Id = item.Id,
                FullName = EmployeeReferenceReader.FullName(item),
                Department = departments[item.DepartmentId],
                JobTitle = item.JobTitle,
                HireDate = item.HireDate,
                Status = item.Status,
                Manager = item.ManagerEmployeeId == null ? null : managers.GetValueOrDefault(item.ManagerEmployeeId.Value),
                RoleNames = item.UserId == null
                    ? new List<string>()
                    : (await _users.GetRoleNamesAsync(item.UserId.Value)).OrderBy(x => x).ToList(),
                HasAccount = item.UserId != null
            });
        }

        return new EmployeeListItemPagedResultDto { TotalCount = total, Items = result };
    }

    [Authorize(HrPermissions.EmployeePermissions.Create)]
    public virtual async Task<EmployeeCreatedDto> CreateAsync(EmployeeCreateDto input)
    {
        await EnsureDepartmentExistsAsync(input.DepartmentId!.Value);

        var roleNames = await _provisioner.ResolveRolesAsync(input.RoleNames, hasEmployeeFile: true);
        var isExecutive = roleNames.Contains(HrRoles.ExecutiveDirector);

        if (isExecutive && input.ManagerEmployeeId != null)
        {
            throw new BusinessException(HrErrorCodes.Employee.ExecutiveCannotHaveManager);
        }

        await _provisioner.EnsureRoleChangeAllowedAsync(null, Array.Empty<string>(), roleNames);

        if (!isExecutive)
        {
            await EnsureWorkShiftAndManagerAsync(input, roleNames);
        }

        await RefuseDuplicateNumberAsync(input.EmployeeNumber, null);

        var (user, temporaryPassword) = await _provisioner.CreateAccountAsync(input.UserName, input.Email, input.FullNameAr, roleNames);

        var employee = new Employee(GuidGenerator.Create(), CurrentTenant.Id, user.Id);
        employee.SetProfile(
            input.FullNameAr,
            input.FullNameEn,
            input.Email,
            input.PhoneNumber,
            input.DateOfBirth,
            input.Address,
            input.DepartmentId!.Value,
            input.JobTitle,
            input.HireDate!.Value,
            input.Gender,
            input.EmployeeNumber);

        if (!isExecutive)
        {
            await _managerLinks.StartAsync(employee, input.ManagerEmployeeId!.Value, input.HireDate!.Value);
            await _assignments.InsertAsync(new EmployeeShiftAssignment(GuidGenerator.Create(), CurrentTenant.Id, employee.Id, input.WorkShiftId!.Value, input.HireDate!.Value));
        }

        await _employees.InsertAsync(employee, autoSave: true);

        return new EmployeeCreatedDto
        {
            Employee = await ToHrDtoAsync(employee),
            Credentials = new TemporaryCredentialsDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                TemporaryPassword = temporaryPassword,
                MustChangePassword = true
            }
        };
    }

    [Authorize(HrPermissions.EmployeePermissions.Lookup)]
    public virtual async Task<EmployeeLookupPagedResultDto> LookupAsync(EmployeeLookupInput input)
    {
        var query = ApplySearch(await _employees.GetQueryableAsync(), input.Search).Where(x => x.Status == EmployeeStatus.Active);

        if (input.DepartmentId != null)
        {
            query = query.Where(x => x.DepartmentId == input.DepartmentId);
        }

        if (CurrentUser.IsInRole(HrRoles.Reviewer) && !FullLookupRoles.Any(CurrentUser.IsInRole))
        {
            var managers = await GetRoleHolderIdsAsync(HrRoles.DepartmentManager);
            query = query.Where(x => x.UserId != null && managers.Contains(x.UserId.Value));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, DefaultNameSorting, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var refs = await _references.ToRefsAsync(items);

        return new EmployeeLookupPagedResultDto { TotalCount = total, Items = items.Select(x => refs[x.Id]).ToList() };
    }

    [Authorize(HrPermissions.EmployeePermissions.Read)]
    public virtual async Task<EmployeeHrDto> GetAsync(Guid id) => await ToHrDtoAsync(await _employees.GetAsync(id));

    [Authorize(HrPermissions.EmployeePermissions.Update)]
    public virtual async Task<EmployeeHrDto> UpdateAsync(Guid id, EmployeeUpdateDto input)
    {
        var employee = await _employees.GetAsync(id);

        // Checked first: changing the email saves through the user manager, which reports a stale stamp as something other than 409.
        if (employee.ConcurrencyStamp != input.ConcurrencyStamp)
        {
            throw new HrConflictException(L["Conflict:RecordChanged"]);
        }

        employee.ConcurrencyStamp = input.ConcurrencyStamp;

        if (input.DepartmentId != employee.DepartmentId)
        {
            await EnsureDepartmentExistsAsync(input.DepartmentId!.Value);
        }

        if (input.HireDate != employee.HireDate && await _leaveLedger.HasEntriesAsync(id))
        {
            throw new BusinessException(HrErrorCodes.Employee.HireDateLocked);
        }

        await RefuseDuplicateNumberAsync(input.EmployeeNumber, id);

        var newStatus = input.Status!.Value;
        if (newStatus == EmployeeStatus.Ended && employee.Status != EmployeeStatus.Ended &&
            await LazyServiceProvider.LazyGetRequiredService<IRepository<HR_Mangment2.Store.Custody, Guid>>()
                .AnyAsync(x => x.EmployeeId == id && x.Status != HR_Mangment2.Store.CustodyStatus.Returned))
        {
            throw new BusinessException(HrErrorCodes.Employee.HasOutstandingCustody);
        }

        if (newStatus != EmployeeStatus.Active && employee.Status == EmployeeStatus.Active &&
            await _employees.AnyAsync(x => x.ManagerEmployeeId == id && x.Status != EmployeeStatus.Ended))
        {
            throw new BusinessException(HrErrorCodes.Employee.HasDirectReports);
        }

        var user = employee.UserId == null ? null : await _userManager.GetByIdAsync(employee.UserId.Value);
        if (user != null)
        {
            await SyncAccountEmailAsync(user, input.Email);
        }

        var deactivating = newStatus != EmployeeStatus.Active && employee.Status == EmployeeStatus.Active;

        employee.SetProfile(
            input.FullNameAr,
            input.FullNameEn,
            input.Email,
            input.PhoneNumber,
            input.DateOfBirth,
            input.Address,
            input.DepartmentId!.Value,
            input.JobTitle,
            input.HireDate!.Value,
            input.Gender,
            input.EmployeeNumber);
        employee.SetStatus(newStatus);
        await _employees.UpdateAsync(employee, autoSave: true);

        if (user != null && user.IsActive != (newStatus == EmployeeStatus.Active))
        {
            user.SetIsActive(newStatus == EmployeeStatus.Active);
            (await _userManager.UpdateAsync(user)).CheckErrors();
        }

        if (user != null && deactivating)
        {
            await _tokenRevoker.RevokeAllAsync(user.Id);
        }

        return await ToHrDtoAsync(employee);
    }

    [Authorize(HrPermissions.EmployeePermissions.AssignManager)]
    public virtual async Task<ManagerChangeResultDto> ChangeManagerAsync(Guid id, ManagerChangeInputDto input)
    {
        var employee = await _employees.GetAsync(id);
        var roles = await _managerLinks.GetRolesAsync(employee);
        var managerId = input.ManagerEmployeeId!.Value;

        if (ManagerLinkService.AllowedManagerRoles(roles) == null)
        {
            throw new BusinessException(HrErrorCodes.Employee.ExecutiveCannotHaveManager);
        }

        if (employee.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Employee.EmployeeEnded);
        }

        if (employee.ManagerEmployeeId == managerId)
        {
            throw new BusinessException(HrErrorCodes.Employee.ManagerUnchanged);
        }

        var violation = await _managerLinks.FindViolationAsync(id, roles, managerId);
        if (violation != null)
        {
            throw new BusinessException(violation);
        }

        employee.ConcurrencyStamp = input.ConcurrencyStamp;
        return await ApplyManagerChangeAsync(employee, await _employees.GetAsync(managerId), input.Reason);
    }

    [Authorize(HrPermissions.EmployeePermissions.ReadManagerHistory)]
    public virtual async Task<EmployeeManagerHistoryPagedResultDto> GetManagerHistoryAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        await _employees.GetAsync(id);

        var query = (await _links.GetQueryableAsync()).Where(x => x.EmployeeId == id);
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(
                    input.Sorting,
                    nameof(EmployeeManagerLink.FromDate) + " desc, " + nameof(EmployeeManagerLink.CreationTime) + " desc",
                    new Dictionary<string, string>
                    {
                        ["fromDate"] = nameof(EmployeeManagerLink.FromDate),
                        ["toDate"] = nameof(EmployeeManagerLink.ToDate)
                    })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var managers = await _references.GetRefsAsync(items.Select(x => x.ManagerEmployeeId));
        var changedBy = await _references.GetUserRefsAsync(items.Where(x => x.CreatorId != null).Select(x => x.CreatorId!.Value));

        return new EmployeeManagerHistoryPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new EmployeeManagerHistoryDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                Manager = managers[x.ManagerEmployeeId],
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                ChangedBy = x.CreatorId == null ? null : changedBy.GetValueOrDefault(x.CreatorId.Value),
                Reason = x.Reason
            }).ToList()
        };
    }

    [Authorize(HrPermissions.EmployeePermissions.ReadTaskLog)]
    public virtual async Task<AdministrativeTaskLogPagedResultDto> GetAdministrativeTaskLogAsync(Guid id, AdministrativeTaskLogInput input)
    {
        await _employees.GetAsync(id);
        return await QueryTaskLogAsync(id, input);
    }

    [Authorize(HrPermissions.EmployeePermissions.AssignManager)]
    public virtual async Task<ManagerAssignEmployeesResultDto> AssignEmployeesAsync(Guid managerEmployeeId, ManagerAssignEmployeesInputDto input)
    {
        var manager = await _employees.GetAsync(managerEmployeeId);
        var ids = input.EmployeeIds.Distinct().ToList();
        var employees = (await _employees.GetListAsync(x => ids.Contains(x.Id))).ToDictionary(x => x.Id);

        // Everything is checked before anything changes: one bad employee rejects the whole request.
        var invalidIds = new List<Guid>();
        string? firstViolation = null;
        foreach (var id in ids)
        {
            var violation = await FindAssignmentViolationAsync(employees.GetValueOrDefault(id), managerEmployeeId);
            if (violation != null)
            {
                invalidIds.Add(id);
                firstViolation ??= violation;
            }
        }

        if (firstViolation != null)
        {
            throw new BusinessException(firstViolation).WithData("invalidEmployeeIds", invalidIds);
        }

        var result = new ManagerAssignEmployeesResultDto { Manager = await _references.ToRefAsync(manager) };
        foreach (var id in ids)
        {
            var employee = employees[id];
            if (employee.ManagerEmployeeId == managerEmployeeId)
            {
                result.AlreadyAssignedCount++;
                continue;
            }

            var item = await ApplyManagerChangeAsync(employee, manager, input.Reason);
            result.Items.Add(item);
            result.AssignedCount++;
            result.MovedPendingItemCount += item.MovedPendingItemCount;
        }

        return result;
    }

    [Authorize(HrPermissions.EmployeePermissions.ReadTeam)]
    public virtual async Task<EmployeeTeamMemberPagedResultDto> GetMyTeamAsync(MyTeamInput input)
    {
        var me = await FindCurrentEmployeeAsync();
        if (me == null)
        {
            return new EmployeeTeamMemberPagedResultDto();
        }

        var query = ApplySearch(await _employees.GetQueryableAsync(), input.Search).Where(x => x.ManagerEmployeeId == me.Id);
        query = input.Status != null
            ? query.Where(x => x.Status == input.Status)
            : query.Where(x => x.Status != EmployeeStatus.Ended);

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, DefaultNameSorting, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var departments = await _references.GetDepartmentsAsync(items.Select(x => x.DepartmentId));
        var shifts = await _references.GetCurrentShiftsAsync(items.Select(x => x.Id));

        return new EmployeeTeamMemberPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => ToTeamMemberDto(x, departments[x.DepartmentId], shifts.GetValueOrDefault(x.Id))).ToList()
        };
    }

    [Authorize(HrPermissions.EmployeePermissions.ReadTeam)]
    public virtual async Task<EmployeeTeamMemberDto> GetTeamMemberAsync(Guid id)
    {
        var me = await FindCurrentEmployeeAsync();
        var employee = await _employees.FindAsync(id);
        if (me == null || employee == null || employee.ManagerEmployeeId != me.Id)
        {
            throw new EntityNotFoundException(typeof(Employee), id);
        }

        var departments = await _references.GetDepartmentsAsync(new[] { employee.DepartmentId });
        var shifts = await _references.GetCurrentShiftsAsync(new[] { id });
        return ToTeamMemberDto(employee, departments[employee.DepartmentId], shifts.GetValueOrDefault(id));
    }

    [Authorize(HrPermissions.EmployeePermissions.ReadSelf)]
    public virtual async Task<EmployeeSelfDto> GetMyProfileAsync()
    {
        var employee = await FindCurrentEmployeeAsync() ?? throw new EntityNotFoundException(typeof(Employee));
        var departments = await _references.GetDepartmentsAsync(new[] { employee.DepartmentId });
        var shifts = await _references.GetCurrentShiftsAsync(new[] { employee.Id });

        return new EmployeeSelfDto
        {
            Id = employee.Id,
            FullName = EmployeeReferenceReader.FullName(employee),
            FullNameAr = employee.FullNameAr,
            FullNameEn = employee.FullNameEn,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            Address = employee.Address,
            Department = departments[employee.DepartmentId],
            JobTitle = employee.JobTitle,
            HireDate = employee.HireDate,
            Status = employee.Status,
            Manager = await GetManagerRefAsync(employee),
            CurrentShift = shifts.GetValueOrDefault(employee.Id),
            CurrentContract = await _currentContracts.GetAsync(employee.Id),
            RoleNames = (await _users.GetRoleNamesAsync(employee.UserId!.Value)).OrderBy(x => x).ToList(),
            Gender = employee.Gender,
            EmployeeNumber = employee.EmployeeNumber
        };
    }

    [Authorize(HrPermissions.EmployeePermissions.ReadOwnTaskLog)]
    public virtual async Task<AdministrativeTaskLogPagedResultDto> GetMyAdministrativeTaskLogAsync(AdministrativeTaskLogInput input)
    {
        var me = await FindCurrentEmployeeAsync();
        return me == null ? new AdministrativeTaskLogPagedResultDto() : await QueryTaskLogAsync(me.Id, input);
    }

    private async Task EnsureWorkShiftAndManagerAsync(EmployeeCreateDto input, IReadOnlyCollection<string> roleNames)
    {
        if (input.WorkShiftId == null)
        {
            throw new BusinessException(HrErrorCodes.Employee.WorkShiftRequired);
        }

        var shift = await _shifts.FindAsync(input.WorkShiftId!.Value);
        if (shift == null)
        {
            throw new BusinessException(HrErrorCodes.Employee.WorkShiftNotFound);
        }

        if (!shift.IsActive)
        {
            throw new BusinessException(HrErrorCodes.Shift.ShiftInactive);
        }

        if (input.ManagerEmployeeId == null)
        {
            throw new BusinessException(HrErrorCodes.Employee.ManagerRequired);
        }

        var violation = await _managerLinks.FindViolationAsync(null, roleNames, input.ManagerEmployeeId.Value);
        if (violation != null)
        {
            throw new BusinessException(violation);
        }
    }

    private async Task EnsureDepartmentExistsAsync(Guid departmentId)
    {
        if (!await _departments.AnyAsync(x => x.Id == departmentId))
        {
            throw new BusinessException(HrErrorCodes.Department.NotFound);
        }
    }

    /// <summary>Q17: the number identifies one person. The unique index answers the race between two requests.</summary>
    private async Task RefuseDuplicateNumberAsync(string? number, Guid? ownId)
    {
        if (!string.IsNullOrWhiteSpace(number) && await _employees.AnyAsync(x => x.EmployeeNumber == number && x.Id != ownId))
        {
            throw new HrConflictException(L["Conflict:EmployeeNumberExists"]);
        }
    }

    [Authorize(HrPermissions.EmployeePermissions.CorrectHireDate)]
    public virtual async Task<EmployeeHrDto> CorrectHireDateAsync(Guid id, EmployeeHireDateCorrectionDto input)
    {
        var employee = await _employees.GetAsync(id);
        if (employee.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Employee.EmployeeEnded);
        }

        var newDate = input.HireDate!.Value;
        var oldDate = employee.HireDate;
        if (newDate == oldDate)
        {
            throw new BusinessException(HrErrorCodes.Employee.HireDateUnchanged);
        }

        employee.ConcurrencyStamp = input.ConcurrencyStamp;
        employee.CorrectHireDate(newDate);
        await _employees.UpdateAsync(employee, autoSave: true);

        await LazyServiceProvider.LazyGetRequiredService<IRepository<EmployeeHireDateCorrection, Guid>>().InsertAsync(
            new EmployeeHireDateCorrection(GuidGenerator.Create(), CurrentTenant.Id, id, oldDate, newDate, input.Reason.Trim(), CurrentUser.GetId(), Clock.Now),
            autoSave: true);

        await LazyServiceProvider.LazyGetRequiredService<HR_Mangment2.Attendance.IAttendanceRecalculator>()
            .RecalculateEmployeeFromAsync(id, newDate < oldDate ? newDate : oldDate);

        return await ToHrDtoAsync(employee);
    }

    private async Task SyncAccountEmailAsync(IdentityUser user, string? email)
    {
        if (string.Equals(user.GetRealEmail(), email, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // UserAccount_ResetPassword: the address may not change while a reset link sent to it is still valid.
        if (user.GetPasswordResetLinkExpiresAt() > Clock.Now)
        {
            throw new HrConflictException("The email cannot change while a password reset link is valid.");
        }

        if (email != null)
        {
            var owner = await _userManager.FindByEmailAsync(email);
            if (owner != null && owner.Id != user.Id)
            {
                throw new HrConflictException("The email is already in use.");
            }
        }

        (await _userManager.SetEmailAsync(user, email ?? HrUserProperties.PlaceholderEmail(user.UserName))).CheckErrors();
    }

    private async Task<string?> FindAssignmentViolationAsync(Employee? employee, Guid managerEmployeeId)
    {
        if (employee == null)
        {
            return HrErrorCodes.Employee.EmployeeNotFound;
        }

        if (employee.Status == EmployeeStatus.Ended)
        {
            return HrErrorCodes.Employee.EmployeeEnded;
        }

        return employee.ManagerEmployeeId == managerEmployeeId
            ? null
            : await _managerLinks.FindViolationAsync(employee.Id, await _managerLinks.GetRolesAsync(employee), managerEmployeeId);
    }

    private async Task<ManagerChangeResultDto> ApplyManagerChangeAsync(Employee employee, Employee newManager, string? reason)
    {
        var outcome = await _managerLinks.ChangeAsync(employee, newManager, reason);
        await _employees.UpdateAsync(employee, autoSave: true);

        return new ManagerChangeResultDto
        {
            Employee = await _references.ToRefAsync(employee),
            PreviousManager = outcome.PreviousManagerEmployeeId == null
                ? null
                : (await _references.GetRefsAsync(new[] { outcome.PreviousManagerEmployeeId.Value })).GetValueOrDefault(outcome.PreviousManagerEmployeeId.Value),
            NewManager = await _references.ToRefAsync(newManager),
            EffectiveFrom = outcome.EffectiveFrom,
            MovedPendingItemCount = outcome.MovedPendingItemCount,
            ConcurrencyStamp = employee.ConcurrencyStamp
        };
    }

    private async Task<AdministrativeTaskLogPagedResultDto> QueryTaskLogAsync(Guid employeeId, AdministrativeTaskLogInput input)
    {
        var query = (await _taskLog.GetQueryableAsync()).Where(x => x.EmployeeId == employeeId);

        if (input.FromDate != null)
        {
            var from = await _companyClock.StartOfDayUtcAsync(input.FromDate.Value);
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (input.ToDate != null)
        {
            var toExclusive = await _companyClock.StartOfDayUtcAsync(input.ToDate.Value.AddDays(1));
            query = query.Where(x => x.OccurredAt < toExclusive);
        }

        if (input.TaskType != null)
        {
            query = query.Where(x => x.TaskType == input.TaskType);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(
                    input.Sorting,
                    nameof(AdministrativeTaskLogEntry.OccurredAt) + " desc",
                    new Dictionary<string, string>
                    {
                        ["occurredAt"] = nameof(AdministrativeTaskLogEntry.OccurredAt),
                        ["taskType"] = nameof(AdministrativeTaskLogEntry.TaskType)
                    })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new AdministrativeTaskLogPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new AdministrativeTaskLogEntryDto
            {
                OccurredAt = x.OccurredAt,
                TaskType = x.TaskType,
                Title = L["TaskLog:" + x.TaskType],
                ReferenceId = x.ReferenceId
            }).ToList()
        };
    }

    private async Task<Employee?> FindCurrentEmployeeAsync()
    {
        var userId = CurrentUser.GetId();
        return await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    private async Task<EmployeeRefDto?> GetManagerRefAsync(Employee employee) =>
        employee.ManagerEmployeeId == null
            ? null
            : (await _references.GetRefsAsync(new[] { employee.ManagerEmployeeId.Value })).GetValueOrDefault(employee.ManagerEmployeeId.Value);

    private async Task<List<Guid>> GetRoleHolderIdsAsync(string roleName)
    {
        var role = await _roles.FindByNormalizedNameAsync(_userManager.NormalizeName(roleName));
        return role == null
            ? new List<Guid>()
            : (await _users.GetListAsync(roleId: role.Id, maxResultCount: int.MaxValue)).Select(x => x.Id).ToList();
    }

    private static IQueryable<Employee> ApplySearch(IQueryable<Employee> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var value = search.Trim().ToLower();
        return query.Where(x =>
            x.FullNameAr.ToLower().Contains(value) ||
            (x.FullNameEn != null && x.FullNameEn.ToLower().Contains(value)) ||
            (x.EmployeeNumber != null && x.EmployeeNumber.ToLower().Contains(value)));
    }

    private static EmployeeTeamMemberDto ToTeamMemberDto(Employee employee, DepartmentRefDto department, EmployeeShiftSummaryDto? shift) => new()
    {
        Id = employee.Id,
        FullName = EmployeeReferenceReader.FullName(employee),
        Email = employee.Email,
        PhoneNumber = employee.PhoneNumber,
        Department = department,
        JobTitle = employee.JobTitle,
        HireDate = employee.HireDate,
        Status = employee.Status,
        CurrentShift = shift
    };

    private async Task<EmployeeHrDto> ToHrDtoAsync(Employee employee)
    {
        var departments = await _references.GetDepartmentsAsync(new[] { employee.DepartmentId });
        var shifts = await _references.GetCurrentShiftsAsync(new[] { employee.Id });

        return new EmployeeHrDto
        {
            Id = employee.Id,
            FullName = EmployeeReferenceReader.FullName(employee),
            FullNameAr = employee.FullNameAr,
            FullNameEn = employee.FullNameEn,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            Address = employee.Address,
            Department = departments[employee.DepartmentId],
            JobTitle = employee.JobTitle,
            HireDate = employee.HireDate,
            Status = employee.Status,
            Manager = await GetManagerRefAsync(employee),
            CurrentShift = shifts.GetValueOrDefault(employee.Id),
            CurrentContract = await _currentContracts.GetAsync(employee.Id),
            Account = await GetAccountSummaryAsync(employee),
            DirectReportCount = await _employees.CountAsync(x => x.ManagerEmployeeId == employee.Id && x.Status != EmployeeStatus.Ended),
            CreationTime = employee.CreationTime,
            LastModificationTime = employee.LastModificationTime,
            ConcurrencyStamp = employee.ConcurrencyStamp,
            Gender = employee.Gender,
            EmployeeNumber = employee.EmployeeNumber
        };
    }

    private async Task<EmployeeAccountSummaryDto?> GetAccountSummaryAsync(Employee employee)
    {
        if (employee.UserId == null)
        {
            return null;
        }

        var user = await _users.FindAsync(employee.UserId.Value, includeDetails: false);
        if (user == null)
        {
            return null;
        }

        var state = await _accessEvaluator.GetAsync(user.Id);
        return new EmployeeAccountSummaryDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            IsActive = user.IsActive,
            MustChangePassword = state.MustChangePassword,
            MfaRequired = state.MfaRequired,
            MfaEnabled = state.MfaEnabled,
            RoleNames = (await _users.GetRoleNamesAsync(user.Id)).OrderBy(x => x).ToList()
        };
    }
}

public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
{
    public EmployeeCreateDtoValidator()
    {
        RuleFor(x => x.FullNameAr).NotNull().Length(1, Employee.MaxFullNameLength);
        RuleFor(x => x.FullNameEn).MaximumLength(Employee.MaxFullNameLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(Employee.MaxEmailLength);
        RuleFor(x => x.PhoneNumber).MaximumLength(Employee.MaxPhoneNumberLength);
        RuleFor(x => x.Address).MaximumLength(Employee.MaxAddressLength);
        RuleFor(x => x.DepartmentId).NotNull();
        RuleFor(x => x.JobTitle).NotNull().Length(1, Employee.MaxJobTitleLength);
        RuleFor(x => x.HireDate).NotNull();
        RuleFor(x => x.UserName).NotNull().Length(3, 64);
        RuleFor(x => x.RoleNames).NotEmpty();
        RuleForEach(x => x.RoleNames).NotEmpty().MaximumLength(64);
        RuleFor(x => x.EmployeeNumber).MaximumLength(Employee.MaxEmployeeNumberLength);
    }
}

public class EmployeeUpdateDtoValidator : AbstractValidator<EmployeeUpdateDto>
{
    public EmployeeUpdateDtoValidator()
    {
        RuleFor(x => x.FullNameAr).NotNull().Length(1, Employee.MaxFullNameLength);
        RuleFor(x => x.FullNameEn).MaximumLength(Employee.MaxFullNameLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(Employee.MaxEmailLength);
        RuleFor(x => x.PhoneNumber).MaximumLength(Employee.MaxPhoneNumberLength);
        RuleFor(x => x.Address).MaximumLength(Employee.MaxAddressLength);
        RuleFor(x => x.DepartmentId).NotNull();
        RuleFor(x => x.JobTitle).NotNull().Length(1, Employee.MaxJobTitleLength);
        RuleFor(x => x.HireDate).NotNull();
        RuleFor(x => x.Status).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.EmployeeNumber).MaximumLength(Employee.MaxEmployeeNumberLength);
    }
}

public class ManagerChangeInputDtoValidator : AbstractValidator<ManagerChangeInputDto>
{
    public ManagerChangeInputDtoValidator()
    {
        RuleFor(x => x.ManagerEmployeeId).NotNull();
        RuleFor(x => x.Reason).MaximumLength(EmployeeManagerLink.MaxReasonLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ManagerAssignEmployeesInputDtoValidator : AbstractValidator<ManagerAssignEmployeesInputDto>
{
    public ManagerAssignEmployeesInputDtoValidator()
    {
        RuleFor(x => x.EmployeeIds).NotEmpty().Must(x => x.Count <= 200);
        RuleFor(x => x.Reason).MaximumLength(EmployeeManagerLink.MaxReasonLength);
    }
}

public class EmployeeHireDateCorrectionDtoValidator : AbstractValidator<EmployeeHireDateCorrectionDto>
{
    public EmployeeHireDateCorrectionDtoValidator()
    {
        RuleFor(x => x.HireDate).NotNull();
        RuleFor(x => x.Reason).NotNull().Length(1, EmployeeHireDateCorrection.MaxReasonLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
