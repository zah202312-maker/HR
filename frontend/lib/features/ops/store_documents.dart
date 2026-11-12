import 'package:dio/dio.dart' show MultipartFile;
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../org/org_providers.dart' show searchEmployees;
import 'ops_shared.dart';
import 'store_shared.dart';

/// One editable line of a receipt or voucher form.
class _Line {
  _Line(this.item, {double? quantity, double? price})
      : quantity = TextEditingController(text: quantity == null ? '' : qty(quantity)),
        price = TextEditingController(text: price == null ? '' : qty(price));
  ItemDto? item;
  String? itemId;
  String? itemLabel;
  final TextEditingController quantity;
  final TextEditingController price;

  double get total => (parseNum(quantity) ?? 0) * (parseNum(price) ?? 0);
}

Widget _lineItemPicker(BuildContext context, WidgetRef ref, _Line line, StateSetter setState) => PickerField(
      label: S.of(context).item,
      value: line.item == null ? line.itemLabel : '${line.item!.sku} · ${itemName(context, line.item!)}',
      onTap: () async {
        final x = await showSearchPicker<ItemDto>(context,
            title: S.of(context).item,
            search: (q) => searchStoreItems(ref, q),
            label: (i) => '${i.sku} · ${itemName(context, i)}',
            subtitle: (i) => [i.category?.name, i.unit?.name, '${S.of(context).t('الرصيد', 'On hand')} ${qty(i.quantityOnHand)}'].whereType<String>().join(' · '));
        if (x != null) setState(() => (line.item = x, line.itemId = x.id));
      },
    );

Future<void> _upload(BuildContext context, Future<void> Function(MultipartFile) send) async {
  final s = S.of(context);
  final picked = await pickFiles();
  if (picked.isEmpty || !context.mounted) return;
  await runAction(context, () => send(multipartOf(picked.first)), done: s.saved);
}

// ---------------------------------------------------------------- purchases

