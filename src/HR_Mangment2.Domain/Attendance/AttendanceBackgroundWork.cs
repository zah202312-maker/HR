using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace HR_Mangment2.Attendance;

[BackgroundJobName("Hr.AttendanceRecalculation")]
public class AttendanceRecalculationArgs
{
    public Guid? TenantId { get; set; }
    public List<AttendanceRecalculationRange>? Ranges { get; set; }

    /// <summary>With <see cref="AllTo"/>: every employee who has not ended, over these dates.</summary>
    public DateOnly? AllFrom { get; set; }
    public DateOnly? AllTo { get; set; }
}

/// <summary>Each employee commits alone, so one bad record does not undo the others. A failed job is retried, which is safe.</summary>
public class AttendanceRecalculationJob : AsyncBackgroundJob<AttendanceRecalculationArgs>, ITransientDependency
{
    private readonly AttendanceDayCalculator _calculator;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public AttendanceRecalculationJob(
        AttendanceDayCalculator calculator,
        IRepository<Employee, Guid> employees,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _calculator = calculator;
        _employees = employees;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public override async Task ExecuteAsync(AttendanceRecalculationArgs args)
    {
        using (_currentTenant.Change(args.TenantId))
        {
            var ranges = new List<AttendanceRecalculationRange>(args.Ranges ?? new List<AttendanceRecalculationRange>());

            if (args.AllFrom != null && args.AllTo != null)
            {
                using var read = _unitOfWorkManager.Begin(requiresNew: true);
                var employees = await _employees.GetListAsync(x => x.Status != EmployeeStatus.Ended);
                ranges.AddRange(employees.Select(x => new AttendanceRecalculationRange { EmployeeId = x.Id, From = args.AllFrom.Value, To = args.AllTo.Value }));
                await read.CompleteAsync();
            }

            Exception? firstError = null;
            foreach (var range in ranges)
            {
                try
                {
                    using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
                    await _calculator.RecalculateAsync(range.EmployeeId, range.From, range.To);
                    await unitOfWork.CompleteAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Attendance recalculation failed for employee {EmployeeId}.", range.EmployeeId);
                    firstError ??= ex;
                }
            }

            if (firstError != null)
            {
                throw new InvalidOperationException("Attendance recalculation failed for at least one employee.", firstError);
            }
        }
    }
}

[BackgroundJobName("Hr.AttendanceImport")]
public class AttendanceImportArgs
{
    public Guid? TenantId { get; set; }
    public Guid ImportId { get; set; }
}

/// <summary>A stored row error. The text is not stored because it is localized by whoever reads it.</summary>
public class AttendanceImportRowError
{
    public int RowNumber { get; set; }
    public string? ColumnName { get; set; }
    public AttendanceImportErrorCode Code { get; set; }
}

public class AttendanceImportJob : AsyncBackgroundJob<AttendanceImportArgs>, ITransientDependency
{
    /// <summary>The total is always counted; only this many rows are kept for display.</summary>
    public const int MaxStoredErrors = 1000;

    private const int ChunkSize = 1000;
    private const int MaxAttempts = 3;

    private readonly IRepository<AttendanceImport, Guid> _imports;
    private readonly AttendancePunchWriter _writer;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly ICompanyClock _companyClock;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IClock _clock;

    public AttendanceImportJob(
        IRepository<AttendanceImport, Guid> imports,
        AttendancePunchWriter writer,
        IBlobContainer<HrFilesContainer> files,
        ICompanyClock companyClock,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        IClock clock)
    {
        _imports = imports;
        _writer = writer;
        _files = files;
        _companyClock = companyClock;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _clock = clock;
    }

