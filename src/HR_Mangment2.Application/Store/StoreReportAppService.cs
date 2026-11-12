using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Auditing;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using HR_Mangment2.Reports;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Store;

/// <summary>The store dashboard, its reports and their export (decision 2026-11-11). Only approved documents count.</summary>
[Authorize]
public class StoreReportAppService : HR_Mangment2AppService, IStoreReportAppService
{
    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<StockMovement, Guid> _movements;
    private readonly IRepository<StorePurchase, Guid> _purchases;
    private readonly IRepository<StoreIssueVoucher, Guid> _vouchers;
    private readonly StoreReferenceReader _storeRefs;
    private readonly ItemReferenceReader _itemRefs;
    private readonly StockMovementMapper _movementMapper;
    private readonly ICompanyClock _companyClock;
    private readonly IRepository<HrAuditEntry, Guid> _auditEntries;
    private readonly IAuditingManager _auditingManager;

    public StoreReportAppService(
        IRepository<Item, Guid> items,
        IRepository<StockMovement, Guid> movements,
        IRepository<StorePurchase, Guid> purchases,
        IRepository<StoreIssueVoucher, Guid> vouchers,
        StoreReferenceReader storeRefs,
        ItemReferenceReader itemRefs,
        StockMovementMapper movementMapper,
        ICompanyClock companyClock,
        IRepository<HrAuditEntry, Guid> auditEntries,
        IAuditingManager auditingManager)
    {
        _items = items;
        _movements = movements;
        _purchases = purchases;
        _vouchers = vouchers;
        _storeRefs = storeRefs;
        _itemRefs = itemRefs;
        _movementMapper = movementMapper;
        _companyClock = companyClock;
        _auditEntries = auditEntries;
        _auditingManager = auditingManager;
    }

    [Authorize(HrPermissions.StorePermissions.StockView)]
    public virtual async Task<StoreDashboardDto> GetDashboardAsync()
    {
        var active = (await _items.GetQueryableAsync()).Where(x => x.IsActive);
        var stock = await AsyncExecuter.ToListAsync(active.Select(x => new { x.QuantityOnHand, x.MinQuantity, x.AverageUnitCost }));

        return new StoreDashboardDto
        {
            ItemsCount = stock.Count,
            PurchasesCount = await _purchases.CountAsync(x => x.Status == StoreDocumentStatus.Approved),
            IssueVouchersCount = await _vouchers.CountAsync(x => x.Status == StoreDocumentStatus.Approved),
            LowStockCount = stock.Count(x => Item.StatusOf(x.QuantityOnHand, x.MinQuantity) == StockStatus.LowStock),
            OutOfStockCount = stock.Count(x => Item.StatusOf(x.QuantityOnHand, x.MinQuantity) == StockStatus.OutOfStock),
            TotalStockValue = stock.Sum(x => Math.Round(x.QuantityOnHand * x.AverageUnitCost, StoreConsts.CostScale)),
            Currency = _storeRefs.Currency
        };
    }

    [Authorize(HrPermissions.StorePermissions.ReportView)]
    public virtual async Task<StorePurchaseReportDto> GetPurchasesAsync(StorePurchaseReportInput input)
    {
        var (from, to) = Period(input.From, input.To);
        var rows = await PurchaseRowsAsync(from, to, input.ItemId, input.Filter);
        return new StorePurchaseReportDto
        {
            From = from,
            To = to,
            PurchasesCount = rows.Select(x => x.PurchaseId).Distinct().Count(),
            TotalQuantity = rows.Sum(x => x.Quantity),
            TotalAmount = rows.Sum(x => x.LineTotal),
            Currency = _storeRefs.Currency,
            TotalCount = rows.Count,
            Items = rows.Skip(input.SkipCount).Take(input.MaxResultCount).ToList()
        };
    }

    [Authorize(HrPermissions.StorePermissions.ReportView)]
    public virtual async Task<StoreStockReportDto> GetStockAsync(StoreStockReportInput input)
    {
        var rows = await StockRowsAsync(input.StockStatus, input.CategoryId, input.Filter);
        return new StoreStockReportDto
        {
            ItemsCount = rows.Count,
            TotalStockValue = rows.Sum(x => x.StockValue),
            Currency = _storeRefs.Currency,
            TotalCount = rows.Count,
            Items = rows.Skip(input.SkipCount).Take(input.MaxResultCount).ToList()
        };
    }

