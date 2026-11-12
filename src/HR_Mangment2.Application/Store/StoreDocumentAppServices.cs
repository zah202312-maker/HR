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
using HR_Mangment2.Warnings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace HR_Mangment2.Store;

/// <summary>The one scan a receipt or voucher may carry. A new upload replaces the old one, which stays soft deleted.</summary>
public class StoreAttachmentManager : ITransientDependency
{
    private readonly IRepository<StoreDocumentAttachment, Guid> _attachments;
    private readonly UploadedFileGuard _fileGuard;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly FileDownloadLinkManager _links;
    private readonly IGuidGenerator _guids;
    private readonly Volo.Abp.MultiTenancy.ICurrentTenant _tenant;
    private readonly Volo.Abp.Timing.IClock _clock;

    public StoreAttachmentManager(
        IRepository<StoreDocumentAttachment, Guid> attachments,
        UploadedFileGuard fileGuard,
        IBlobContainer<HrFilesContainer> files,
        FileDownloadLinkManager links,
        IGuidGenerator guids,
        Volo.Abp.MultiTenancy.ICurrentTenant tenant,
        Volo.Abp.Timing.IClock clock)
    {
        _attachments = attachments;
        _fileGuard = fileGuard;
        _files = files;
        _links = links;
        _guids = guids;
        _tenant = tenant;
        _clock = clock;
    }

    public static AttachmentDto ToDto(StoreDocumentAttachment x) =>
        new() { Id = x.Id, FileName = x.FileName, ContentType = x.ContentType, SizeBytes = x.SizeBytes, CreationTime = x.CreationTime };

    public virtual Task<StoreDocumentAttachment?> FindAsync(Guid documentId) => _attachments.FirstOrDefaultAsync(x => x.DocumentId == documentId);

    public virtual async Task<Dictionary<Guid, StoreDocumentAttachment>> FindManyAsync(IEnumerable<Guid> documentIds)
    {
        var ids = documentIds.Distinct().ToList();
        return ids.Count == 0 ? new() : (await _attachments.GetListAsync(x => ids.Contains(x.DocumentId))).GroupBy(x => x.DocumentId).ToDictionary(x => x.Key, x => x.First());
    }

    public virtual async Task<AttachmentDto> SaveAsync(Guid documentId, UploadedFileInput file, Guid userId)
    {
        await _fileGuard.EnsureAcceptedAsync(file.Content, file.ContentType, HrErrorCodes.StoreDocument.AttachmentRejected);
        await RemoveAsync(documentId, userId, required: false);

        var id = _guids.Create();
        var blobName = id.ToString("N");
        await _files.SaveAsync(blobName, file.Content);
        var attachment = await _attachments.InsertAsync(
            new StoreDocumentAttachment(id, _tenant.Id, documentId, Path.GetFileName(file.FileName), file.ContentType, file.Content.LongLength, blobName, _clock.Now, userId),
            autoSave: true);
        return ToDto(attachment);
    }

    public virtual async Task RemoveAsync(Guid documentId, Guid userId, bool required = true)
    {
        var current = await FindAsync(documentId);
        if (current == null)
        {
            if (required)
            {
                throw new EntityNotFoundException(typeof(StoreDocumentAttachment), documentId);
            }

            return;
        }

        current.MarkDeleted(userId, _clock.Now);
        await _attachments.UpdateAsync(current, autoSave: true);
    }

    public virtual async Task<FileDownloadUrlDto> LinkAsync(Guid documentId)
    {
        var current = await FindAsync(documentId) ?? throw new EntityNotFoundException(typeof(StoreDocumentAttachment), documentId);
        var link = await _links.IssueAsync(current.Id, current.BlobName, current.FileName, current.ContentType);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }
}

/// <summary>Validation shared by receipts and vouchers: active items, each once.</summary>
public abstract class StoreDocumentAppServiceBase : HR_Mangment2AppService
{
    protected IRepository<Item, Guid> Items => LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>();
    protected StockLedger Ledger => LazyServiceProvider.LazyGetRequiredService<StockLedger>();
    protected StoreAttachmentManager Attachments => LazyServiceProvider.LazyGetRequiredService<StoreAttachmentManager>();
    protected StoreReferenceReader StoreRefs => LazyServiceProvider.LazyGetRequiredService<StoreReferenceReader>();
    protected ItemReferenceReader ItemRefs => LazyServiceProvider.LazyGetRequiredService<ItemReferenceReader>();
    protected EmployeeReferenceReader References => LazyServiceProvider.LazyGetRequiredService<EmployeeReferenceReader>();
    protected IdempotencyStore Idempotency => LazyServiceProvider.LazyGetRequiredService<IdempotencyStore>();
    protected ICompanyClock CompanyClock => LazyServiceProvider.LazyGetRequiredService<ICompanyClock>();

