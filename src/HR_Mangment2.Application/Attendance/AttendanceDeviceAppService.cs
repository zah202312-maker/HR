using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Permissions;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Validation;

namespace HR_Mangment2.Attendance;

[Authorize]
public class AttendanceSettingsAppService : HR_Mangment2AppService, IAttendanceSettingsAppService
{
    /// <summary>Stays false until the GPS feature ships.</summary>
    private const bool GpsAvailable = false;

    private readonly HrSettingsStore _settings;

    public AttendanceSettingsAppService(HrSettingsStore settings)
    {
        _settings = settings;
    }

    [Authorize(HrPermissions.AttendancePermissions.SettingsView)]
    public virtual async Task<AttendanceSettingsDto> GetAsync() => ToDto(await _settings.GetAsync<AttendanceSettings>());

    [Authorize(HrPermissions.AttendancePermissions.SettingsManage)]
    public virtual async Task<AttendanceSettingsDto> UpdateAsync(AttendanceSettingsUpdateDto input)
    {
        var method = input.Method!.Value;
        if (method != AttendanceCaptureMethod.Device && !GpsAvailable)
        {
            throw new BusinessException(HrErrorCodes.Attendance.GpsNotAvailable);
        }

        return ToDto(await _settings.SetAsync(new AttendanceSettings { Method = method, TimeZone = input.TimeZone }, input.ConcurrencyStamp));
    }

    private static AttendanceSettingsDto ToDto(SettingsSnapshot<AttendanceSettings> snapshot) => new()
    {
        Method = snapshot.Value.Method,
        GpsAvailable = GpsAvailable,
        TimeZone = snapshot.Value.TimeZone,
        ConcurrencyStamp = snapshot.ConcurrencyStamp
    };
}

public class AttendanceSettingsUpdateDtoValidator : AbstractValidator<AttendanceSettingsUpdateDto>
{
    public AttendanceSettingsUpdateDtoValidator()
    {
        RuleFor(x => x.Method).NotNull();
        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .Must(value => TimeZoneInfo.TryConvertIanaIdToWindowsId(value, out _))
            .WithMessage("TimeZone must be a valid IANA identifier.");
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

[Authorize]
public class AttendanceDeviceAppService : HR_Mangment2AppService, IAttendanceDeviceAppService
{
    private const string CreateOperation = "AttendanceDevice_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["name"] = nameof(AttendanceDevice.Name),
        ["serialNumber"] = nameof(AttendanceDevice.SerialNumber),
        ["isActive"] = nameof(AttendanceDevice.IsActive),
        ["creationTime"] = nameof(AttendanceDevice.CreationTime)
    };

    private readonly IRepository<AttendanceDevice, Guid> _devices;
    private readonly IRepository<AttendanceDeviceActivity, Guid> _activity;
    private readonly IRepository<DeviceUserMap, Guid> _maps;
    private readonly IRepository<AttendancePunch, Guid> _punches;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly ManagerLinkService _managerLinks;
    private readonly AttendancePunchWriter _writer;
    private readonly IAttendanceRecalculator _recalculator;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;

    public AttendanceDeviceAppService(
        IRepository<AttendanceDevice, Guid> devices,
        IRepository<AttendanceDeviceActivity, Guid> activity,
        IRepository<DeviceUserMap, Guid> maps,
        IRepository<AttendancePunch, Guid> punches,
        IRepository<Employee, Guid> employees,
        ManagerLinkService managerLinks,
        AttendancePunchWriter writer,
        IAttendanceRecalculator recalculator,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency)
    {
        _devices = devices;
        _activity = activity;
        _maps = maps;
        _punches = punches;
        _employees = employees;
        _managerLinks = managerLinks;
        _writer = writer;
        _recalculator = recalculator;
        _references = references;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceView)]
    public virtual async Task<AttendanceDevicePagedResultDto> GetListAsync(AttendanceDeviceListInput input)
    {
        var query = await _devices.GetQueryableAsync();

        if (input.IsActive != null)
        {
            query = query.Where(x => x.IsActive == input.IsActive);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(AttendanceDevice.Name), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new AttendanceDevicePagedResultDto { TotalCount = total, Items = await ToDtosAsync(items) };
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceManage)]
    public virtual async Task<AttendanceDeviceKeyDto> CreateAsync(AttendanceDeviceCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            // Only the hash is kept, so the lost key cannot be sent again. A retry means the caller never saw it,
            // so a new one replaces it, and the rule that a key is shown once still holds.
            return await RotateKeyAsync(earlier.Value);
        }