    [Authorize(HrPermissions.StorePermissions.ReportView)]
    public virtual async Task<StoreTeamIssueReportDto> GetIssuesByTeamAsync(StoreTeamIssueReportInput input)
    {
        var (from, to) = Period(input.From, input.To);
        var rows = await TeamRowsAsync(from, to, input.TeamId, input.ItemId);
        return new StoreTeamIssueReportDto
        {
            From = from,
            To = to,
            TotalQuantity = rows.Sum(x => x.Quantity),
            TotalValue = rows.Sum(x => x.Value),
            Currency = _storeRefs.Currency,
            TotalCount = rows.Count,
            Items = rows.Skip(input.SkipCount).Take(input.MaxResultCount).ToList()
        };
    }

    [Authorize(HrPermissions.StorePermissions.ReportView)]
    public virtual async Task<StoreItemMovementReportDto> GetItemMovementAsync(StoreItemMovementReportInput input)
    {
        var (from, to) = Period(input.From, input.To);
        var (report, _) = await ItemMovementAsync(input.ItemId!.Value, from, to, input.SkipCount, input.MaxResultCount);
        return report;
    }

    [Authorize(HrPermissions.StorePermissions.ReportExport)]
    public virtual async Task<ReportFile> ExportAsync(StoreReportCode code, StoreReportExportInput input)
    {
        var table = await BuildTableAsync(code, input);
        var isPdf = input.Format == ReportExportFormat.Pdf;
        var content = isPdf ? ReportFileWriter.WritePdf(table, !LocalizedName.IsEnglish) : ReportFileWriter.WriteExcel(table, !LocalizedName.IsEnglish);
        var codeName = "store" + code;

        await _auditEntries.InsertAsync(
            new HrAuditEntry(GuidGenerator.Create(), CurrentTenant.Id, Clock.Now, CurrentUser.GetId(), AuditActionType.Export, "StoreReport", codeName, Array.Empty<string>(), _auditingManager.Current?.Log.ClientIpAddress, true),
            autoSave: true);

        return new ReportFile(
            content,
            isPdf ? ReportFileWriter.PdfContentType : ReportFileWriter.ExcelContentType,
            $"{JsonNamingPolicy.CamelCase.ConvertName(codeName)}-{await _companyClock.GetTodayAsync():yyyyMMdd}.{(isPdf ? "pdf" : "xlsx")}");
    }

