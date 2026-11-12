import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import 'ops_shared.dart';
import 'store_admin.dart';
import 'store_documents.dart';
import 'store_shared.dart';

String itemKindLabel(S s, ItemKind k) => switch (k) {
      ItemKind.returnable => s.t('يُسترد (عهدة)', 'Returnable (custody)'),
      ItemKind.consumable => s.t('مستهلك', 'Consumable'),
      _ => '—',
    };

String movementTypeLabel(S s, StockMovementType t) => switch (t) {
      StockMovementType.receive => s.t('إدخال', 'Receive'),
      StockMovementType.purchase => s.t('شراء', 'Purchase'),
      StockMovementType.issue => s.t('صرف', 'Issue'),
      StockMovementType.return_ => s.t('إرجاع', 'Return'),
      StockMovementType.adjustment => s.t('تسوية', 'Adjustment'),
      _ => '—',
    };

String issueStatusLabel(S s, StockIssueStatus st) => switch (st) {
      StockIssueStatus.pendingAcknowledgement => s.t('بانتظار الاطلاع', 'Awaiting acknowledgement'),
      StockIssueStatus.acknowledged => s.t('تم الاطلاع', 'Acknowledged'),
      StockIssueStatus.purchasing => s.t('بانتظار الشراء', 'Purchasing'),
      StockIssueStatus.delivered => s.t('سُلّم', 'Delivered'),
      StockIssueStatus.cancelled => s.t('ملغى', 'Cancelled'),
      _ => '—',
    };

Tone issueTone(StockIssueStatus st) => switch (st) {
      StockIssueStatus.delivered => Tone.success,
      StockIssueStatus.cancelled => Tone.neutral,
      StockIssueStatus.purchasing => Tone.warning,
      _ => Tone.info,
    };

/// S-STO: store officer — items, stock issues (after approvals) and movements.
class StorePage extends ConsumerStatefulWidget {
  const StorePage({super.key});

  @override
  ConsumerState<StorePage> createState() => _StorePageState();
}

class _StorePageState extends ConsumerState<StorePage> {
  int _version = 0;
  StockStatus? _stockStatus;
  String _filter = '';
  String _moveFilter = '';
  StoreTeamDto? _moveTeam;
  StockIssueStatus? _issueStatus = StockIssueStatus.pendingAcknowledgement;
  StockMovementType? _moveType;
  ItemRefDto? _moveItem;
  List<String> _tabKeys = const [];

  void _bump() => setState(() => _version++);

  void _openTab(BuildContext context, String key) {
    final index = _tabKeys.indexOf(key);
    final controller = DefaultTabController.maybeOf(context);
    if (index >= 0 && controller != null) controller.animateTo(index);
  }

  Future<void> _deleteItem(ItemDto i) async {
    final s = S.of(context);
    if (!await confirmAction(context,
        title: s.t('حذف المادة', 'Delete item'),
        message: s.t('تُحذف المادة إذا ما عليها أي حركة. المادة اللي عليها حركات تنأرشف بإيقافها.', 'Only an item without movements is deleted. Archive one with history by deactivating it.'),
        destructive: true)) {
      return;
    }
    if (!mounted) return;
    if (await runAction(context, () => ref.read(hrApiProvider).store.Item_Delete(id: i.id), done: s.done)) _bump();
  }