    public override async Task ExecuteAsync(AttendanceImportArgs args)
    {
        using (_currentTenant.Change(args.TenantId))
        {
            byte[]? content;
            string fileName;
            string blobName;
            using (var start = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var import = await _imports.GetAsync(args.ImportId);
                if (import.Status is not (AttendanceImportStatus.Queued or AttendanceImportStatus.Processing))
                {
                    return;
                }

                import.MarkProcessing();
                await _imports.UpdateAsync(import);
                fileName = import.FileName;
                blobName = import.BlobName;
                content = await _files.GetAllBytesOrNullAsync(blobName);
                await start.CompleteAsync();
            }

            AttendanceImportFile? file = null;
            try
            {
                file = content == null ? null : AttendanceImportFileReader.Read(fileName, content);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Attendance import {ImportId} could not be read.", args.ImportId);
            }

            if (file == null)
            {
                await FinishFailedAsync(args.ImportId, blobName, new List<AttendanceImportRowError>());
                return;
            }

            if (file.MissingColumns.Count > 0)
            {
                await FinishFailedAsync(
                    args.ImportId,
                    blobName,
                    file.MissingColumns.Select(c => new AttendanceImportRowError { RowNumber = 1, ColumnName = c, Code = AttendanceImportErrorCode.MissingColumn }).ToList());
                return;
            }

            // A racing device can insert the same punch first. The whole file rolls back and the next pass counts it as a duplicate.
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await ImportRowsAsync(args.ImportId, blobName, file);
                    return;
                }
                catch (Exception ex) when (attempt < MaxAttempts)
                {
                    Logger.LogWarning(ex, "Attendance import {ImportId} attempt {Attempt} failed.", args.ImportId, attempt);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Attendance import {ImportId} failed.", args.ImportId);
                    await FinishFailedAsync(args.ImportId, blobName, new List<AttendanceImportRowError>());
                    return;
                }
            }
        }
    }

    private async Task ImportRowsAsync(Guid importId, string blobName, AttendanceImportFile file)
    {
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

        var import = await _imports.GetAsync(importId);
        var zone = await _companyClock.GetTimeZoneAsync();
        var now = _clock.Now;

        var errors = new List<AttendanceImportRowError>();
        var valid = new List<AttendancePunchInput>();

        foreach (var row in file.Rows)
        {
            var error = Validate(row, zone, now, out var punchTime);
            if (error != null)
            {
                errors.Add(error);
            }
            else
            {
                valid.Add(new AttendancePunchInput(row.RowNumber, row.DeviceUserId!.Trim(), punchTime));
            }
        }

        int accepted = 0, duplicate = 0, unmapped = 0;
        foreach (var chunk in valid.Chunk(ChunkSize))
        {
            var results = await _writer.WriteAsync(import.DeviceId, chunk, AttendancePunchSource.File, importId);
            accepted += results.Values.Count(x => x == AttendancePunchResultStatus.Accepted);
            duplicate += results.Values.Count(x => x == AttendancePunchResultStatus.Duplicate);
            unmapped += results.Values.Count(x => x == AttendancePunchResultStatus.UnmappedDeviceUser);
        }

        import.Complete(
            errors.Count > 0 ? AttendanceImportStatus.CompletedWithErrors : AttendanceImportStatus.Completed,
            file.Rows.Count,
            accepted,
            duplicate,
            unmapped,
            errors.Count,
            JsonSerializer.Serialize(errors.Take(MaxStoredErrors).ToList()),
            now);
        await _imports.UpdateAsync(import);
        await _files.DeleteAsync(blobName);

        await unitOfWork.CompleteAsync();
    }

    private static AttendanceImportRowError? Validate(AttendanceImportRow row, TimeZoneInfo zone, DateTime now, out DateTime punchTime)
    {
        punchTime = default;

        var userId = row.DeviceUserId?.Trim();
        if (string.IsNullOrEmpty(userId))
        {
            return new AttendanceImportRowError { RowNumber = row.RowNumber, ColumnName = AttendanceImportFileReader.DeviceUserIdColumn, Code = AttendanceImportErrorCode.EmptyDeviceUserId };
        }

        if (userId.Length > DeviceUserMap.MaxDeviceUserIdLength)
        {
            return new AttendanceImportRowError { RowNumber = row.RowNumber, ColumnName = AttendanceImportFileReader.DeviceUserIdColumn, Code = AttendanceImportErrorCode.InvalidDeviceUserId };
        }

        if (row.PunchLocalTime == null)
        {
            return new AttendanceImportRowError { RowNumber = row.RowNumber, ColumnName = AttendanceImportFileReader.PunchDateTimeColumn, Code = AttendanceImportErrorCode.InvalidDateTime };
        }

        var local = row.PunchLocalTime.Value;
        punchTime = CompanyClock.ToUtc(DateOnly.FromDateTime(local), TimeOnly.FromDateTime(local), zone);

        return punchTime > now
            ? new AttendanceImportRowError { RowNumber = row.RowNumber, ColumnName = AttendanceImportFileReader.PunchDateTimeColumn, Code = AttendanceImportErrorCode.FuturePunch }
            : null;
    }

    private async Task FinishFailedAsync(Guid importId, string blobName, List<AttendanceImportRowError> errors)
    {
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var import = await _imports.GetAsync(importId);
        import.Complete(AttendanceImportStatus.Failed, 0, 0, 0, 0, errors.Count, JsonSerializer.Serialize(errors), _clock.Now);
        await _imports.UpdateAsync(import);
        await _files.DeleteAsync(blobName);
        await unitOfWork.CompleteAsync();
    }
}

/// <summary>
/// Turns scheduled days into rows even when no punch arrives: this is what produces absences, and what turns a
/// day with only a checkin into incomplete once the shift has ended.
/// </summary>
public class AttendanceDailyWorker : AsyncPeriodicBackgroundWorkerBase
{
    /// <summary>Also covers a server that was down, without marking the distant past absent.</summary>
    public const int LookbackDays = 7;

    public AttendanceDailyWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var currentTenant = services.GetRequiredService<ICurrentTenant>();

        List<Guid> tenantIds;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            tenantIds = (await services.GetRequiredService<ITenantRepository>().GetListAsync()).Select(x => x.Id).ToList();
            await read.CompleteAsync();
        }

        foreach (var tenantId in tenantIds)
        {
            using (currentTenant.Change(tenantId))
            using (var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var today = await services.GetRequiredService<ICompanyClock>().GetTodayAsync();
                await services.GetRequiredService<IBackgroundJobManager>().EnqueueAsync(new AttendanceRecalculationArgs
                {
                    TenantId = tenantId,
                    AllFrom = today.AddDays(-LookbackDays),
                    AllTo = today
                });
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