    private async Task<ReportTable> BuildTableAsync(StoreReportCode code, StoreReportExportInput input)
    {
        static string Day(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        static string Period(DateOnly from, DateOnly to) => $"{Day(from)} - {Day(to)}";
        string Status(StockStatus s) => L["Store:StockStatus:" + JsonNamingPolicy.CamelCase.ConvertName(s.ToString())];
        string MovementType(StockMovementType t) => L["Store:MovementType:" + JsonNamingPolicy.CamelCase.ConvertName(t.ToString())];

        ReportTable Table(string? period, string[] columns, IEnumerable<object?[]> rows) => new(
            L["Report:Title:store" + code],
            period,
            columns.Select(x => (string)L["Report:Column:store" + x]).ToList(),
            rows.ToList());

        switch (code)
        {
            case StoreReportCode.Purchases:
            {
                var (from, to) = RequirePeriod(input);
                var rows = await PurchaseRowsAsync(from, to, input.ItemId, input.Filter);
                return Table(Period(from, to), new[] { "ReceiptNumber", "Supplier", "PurchaseDate", "Sku", "Item", "Unit", "Quantity", "UnitPrice", "LineTotal" },
                    rows.Select(x => new object?[] { x.ReceiptNumber, x.SupplierName, Day(x.PurchaseDate), x.Sku, x.Item.Name, x.Unit?.Name, x.Quantity, x.UnitPrice, x.LineTotal }));
            }

            case StoreReportCode.CurrentStock:
            case StoreReportCode.LowStock:
            case StoreReportCode.OutOfStock:
            case StoreReportCode.StockValue:
            {
                StockStatus? status = code switch
                {
                    StoreReportCode.LowStock => StockStatus.LowStock,
                    StoreReportCode.OutOfStock => StockStatus.OutOfStock,
                    _ => null
                };
                var rows = await StockRowsAsync(status, input.CategoryId, input.Filter);
                return Table(null, new[] { "Sku", "Item", "Category", "Unit", "QuantityOnHand", "MinQuantity", "AverageUnitCost", "StockValue", "StockStatus" },
                    rows.Select(x => new object?[] { x.Sku, x.Item.Name, x.Category?.Name, x.Unit?.Name, x.QuantityOnHand, x.MinQuantity, x.AverageUnitCost, x.StockValue, Status(x.StockStatus) }));
            }

            case StoreReportCode.IssuesByTeam:
            {
                var (from, to) = RequirePeriod(input);
                var rows = await TeamRowsAsync(from, to, input.TeamId, input.ItemId);
                return Table(Period(from, to), new[] { "Team", "Sku", "Item", "Unit", "Quantity", "Value" },
                    rows.Select(x => new object?[] { x.Team.Name, x.Sku, x.Item.Name, x.Unit?.Name, x.Quantity, x.Value }));
            }

            default:
            {
                var (from, to) = RequirePeriod(input);
                if (input.ItemId == null)
                {
                    throw new BusinessException(HrErrorCodes.Reports.MissingFilter);
                }

                var (report, zone) = await ItemMovementAsync(input.ItemId.Value, from, to, 0, int.MaxValue);
                string Local(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                return Table($"{report.Sku} {report.Item.Name} · {Period(from, to)}", new[] { "Date", "MovementType", "QuantityIn", "QuantityOut", "BalanceAfter", "DocumentNumber", "Team", "PerformedBy" },
                    report.Items.Select(x => new object?[] { Local(x.CreationTime), MovementType(x.Type), x.Quantity > 0 ? x.Quantity : null, x.Quantity < 0 ? -x.Quantity : null, x.BalanceAfter, x.DocumentNumber, x.Team?.Name, x.PerformedBy.DisplayName }));
            }
        }
    }

    private static (DateOnly From, DateOnly To) Period(DateOnly? from, DateOnly? to)
    {
        if (to < from)
        {
            throw new BusinessException(HrErrorCodes.Reports.InvalidPeriod);
        }

        return (from!.Value, to!.Value);
    }

    private static (DateOnly From, DateOnly To) RequirePeriod(StoreReportExportInput input)
    {
        if (input.From == null || input.To == null)
        {
            throw new BusinessException(HrErrorCodes.Reports.MissingFilter);
        }

        return Period(input.From, input.To);
    }

    private async Task<List<StorePurchaseReportRowDto>> PurchaseRowsAsync(DateOnly from, DateOnly to, Guid? itemId, string? filter)
    {
        var purchases = await AsyncExecuter.ToListAsync((await _purchases.WithDetailsAsync(x => x.Lines))
            .Where(x => x.Status == StoreDocumentStatus.Approved && x.PurchaseDate >= from && x.PurchaseDate <= to)
            .WhereIf(!string.IsNullOrWhiteSpace(filter), x => x.ReceiptNumber.Contains(filter!) || x.SupplierName.Contains(filter!))
            .OrderBy(x => x.PurchaseDate).ThenBy(x => x.ReceiptNumber));

        var lines = purchases.SelectMany(p => p.Lines.Where(l => itemId == null || l.ItemId == itemId).Select(l => (p, l))).ToList();
        var items = await _itemRefs.GetRefsAsync(lines.Select(x => x.l.ItemId));
        var units = await _storeRefs.GetUnitsAsync(lines.Select(x => x.l.UnitId));
        return lines.Select(x => new StorePurchaseReportRowDto
        {
            PurchaseId = x.p.Id,
            ReceiptNumber = x.p.ReceiptNumber,
            SupplierName = x.p.SupplierName,
            PurchaseDate = x.p.PurchaseDate,
            Item = items[x.l.ItemId],
            Sku = x.l.Sku,
            Unit = StoreReferenceReader.Pick(units, x.l.UnitId),
            Quantity = x.l.Quantity,
            UnitPrice = x.l.UnitPrice,
            LineTotal = x.l.LineTotal
        }).ToList();
    }

    private async Task<List<StoreStockReportRowDto>> StockRowsAsync(StockStatus? status, Guid? categoryId, string? filter)
    {
        var items = await AsyncExecuter.ToListAsync((await _items.GetQueryableAsync())
            .Where(x => x.IsActive)
            .WhereIf(categoryId != null, x => x.CategoryId == categoryId)
            .WhereStatus(status)
            .WhereIf(!string.IsNullOrWhiteSpace(filter), x => x.NameAr.Contains(filter!) || x.NameEn.Contains(filter!) || x.Sku.Contains(filter!))
            .OrderBy(x => x.Sku));

        var categories = await _storeRefs.GetCategoriesAsync(items.Select(x => x.CategoryId));
        var units = await _storeRefs.GetUnitsAsync(items.Select(x => x.UnitId));
        return items.Select(x => new StoreStockReportRowDto
        {
            Item = ItemReferenceReader.ToRef(x),
            Sku = x.Sku,
            Category = StoreReferenceReader.Pick(categories, x.CategoryId),
            Unit = StoreReferenceReader.Pick(units, x.UnitId),
            QuantityOnHand = x.QuantityOnHand,
            MinQuantity = x.MinQuantity,
            AverageUnitCost = x.AverageUnitCost,
            StockValue = x.StockValue,
            StockStatus = x.StockStatus
        }).ToList();
    }

    /// <summary>Approved vouchers only, so a reversed one drops out and the totals are net.</summary>
    private async Task<List<StoreTeamIssueReportRowDto>> TeamRowsAsync(DateOnly from, DateOnly to, Guid? teamId, Guid? itemId)
    {
        var vouchers = await AsyncExecuter.ToListAsync((await _vouchers.WithDetailsAsync(x => x.Lines))
            .Where(x => x.Status == StoreDocumentStatus.Approved && x.IssueDate >= from && x.IssueDate <= to)
            .WhereIf(teamId != null, x => x.TeamId == teamId));

        var lines = vouchers.SelectMany(v => v.Lines.Where(l => itemId == null || l.ItemId == itemId).Select(l => (v.TeamId, l))).ToList();
        var teams = await _storeRefs.GetTeamsAsync(lines.Select(x => (Guid?)x.TeamId));
        var items = await _itemRefs.GetRefsAsync(lines.Select(x => x.l.ItemId));
        var units = await _storeRefs.GetUnitsAsync(lines.Select(x => x.l.UnitId));

        return lines
            .GroupBy(x => new { x.TeamId, x.l.ItemId })
            .Select(g => new StoreTeamIssueReportRowDto
            {
                Team = teams[g.Key.TeamId],
                Item = items[g.Key.ItemId],
                Sku = g.First().l.Sku,
                Unit = StoreReferenceReader.Pick(units, g.First().l.UnitId),
                Quantity = g.Sum(x => x.l.Quantity),
                Value = g.Sum(x => x.l.LineValue ?? 0)
            })
            .OrderBy(x => x.Team.Name).ThenBy(x => x.Sku)
            .ToList();
    }

    private async Task<(StoreItemMovementReportDto Report, TimeZoneInfo Zone)> ItemMovementAsync(Guid itemId, DateOnly from, DateOnly to, int skip, int take)
    {
        var item = await _items.FindAsync(itemId) ?? throw new EntityNotFoundException(typeof(Item), itemId);
        var start = await _companyClock.StartOfDayUtcAsync(from);
        var end = await _companyClock.StartOfDayUtcAsync(to.AddDays(1));
        var all = await _movements.GetQueryableAsync();

        var opening = await AsyncExecuter.SumAsync(all.Where(x => x.ItemId == itemId && x.CreationTime < start), x => x.Quantity);
        var inPeriod = all.Where(x => x.ItemId == itemId && x.CreationTime >= start && x.CreationTime < end);
        var totalIn = await AsyncExecuter.SumAsync(inPeriod.Where(x => x.Quantity > 0), x => x.Quantity);
        var totalOut = -await AsyncExecuter.SumAsync(inPeriod.Where(x => x.Quantity < 0), x => x.Quantity);
        var count = await AsyncExecuter.LongCountAsync(inPeriod);
        var rows = await AsyncExecuter.ToListAsync(inPeriod.OrderBy(x => x.CreationTime).Skip(skip).Take(take));
        var units = await _storeRefs.GetUnitsAsync(new[] { item.UnitId });

        return (new StoreItemMovementReportDto
        {
            Item = ItemReferenceReader.ToRef(item),
            Sku = item.Sku,
            Unit = StoreReferenceReader.Pick(units, item.UnitId),
            From = from,
            To = to,
            OpeningBalance = opening,
            TotalIn = totalIn,
            TotalOut = totalOut,
            ClosingBalance = opening + totalIn - totalOut,
            TotalCount = count,
            Items = await _movementMapper.ToDtosAsync(rows)
        }, await _companyClock.GetTimeZoneAsync());
    }
}

public class StorePurchaseReportInputValidator : AbstractValidator<StorePurchaseReportInput>
{
    public StorePurchaseReportInputValidator()
    {
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.To).NotNull();
    }
}

public class StoreTeamIssueReportInputValidator : AbstractValidator<StoreTeamIssueReportInput>
{
    public StoreTeamIssueReportInputValidator()
    {
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.To).NotNull();
    }
}

public class StoreItemMovementReportInputValidator : AbstractValidator<StoreItemMovementReportInput>
{
    public StoreItemMovementReportInputValidator()
    {
        RuleFor(x => x.ItemId).NotNull();
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.To).NotNull();
    }
}

public class StoreReportExportInputValidator : AbstractValidator<StoreReportExportInput>
{
    public StoreReportExportInputValidator()
    {
        RuleFor(x => x.Format).NotNull();
    }
}