        if (await _devices.AnyAsync(x => x.SerialNumber == input.SerialNumber))
        {
            throw new HrConflictException(L["Conflict:DeviceSerialExists"]);
        }

        var key = AttendanceDevice.NewKey();
        var device = new AttendanceDevice(GuidGenerator.Create(), CurrentTenant.Id, input.SerialNumber, input.Name, AttendanceDevice.HashKey(key));
        await _devices.InsertAsync(device, autoSave: true);
        await _activity.InsertAsync(new AttendanceDeviceActivity(device.Id, CurrentTenant.Id), autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, device.Id);

        return new AttendanceDeviceKeyDto { Device = (await ToDtosAsync(new[] { device })).Single(), DeviceKey = key };
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceManage)]
    public virtual async Task<AttendanceDeviceDto> UpdateAsync(Guid id, AttendanceDeviceUpdateDto input)
    {
        var device = await _devices.GetAsync(id);
        device.ConcurrencyStamp = input.ConcurrencyStamp;
        device.Set(input.Name, input.IsActive!.Value);
        await _devices.UpdateAsync(device, autoSave: true);

        return (await ToDtosAsync(new[] { device })).Single();
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceManage)]
    public virtual async Task<AttendanceDeviceKeyDto> RotateKeyAsync(Guid id)
    {
        var device = await _devices.GetAsync(id);
        var key = AttendanceDevice.NewKey();
        device.SetKeyHash(AttendanceDevice.HashKey(key));
        await _devices.UpdateAsync(device, autoSave: true);

        return new AttendanceDeviceKeyDto { Device = (await ToDtosAsync(new[] { device })).Single(), DeviceKey = key };
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceMapManage)]
    public virtual async Task<DeviceUserMapPagedResultDto> GetUserMapAsync(Guid id, DeviceUserMapListInput input)
    {
        await EnsureDeviceExistsAsync(id);

        var query = (await _maps.GetQueryableAsync()).Where(x => x.DeviceId == id);
        if (input.EmployeeId != null)
        {
            query = query.Where(x => x.EmployeeId == input.EmployeeId);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(
                    input.Sorting,
                    nameof(DeviceUserMap.CreationTime) + " desc",
                    new Dictionary<string, string>
                    {
                        ["deviceUserId"] = nameof(DeviceUserMap.DeviceUserId),
                        ["creationTime"] = nameof(DeviceUserMap.CreationTime)
                    })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var employees = await _references.GetRefsAsync(items.Select(x => x.EmployeeId));

        return new DeviceUserMapPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => ToDto(x, employees[x.EmployeeId])).ToList()
        };
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceMapManage)]
    public virtual async Task<DeviceUserMapDto> CreateUserMapAsync(Guid id, DeviceUserMapCreateDto input)
    {
        await EnsureDeviceExistsAsync(id);

        var employeeId = input.EmployeeId!.Value;
        var employee = await _employees.GetAsync(employeeId);

        if ((await _managerLinks.GetRolesAsync(employee)).Contains(HrRoles.ExecutiveDirector))
        {
            throw new BusinessException(HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
        }

        if (await _maps.AnyAsync(x => x.DeviceId == id && (x.DeviceUserId == input.DeviceUserId || x.EmployeeId == employeeId)))
        {
            throw new HrConflictException(L["Conflict:DeviceUserMapExists"]);
        }

        var map = await _maps.InsertAsync(new DeviceUserMap(GuidGenerator.Create(), CurrentTenant.Id, id, input.DeviceUserId, employeeId), autoSave: true);

        var earlier = await _punches.GetListAsync(x => x.DeviceId == id && x.DeviceUserId == input.DeviceUserId && x.EmployeeId == null);
        foreach (var punch in earlier)
        {
            punch.LinkEmployee(employeeId);
        }

        if (earlier.Count > 0)
        {
            await _punches.UpdateManyAsync(earlier, autoSave: true);
            await _recalculator.RecalculateRangesAsync(await _writer.ToRangesAsync(earlier));
        }

        return ToDto(map, await _references.ToRefAsync(employee));
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceMapManage)]
    public virtual async Task DeleteUserMapAsync(Guid id, Guid mapId)
    {
        var map = await _maps.FirstOrDefaultAsync(x => x.Id == mapId && x.DeviceId == id)
                  ?? throw new EntityNotFoundException(typeof(DeviceUserMap), mapId);
        await _maps.DeleteAsync(map);
    }

    [Authorize(HrPermissions.AttendancePermissions.DeviceMapManage)]
    public virtual async Task<UnmappedDeviceUserPagedResultDto> GetUnmappedUsersAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        await EnsureDeviceExistsAsync(id);

        // One row per user number on one device, so the list is small enough to sort and page in memory.
        var grouped = await AsyncExecuter.ToListAsync(
            (await _punches.GetQueryableAsync())
                .Where(x => x.DeviceId == id && x.EmployeeId == null)
                .GroupBy(x => x.DeviceUserId)
                .Select(g => new UnmappedDeviceUserDto
                {
                    DeviceUserId = g.Key,
                    PunchCount = g.Count(),
                    FirstPunchTime = g.Min(x => x.PunchTime),
                    LastPunchTime = g.Max(x => x.PunchTime)
                }));

        var items = grouped.AsQueryable()
            .ApplySorting(
                input.Sorting,
                nameof(UnmappedDeviceUserDto.LastPunchTime) + " desc",
                new Dictionary<string, string>
                {
                    ["deviceUserId"] = nameof(UnmappedDeviceUserDto.DeviceUserId),
                    ["punchCount"] = nameof(UnmappedDeviceUserDto.PunchCount),
                    ["firstPunchTime"] = nameof(UnmappedDeviceUserDto.FirstPunchTime),
                    ["lastPunchTime"] = nameof(UnmappedDeviceUserDto.LastPunchTime)
                },
                nameof(UnmappedDeviceUserDto.DeviceUserId))
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new UnmappedDeviceUserPagedResultDto { TotalCount = grouped.Count, Items = items };
    }

    private async Task EnsureDeviceExistsAsync(Guid id)
    {
        if (!await _devices.AnyAsync(x => x.Id == id))
        {
            throw new EntityNotFoundException(typeof(AttendanceDevice), id);
        }
    }

    private async Task<List<AttendanceDeviceDto>> ToDtosAsync(IReadOnlyCollection<AttendanceDevice> devices)
    {
        var ids = devices.Select(x => x.Id).ToList();

        var seen = (await _activity.GetListAsync(x => ids.Contains(x.Id))).ToDictionary(x => x.Id, x => x.LastSeenAt);

        var mapped = (await AsyncExecuter.ToListAsync(
                (await _maps.GetQueryableAsync())
                    .Where(x => ids.Contains(x.DeviceId))
                    .GroupBy(x => x.DeviceId)
                    .Select(g => new { DeviceId = g.Key, Count = g.Count() })))
            .ToDictionary(x => x.DeviceId, x => x.Count);

        var unmapped = (await AsyncExecuter.ToListAsync(
                (await _punches.GetQueryableAsync())
                    .Where(x => ids.Contains(x.DeviceId) && x.EmployeeId == null)
                    .Select(x => new { x.DeviceId, x.DeviceUserId })
                    .Distinct()
                    .GroupBy(x => x.DeviceId)
                    .Select(g => new { DeviceId = g.Key, Count = g.Count() })))
            .ToDictionary(x => x.DeviceId, x => x.Count);

        return devices.Select(x => new AttendanceDeviceDto
        {
            Id = x.Id,
            SerialNumber = x.SerialNumber,
            Name = x.Name,
            IsActive = x.IsActive,
            LastSeenAt = seen.GetValueOrDefault(x.Id),
            MappedUserCount = mapped.GetValueOrDefault(x.Id),
            UnmappedUserCount = unmapped.GetValueOrDefault(x.Id),
            CreationTime = x.CreationTime,
            ConcurrencyStamp = x.ConcurrencyStamp
        }).ToList();
    }

    private static DeviceUserMapDto ToDto(DeviceUserMap map, EmployeeRefDto employee) => new()
    {
        Id = map.Id,
        DeviceId = map.DeviceId,
        DeviceUserId = map.DeviceUserId,
        Employee = employee,
        CreationTime = map.CreationTime
    };
}