/// Add or edit a draft purchase receipt with one or more items; totals are computed live and by the server.
Future<bool> editPurchase(BuildContext context, WidgetRef ref, {StorePurchaseDto? draft}) async {
  final s = S.of(context);
  final api = ref.read(hrApiProvider).store;
  final receipt = TextEditingController(text: draft?.receiptNumber);
  final supplier = TextEditingController(text: draft?.supplierName);
  final note = TextEditingController(text: draft?.note);
  var purchaseDate = draft?.purchaseDate ?? DateTime.now();
  var receivedDate = draft?.receivedDate ?? DateTime.now();
  final lines = draft == null
      ? [_Line(null)]
      : draft.lines.map((l) => _Line(null, quantity: l.quantity, price: l.unitPrice)..itemId = l.item.id..itemLabel = '${l.sku} · ${l.item.name}').toList();
  PlatformFile? file;
  final key = const Uuid().v4();
  final currency = draft?.currency ?? 'IQD';

  return showFormDialog(
    context,
    width: 900,
    title: draft == null ? s.t('عملية شراء جديدة', 'New purchase') : s.t('تعديل وصل الشراء', 'Edit purchase'),
    submitLabel: s.t('حفظ كمسودة', 'Save draft'),
    fields: (setState) {
      final totalQty = lines.fold<double>(0, (a, l) => a + (parseNum(l.quantity) ?? 0));
      final totalAmount = lines.fold<double>(0, (a, l) => a + l.total);
      return [
        FieldGrid(children: [
          TextFormField(controller: receipt, maxLength: 64, decoration: InputDecoration(labelText: s.t('رقم الوصل', 'Receipt number')), validator: (v) => requiredValidator(context, v)),
          TextFormField(controller: supplier, maxLength: 200, decoration: InputDecoration(labelText: s.t('المورد أو جهة الشراء', 'Supplier')), validator: (v) => requiredValidator(context, v)),
          PickerField(
            label: s.t('تاريخ الشراء', 'Purchase date'),
            value: dateText(purchaseDate),
            icon: Icons.event,
            onTap: () async {
              final d = await pickDate(context, initial: purchaseDate, last: DateTime.now());
              if (d != null) setState(() => purchaseDate = d);
            },
          ),
          PickerField(
            label: s.t('تاريخ الاستلام', 'Received date'),
            value: dateText(receivedDate),
            icon: Icons.event_available,
            onTap: () async {
              final d = await pickDate(context, initial: receivedDate, first: purchaseDate, last: DateTime.now());
              if (d != null) setState(() => receivedDate = d);
            },
          ),
        ]),
        const SizedBox(height: 8),
        SectionTitle(s.t('المواد', 'Items')),
        for (var i = 0; i < lines.length; i++)
          Padding(
            padding: const EdgeInsets.only(bottom: 8),
            child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Expanded(flex: 4, child: _lineItemPicker(context, ref, lines[i], setState)),
              const SizedBox(width: 8),
              Expanded(flex: 2, child: numberField(context, lines[i].quantity, s.quantity, positive: true)),
              const SizedBox(width: 8),
              Expanded(flex: 2, child: numberField(context, lines[i].price, s.t('سعر الوحدة', 'Unit price'), min: 0)),
              const SizedBox(width: 8),
              SizedBox(width: 110, child: Padding(padding: const EdgeInsets.only(top: 16), child: Text(money(lines[i].total, currency), textAlign: TextAlign.end))),
              IconButton(
                tooltip: s.t('حذف السطر', 'Remove line'),
                onPressed: lines.length == 1 ? null : () => setState(() => lines.removeAt(i)),
                icon: const Icon(Icons.remove_circle_outline),
              ),
            ]),
          ),
        Align(
          alignment: AlignmentDirectional.centerStart,
          child: TextButton.icon(onPressed: () => setState(() => lines.add(_Line(null))), icon: const Icon(Icons.add), label: Text(s.t('إضافة مادة', 'Add item'))),
        ),
        const Divider(),
        InfoRow(s.t('إجمالي الكميات', 'Total quantity'), qty(totalQty)),
        InfoRow(s.t('المبلغ الإجمالي للوصل', 'Receipt total'), money(totalAmount, currency)),
        TextFormField(controller: note, maxLines: 2, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
        if (draft == null)
          OutlinedButton.icon(
            onPressed: () async {
              final picked = await pickFiles();
              if (picked.isNotEmpty) setState(() => file = picked.first);
            },
            icon: const Icon(Icons.attach_file),
            label: Text(file?.name ?? s.t('صورة أو PDF الوصل (اختياري)', 'Receipt scan (optional)')),
          ),
      ];
    },
    onSubmit: () async {
      if (lines.any((l) => l.itemId == null || parseNum(l.quantity) == null || parseNum(l.price) == null)) {
        showError(context, s.validation);
        return false;
      }
      final body = lines.map((l) => StorePurchaseLineInputDto(itemId: l.itemId!, quantity: parseNum(l.quantity)!, unitPrice: parseNum(l.price)!)).toList();
      return runAction(context, () async {
        if (draft == null) {
          final created = await api.StorePurchase_Create(
            idempotencyKey: key,
            body: StorePurchaseCreateInputDto(
                receiptNumber: receipt.text.trim(), supplierName: supplier.text.trim(), purchaseDate: purchaseDate, receivedDate: receivedDate, note: optText(note), lines: body),
          );
          if (file != null) await api.StorePurchase_UploadAttachment(id: created.id, body: StoreAttachmentUploadInputDto(file: multipartOf(file!)));
        } else {
          await api.StorePurchase_Update(
            id: draft.id,
            body: StorePurchaseUpdateInputDto(
                receiptNumber: receipt.text.trim(),
                supplierName: supplier.text.trim(),
                purchaseDate: purchaseDate,
                receivedDate: receivedDate,
                note: optText(note),
                lines: body,
                concurrencyStamp: draft.concurrencyStamp),
          );
        }
      }, done: s.saved);
    },
  );
}

/// The purchase log with status, period and search filters.
class PurchasesTab extends ConsumerStatefulWidget {
  const PurchasesTab({super.key});

  @override
  ConsumerState<PurchasesTab> createState() => _PurchasesTabState();
}

class _PurchasesTabState extends ConsumerState<PurchasesTab> {
  int _version = 0;
  StoreDocumentStatus? _status;
  String _filter = '';

