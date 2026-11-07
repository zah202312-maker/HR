using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Errors;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Evaluations;

[Authorize(HrPermissions.EvaluationsPermissions.CriteriaManage)]
public class EvaluationCriterionAppService : HR_Mangment2AppService, IEvaluationCriterionAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["sortOrder"] = nameof(EvaluationCriterion.SortOrder),
        ["nameAr"] = nameof(EvaluationCriterion.NameAr),
        ["nameEn"] = nameof(EvaluationCriterion.NameEn)
    };

    private readonly IRepository<EvaluationCriterion, Guid> _criteria;

    public EvaluationCriterionAppService(IRepository<EvaluationCriterion, Guid> criteria)
    {
        _criteria = criteria;
    }

    public virtual async Task<EvaluationCriterionPagedResultDto> GetListAsync(EvaluationScaleListInput input)
    {
        var query = (await _criteria.GetQueryableAsync()).WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.ApplySorting(input.Sorting, nameof(EvaluationCriterion.SortOrder), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new EvaluationCriterionPagedResultDto { TotalCount = total, Items = rows.Select(ToDto).ToList() };
    }

    public virtual async Task<EvaluationCriterionDto> CreateAsync(EvaluationCriterionCreateDto input)
    {
        await CheckNamesAreFreeAsync(input, null);
        var criterion = new EvaluationCriterion(GuidGenerator.Create(), CurrentTenant.Id, input.NameAr, input.NameEn, input.SortOrder!.Value, input.IsActive!.Value);
        await _criteria.InsertAsync(criterion, autoSave: true);
        return ToDto(criterion);
    }

    public virtual async Task<EvaluationCriterionDto> UpdateAsync(Guid id, EvaluationCriterionUpdateDto input)
    {
        var criterion = await _criteria.GetAsync(id);
        await CheckNamesAreFreeAsync(input, id);

        if (criterion.IsActive && input.IsActive == false && !await _criteria.AnyAsync(x => x.IsActive && x.Id != id))
        {
            throw new BusinessException(HrErrorCodes.Evaluations.LastActiveCriterion);
        }

        criterion.ConcurrencyStamp = input.ConcurrencyStamp;
        criterion.Set(input.NameAr, input.NameEn, input.SortOrder!.Value, input.IsActive!.Value);
        await _criteria.UpdateAsync(criterion, autoSave: true);
        return ToDto(criterion);
    }

    private async Task CheckNamesAreFreeAsync(EvaluationCriterionCreateDto input, Guid? exceptId)
    {
        if (await _criteria.AnyAsync(x => (x.NameAr == input.NameAr || x.NameEn == input.NameEn) && x.Id != exceptId))
        {
            throw new HrConflictException(L["Conflict:EvaluationCriterionNameExists"]);
        }
    }

    private static EvaluationCriterionDto ToDto(EvaluationCriterion x) => new()
    {
        Id = x.Id,
        Name = LocalizedName.Pick(x.NameAr, x.NameEn),
        NameAr = x.NameAr,
        NameEn = x.NameEn,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive,
        ConcurrencyStamp = x.ConcurrencyStamp
    };
}

[Authorize(HrPermissions.EvaluationsPermissions.GradesManage)]
public class EvaluationGradeAppService : HR_Mangment2AppService, IEvaluationGradeAppService
{
    private const int MinimumActiveGrades = 2;

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["sortOrder"] = nameof(EvaluationGrade.SortOrder),
        ["code"] = nameof(EvaluationGrade.Code)
    };

    private readonly IRepository<EvaluationGrade, Guid> _grades;

    public EvaluationGradeAppService(IRepository<EvaluationGrade, Guid> grades)
    {
        _grades = grades;
    }

    public virtual async Task<EvaluationGradePagedResultDto> GetListAsync(EvaluationScaleListInput input)
    {
        var query = (await _grades.GetQueryableAsync()).WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.ApplySorting(input.Sorting, nameof(EvaluationGrade.SortOrder), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new EvaluationGradePagedResultDto { TotalCount = total, Items = rows.Select(ToDto).ToList() };
    }

    public virtual async Task<EvaluationGradeDto> CreateAsync(EvaluationGradeCreateDto input)
    {
        if (await _grades.AnyAsync(x => x.Code == input.Code))
        {
            throw new HrConflictException(L["Conflict:EvaluationGradeCodeExists"]);
        }

        var grade = new EvaluationGrade(GuidGenerator.Create(), CurrentTenant.Id, input.Code, input.NameAr, input.NameEn, input.SortOrder!.Value, input.IsActive!.Value);
        await _grades.InsertAsync(grade, autoSave: true);
        return ToDto(grade);
    }

    public virtual async Task<EvaluationGradeDto> UpdateAsync(Guid id, EvaluationGradeUpdateDto input)
    {
        var grade = await _grades.GetAsync(id);
        if (grade.IsActive && input.IsActive == false && await _grades.CountAsync(x => x.IsActive && x.Id != id) < MinimumActiveGrades)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.MinimumActiveGrades);
        }

        grade.ConcurrencyStamp = input.ConcurrencyStamp;
        grade.Set(input.NameAr, input.NameEn, input.SortOrder!.Value, input.IsActive!.Value);
        await _grades.UpdateAsync(grade, autoSave: true);
        return ToDto(grade);
    }

    private static EvaluationGradeDto ToDto(EvaluationGrade x) => new()
    {
        Id = x.Id,
        Code = x.Code,
        Name = LocalizedName.Pick(x.NameAr, x.NameEn),
        NameAr = x.NameAr,
        NameEn = x.NameEn,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive,
        ConcurrencyStamp = x.ConcurrencyStamp
    };
}