public class AttendanceDeviceCreateDtoValidator : AbstractValidator<AttendanceDeviceCreateDto>
{
    public AttendanceDeviceCreateDtoValidator()
    {
        RuleFor(x => x.SerialNumber).NotNull().Length(1, AttendanceDevice.MaxSerialNumberLength);
        RuleFor(x => x.Name).NotNull().Length(1, AttendanceDevice.MaxNameLength);
    }
}

public class AttendanceDeviceUpdateDtoValidator : AbstractValidator<AttendanceDeviceUpdateDto>
{
    public AttendanceDeviceUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotNull().Length(1, AttendanceDevice.MaxNameLength);
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class DeviceUserMapCreateDtoValidator : AbstractValidator<DeviceUserMapCreateDto>
{
    public DeviceUserMapCreateDtoValidator()
    {
        RuleFor(x => x.DeviceUserId).NotNull().Length(1, DeviceUserMap.MaxDeviceUserIdLength);
        RuleFor(x => x.EmployeeId).NotNull();
    }
}

/// <summary>A missing, wrong or disabled device key. The contract answers 401 for all three, with no way to tell them apart.</summary>
public class AttendanceDeviceUnauthorizedException : UserFriendlyException, IHasHttpStatusCode
{
    public AttendanceDeviceUnauthorizedException(string message)
        : base(message)
    {
    }