  void _bump() => setState(() => _version++);

  Future<void> _open(String id) async {
    final changed = await showPurchase(context, ref, id);
    if (changed) _bump();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).store;
    final session = ref.watch(sessionProvider);
    return PagedList<StorePurchaseSummaryDto>(
      key: ValueKey('purchases|$_version|$_status|$_filter'),
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
          if (session.can(HrOps.StorePurchase_Create))
            FilledButton.icon(
              onPressed: () async {
                if (await editPurchase(context, ref)) _bump();
              },
              icon: const Icon(Icons.add_shopping_cart),
              label: Text(s.t('عملية شراء جديدة', 'New purchase')),
            ),
          SizedBox(
            width: 240,
            child: TextField(
              decoration: InputDecoration(labelText: s.t('رقم الوصل أو المورد', 'Receipt or supplier'), prefixIcon: const Icon(Icons.search), isDense: true),
              onSubmitted: (v) => setState(() => _filter = v.trim()),
            ),
          ),
          enumFilter<StoreDocumentStatus>(context,
              label: s.status, value: _status, values: StoreDocumentStatus.values.where((x) => x.value.isNotEmpty).toList(), text: (v) => documentStatusLabel(s, v), onChanged: (v) => setState(() => _status = v)),
        ]),
      ),
      fetch: (skip, max) async {
        final r = await api.StorePurchase_GetList(skipCount: skip, maxResultCount: max, status: _status, filter: _filter.isEmpty ? null : _filter);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, p) => OpsTile(
        leading: Icons.receipt_long_outlined,
        title: '${s.t('وصل', 'Receipt')} ${p.receiptNumber} · ${p.supplierName}',
        subtitle: [
          '${s.t('الشراء', 'Bought')} ${f.date(p.purchaseDate)}',
          '${s.t('الاستلام', 'Received')} ${f.date(p.receivedDate)}',
          '${p.linesCount} ${s.t('مادة', 'items')}',
          if (p.hasAttachment) s.t('مرفق', 'Attached'),
        ].join(' · '),
        trailing: Row(mainAxisSize: MainAxisSize.min, children: [
          Text(money(p.totalAmount, p.currency), style: Theme.of(context).textTheme.titleSmall),
          const SizedBox(width: 8),
          StatusPill(documentStatusLabel(s, p.status), tone: documentStatusTone(p.status)),
        ]),
        onTap: () => _open(p.id),
      ),
    );
  }
}

