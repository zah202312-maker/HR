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

String itemKindLabel(S s, ItemKind k) => switch (k) {
      ItemKind.returnable => s.t('يُسترد (عهدة)', 'Returnable (custody)'),
      ItemKind.consumable => s.t('مستهلك', 'Consumable'),
      _ => '—',
    };

String movementTypeLabel(S s, StockMovementType t) => switch (t) {
      StockMovementType.receive => s.t('إدخال', 'Receive'),
      StockMovementType.issue => s.t('صرف', 'Issue'),
      StockMovementType.return_ => s.t('إرجاع', 'Return'),
      StockMovementType.adjustment => s.t('تصحيح', 'Adjustment'),
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
  bool _belowMin = false;
  String _filter = '';
  StockIssueStatus? _issueStatus = StockIssueStatus.pendingAcknowledgement;
  StockMovementType? _moveType;
  ItemRefDto? _moveItem;

  void _bump() => setState(() => _version++);

  Future<void> _editItem([ItemDto? item]) async {
    final s = S.of(context);
    final ar = TextEditingController(text: item?.nameAr);
    final en = TextEditingController(text: item?.nameEn);
    final min = TextEditingController(text: item == null ? '0' : qty(item.minQuantity));
    var kind = item?.kind ?? ItemKind.consumable;
    var active = item?.isActive ?? true;
    final key = const Uuid().v4();
    final api = ref.read(hrApiProvider).store;
    final ok = await showFormDialog(
      context,
      title: item == null ? s.t('صنف جديد', 'New item') : s.t('تعديل الصنف', 'Edit item'),
      fields: (setState) => [
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
        numberField(context, min, s.t('الحد الأدنى', 'Minimum quantity'), min: 0),
        if (item != null)
          SwitchListTile(contentPadding: EdgeInsets.zero, value: active, title: Text(s.t('مفعّل', 'Active')), onChanged: (v) => setState(() => active = v)),
      ],
      onSubmit: () => runAction(context, () async {
        if (item == null) {
          await api.Item_Create(
              idempotencyKey: key, body: ItemCreateInputDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), kind: kind, minQuantity: parseNum(min)!));
        } else {
          await api.Item_Update(
            id: item.id,
            body: ItemUpdateInputDto(
                nameAr: ar.text.trim(), nameEn: en.text.trim(), kind: kind, minQuantity: parseNum(min)!, isActive: active, concurrencyStamp: item.concurrencyStamp),
          );
        }
      }, done: s.saved),
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
            required: forIssue != null, min: 0, helper: s.t('عند إدخالها ينشأ مصروف عند المحاسب، ولا تظهر هنا بعد الحفظ.', 'Creates an expense for the accountant; not shown here after saving.')),
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
      key: ValueKey('items|$_version|$_belowMin|$_filter'),
      pageSize: 50,
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
          SizedBox(
            width: 240,
            child: TextField(
              decoration: InputDecoration(labelText: s.t('بحث', 'Search'), prefixIcon: const Icon(Icons.search), isDense: true),
              onSubmitted: (v) => setState(() => _filter = v.trim()),
            ),
          ),
          FilterChip(label: Text(s.t('تحت الحد الأدنى', 'Below minimum')), selected: _belowMin, onSelected: (v) => setState(() => _belowMin = v)),
        ]),
      ),
      fetch: (skip, max) async {
        final r = await api.Item_GetList(skipCount: skip, maxResultCount: max, belowMinimum: _belowMin ? true : null, filter: _filter.isEmpty ? null : _filter);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, i) => OpsTile(
        leading: i.kind == ItemKind.returnable ? Icons.inventory_2_outlined : Icons.shopping_basket_outlined,
        title: ar ? i.nameAr : i.nameEn,
        subtitle: '${itemKindLabel(s, i.kind)} · ${s.t('الحد الأدنى', 'Min')} ${qty(i.minQuantity)}',
        trailing: Row(mainAxisSize: MainAxisSize.min, children: [
          Text(qty(i.quantityOnHand), style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(width: 8),
          if (i.isBelowMinimum) StatusPill(s.t('تحت الحد', 'Low'), tone: Tone.warning),
          if (!i.isActive) StatusPill(s.t('موقوف', 'Inactive'), tone: Tone.neutral),
        ]),
        onTap: session.can(HrOps.Item_Update) ? () => _editItem(i) : null,
        actions: [
          if (session.can(HrOps.Stock_Receive) && i.isActive)
            TextButton.icon(onPressed: () => _receive(item: ItemRefDto(id: i.id, name: ar ? i.nameAr : i.nameEn, kind: i.kind)), icon: const Icon(Icons.add_box_outlined), label: Text(s.t('إدخال', 'Receive'))),
          if (session.can(HrOps.Stock_Adjust))
            TextButton.icon(onPressed: () => _adjust(item: ItemRefDto(id: i.id, name: ar ? i.nameAr : i.nameEn, kind: i.kind)), icon: const Icon(Icons.tune), label: Text(s.t('تصحيح', 'Adjust'))),
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
      key: ValueKey('moves|$_version|$_moveType|${_moveItem?.id}'),
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
        ]),
      ),
      fetch: (skip, max) async {
        final r = await api.Stock_GetMovements(skipCount: skip, maxResultCount: max, type: _moveType, itemId: _moveItem?.id, sorting: 'creationTime desc');
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, m) => OpsTile(
        leading: m.quantity >= 0 ? Icons.south_west : Icons.north_east,
        title: '${movementTypeLabel(s, m.type)} · ${m.item.name}',
        subtitle: [
          f.dateTime(m.creationTime),
          m.performedBy.displayName,
          if (m.employee != null) m.employee!.fullName,
          if (m.note != null) m.note!,
          if (m.reversesMovementId != null) s.t('عكس حركة سابقة', 'Reverses a movement'),
        ].join(' · '),
        trailing: Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
          Text(m.quantity > 0 ? '+${qty(m.quantity)}' : qty(m.quantity), style: const TextStyle(fontWeight: FontWeight.w700)),
          Text('${s.t('الرصيد', 'Balance')} ${qty(m.balanceAfter)}', style: Theme.of(context).textTheme.labelSmall),
        ]),
        actions: [
          if (session.can(HrOps.Stock_Adjust) && m.type != StockMovementType.adjustment)
            TextButton(onPressed: () => _adjust(reverses: m), child: Text(s.t('عكس بتصحيح', 'Reverse'))),
        ],
      ),
    );

    final tabs = <(String, Widget)>[
      if (session.can(HrOps.StockIssue_GetList)) (s.t('طلبات الصرف', 'Stock issues'), issues),
      if (session.can(HrOps.Item_GetList)) (s.t('الأصناف', 'Items'), items),
      if (session.can(HrOps.Stock_GetMovements)) (s.t('الحركات', 'Movements'), movements),
    ];
    return HrPage(
      title: s.t('المخزن', 'Store'),
      actions: [
        if (session.can(HrOps.Item_Create)) FilledButton.icon(onPressed: () => _editItem(), icon: const Icon(Icons.add), label: Text(s.t('صنف جديد', 'New item'))),
      ],
      child: tabbedBody(context, tabs),
    );
  }
}