    public int HttpStatusCode => 401;
}

/// <summary>Anonymous on purpose: the device key is the credential, and it also decides the company.</summary>
[AllowAnonymous]
[DisableAuditing]
public class AttendancePunchAppService : HR_Mangment2AppService, IAttendancePunchAppService
{
    private const int MaxBatchSize = 1000;
    private const int MaxAttempts = 3;

    /// <summary>A small allowance for a device clock slightly ahead of the server.</summary>
    private static readonly TimeSpan FutureAllowance = TimeSpan.FromMinutes(5);

    private readonly IRepository<AttendanceDevice, Guid> _devices;
    private readonly IRepository<AttendanceDeviceActivity, Guid> _activity;
    private readonly AttendancePunchWriter _writer;
    private readonly IDataFilter _dataFilter;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public AttendancePunchAppService(
        IRepository<AttendanceDevice, Guid> devices,
        IRepository<AttendanceDeviceActivity, Guid> activity,
        AttendancePunchWriter writer,
        IDataFilter dataFilter,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _devices = devices;
        _activity = activity;
        _writer = writer;
        _dataFilter = dataFilter;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [DisableValidation]
    public virtual async Task<AttendancePunchBatchResultDto> IngestAsync(string? deviceKey, AttendancePunchBatchDto? input)
    {
        var device = await AuthenticateAsync(deviceKey);

        using (CurrentTenant.Change(device.TenantId))
        {
            var punches = input?.Punches;
            if (punches == null || punches.Count is 0 or > MaxBatchSize)
            {
                var message = $"Punches must contain between 1 and {MaxBatchSize} items.";
                throw new AbpValidationException(message, new List<ValidationResult> { new(message, new[] { "punches" }) });
            }

            var now = Clock.Now;
            var valid = new List<AttendancePunchInput>();
            var invalid = new HashSet<int>();

            for (var index = 0; index < punches.Count; index++)
            {
                var item = punches[index];
                if (item == null ||
                    string.IsNullOrWhiteSpace(item.DeviceUserId) ||
                    item.DeviceUserId.Length > DeviceUserMap.MaxDeviceUserIdLength ||
                    item.PunchTime == null ||
                    item.PunchTime > now + FutureAllowance)
                {
                    invalid.Add(index);
                    continue;
                }

                valid.Add(new AttendancePunchInput(index, item.DeviceUserId, item.PunchTime.Value));
            }

            var results = await WriteWithRetryAsync(device.Id, valid, now);

            var issues = new List<AttendancePunchIssueDto>();
            for (var index = 0; index < punches.Count; index++)
            {
                var status = invalid.Contains(index) ? AttendancePunchResultStatus.Invalid : results[index];
                if (status is AttendancePunchResultStatus.Invalid or AttendancePunchResultStatus.UnmappedDeviceUser)
                {
                    issues.Add(new AttendancePunchIssueDto
                    {
                        Index = index,
                        DeviceUserId = punches[index]?.DeviceUserId ?? string.Empty,
                        PunchTime = punches[index]?.PunchTime,
                        Status = status
                    });
                }
            }

            return new AttendancePunchBatchResultDto
            {
                ReceivedCount = punches.Count,
                AcceptedCount = results.Values.Count(x => x == AttendancePunchResultStatus.Accepted),
                DuplicateCount = results.Values.Count(x => x == AttendancePunchResultStatus.Duplicate),
                UnmappedCount = results.Values.Count(x => x == AttendancePunchResultStatus.UnmappedDeviceUser),
                InvalidCount = invalid.Count,
                Issues = issues
            };
        }
    }

    private async Task<AttendanceDevice> AuthenticateAsync(string? deviceKey)
    {
        AttendanceDevice? device = null;

        if (!string.IsNullOrEmpty(deviceKey))
        {
            var hash = AttendanceDevice.HashKey(deviceKey);
            using (_dataFilter.Disable<IMultiTenant>())
            {
                device = await _devices.FirstOrDefaultAsync(x => x.KeyHash == hash);
            }
        }

        if (device is not { IsActive: true })
        {
            throw new AttendanceDeviceUnauthorizedException(L["Attendance:DeviceKeyInvalid"]);
        }

        return device;
    }

    /// <summary>
    /// A relay and a resend, or two relays, can race on the same punch. The unique index stops the second writer and
    /// its whole batch rolls back, so the next pass sees those punches and reports them as duplicates, still with 200.
    /// </summary>
    private async Task<Dictionary<int, AttendancePunchResultStatus>> WriteWithRetryAsync(Guid deviceId, List<AttendancePunchInput> items, DateTime now)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
                var results = await _writer.WriteAsync(deviceId, items, AttendancePunchSource.Device, null);

                var activity = await _activity.GetAsync(deviceId);
                activity.MarkSeen(now);
                await _activity.UpdateAsync(activity);

                await unitOfWork.CompleteAsync();
                return results;
            }
            catch (Exception ex) when (attempt < MaxAttempts && ex.InnerException is System.Data.Common.DbException { SqlState: "23505" })
            {
                Logger.LogInformation("Punch batch for device {DeviceId} raced with another writer; retrying.", deviceId);
            }
        }
    }
}