/// Receipt details with its actions: edit, approve, delete, reverse, attachment.
Future<bool> showPurchase(BuildContext context, WidgetRef ref, String id) async {
  final s = S.of(context);
  final f = Fmt(context);
  final api = ref.read(hrApiProvider).store;
  final session = ref.read(sessionProvider);
  var changed = false;

  await showHrDialog<void>(
    context: context,
    builder: (ctx) => StatefulBuilder(builder: (ctx, setState) {
      return FutureBuilder<StorePurchaseDto>(
        key: ValueKey(changed),
        future: api.StorePurchase_Get(id: id),
        builder: (ctx, snap) {
          if (!snap.hasData) {
            return AlertDialog(content: snap.hasError ? ErrorState(error: snap.error!) : const SizedBox(height: 120, child: Center(child: CircularProgressIndicator())));
          }
          final p = snap.data!;
          Future<void> act(Future<void> Function() action, {String? done}) async {
            if (await runAction(ctx, action, done: done ?? s.done)) setState(() => changed = !changed);
          }

          final draft = p.status == StoreDocumentStatus.draft;
          return AlertDialog(
            title: Row(children: [
              Expanded(child: Text('${s.t('وصل', 'Receipt')} ${p.receiptNumber}')),
              StatusPill(documentStatusLabel(s, p.status), tone: documentStatusTone(p.status)),
            ]),
            content: SizedBox(
              width: 900,
              child: SingleChildScrollView(
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, mainAxisSize: MainAxisSize.min, children: [
                  InfoRow(s.t('المورد', 'Supplier'), p.supplierName),
                  InfoRow(s.t('تاريخ الشراء', 'Purchase date'), f.date(p.purchaseDate)),
                  InfoRow(s.t('تاريخ الاستلام', 'Received date'), f.date(p.receivedDate)),
                  if (p.note != null) InfoRow(s.note, p.note!),
                  InfoRow(s.t('أنشأه', 'Created by'), '${p.createdBy.displayName} · ${f.dateTime(p.creationTime)}'),
                  if (p.approvedBy != null) InfoRow(s.t('اعتمده', 'Approved by'), '${p.approvedBy!.displayName} · ${f.dateTime(p.approvedAt)}'),
                  if (p.reversedBy != null) InfoRow(s.t('عكسه', 'Reversed by'), '${p.reversedBy!.displayName} · ${f.dateTime(p.reversedAt)} · ${p.reverseReason ?? ''}'),
                  const SizedBox(height: 12),
                  StoreTable(
                    columns: [s.t('الرمز', 'SKU'), s.item, s.t('التصنيف', 'Category'), s.t('الوحدة', 'Unit'), s.quantity, s.t('سعر الوحدة', 'Unit price'), s.t('المجموع', 'Total')],
                    rows: [
                      for (final l in p.lines)
                        [Text(l.sku), Text(l.item.name), Text(l.category?.name ?? '—'), Text(l.unit?.name ?? '—'), Text(qty(l.quantity)), Text(money(l.unitPrice, p.currency)), Text(money(l.lineTotal, p.currency))],
                    ],
                    footer: [Text(s.t('الإجمالي', 'Total')), const Text(''), const Text(''), const Text(''), Text(qty(p.totalQuantity)), const Text(''), Text(money(p.totalAmount, p.currency))],
                  ),
                  const SizedBox(height: 12),
                  Wrap(spacing: 8, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                    Icon(Icons.attach_file, size: 18, color: Theme.of(ctx).colorScheme.outline),
                    if (p.attachment != null)
                      TextButton(onPressed: () => act(() async => openDownload(await api.StorePurchase_GetAttachmentDownloadUrl(id: p.id)), done: null), child: Text(p.attachment!.fileName))
                    else
                      Text(s.t('بلا مرفق', 'No attachment')),
                    if (session.can(HrOps.StorePurchase_UploadAttachment))
                      TextButton(onPressed: () => _upload(ctx, (file) async {
                            await api.StorePurchase_UploadAttachment(id: p.id, body: StoreAttachmentUploadInputDto(file: file));
                            setState(() => changed = !changed);
                          }), child: Text(p.attachment == null ? s.t('إرفاق', 'Attach') : s.t('استبدال', 'Replace'))),
                    if (p.attachment != null && session.can(HrOps.StorePurchase_DeleteAttachment))
                      TextButton(onPressed: () => act(() => api.StorePurchase_DeleteAttachment(id: p.id)), child: Text(s.t('حذف المرفق', 'Remove'))),
                  ]),
                ]),
              ),
            ),
            actions: [
              TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.t('إغلاق', 'Close'))),
              if (draft && session.can(HrOps.StorePurchase_Delete))
                TextButton(
                  onPressed: () async {
                    if (!await confirmAction(ctx, title: s.t('حذف المسودة', 'Delete draft'), message: s.t('تُحذف المسودة ومرفقها.', 'The draft and its attachment are deleted.'), destructive: true) || !ctx.mounted) return;
                    if (await runAction(ctx, () => api.StorePurchase_Delete(id: p.id), done: s.done)) {
                      changed = true;
                      if (ctx.mounted) Navigator.pop(ctx);
                    }
                  },
                  child: Text(s.t('حذف', 'Delete')),
                ),
              if (draft && session.can(HrOps.StorePurchase_Update))
                OutlinedButton(
                  onPressed: () async {
                    if (await editPurchase(ctx, ref, draft: p)) setState(() => changed = !changed);
                  },
                  child: Text(s.t('تعديل', 'Edit')),
                ),
              if (draft && session.can(HrOps.StorePurchase_Approve))
                FilledButton(
                  onPressed: () async {
                    if (await confirmAction(ctx, title: s.t('اعتماد الوصل', 'Approve receipt'), message: s.t('تدخل المواد للمخزن ويتحدث الرصيد والسعر، وينشأ مصروف عند المحاسب. بعدها لا يتعدل الوصل.', 'Stock and average cost update and the accountant gets an expense. The receipt is then locked.'))) {
                      await act(() => api.StorePurchase_Approve(id: p.id));
                    }
                  },
                  child: Text(s.t('اعتماد', 'Approve')),
                ),
              if (p.status == StoreDocumentStatus.approved && session.can(HrOps.StorePurchase_Reverse))
                OutlinedButton(
                  onPressed: () async {
                    final reason = await askText(ctx, title: s.t('عكس الوصل', 'Reverse receipt'), label: s.reason);
                    if (reason == null || reason.isEmpty) return;
                    await act(() => api.StorePurchase_Reverse(id: p.id, body: ReasonInputDto(reason: reason)));
                  },
                  child: Text(s.t('عكس', 'Reverse')),
                ),
            ],
          );
        },
      );
    }),
  );
  return changed;
}