  Future<void> _editItem([ItemDto? item]) async {
    final s = S.of(context);
    final ar = TextEditingController(text: item?.nameAr);
    final en = TextEditingController(text: item?.nameEn);
    final min = TextEditingController(text: item == null ? '0' : qty(item.minQuantity));
    final sku = TextEditingController(text: item?.sku);
    StoreLookupRefDto? category = item?.category;
    StoreLookupRefDto? unit = item?.unit;
    var kind = item?.kind ?? ItemKind.consumable;
    var active = item?.isActive ?? true;
    final key = const Uuid().v4();
    final api = ref.read(hrApiProvider).store;
    final ok = await showFormDialog(
      context,
      title: item == null ? s.t('مادة جديدة', 'New item') : s.t('تعديل المادة', 'Edit item'),
      fields: (setState) => [
        TextFormField(controller: sku, maxLength: 32, decoration: InputDecoration(labelText: s.t('رمز المادة (SKU)', 'SKU')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: ar, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Arabic name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'English name')), validator: (v) => requiredValidator(context, v)),
        SegmentedButton<ItemKind>(
          segments: [
            ButtonSegment(value: ItemKind.consumable, label: Text(itemKindLabel(s, ItemKind.consumable))),
            ButtonSegment(value: ItemKind.returnable, label: Text(itemKindLabel(s, ItemKind.returnable))),
          ],
          selected: {kind},
          onSelectionChanged: (v) => setState(() => kind = v.first),
        ),
        if (item != null) Text(s.t('النوع لا يتغير بعد أول حركة.', 'The kind cannot change after the first movement.'), style: Theme.of(context).textTheme.labelSmall),
        PickerField(
          label: s.t('التصنيف', 'Category'),
          value: category?.name,
          onTap: () async {
            final x = await showSearchPicker<ItemCategoryDto>(context, title: s.t('التصنيف', 'Category'), search: (q) => searchCategories(ref, q), label: (c) => catalogName(context, c.nameAr, c.nameEn));
            if (x != null) setState(() => category = StoreLookupRefDto(id: x.id, name: catalogName(context, x.nameAr, x.nameEn)));
          },
        ),
        const SizedBox(height: 8),
        PickerField(
          label: s.t('وحدة القياس', 'Unit of measure'),
          value: unit?.name,
          onTap: () async {
            final x = await showSearchPicker<ItemUnitDto>(context, title: s.t('وحدة القياس', 'Unit'), search: (q) => searchUnits(ref, q), label: (c) => catalogName(context, c.nameAr, c.nameEn));
            if (x != null) setState(() => unit = StoreLookupRefDto(id: x.id, name: catalogName(context, x.nameAr, x.nameEn)));
          },
        ),
        numberField(context, min, s.t('الحد الأدنى للمخزون', 'Minimum stock'), min: 0),
        if (item != null)
          SwitchListTile(
              contentPadding: EdgeInsets.zero,
              value: active,
              title: Text(s.t('مفعّل (الإيقاف يؤرشف المادة)', 'Active (off archives the item)')),
              onChanged: (v) => setState(() => active = v)),
      ],
      onSubmit: () async {
        if (category == null || unit == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(context, () async {
          if (item == null) {
            await api.Item_Create(
                idempotencyKey: key,
                body: ItemCreateInputDto(
                    nameAr: ar.text.trim(), nameEn: en.text.trim(), sku: sku.text.trim(), categoryId: category!.id, unitId: unit!.id, kind: kind, minQuantity: parseNum(min)!));
          } else {
            await api.Item_Update(
              id: item.id,
              body: ItemUpdateInputDto(
                  nameAr: ar.text.trim(),
                  nameEn: en.text.trim(),
                  sku: sku.text.trim(),
                  categoryId: category!.id,
                  unitId: unit!.id,
                  kind: kind,
                  minQuantity: parseNum(min)!,
                  isActive: active,
                  concurrencyStamp: item.concurrencyStamp),
            );
          }
        }, done: s.saved);
      },
    );
    if (ok) _bump();
  }