[Authorize(HrPermissions.AttendancePermissions.Import)]
public class AttendanceImportAppService : HR_Mangment2AppService, IAttendanceImportAppService
{
    public const int MaxFileBytes = 20 * 1024 * 1024;

    private readonly IRepository<AttendanceImport, Guid> _imports;
    private readonly IRepository<AttendanceDevice, Guid> _devices;
    private readonly Volo.Abp.BlobStoring.IBlobContainer<Files.HrFilesContainer> _files;
    private readonly Volo.Abp.BackgroundJobs.IBackgroundJobManager _jobs;
    private readonly EmployeeReferenceReader _references;

    public AttendanceImportAppService(
        IRepository<AttendanceImport, Guid> imports,
        IRepository<AttendanceDevice, Guid> devices,
        Volo.Abp.BlobStoring.IBlobContainer<Files.HrFilesContainer> files,
        Volo.Abp.BackgroundJobs.IBackgroundJobManager jobs,
        EmployeeReferenceReader references)
    {
        _imports = imports;
        _devices = devices;
        _files = files;
        _jobs = jobs;
        _references = references;
    }

    public virtual async Task<AttendanceImportDto> CreateAsync(AttendanceImportUploadInput input)
    {
        var device = await _devices.GetAsync(input.DeviceId);
        if (!device.IsActive)
        {
            throw new BusinessException(HrErrorCodes.Attendance.DeviceInactive);
        }

        var fileName = System.IO.Path.GetFileName(input.FileName);
        if (input.Content.Length > MaxFileBytes ||
            fileName.Length > AttendanceImport.MaxFileNameLength ||
            !AttendanceImportFileReader.LooksValid(fileName, input.Content))
        {
            throw new BusinessException(HrErrorCodes.Attendance.ImportFileInvalid);
        }

        var id = GuidGenerator.Create();
        var blobName = "attendance-import-" + id.ToString("N");
        await _files.SaveAsync(blobName, input.Content);

        var import = new AttendanceImport(id, CurrentTenant.Id, fileName, blobName, device.Id);
        await _imports.InsertAsync(import, autoSave: true);
        await _jobs.EnqueueAsync(new AttendanceImportArgs { TenantId = CurrentTenant.Id, ImportId = id });

        return (await ToDtosAsync(new[] { import })).Single();
    }