// ---------------------------------------------------------------- issue vouchers

Future<bool> editVoucher(BuildContext context, WidgetRef ref, {StoreIssueVoucherDto? draft}) async {
  final s = S.of(context);
  final api = ref.read(hrApiProvider).store;
  final number = TextEditingController(text: draft?.voucherNumber);
  final note = TextEditingController(text: draft?.note);
  var issueDate = draft?.issueDate ?? DateTime.now();
  String? teamId = draft?.team.id;
  String? teamName = draft?.team.name;
  EmployeeRefDto? employee = draft?.employee;
  final lines = draft == null
      ? [_Line(null)]
      : draft.lines.map((l) => _Line(null, quantity: l.quantity)..itemId = l.item.id..itemLabel = '${l.sku} · ${l.item.name}').toList();
  PlatformFile? file;
  final key = const Uuid().v4();

  return showFormDialog(
    context,
    width: 800,
    title: draft == null ? s.t('صرف مواد', 'Issue stock') : s.t('تعديل سند الصرف', 'Edit issue voucher'),
    submitLabel: s.t('حفظ كمسودة', 'Save draft'),
    fields: (setState) => [
      FieldGrid(children: [
        TextFormField(controller: number, maxLength: 64, decoration: InputDecoration(labelText: s.t('رقم سند الصرف', 'Voucher number')), validator: (v) => requiredValidator(context, v)),
        PickerField(
          label: s.t('تاريخ الصرف', 'Issue date'),
          value: dateText(issueDate),
          icon: Icons.event,
          onTap: () async {
            final d = await pickDate(context, initial: issueDate, last: DateTime.now());
            if (d != null) setState(() => issueDate = d);
          },
        ),
        PickerField(
          label: s.t('الفريق أو القسم المستلم', 'Receiving team'),
          value: teamName,
          onTap: () async {
            final x = await showSearchPicker<StoreTeamDto>(context, title: s.t('الفريق', 'Team'), search: (q) => searchTeams(ref, q), label: (t) => t.name, subtitle: (t) => t.department?.name ?? '');
            if (x != null) setState(() => (teamId = x.id, teamName = x.name));
          },
        ),
        PickerField(
          label: s.t('الموظف المستلم (اختياري)', 'Receiving employee (optional)'),
          value: employee?.fullName,
          onClear: () => setState(() => employee = null),
          onTap: () async {
            final x = await showSearchPicker<EmployeeRefDto>(context, title: s.employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
            if (x != null) setState(() => employee = x);
          },
        ),
      ]),
      const SizedBox(height: 8),
      SectionTitle(s.t('المواد', 'Items')),
      Text(s.t('المادة التي تُسترد تصير عهدة: شخصية للموظف، أو مشتركة على قسم الفريق.', 'A returnable item becomes custody: personal for the employee, or shared on the team’s department.'),
          style: Theme.of(context).textTheme.labelSmall),
      const SizedBox(height: 8),
      for (var i = 0; i < lines.length; i++)
        Padding(
          padding: const EdgeInsets.only(bottom: 8),
          child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Expanded(flex: 4, child: _lineItemPicker(context, ref, lines[i], setState)),
            const SizedBox(width: 8),
            Expanded(
              flex: 2,
              child: numberField(context, lines[i].quantity, s.quantity, positive: true,
                  helper: lines[i].item == null ? null : '${s.t('المتوفر', 'On hand')} ${qty(lines[i].item!.quantityOnHand)}',
                  max: lines[i].item?.quantityOnHand),
            ),
            IconButton(onPressed: lines.length == 1 ? null : () => setState(() => lines.removeAt(i)), icon: const Icon(Icons.remove_circle_outline)),
          ]),
        ),
      Align(
        alignment: AlignmentDirectional.centerStart,
        child: TextButton.icon(onPressed: () => setState(() => lines.add(_Line(null))), icon: const Icon(Icons.add), label: Text(s.t('إضافة مادة', 'Add item'))),
      ),
      TextFormField(controller: note, maxLines: 2, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
      if (draft == null)
        OutlinedButton.icon(
          onPressed: () async {
            final picked = await pickFiles();
            if (picked.isNotEmpty) setState(() => file = picked.first);
          },
          icon: const Icon(Icons.attach_file),
          label: Text(file?.name ?? s.t('مستند الصرف (اختياري)', 'Voucher document (optional)')),
        ),
    ],
    onSubmit: () async {
      if (teamId == null || lines.any((l) => l.itemId == null || parseNum(l.quantity) == null)) {
        showError(context, s.validation);
        return false;
      }
      final body = lines.map((l) => StoreIssueVoucherLineInputDto(itemId: l.itemId!, quantity: parseNum(l.quantity)!)).toList();
      return runAction(context, () async {
        if (draft == null) {
          final created = await api.StoreIssueVoucher_Create(
            idempotencyKey: key,
            body: StoreIssueVoucherCreateInputDto(voucherNumber: number.text.trim(), issueDate: issueDate, teamId: teamId!, employeeId: employee?.id, note: optText(note), lines: body),
          );
          if (file != null) await api.StoreIssueVoucher_UploadAttachment(id: created.id, body: StoreAttachmentUploadInputDto(file: multipartOf(file!)));
        } else {
          await api.StoreIssueVoucher_Update(
            id: draft.id,
            body: StoreIssueVoucherUpdateInputDto(
                voucherNumber: number.text.trim(), issueDate: issueDate, teamId: teamId!, employeeId: employee?.id, note: optText(note), lines: body, concurrencyStamp: draft.concurrencyStamp),
          );
        }
      }, done: s.saved);
    },
  );
}