    protected async Task<Dictionary<Guid, Item>> LoadActiveItemsAsync(IReadOnlyCollection<Guid> itemIds, string duplicateCode)
    {
        if (itemIds.Distinct().Count() != itemIds.Count)
        {
            throw new BusinessException(duplicateCode);
        }

        var items = (await Items.GetListAsync(x => itemIds.Contains(x.Id))).ToDictionary(x => x.Id);
        foreach (var id in itemIds)
        {
            if (!items.TryGetValue(id, out var item))
            {
                throw new EntityNotFoundException(typeof(Item), id);
            }

            if (!item.IsActive)
            {
                throw new BusinessException(HrErrorCodes.Stock.ItemInactive);
            }
        }

        return items;
    }

    protected async Task<Dictionary<Guid, UserRefDto>> UsersAsync(IEnumerable<Guid?> ids) =>
        await References.GetUserRefsAsync(ids.Where(x => x != null).Select(x => x!.Value));

    protected static UserRefDto? UserOf(Dictionary<Guid, UserRefDto> users, Guid? id) => id == null ? null : users.GetValueOrDefault(id.Value);
}

[Authorize]
public class StorePurchaseAppService : StoreDocumentAppServiceBase, IStorePurchaseAppService
{
    private const string CreateOperation = "StorePurchase_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["receiptNumber"] = nameof(StorePurchase.ReceiptNumber),
        ["supplierName"] = nameof(StorePurchase.SupplierName),
        ["purchaseDate"] = nameof(StorePurchase.PurchaseDate),
        ["receivedDate"] = nameof(StorePurchase.ReceivedDate),
        ["status"] = nameof(StorePurchase.Status),
        ["creationTime"] = nameof(StorePurchase.CreationTime)
    };

    private readonly IRepository<StorePurchase, Guid> _purchases;
    private readonly IRepository<StockMovement, Guid> _movements;
    private readonly IStockExpenseGateway _expenses;

    public StorePurchaseAppService(IRepository<StorePurchase, Guid> purchases, IRepository<StockMovement, Guid> movements, IStockExpenseGateway expenses)
    {
        _purchases = purchases;
        _movements = movements;
        _expenses = expenses;
    }

    [Authorize(HrPermissions.StorePermissions.PurchaseView)]
    public virtual async Task<StorePurchaseSummaryPagedResultDto> GetListAsync(StorePurchaseListInput input)
    {
        var query = (await _purchases.WithDetailsAsync(x => x.Lines))
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.FromDate != null, x => x.PurchaseDate >= input.FromDate)
            .WhereIf(input.ToDate != null, x => x.PurchaseDate <= input.ToDate)
            .WhereIf(input.ItemId != null, x => x.Lines.Any(l => l.ItemId == input.ItemId))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.ReceiptNumber.Contains(input.Filter!) || x.SupplierName.Contains(input.Filter!));

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.ApplySorting(input.Sorting, nameof(StorePurchase.PurchaseDate) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var users = await UsersAsync(rows.Select(x => x.CreatorId));
        var attachments = await Attachments.FindManyAsync(rows.Select(x => x.Id));
        return new StorePurchaseSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new StorePurchaseSummaryDto
            {
                Id = x.Id,
                ReceiptNumber = x.ReceiptNumber,
                SupplierName = x.SupplierName,
                PurchaseDate = x.PurchaseDate,
                ReceivedDate = x.ReceivedDate,
                Status = x.Status,
                LinesCount = x.Lines.Count,
                TotalQuantity = x.TotalQuantity,
                TotalAmount = x.TotalAmount,
                Currency = x.Currency,
                HasAttachment = attachments.ContainsKey(x.Id),
                CreatedBy = UserOf(users, x.CreatorId)!,
                CreationTime = x.CreationTime
            }).ToList()
        };
    }

    [Authorize(HrPermissions.StorePermissions.PurchaseManage)]
    public virtual async Task<StorePurchaseDto> CreateAsync(StorePurchaseCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await Idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var purchase = new StorePurchase(GuidGenerator.Create(), CurrentTenant.Id, StoreRefs.Currency);
        await FillAsync(purchase, input);
        await _purchases.InsertAsync(purchase, autoSave: true);
        await Idempotency.SaveAsync(CreateOperation, idempotencyKey, input, purchase.Id);
        return await ToDtoAsync(purchase);
    }

    [Authorize(HrPermissions.StorePermissions.PurchaseView)]
    public virtual async Task<StorePurchaseDto> GetAsync(Guid id) => await ToDtoAsync(await LoadAsync(id));

    [Authorize(HrPermissions.StorePermissions.PurchaseManage)]
    public virtual async Task<StorePurchaseDto> UpdateAsync(Guid id, StorePurchaseUpdateInputDto input)
    {
        var purchase = await LoadAsync(id);
        purchase.EnsureDraft();
        purchase.ConcurrencyStamp = input.ConcurrencyStamp;
        await FillAsync(purchase, input);
        await _purchases.UpdateAsync(purchase, autoSave: true);
        return await ToDtoAsync(purchase);
    }

    [Authorize(HrPermissions.StorePermissions.PurchaseManage)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var purchase = await LoadAsync(id);
        purchase.EnsureDraft();
        await Attachments.RemoveAsync(id, CurrentUser.GetId(), required: false);
        await _purchases.DeleteAsync(purchase, autoSave: true);
    }

    /// <summary>One transaction: a purchase movement per line at its price, which moves the average, and an expense per line.</summary>
    [Authorize(HrPermissions.StorePermissions.PurchaseApprove)]
    public virtual async Task<StorePurchaseDto> ApproveAsync(Guid id)
    {
        var purchase = await LoadAsync(id);
        purchase.EnsureDraft();
        var items = await LoadActiveItemsAsync(purchase.Lines.Select(x => x.ItemId).ToList(), HrErrorCodes.StorePurchase.DuplicateItem);
        var me = CurrentUser.GetId();
        var description = $"{purchase.ReceiptNumber} - {purchase.SupplierName}";

        foreach (var line in purchase.Lines)
        {
            var movement = await Ledger.RecordAsync(items[line.ItemId], new StockMovementDraft(
                StockMovementType.Purchase,
                line.Quantity,
                me,
                purchase.Note,
                TotalCost: line.LineTotal,
                UnitCost: line.UnitPrice,
                PurchaseId: purchase.Id,
                DocumentNumber: purchase.ReceiptNumber));
            line.Posted(movement.Id);

            if (line.LineTotal > 0)
            {
                await _expenses.OnPurchaseRecordedAsync(movement, description);
            }
        }

        purchase.MarkApproved(me, Clock.Now);
        await _purchases.UpdateAsync(purchase, autoSave: true);
        return await ToDtoAsync(purchase);
    }

    /// <summary>Takes every line back out at its price. Refused if some of it was already issued.</summary>
    [Authorize(HrPermissions.StorePermissions.PurchaseReverse)]
    public virtual async Task<StorePurchaseDto> ReverseAsync(Guid id, ReasonInputDto input)
    {
        var purchase = await LoadAsync(id);
        purchase.EnsureApproved();
        var items = (await Items.GetListAsync(x => purchase.Lines.Select(l => l.ItemId).Contains(x.Id))).ToDictionary(x => x.Id);
        if (purchase.Lines.Any(l => items[l.ItemId].QuantityOnHand < l.Quantity))
        {
            throw new BusinessException(HrErrorCodes.Stock.InsufficientStock);
        }

        var me = CurrentUser.GetId();
        foreach (var line in purchase.Lines)
        {
            var original = await _movements.GetAsync(line.StockMovementId!.Value);
            var reversal = await Ledger.RecordAsync(items[line.ItemId], new StockMovementDraft(
                StockMovementType.Adjustment,
                -line.Quantity,
                me,
                input.Reason,
                ReversesMovementId: original.Id,
                TotalCost: line.LineTotal,
                UnitCost: line.UnitPrice,
                PurchaseId: purchase.Id,
                DocumentNumber: purchase.ReceiptNumber));

            if (line.LineTotal > 0)
            {
                await _expenses.OnPurchaseReversedAsync(original, reversal);
            }
        }

        purchase.MarkReversed(me, Clock.Now, input.Reason.Trim());
        await _purchases.UpdateAsync(purchase, autoSave: true);
        return await ToDtoAsync(purchase);
    }

    [Authorize(HrPermissions.StorePermissions.PurchaseManage)]
    public virtual async Task<AttachmentDto> UploadAttachmentAsync(Guid id, UploadedFileInput file)
    {
        await LoadAsync(id);
        return await Attachments.SaveAsync(id, file, CurrentUser.GetId());
    }

    [Authorize(HrPermissions.StorePermissions.PurchaseManage)]
    public virtual async Task DeleteAttachmentAsync(Guid id)
    {
        await LoadAsync(id);
        await Attachments.RemoveAsync(id, CurrentUser.GetId());
    }

    [Authorize(HrPermissions.StorePermissions.PurchaseView)]
    public virtual async Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id)
    {
        await LoadAsync(id);
        return await Attachments.LinkAsync(id);
    }

    private async Task<StorePurchase> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await _purchases.WithDetailsAsync(x => x.Lines)).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(StorePurchase), id);

    private async Task FillAsync(StorePurchase purchase, StorePurchaseCreateInputDto input)
    {
        var receipt = input.ReceiptNumber.Trim();
        if (await _purchases.AnyAsync(x => x.ReceiptNumber == receipt && x.Id != purchase.Id))
        {
            throw new BusinessException(HrErrorCodes.StorePurchase.ReceiptNumberAlreadyExists);
        }

        var today = await CompanyClock.GetTodayAsync();
        if (input.PurchaseDate > today || input.ReceivedDate > today || input.ReceivedDate < input.PurchaseDate)
        {
            throw new BusinessException(HrErrorCodes.StorePurchase.InvalidDates);
        }

        var items = await LoadActiveItemsAsync(input.Lines.Select(x => x.ItemId!.Value).ToList(), HrErrorCodes.StorePurchase.DuplicateItem);
        var lines = input.Lines.Select(x => new StorePurchaseLine(GuidGenerator.Create(), CurrentTenant.Id, purchase.Id, items[x.ItemId!.Value], x.Quantity!.Value, x.UnitPrice!.Value)).ToList();
        purchase.Set(receipt, input.SupplierName.Trim(), input.PurchaseDate!.Value, input.ReceivedDate!.Value, string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim(), lines);
    }

    private async Task<StorePurchaseDto> ToDtoAsync(StorePurchase x)
    {
        var items = await ItemRefs.GetRefsAsync(x.Lines.Select(l => l.ItemId));
        var categories = await StoreRefs.GetCategoriesAsync(x.Lines.Select(l => l.CategoryId));
        var units = await StoreRefs.GetUnitsAsync(x.Lines.Select(l => l.UnitId));
        var users = await UsersAsync(new[] { x.CreatorId, x.ApprovedByUserId, x.ReversedByUserId });
        var attachment = await Attachments.FindAsync(x.Id);

        return new StorePurchaseDto
        {
            Id = x.Id,
            ReceiptNumber = x.ReceiptNumber,
            SupplierName = x.SupplierName,
            PurchaseDate = x.PurchaseDate,
            ReceivedDate = x.ReceivedDate,
            Note = x.Note,
            Status = x.Status,
            Currency = x.Currency,
            Lines = x.Lines.Select(l => new StorePurchaseLineDto
            {
                Id = l.Id,
                Item = items[l.ItemId],
                Sku = l.Sku,
                Category = StoreReferenceReader.Pick(categories, l.CategoryId),
                Unit = StoreReferenceReader.Pick(units, l.UnitId),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList(),
            TotalQuantity = x.TotalQuantity,
            TotalAmount = x.TotalAmount,
            Attachment = attachment == null ? null : StoreAttachmentManager.ToDto(attachment),
            ApprovedAt = x.ApprovedAt,
            ApprovedBy = UserOf(users, x.ApprovedByUserId),
            ReversedAt = x.ReversedAt,
            ReversedBy = UserOf(users, x.ReversedByUserId),
            ReverseReason = x.ReverseReason,
            CreatedBy = UserOf(users, x.CreatorId)!,
            ConcurrencyStamp = x.ConcurrencyStamp,
            CreationTime = x.CreationTime,
            LastModificationTime = x.LastModificationTime
        };
    }
}

[Authorize]
public class StoreIssueVoucherAppService : StoreDocumentAppServiceBase, IStoreIssueVoucherAppService
{
    private const string CreateOperation = "StoreIssueVoucher_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["voucherNumber"] = nameof(StoreIssueVoucher.VoucherNumber),
        ["issueDate"] = nameof(StoreIssueVoucher.IssueDate),
        ["status"] = nameof(StoreIssueVoucher.Status),
        ["creationTime"] = nameof(StoreIssueVoucher.CreationTime)
    };

    private readonly IRepository<StoreIssueVoucher, Guid> _vouchers;
    private readonly IRepository<StoreTeam, Guid> _teams;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<Custody, Guid> _custodies;
    private readonly CustodyDepartmentScope _scope;
    private readonly HrNotificationPublisher _notifications;

    public StoreIssueVoucherAppService(
        IRepository<StoreIssueVoucher, Guid> vouchers,
        IRepository<StoreTeam, Guid> teams,
        IRepository<Employee, Guid> employees,
        IRepository<Custody, Guid> custodies,
        CustodyDepartmentScope scope,
        HrNotificationPublisher notifications)
    {
        _vouchers = vouchers;
        _teams = teams;
        _employees = employees;
        _custodies = custodies;
        _scope = scope;
        _notifications = notifications;
    }

    [Authorize(HrPermissions.StorePermissions.IssueVoucherView)]
    public virtual async Task<StoreIssueVoucherSummaryPagedResultDto> GetListAsync(StoreIssueVoucherListInput input)
    {
        var query = (await _vouchers.WithDetailsAsync(x => x.Lines))
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.TeamId != null, x => x.TeamId == input.TeamId)
            .WhereIf(input.FromDate != null, x => x.IssueDate >= input.FromDate)
            .WhereIf(input.ToDate != null, x => x.IssueDate <= input.ToDate)
            .WhereIf(input.ItemId != null, x => x.Lines.Any(l => l.ItemId == input.ItemId))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.VoucherNumber.Contains(input.Filter!));

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.ApplySorting(input.Sorting, nameof(StoreIssueVoucher.IssueDate) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var users = await UsersAsync(rows.Select(x => x.CreatorId));
        var teams = await StoreRefs.GetTeamsAsync(rows.Select(x => (Guid?)x.TeamId));
        var employees = await References.GetRefsAsync(rows.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value));
        var attachments = await Attachments.FindManyAsync(rows.Select(x => x.Id));
        return new StoreIssueVoucherSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new StoreIssueVoucherSummaryDto
            {
                Id = x.Id,
                VoucherNumber = x.VoucherNumber,
                IssueDate = x.IssueDate,
                Team = teams[x.TeamId],
                Employee = x.EmployeeId == null ? null : employees.GetValueOrDefault(x.EmployeeId.Value),
                Status = x.Status,
                LinesCount = x.Lines.Count,
                TotalQuantity = x.TotalQuantity,
                TotalValue = x.TotalValue,
                Currency = x.Currency,
                HasAttachment = attachments.ContainsKey(x.Id),
                CreatedBy = UserOf(users, x.CreatorId)!,
                CreationTime = x.CreationTime
            }).ToList()
        };
    }

    [Authorize(HrPermissions.StorePermissions.IssueVoucherManage)]
    public virtual async Task<StoreIssueVoucherDto> CreateAsync(StoreIssueVoucherCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await Idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var voucher = new StoreIssueVoucher(GuidGenerator.Create(), CurrentTenant.Id, StoreRefs.Currency);
        await FillAsync(voucher, input);
        await _vouchers.InsertAsync(voucher, autoSave: true);
        await Idempotency.SaveAsync(CreateOperation, idempotencyKey, input, voucher.Id);
        return await ToDtoAsync(voucher);
    }

    [Authorize(HrPermissions.StorePermissions.IssueVoucherView)]
    public virtual async Task<StoreIssueVoucherDto> GetAsync(Guid id) => await ToDtoAsync(await LoadAsync(id));

    [Authorize(HrPermissions.StorePermissions.IssueVoucherManage)]
    public virtual async Task<StoreIssueVoucherDto> UpdateAsync(Guid id, StoreIssueVoucherUpdateInputDto input)
    {
        var voucher = await LoadAsync(id);
        voucher.EnsureDraft();
        voucher.ConcurrencyStamp = input.ConcurrencyStamp;
        await FillAsync(voucher, input);
        await _vouchers.UpdateAsync(voucher, autoSave: true);
        return await ToDtoAsync(voucher);
    }

    [Authorize(HrPermissions.StorePermissions.IssueVoucherManage)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var voucher = await LoadAsync(id);
        voucher.EnsureDraft();
        await Attachments.RemoveAsync(id, CurrentUser.GetId(), required: false);
        await _vouchers.DeleteAsync(voucher, autoSave: true);
    }

    /// <summary>
    /// One transaction: an issue movement per line at the current average cost, never past the balance. A returnable
    /// item becomes custody: personal for the named employee, otherwise shared on the team's department.
    /// </summary>
    [Authorize(HrPermissions.StorePermissions.IssueVoucherApprove)]
    public virtual async Task<StoreIssueVoucherDto> ApproveAsync(Guid id)
    {
        var voucher = await LoadAsync(id);
        voucher.EnsureDraft();
        var team = await CheckTeamAndEmployeeAsync(voucher.TeamId, voucher.EmployeeId);
        var items = await LoadActiveItemsAsync(voucher.Lines.Select(x => x.ItemId).ToList(), HrErrorCodes.StoreIssueVoucher.DuplicateItem);

        foreach (var line in voucher.Lines)
        {
            if (items[line.ItemId].QuantityOnHand < line.Quantity)
            {
                throw new BusinessException(HrErrorCodes.Stock.InsufficientStock);
            }
        }

        if (voucher.EmployeeId == null && team.DepartmentId == null && voucher.Lines.Any(l => items[l.ItemId].Kind == ItemKind.Returnable))
        {
            throw new BusinessException(HrErrorCodes.StoreIssueVoucher.TeamHasNoDepartment);
        }

        var me = CurrentUser.GetId();
        var created = new List<Custody>();
        foreach (var line in voucher.Lines)
        {
            var item = items[line.ItemId];
            Custody? custody = null;
            if (item.Kind == ItemKind.Returnable)
            {
                var personal = voucher.EmployeeId != null;
                custody = await _custodies.InsertAsync(
                    new Custody(
                        GuidGenerator.Create(),
                        CurrentTenant.Id,
                        item.Id,
                        personal ? CustodyKind.Personal : CustodyKind.Shared,
                        voucher.EmployeeId,
                        personal ? null : team.DepartmentId,
                        line.Quantity,
                        Clock.Now,
                        me,
                        null,
                        null,
                        $"{voucher.VoucherNumber} - {team.Name}"),
                    autoSave: true);
                created.Add(custody);
            }

            var movement = await Ledger.RecordAsync(item, new StockMovementDraft(
                StockMovementType.Issue,
                -line.Quantity,
                me,
                voucher.Note,
                CustodyId: custody?.Id,
                EmployeeId: voucher.EmployeeId,
                IssueVoucherId: voucher.Id,
                TeamId: voucher.TeamId,
                DocumentNumber: voucher.VoucherNumber));
            line.Posted(movement, custody?.Id);
        }

        voucher.MarkApproved(me, Clock.Now);
        await _vouchers.UpdateAsync(voucher, autoSave: true);

        foreach (var custody in created)
        {
            await NotifyHoldersAsync(custody);
        }

        return await ToDtoAsync(voucher);
    }

    /// <summary>Brings every line back at the cost it left with, and closes the custody it created as returned.</summary>
    [Authorize(HrPermissions.StorePermissions.IssueVoucherReverse)]
    public virtual async Task<StoreIssueVoucherDto> ReverseAsync(Guid id, ReasonInputDto input)
    {
        var voucher = await LoadAsync(id);
        voucher.EnsureApproved();
        var custodyIds = voucher.Lines.Where(x => x.CustodyId != null).Select(x => x.CustodyId!.Value).ToList();
        var custodies = (await _custodies.GetListAsync(x => custodyIds.Contains(x.Id))).ToDictionary(x => x.Id);
        if (custodies.Values.Any(x => x.Status == CustodyStatus.Returned))
        {
            throw new BusinessException(HrErrorCodes.StoreIssueVoucher.CustodyAlreadyReturned);
        }

        var items = (await Items.GetListAsync(x => voucher.Lines.Select(l => l.ItemId).Contains(x.Id))).ToDictionary(x => x.Id);
        var me = CurrentUser.GetId();
        var reason = input.Reason.Trim();
        foreach (var line in voucher.Lines)
        {
            if (line.CustodyId != null)
            {
                var custody = custodies[line.CustodyId.Value];
                custody.Return(me, Clock.Now, true, reason);
                await _custodies.UpdateAsync(custody, autoSave: true);
            }

            await Ledger.RecordAsync(items[line.ItemId], new StockMovementDraft(
                StockMovementType.Return,
                line.Quantity,
                me,
                reason,
                CustodyId: line.CustodyId,
                ReversesMovementId: line.StockMovementId,
                EmployeeId: voucher.EmployeeId,
                UnitCost: line.UnitCost,
                IssueVoucherId: voucher.Id,
                TeamId: voucher.TeamId,
                DocumentNumber: voucher.VoucherNumber));
        }

        voucher.MarkReversed(me, Clock.Now, reason);
        await _vouchers.UpdateAsync(voucher, autoSave: true);
        return await ToDtoAsync(voucher);
    }

    [Authorize(HrPermissions.StorePermissions.IssueVoucherManage)]
    public virtual async Task<AttachmentDto> UploadAttachmentAsync(Guid id, UploadedFileInput file)
    {
        await LoadAsync(id);
        return await Attachments.SaveAsync(id, file, CurrentUser.GetId());
    }

    [Authorize(HrPermissions.StorePermissions.IssueVoucherManage)]
    public virtual async Task DeleteAttachmentAsync(Guid id)
    {
        await LoadAsync(id);
        await Attachments.RemoveAsync(id, CurrentUser.GetId());
    }

    [Authorize(HrPermissions.StorePermissions.IssueVoucherView)]
    public virtual async Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id)
    {
        await LoadAsync(id);
        return await Attachments.LinkAsync(id);
    }

    private async Task<StoreIssueVoucher> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await _vouchers.WithDetailsAsync(x => x.Lines)).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(StoreIssueVoucher), id);

    private async Task<StoreTeam> CheckTeamAndEmployeeAsync(Guid teamId, Guid? employeeId)
    {
        var team = await _teams.FindAsync(teamId) ?? throw new EntityNotFoundException(typeof(StoreTeam), teamId);
        if (!team.IsActive)
        {
            throw new BusinessException(HrErrorCodes.StoreIssueVoucher.TeamInactive);
        }

        if (employeeId != null && !await _employees.AnyAsync(x => x.Id == employeeId && x.Status != EmployeeStatus.Ended))
        {
            throw new BusinessException(HrErrorCodes.StoreIssueVoucher.EmployeeNotActive);
        }

        return team;
    }

    private async Task FillAsync(StoreIssueVoucher voucher, StoreIssueVoucherCreateInputDto input)
    {
        var number = input.VoucherNumber.Trim();
        if (await _vouchers.AnyAsync(x => x.VoucherNumber == number && x.Id != voucher.Id))
        {
            throw new BusinessException(HrErrorCodes.StoreIssueVoucher.VoucherNumberAlreadyExists);
        }

        if (input.IssueDate > await CompanyClock.GetTodayAsync())
        {
            throw new BusinessException(HrErrorCodes.StoreIssueVoucher.InvalidDate);
        }

        await CheckTeamAndEmployeeAsync(input.TeamId!.Value, input.EmployeeId);
        var items = await LoadActiveItemsAsync(input.Lines.Select(x => x.ItemId!.Value).ToList(), HrErrorCodes.StoreIssueVoucher.DuplicateItem);
        var lines = input.Lines.Select(x => new StoreIssueVoucherLine(GuidGenerator.Create(), CurrentTenant.Id, voucher.Id, items[x.ItemId!.Value], x.Quantity!.Value)).ToList();
        voucher.Set(number, input.IssueDate!.Value, input.TeamId.Value, input.EmployeeId, string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim(), lines);
    }

    private async Task NotifyHoldersAsync(Custody custody)
    {
        List<Guid> recipients;
        if (custody.Kind == CustodyKind.Shared)
        {
            recipients = await _scope.GetDepartmentManagerUserIdsAsync(custody.DepartmentId!.Value);
        }
        else
        {
            var holder = await _employees.GetAsync(custody.EmployeeId!.Value);
            recipients = holder.UserId == null ? new List<Guid>() : new List<Guid> { holder.UserId.Value };
        }

        foreach (var userId in recipients)
        {
            await _notifications.PublishAsync(userId, NotificationTemplateCode.CustodyAssigned, NotificationResourceType.Custody, custody.Id);
        }
    }

    private async Task<StoreIssueVoucherDto> ToDtoAsync(StoreIssueVoucher x)
    {
        var items = await ItemRefs.GetRefsAsync(x.Lines.Select(l => l.ItemId));
        var units = await StoreRefs.GetUnitsAsync(x.Lines.Select(l => l.UnitId));
        var teams = await StoreRefs.GetTeamsAsync(new Guid?[] { x.TeamId });
        var users = await UsersAsync(new[] { x.CreatorId, x.ApprovedByUserId, x.ReversedByUserId });
        var employee = x.EmployeeId == null ? null : (await References.GetRefsAsync(new[] { x.EmployeeId.Value })).GetValueOrDefault(x.EmployeeId.Value);
        var attachment = await Attachments.FindAsync(x.Id);

        return new StoreIssueVoucherDto
        {
            Id = x.Id,
            VoucherNumber = x.VoucherNumber,
            IssueDate = x.IssueDate,
            Team = teams[x.TeamId],
            Employee = employee,
            Note = x.Note,
            Status = x.Status,
            Lines = x.Lines.Select(l => new StoreIssueVoucherLineDto
            {
                Id = l.Id,
                Item = items[l.ItemId],
                Sku = l.Sku,
                Unit = StoreReferenceReader.Pick(units, l.UnitId),
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                LineValue = l.LineValue,
                CustodyId = l.CustodyId
            }).ToList(),
            TotalQuantity = x.TotalQuantity,
            TotalValue = x.TotalValue,
            Currency = x.Currency,
            Attachment = attachment == null ? null : StoreAttachmentManager.ToDto(attachment),
            ApprovedAt = x.ApprovedAt,
            ApprovedBy = UserOf(users, x.ApprovedByUserId),
            ReversedAt = x.ReversedAt,
            ReversedBy = UserOf(users, x.ReversedByUserId),
            ReverseReason = x.ReverseReason,
            CreatedBy = UserOf(users, x.CreatorId)!,
            ConcurrencyStamp = x.ConcurrencyStamp,
            CreationTime = x.CreationTime,
            LastModificationTime = x.LastModificationTime
        };
    }
}