  /// Receive stock; optionally for a stock issue waiting for purchase (cost then required).
  Future<void> _receive({ItemRefDto? item, StockIssueDto? forIssue}) async {
    final s = S.of(context);
    var picked = item ?? forIssue?.item;
    final quantity = TextEditingController(text: forIssue == null ? '' : qty(forIssue.quantity));
    final cost = TextEditingController();
    final note = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('إدخال مواد', 'Receive stock'),
      fields: (setState) => [
        PickerField(
          label: s.item,
          value: picked?.name,
          onTap: forIssue != null
              ? () {}
              : () async {
                  final x = await showSearchPicker<ItemRefDto>(context, title: s.item, search: (q) => searchItems(ref, q), label: (i) => i.name);
                  if (x != null) setState(() => picked = x);
                },
        ),
        numberField(context, quantity, s.quantity, positive: true),
        numberField(context, cost, s.t('إجمالي التكلفة', 'Total cost'),
            required: forIssue != null, min: 0, helper: s.t('عند إدخالها ينشأ مصروف عند المحاسب ويتحدث متوسط السعر.', 'Creates an expense for the accountant and updates the average cost.')),
        TextFormField(controller: note, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () async {
        if (picked == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).store.Stock_Receive(
                idempotencyKey: key,
                body: StockReceiveInputDto(itemId: picked!.id, quantity: parseNum(quantity)!, totalCost: parseNum(cost), stockIssueId: forIssue?.id, note: optText(note)),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) _bump();
  }

  Future<void> _adjust({ItemRefDto? item, StockMovementDto? reverses}) async {
    final s = S.of(context);
    var picked = item ?? reverses?.item;
    final quantity = TextEditingController(text: reverses == null ? '' : qty(-reverses.quantity));
    final reason = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: reverses == null ? s.t('تصحيح الرصيد', 'Adjust stock') : s.t('عكس الحركة', 'Reverse movement'),
      fields: (setState) => [
        PickerField(
          label: s.item,
          value: picked?.name,
          onTap: reverses != null
              ? () {}
              : () async {
                  final x = await showSearchPicker<ItemRefDto>(context, title: s.item, search: (q) => searchItems(ref, q), label: (i) => i.name);
                  if (x != null) setState(() => picked = x);
                },
        ),
        numberField(context, quantity, s.t('الكمية (موجبة تزيد، سالبة تنقص)', 'Quantity (+ adds, − removes)'), signed: true),
        TextFormField(controller: reason, maxLines: 2, maxLength: 2000, decoration: InputDecoration(labelText: s.reason), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () async {
        final q = parseNum(quantity);
        if (picked == null || q == null || q == 0) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).store.Stock_Adjust(
                idempotencyKey: key,
                body: StockAdjustInputDto(itemId: picked!.id, quantity: q, reason: reason.text.trim(), reversesMovementId: reverses?.id),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) _bump();
  }

  Future<void> _issueAction(StockIssueDto i, String action) async {
    final s = S.of(context);
    final api = ref.read(hrApiProvider).store;
    bool ok = false;
    switch (action) {
      case 'ack':
        ok = await runAction(context, () => api.StockIssue_Acknowledge(id: i.id), done: s.done);
      case 'purchasing':
        final note = await askText(context, title: s.t('بانتظار الشراء', 'Mark purchasing'), label: s.note, mandatory: false);
        if (note == null || !mounted) return;
        ok = await runAction(context, () => api.StockIssue_MarkPurchasing(id: i.id, body: NoteInputDto(note: note.isEmpty ? null : note)), done: s.done);
      case 'deliver':
        var kind = CustodyKind.personal;
        final note = TextEditingController();
        final key = const Uuid().v4();
        final returnable = i.item.kind == ItemKind.returnable;
        ok = await showFormDialog(
          context,
          title: s.t('تسليم ${qty(i.quantity)} × ${i.item.name}', 'Deliver ${qty(i.quantity)} × ${i.item.name}'),
          submitLabel: s.t('تسليم', 'Deliver'),
          fields: (setState) => [
            Text(i.employee.fullName),
            if (returnable)
              SegmentedButton<CustodyKind>(
                segments: [
                  ButtonSegment(value: CustodyKind.personal, label: Text(s.t('عهدة شخصية', 'Personal custody'))),
                  ButtonSegment(value: CustodyKind.shared, label: Text(s.t('عهدة القسم', 'Department custody'))),
                ],
                selected: {kind},
                onSelectionChanged: (v) => setState(() => kind = v.first),
              ),
            TextFormField(controller: note, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
          ],
          onSubmit: () => runAction(
            context,
            () => api.StockIssue_Deliver(
                id: i.id, idempotencyKey: key, body: StockIssueDeliverInputDto(custodyKind: returnable ? kind : null, note: optText(note))),
            done: s.done,
          ),
        );
      case 'receive':
        await _receive(forIssue: i);
        return;
    }
    if (ok) _bump();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).store;
    final ar = Localizations.localeOf(context).languageCode == 'ar';

    final items = PagedList<ItemDto>(
      key: ValueKey('items|$_version|$_stockStatus|$_filter'),
      pageSize: 50,
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
          if (session.can(HrOps.Item_Create)) FilledButton.icon(onPressed: () => _editItem(), icon: const Icon(Icons.add), label: Text(s.t('مادة جديدة', 'New item'))),
          SizedBox(
            width: 240,
            child: TextField(
              decoration: InputDecoration(labelText: s.t('بحث بالاسم أو الرمز', 'Search name or SKU'), prefixIcon: const Icon(Icons.search), isDense: true),
              onSubmitted: (v) => setState(() => _filter = v.trim()),
            ),
          ),
          enumFilter<StockStatus>(context,
              label: s.status,
              value: _stockStatus,
              values: StockStatus.values.where((x) => x.value.isNotEmpty).toList(),
              text: (v) => stockStatusLabel(s, v),
              onChanged: (v) => setState(() => _stockStatus = v)),
        ]),
      ),
      fetch: (skip, max) async {
        final r = await api.Item_GetList(skipCount: skip, maxResultCount: max, stockStatus: _stockStatus, filter: _filter.isEmpty ? null : _filter);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, i) => OpsTile(
        leading: i.kind == ItemKind.returnable ? Icons.inventory_2_outlined : Icons.shopping_basket_outlined,
        title: '${i.sku} · ${ar ? i.nameAr : i.nameEn}',
        subtitle: [
          if (i.category != null) i.category!.name,
          itemKindLabel(s, i.kind),
          '${s.t('الحد الأدنى', 'Min')} ${qty(i.minQuantity)}',
          '${s.t('سعر الوحدة', 'Unit price')} ${qty(i.averageUnitCost)}',
          '${s.t('القيمة', 'Value')} ${qty(i.stockValue)}',
        ].join(' · '),
        trailing: Row(mainAxisSize: MainAxisSize.min, children: [
          Text('${qty(i.quantityOnHand)} ${i.unit?.name ?? ''}', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(width: 8),
          StatusPill(stockStatusLabel(s, i.stockStatus), tone: stockStatusTone(i.stockStatus)),
          if (!i.isActive) ...[const SizedBox(width: 6), StatusPill(s.t('مؤرشف', 'Archived'), tone: Tone.neutral)],
        ]),
        onTap: session.can(HrOps.Item_Update) ? () => _editItem(i) : null,
        actions: [
          if (session.can(HrOps.Stock_Receive) && i.isActive)
            TextButton.icon(onPressed: () => _receive(item: ItemRefDto(id: i.id, name: ar ? i.nameAr : i.nameEn, kind: i.kind)), icon: const Icon(Icons.add_box_outlined), label: Text(s.t('إدخال', 'Receive'))),
          if (session.can(HrOps.Stock_Adjust))
            TextButton.icon(onPressed: () => _adjust(item: ItemRefDto(id: i.id, name: ar ? i.nameAr : i.nameEn, kind: i.kind)), icon: const Icon(Icons.tune), label: Text(s.t('تسوية', 'Adjust'))),
          if (session.can(HrOps.Item_Delete))
            TextButton.icon(onPressed: () => _deleteItem(i), icon: const Icon(Icons.delete_outline), label: Text(s.t('حذف', 'Delete'))),
        ],
      ),
    );

    final issues = PagedList<StockIssueDto>(
      key: ValueKey('issues|$_version|$_issueStatus'),
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Align(
          alignment: AlignmentDirectional.centerStart,
          child: enumFilter<StockIssueStatus>(context,
              label: s.status, value: _issueStatus, values: StockIssueStatus.values, text: (v) => issueStatusLabel(s, v), onChanged: (v) => setState(() => _issueStatus = v)),
        ),
      ),
      fetch: (skip, max) async {
        final r = await api.StockIssue_GetList(skipCount: skip, maxResultCount: max, status: _issueStatus);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, i) => OpsTile(
        leading: Icons.outbox_outlined,
        title: '${qty(i.quantity)} × ${i.item.name}',
        subtitle: [
          i.employee.fullName,
          if (i.employee.departmentName != null) i.employee.departmentName!,
          f.dateTime(i.creationTime),
          '${s.t('المتوفر', 'On hand')} ${qty(i.quantityOnHand)}',
        ].join(' · '),
        trailing: Row(mainAxisSize: MainAxisSize.min, children: [
          if (!i.isAvailable && i.status != StockIssueStatus.delivered) ...[StatusPill(s.t('غير متوفر', 'Not available'), tone: Tone.danger), const SizedBox(width: 6)],
          StatusPill(i.statusLabel, tone: issueTone(i.status)),
        ]),
        onTap: () => context.go('/requests/${i.requestId}'),
        actions: [
          if (i.status == StockIssueStatus.pendingAcknowledgement && session.can(HrOps.StockIssue_Acknowledge))
            FilledButton.tonal(onPressed: () => _issueAction(i, 'ack'), child: Text(s.t('اطلعت', 'Acknowledge'))),
          if (i.status == StockIssueStatus.acknowledged && session.can(HrOps.StockIssue_MarkPurchasing))
            OutlinedButton(onPressed: () => _issueAction(i, 'purchasing'), child: Text(s.t('بانتظار الشراء', 'Mark purchasing'))),
          if (i.status == StockIssueStatus.purchasing && session.can(HrOps.Stock_Receive))
            OutlinedButton(onPressed: () => _issueAction(i, 'receive'), child: Text(s.t('إدخال المشتريات', 'Receive purchase'))),
          if ((i.status == StockIssueStatus.acknowledged || i.status == StockIssueStatus.purchasing) && session.can(HrOps.StockIssue_Deliver))
            FilledButton(onPressed: () => _issueAction(i, 'deliver'), child: Text(s.t('تسليم', 'Deliver'))),
        ],
      ),
    );

    final movements = PagedList<StockMovementDto>(
      key: ValueKey('moves|$_version|$_moveType|${_moveItem?.id}|${_moveTeam?.id}|$_moveFilter'),
      pageSize: 50,
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Wrap(spacing: 12, runSpacing: 8, children: [
          enumFilter<StockMovementType>(context,
              label: s.t('نوع الحركة', 'Type'), value: _moveType, values: StockMovementType.values, text: (v) => movementTypeLabel(s, v), onChanged: (v) => setState(() => _moveType = v)),
          SizedBox(
            width: 240,
            child: PickerField(
              label: s.item,
              value: _moveItem?.name,
              onClear: () => setState(() => _moveItem = null),
              onTap: () async {
                final x = await showSearchPicker<ItemRefDto>(context, title: s.item, search: (q) => searchItems(ref, q), label: (i) => i.name);
                if (x != null) setState(() => _moveItem = x);
              },
            ),
          ),
          SizedBox(
            width: 220,
            child: PickerField(
              label: s.t('الفريق', 'Team'),
              value: _moveTeam?.name,
              onClear: () => setState(() => _moveTeam = null),
              onTap: () async {
                final x = await showSearchPicker<StoreTeamDto>(context, title: s.t('الفريق', 'Team'), search: (q) => searchTeams(ref, q), label: (t) => t.name);
                if (x != null) setState(() => _moveTeam = x);
              },
            ),
          ),
          SizedBox(
            width: 200,
            child: TextField(
              decoration: InputDecoration(labelText: s.t('رقم الوصل أو السند', 'Receipt or voucher no.'), prefixIcon: const Icon(Icons.search), isDense: true),
              onSubmitted: (v) => setState(() => _moveFilter = v.trim()),
            ),
          ),
        ]),
      ),
      fetch: (skip, max) async {
        final r = await api.Stock_GetMovements(
            skipCount: skip,
            maxResultCount: max,
            type: _moveType,
            itemId: _moveItem?.id,
            teamId: _moveTeam?.id,
            filter: _moveFilter.isEmpty ? null : _moveFilter,
            sorting: 'creationTime desc');
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, m) => OpsTile(
        leading: m.quantity >= 0 ? Icons.south_west : Icons.north_east,
        title: '${movementTypeLabel(s, m.type)} · ${m.item.name}',
        subtitle: [
          f.dateTime(m.creationTime),
          m.performedBy.displayName,
          if (m.documentNumber != null) m.documentNumber!,
          if (m.team != null) m.team!.name,
          if (m.employee != null) m.employee!.fullName,
          if (m.totalCost != null) '${s.t('القيمة', 'Value')} ${qty(m.totalCost)}',
          if (m.note != null) m.note!,
          if (m.reversesMovementId != null) s.t('عكس حركة سابقة', 'Reverses a movement'),
        ].join(' · '),
        trailing: Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
          Text(m.quantity > 0 ? '+${qty(m.quantity)}' : qty(m.quantity), style: const TextStyle(fontWeight: FontWeight.w700)),
          Text('${s.t('الرصيد', 'Balance')} ${qty(m.balanceAfter)}', style: Theme.of(context).textTheme.labelSmall),
        ]),
        actions: [
          if (session.can(HrOps.Stock_Adjust) && m.type != StockMovementType.adjustment && m.purchaseId == null && m.issueVoucherId == null)
            TextButton(onPressed: () => _adjust(reverses: m), child: Text(s.t('عكس بتصحيح', 'Reverse'))),
        ],
      ),
    );