class VouchersTab extends ConsumerStatefulWidget {
  const VouchersTab({super.key});

  @override
  ConsumerState<VouchersTab> createState() => _VouchersTabState();
}

class _VouchersTabState extends ConsumerState<VouchersTab> {
  int _version = 0;
  StoreDocumentStatus? _status;
  StoreTeamDto? _team;
  String _filter = '';

  void _bump() => setState(() => _version++);

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).store;
    final session = ref.watch(sessionProvider);
    return PagedList<StoreIssueVoucherSummaryDto>(
      key: ValueKey('vouchers|$_version|$_status|${_team?.id}|$_filter'),
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
          if (session.can(HrOps.StoreIssueVoucher_Create))
            FilledButton.icon(
              onPressed: () async {
                if (await editVoucher(context, ref)) _bump();
              },
              icon: const Icon(Icons.outbox),
              label: Text(s.t('صرف مواد', 'Issue stock')),
            ),
          SizedBox(
            width: 200,
            child: TextField(
              decoration: InputDecoration(labelText: s.t('رقم السند', 'Voucher no.'), prefixIcon: const Icon(Icons.search), isDense: true),
              onSubmitted: (v) => setState(() => _filter = v.trim()),
            ),
          ),
          SizedBox(
            width: 220,
            child: PickerField(
              label: s.t('الفريق', 'Team'),
              value: _team?.name,
              onClear: () => setState(() => _team = null),
              onTap: () async {
                final x = await showSearchPicker<StoreTeamDto>(context, title: s.t('الفريق', 'Team'), search: (q) => searchTeams(ref, q), label: (t) => t.name);
                if (x != null) setState(() => _team = x);
              },
            ),
          ),
          enumFilter<StoreDocumentStatus>(context,
              label: s.status, value: _status, values: StoreDocumentStatus.values.where((x) => x.value.isNotEmpty).toList(), text: (v) => documentStatusLabel(s, v), onChanged: (v) => setState(() => _status = v)),
        ]),
      ),
      fetch: (skip, max) async {
        final r = await api.StoreIssueVoucher_GetList(skipCount: skip, maxResultCount: max, status: _status, teamId: _team?.id, filter: _filter.isEmpty ? null : _filter);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, v) => OpsTile(
        leading: Icons.outbox_outlined,
        title: '${s.t('سند', 'Voucher')} ${v.voucherNumber} · ${v.team.name}',
        subtitle: [
          f.date(v.issueDate),
          if (v.employee != null) v.employee!.fullName,
          '${v.linesCount} ${s.t('مادة', 'items')} · ${qty(v.totalQuantity)}',
          if (v.hasAttachment) s.t('مرفق', 'Attached'),
        ].join(' · '),
        trailing: Row(mainAxisSize: MainAxisSize.min, children: [
          if (v.totalValue != null) ...[Text(money(v.totalValue, v.currency), style: Theme.of(context).textTheme.titleSmall), const SizedBox(width: 8)],
          StatusPill(documentStatusLabel(s, v.status), tone: documentStatusTone(v.status)),
        ]),
        onTap: () async {
          if (await showVoucher(context, ref, v.id)) _bump();
        },
      ),
    );
  }
}

