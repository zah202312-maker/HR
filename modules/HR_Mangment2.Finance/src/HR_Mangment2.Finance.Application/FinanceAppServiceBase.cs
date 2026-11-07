using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Auditing;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Files;
using HR_Mangment2.Localization;
using HR_Mangment2.Reports;
using HR_Mangment2.Requests;
using HR_Mangment2.Warnings;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Finance;

/// <summary>What the finance services share: the read and export log, the employee checks, and file handling.</summary>
public abstract class FinanceAppServiceBase : HR_Mangment2AppService
{
    protected IRepository<Employee, Guid> Employees => LazyServiceProvider.LazyGetRequiredService<IRepository<Employee, Guid>>();
    protected EmployeeReferenceReader References => LazyServiceProvider.LazyGetRequiredService<EmployeeReferenceReader>();
    protected UploadedFileGuard FileGuard => LazyServiceProvider.LazyGetRequiredService<UploadedFileGuard>();
    protected IBlobContainer<HrFilesContainer> Files => LazyServiceProvider.LazyGetRequiredService<IBlobContainer<HrFilesContainer>>();
    protected FileDownloadLinkManager Links => LazyServiceProvider.LazyGetRequiredService<FileDownloadLinkManager>();
    protected PayrollPeriodGuard Periods => LazyServiceProvider.LazyGetRequiredService<PayrollPeriodGuard>();

    /// <summary>Every look at financial data leaves a row, without any of the values.</summary>
    protected Task LogReadAsync(string entityType, Guid? entityId = null) => WriteLogAsync(AuditActionType.Read, entityType, entityId?.ToString());

    protected Task LogExportAsync(string entityType, string? entityId) => WriteLogAsync(AuditActionType.Export, entityType, entityId);

    private async Task WriteLogAsync(AuditActionType action, string entityType, string? entityId)
    {
        await LazyServiceProvider.LazyGetRequiredService<IRepository<HrAuditEntry, Guid>>().InsertAsync(
            new HrAuditEntry(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                Clock.Now,
                CurrentUser.GetId(),
                action,
                entityType,
                entityId,
                Array.Empty<string>(),
                LazyServiceProvider.LazyGetRequiredService<IAuditingManager>().Current?.Log.ClientIpAddress,
                true),
            autoSave: true);
    }

    /// <summary>Nobody changes their own financial records. Such a change goes to the executive director outside this flow.</summary>
    protected async Task<Employee> LoadOtherEmployeeAsync(Guid employeeId)
    {
        var employee = await Employees.FindAsync(employeeId);
        if (employee == null)
        {
            throw new BusinessException(HrErrorCodes.Finance.EmployeeNotFound);
        }

        RefuseSelf(employee);
        return employee;
    }

    protected void RefuseSelf(Employee employee)
    {
        if (employee.UserId == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Finance.SelfChangeNotAllowed);
        }
    }

    protected async Task CheckRequestAtAccountantStepAsync(Guid? requestId)
    {
        if (requestId == null)
        {
            return;
        }

        var request = await LazyServiceProvider.LazyGetRequiredService<IRepository<Request, Guid>>().FindAsync(requestId.Value, includeDetails: false);
        if (request == null || request.Destination != RequestDestination.Accountant || request.Status != RequestStatus.PendingDestination)
        {
            throw new BusinessException(HrErrorCodes.Finance.RequestNotAtAccountantStep);
        }
    }

    protected async Task<(Guid Id, string FileName, string BlobName)> StoreAttachmentAsync(UploadedFileInput file)
    {
        await FileGuard.EnsureAcceptedAsync(file.Content, file.ContentType, HrErrorCodes.Finance.AttachmentRejected);

        var id = GuidGenerator.Create();
        var blobName = id.ToString("N");
        await Files.SaveAsync(blobName, file.Content);

        return (id, Path.GetFileName(file.FileName), blobName);
    }

    protected async Task<List<Guid>?> DepartmentEmployeeIdsAsync(Guid? departmentId)
    {
        if (departmentId == null)
        {
            return null;
        }

        return await AsyncExecuter.ToListAsync((await Employees.GetQueryableAsync()).Where(x => x.DepartmentId == departmentId).Select(x => x.Id));
    }

    protected string ExportFileName(string name, FinanceExportFormat format) =>
        $"{name}.{(format == FinanceExportFormat.Pdf ? "pdf" : "xlsx")}";

    protected ReportFile WriteFile(ReportTable table, string fileBaseName, FinanceExportFormat format)
    {
        var rightToLeft = !LocalizedName.IsEnglish;
        return format == FinanceExportFormat.Pdf
            ? new ReportFile(ReportFileWriter.WritePdf(table, rightToLeft), ReportFileWriter.PdfContentType, ExportFileName(fileBaseName, format))
            : new ReportFile(ReportFileWriter.WriteExcel(table, rightToLeft), ReportFileWriter.ExcelContentType, ExportFileName(fileBaseName, format));
    }

    protected string ColumnName(string column) => L["Report:Column:" + column];

    protected static string EnumName<T>(T value) where T : struct, Enum => JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
}