    final keyed = <(String, String, Widget)>[
      if (session.can(HrOps.StoreDashboard_Get))
        ('dashboard', s.t('لوحة التحكم', 'Dashboard'), Builder(builder: (context) => StoreDashboardTab(version: _version, onOpenTab: (k) => _openTab(context, k)))),
      if (session.can(HrOps.Item_GetList)) ('items', s.t('المواد', 'Items'), items),
      if (session.can(HrOps.StorePurchase_GetList)) ('purchases', s.t('المشتريات', 'Purchases'), const PurchasesTab()),
      if (session.can(HrOps.StoreIssueVoucher_GetList)) ('vouchers', s.t('صرف المواد', 'Issue stock'), const VouchersTab()),
      if (session.can(HrOps.StoreTeam_GetList)) ('teams', s.t('الفرق', 'Teams'), const TeamsTab()),
      if (session.can(HrOps.Stock_GetMovements)) ('movements', s.t('حركة المخزون', 'Movements'), movements),
      if (session.can(HrOps.StoreReport_GetStock)) ('reports', s.t('التقارير', 'Reports'), const StoreReportsTab()),
      if (session.can(HrOps.StockIssue_GetList)) ('issues', s.t('طلبات الصرف', 'Stock requests'), issues),
      if (session.can(HrOps.ItemCategory_GetList)) ('catalog', s.t('التصنيفات والوحدات', 'Categories & units'), const CatalogTab()),
    ];
    _tabKeys = keyed.map((x) => x.$1).toList();
    return HrPage(
      title: s.t('المخزن', 'Store'),
      child: tabbedBody(context, keyed.map((x) => (x.$2, x.$3)).toList()),
    );
  }
}