Future<bool> showVoucher(BuildContext context, WidgetRef ref, String id) async {
  final s = S.of(context);
  final f = Fmt(context);
  final api = ref.read(hrApiProvider).store;
  final session = ref.read(sessionProvider);
  var changed = false;

  await showHrDialog<void>(
    context: context,
    builder: (ctx) => StatefulBuilder(builder: (ctx, setState) {
      return FutureBuilder<StoreIssueVoucherDto>(
        key: ValueKey(changed),
        future: api.StoreIssueVoucher_Get(id: id),
        builder: (ctx, snap) {
          if (!snap.hasData) {
            return AlertDialog(content: snap.hasError ? ErrorState(error: snap.error!) : const SizedBox(height: 120, child: Center(child: CircularProgressIndicator())));
          }
          final v = snap.data!;
          Future<void> act(Future<void> Function() action) async {
            if (await runAction(ctx, action, done: s.done)) setState(() => changed = !changed);
          }

          final draft = v.status == StoreDocumentStatus.draft;
          return AlertDialog(
            title: Row(children: [
              Expanded(child: Text('${s.t('سند صرف', 'Issue voucher')} ${v.voucherNumber}')),
              StatusPill(documentStatusLabel(s, v.status), tone: documentStatusTone(v.status)),
            ]),
            content: SizedBox(
              width: 820,
              child: SingleChildScrollView(
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, mainAxisSize: MainAxisSize.min, children: [
                  InfoRow(s.t('الفريق', 'Team'), v.team.name),
                  InfoRow(s.t('تاريخ الصرف', 'Issue date'), f.date(v.issueDate)),
                  if (v.employee != null) InfoRow(s.t('الموظف المستلم', 'Received by'), v.employee!.fullName),
                  if (v.note != null) InfoRow(s.note, v.note!),
                  InfoRow(s.t('أنشأه', 'Created by'), '${v.createdBy.displayName} · ${f.dateTime(v.creationTime)}'),
                  if (v.approvedBy != null) InfoRow(s.t('اعتمده', 'Approved by'), '${v.approvedBy!.displayName} · ${f.dateTime(v.approvedAt)}'),
                  if (v.reversedBy != null) InfoRow(s.t('عكسه', 'Reversed by'), '${v.reversedBy!.displayName} · ${f.dateTime(v.reversedAt)} · ${v.reverseReason ?? ''}'),
                  const SizedBox(height: 12),
                  StoreTable(
                    columns: [s.t('الرمز', 'SKU'), s.item, s.t('الوحدة', 'Unit'), s.quantity, s.t('تكلفة الوحدة', 'Unit cost'), s.t('القيمة', 'Value'), s.t('عهدة', 'Custody')],
                    rows: [
                      for (final l in v.lines)
                        [
                          Text(l.sku),
                          Text(l.item.name),
                          Text(l.unit?.name ?? '—'),
                          Text(qty(l.quantity)),
                          Text(money(l.unitCost, v.currency)),
                          Text(money(l.lineValue, v.currency)),
                          Text(l.custodyId == null ? '—' : s.t('نعم', 'Yes')),
                        ],
                    ],
                    footer: [Text(s.t('الإجمالي', 'Total')), const Text(''), const Text(''), Text(qty(v.totalQuantity)), const Text(''), Text(money(v.totalValue, v.currency)), const Text('')],
                  ),
                  const SizedBox(height: 12),
                  Wrap(spacing: 8, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                    Icon(Icons.attach_file, size: 18, color: Theme.of(ctx).colorScheme.outline),
                    if (v.attachment != null)
                      TextButton(onPressed: () async => openDownload(await api.StoreIssueVoucher_GetAttachmentDownloadUrl(id: v.id)), child: Text(v.attachment!.fileName))
                    else
                      Text(s.t('بلا مرفق', 'No attachment')),
                    if (session.can(HrOps.StoreIssueVoucher_UploadAttachment))
                      TextButton(onPressed: () => _upload(ctx, (file) async {
                            await api.StoreIssueVoucher_UploadAttachment(id: v.id, body: StoreAttachmentUploadInputDto(file: file));
                            setState(() => changed = !changed);
                          }), child: Text(v.attachment == null ? s.t('إرفاق', 'Attach') : s.t('استبدال', 'Replace'))),
                    if (v.attachment != null && session.can(HrOps.StoreIssueVoucher_DeleteAttachment))
                      TextButton(onPressed: () => act(() => api.StoreIssueVoucher_DeleteAttachment(id: v.id)), child: Text(s.t('حذف المرفق', 'Remove'))),
                  ]),
                ]),
              ),
            ),
            actions: [
              TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.t('إغلاق', 'Close'))),
              if (draft && session.can(HrOps.StoreIssueVoucher_Delete))
                TextButton(
                  onPressed: () async {
                    if (!await confirmAction(ctx, title: s.t('حذف المسودة', 'Delete draft'), message: s.t('تُحذف المسودة ومرفقها.', 'The draft and its attachment are deleted.'), destructive: true) || !ctx.mounted) return;
                    if (await runAction(ctx, () => api.StoreIssueVoucher_Delete(id: v.id), done: s.done)) {
                      changed = true;
                      if (ctx.mounted) Navigator.pop(ctx);
                    }
                  },
                  child: Text(s.t('حذف', 'Delete')),
                ),
              if (draft && session.can(HrOps.StoreIssueVoucher_Update))
                OutlinedButton(
                  onPressed: () async {
                    if (await editVoucher(ctx, ref, draft: v)) setState(() => changed = !changed);
                  },
                  child: Text(s.t('تعديل', 'Edit')),
                ),
              if (draft && session.can(HrOps.StoreIssueVoucher_Approve))
                FilledButton(
                  onPressed: () async {
                    if (await confirmAction(ctx, title: s.t('اعتماد الصرف', 'Approve issue'), message: s.t('تُخصم الكميات من الرصيد، والمواد التي تُسترد تصير عهدة. بعدها لا يتعدل السند.', 'Quantities leave stock and returnable items become custody. The voucher is then locked.'))) {
                      await act(() => api.StoreIssueVoucher_Approve(id: v.id));
                    }
                  },
                  child: Text(s.t('اعتماد', 'Approve')),
                ),
              if (v.status == StoreDocumentStatus.approved && session.can(HrOps.StoreIssueVoucher_Reverse))
                OutlinedButton(
                  onPressed: () async {
                    final reason = await askText(ctx, title: s.t('عكس السند', 'Reverse voucher'), label: s.reason);
                    if (reason == null || reason.isEmpty) return;
                    await act(() => api.StoreIssueVoucher_Reverse(id: v.id, body: ReasonInputDto(reason: reason)));
                  },
                  child: Text(s.t('عكس', 'Reverse')),
                ),
            ],
          );
        },
      );
    }),
  );
  return changed;
}