    public virtual async Task<AttendanceImportPagedResultDto> GetListAsync(AttendanceImportListInput input)
    {
        var query = await _imports.GetQueryableAsync();

        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(
                    input.Sorting,
                    nameof(AttendanceImport.CreationTime) + " desc",
                    new Dictionary<string, string>
                    {
                        ["creationTime"] = nameof(AttendanceImport.CreationTime),
                        ["fileName"] = nameof(AttendanceImport.FileName),
                        ["status"] = nameof(AttendanceImport.Status)
                    })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new AttendanceImportPagedResultDto { TotalCount = total, Items = await ToDtosAsync(items) };
    }

    public virtual async Task<AttendanceImportDto> GetAsync(Guid id) =>
        (await ToDtosAsync(new[] { await _imports.GetAsync(id) })).Single();

    private async Task<List<AttendanceImportDto>> ToDtosAsync(IReadOnlyCollection<AttendanceImport> imports)
    {
        var uploaders = await _references.GetUserRefsAsync(imports.Where(x => x.CreatorId != null).Select(x => x.CreatorId!.Value));

        return imports.Select(x => new AttendanceImportDto
        {
            Id = x.Id,
            FileName = x.FileName,
            DeviceId = x.DeviceId,
            Status = x.Status,
            TotalRows = x.TotalRows,
            AcceptedCount = x.AcceptedCount,
            DuplicateCount = x.DuplicateCount,
            UnmappedCount = x.UnmappedCount,
            ErrorCount = x.ErrorCount,
            Errors = System.Text.Json.JsonSerializer.Deserialize<List<AttendanceImportRowError>>(x.ErrorsJson)!
                .Select(e => new AttendanceImportErrorDto
                {
                    RowNumber = e.RowNumber,
                    ColumnName = e.ColumnName,
                    Code = e.Code,
                    Message = L["AttendanceImportError:" + e.Code]
                })
                .ToList(),
            UploadedBy = x.CreatorId == null ? null! : uploaders[x.CreatorId.Value],
            CreationTime = x.CreationTime,
            CompletedAt = x.CompletedAt
        }).ToList();
    }
}