public class StorePurchaseLineInputDtoValidator : AbstractValidator<StorePurchaseLineInputDto>
{
    public StorePurchaseLineInputDtoValidator()
    {
        RuleFor(x => x.ItemId).NotNull();
        RuleFor(x => x.Quantity).NotNull().GreaterThan(0).PrecisionScale(StoreConsts.QuantityPrecision, StoreConsts.QuantityScale, true);
        RuleFor(x => x.UnitPrice).NotNull().GreaterThanOrEqualTo(0).PrecisionScale(StoreConsts.CostPrecision, StoreConsts.CostScale, true);
    }
}

public abstract class StorePurchaseInputValidatorBase<T> : AbstractValidator<T> where T : StorePurchaseCreateInputDto
{
    protected StorePurchaseInputValidatorBase()
    {
        RuleFor(x => x.ReceiptNumber).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxDocumentNumberLength);
        RuleFor(x => x.SupplierName).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxSupplierNameLength);
        RuleFor(x => x.PurchaseDate).NotNull();
        RuleFor(x => x.ReceivedDate).NotNull();
        RuleFor(x => x.Note).MaximumLength(StoreConsts.MaxNoteLength);
        RuleFor(x => x.Lines).NotNull().Must(x => x.Count is >= 1 and <= StoreConsts.MaxDocumentLines);
        RuleForEach(x => x.Lines).SetValidator(new StorePurchaseLineInputDtoValidator());
    }
}