[Authorize(HrPermissions.EvaluationsPermissions.CycleRead)]
public class EvaluationCycleAppService : HR_Mangment2AppService, IEvaluationCycleAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["year"] = nameof(EvaluationCycle.Year),
        ["month"] = nameof(EvaluationCycle.Month),
        ["status"] = nameof(EvaluationCycle.Status)
    };

    private readonly IRepository<EvaluationCycle, Guid> _cycles;
    private readonly IRepository<DepartmentReport, Guid> _reports;
    private readonly MonthlyCompletionReader _completion;

    public EvaluationCycleAppService(IRepository<EvaluationCycle, Guid> cycles, IRepository<DepartmentReport, Guid> reports, MonthlyCompletionReader completion)
    {
        _cycles = cycles;
        _reports = reports;
        _completion = completion;
    }

    /// <summary>The open cycle if there is one, because on the first working days of a month the previous month is the one still being worked on.</summary>
    public virtual async Task<EvaluationCycleDto> GetCurrentAsync()
    {
        var query = await _cycles.GetQueryableAsync();
        var cycle = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Status == EvaluationCycleStatus.Open).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
                    ?? await AsyncExecuter.FirstOrDefaultAsync(query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
                    ?? throw new EntityNotFoundException(typeof(EvaluationCycle));

        return await ToDtoAsync(cycle);
    }

    public virtual async Task<EvaluationCyclePagedResultDto> GetListAsync(EvaluationCycleListInput input)
    {
        var query = (await _cycles.GetQueryableAsync()).WhereIf(input.Year != null, x => x.Year == input.Year);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(EvaluationCycle.Year) + " desc, " + nameof(EvaluationCycle.Month) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = new List<EvaluationCycleDto>();
        foreach (var row in rows)
        {
            items.Add(await ToDtoAsync(row));
        }

        return new EvaluationCyclePagedResultDto { TotalCount = total, Items = items };
    }

    private async Task<EvaluationCycleDto> ToDtoAsync(EvaluationCycle cycle) => new()
    {
        Id = cycle.Id,
        Year = cycle.Year,
        Month = cycle.Month,
        Status = cycle.Status,
        OpensAt = cycle.OpensAt,
        DataFreezeAt = cycle.DataFreezeAt,
        DueAt = cycle.DueAt,
        EscalatesAt = cycle.EscalatesAt,
        ManagerProgress = await GetProgressAsync(cycle)
    };

    private async Task<EvaluationManagerProgressDto?> GetProgressAsync(EvaluationCycle cycle)
    {
        var me = CurrentUser.GetId();
        if (!await AuthorizationService.IsGrantedAsync(HrPermissions.EvaluationsPermissions.Manage))
        {
            return null;
        }

        var staff = await _completion.GetCurrentStaffAsync(me);
        if (staff.Count == 0)
        {
            return null;
        }

        var report = await _reports.FirstOrDefaultAsync(x => x.CycleId == cycle.Id && x.ManagerUserId == me);
        var completion = await _completion.ReadAsync(cycle, staff, report, Clock.Now);

        return new EvaluationManagerProgressDto
        {
            ExpectedCount = completion.Expected,
            SubmittedCount = completion.Submitted,
            RemainingCount = completion.Expected - completion.Submitted,
            ReportId = report?.Id,
            ReportStatus = report?.Status,
            IsCompleted = completion.IsCompleted,
            IsLate = completion.IsLate
        };
    }
}

public abstract class EvaluationCriterionValidatorBase<T> : AbstractValidator<T> where T : EvaluationCriterionCreateDto
{
    protected EvaluationCriterionValidatorBase()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, EvaluationConsts.MaxCriterionNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, EvaluationConsts.MaxCriterionNameLength);
        RuleFor(x => x.SortOrder).NotNull();
        RuleFor(x => x.IsActive).NotNull();
    }
}

public class EvaluationCriterionCreateDtoValidator : EvaluationCriterionValidatorBase<EvaluationCriterionCreateDto>
{
}

public class EvaluationCriterionUpdateDtoValidator : EvaluationCriterionValidatorBase<EvaluationCriterionUpdateDto>
{
    public EvaluationCriterionUpdateDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class EvaluationGradeCreateDtoValidator : AbstractValidator<EvaluationGradeCreateDto>
{
    public EvaluationGradeCreateDtoValidator()
    {
        RuleFor(x => x.Code).NotNull().Length(1, EvaluationConsts.MaxGradeCodeLength).Matches("^[a-z][A-Za-z0-9]*$");
        RuleFor(x => x.NameAr).NotNull().Length(1, EvaluationConsts.MaxGradeNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, EvaluationConsts.MaxGradeNameLength);
        RuleFor(x => x.SortOrder).NotNull();
        RuleFor(x => x.IsActive).NotNull();
    }
}

public class EvaluationGradeUpdateDtoValidator : AbstractValidator<EvaluationGradeUpdateDto>
{
    public EvaluationGradeUpdateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, EvaluationConsts.MaxGradeNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, EvaluationConsts.MaxGradeNameLength);
        RuleFor(x => x.SortOrder).NotNull();
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