public class StorePurchaseCreateInputDtoValidator : StorePurchaseInputValidatorBase<StorePurchaseCreateInputDto>
{
}

public class StorePurchaseUpdateInputDtoValidator : StorePurchaseInputValidatorBase<StorePurchaseUpdateInputDto>
{
    public StorePurchaseUpdateInputDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class StoreIssueVoucherLineInputDtoValidator : AbstractValidator<StoreIssueVoucherLineInputDto>
{
    public StoreIssueVoucherLineInputDtoValidator()
    {
        RuleFor(x => x.ItemId).NotNull();
        RuleFor(x => x.Quantity).NotNull().GreaterThan(0).PrecisionScale(StoreConsts.QuantityPrecision, StoreConsts.QuantityScale, true);
    }
}

public abstract class StoreIssueVoucherInputValidatorBase<T> : AbstractValidator<T> where T : StoreIssueVoucherCreateInputDto
{
    protected StoreIssueVoucherInputValidatorBase()
    {
        RuleFor(x => x.VoucherNumber).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxDocumentNumberLength);
        RuleFor(x => x.IssueDate).NotNull();
        RuleFor(x => x.TeamId).NotNull();
        RuleFor(x => x.Note).MaximumLength(StoreConsts.MaxNoteLength);
        RuleFor(x => x.Lines).NotNull().Must(x => x.Count is >= 1 and <= StoreConsts.MaxDocumentLines);
        RuleForEach(x => x.Lines).SetValidator(new StoreIssueVoucherLineInputDtoValidator());
    }
}

public class StoreIssueVoucherCreateInputDtoValidator : StoreIssueVoucherInputValidatorBase<StoreIssueVoucherCreateInputDto>
{
}

public class StoreIssueVoucherUpdateInputDtoValidator : StoreIssueVoucherInputValidatorBase<StoreIssueVoucherUpdateInputDto>
{
    public StoreIssueVoucherUpdateInputDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
