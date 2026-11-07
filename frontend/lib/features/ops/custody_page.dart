import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../org/org_providers.dart' show searchDepartments, searchEmployees;
import 'ops_shared.dart';

String custodyStatusLabel(S s, CustodyStatus st) => switch (st) {
      CustodyStatus.pendingAcknowledgement => s.t('بانتظار تأكيد الاستلام', 'Awaiting acknowledgement'),
      CustodyStatus.acknowledged => s.t('مستلمة', 'Acknowledged'),
      CustodyStatus.returned => s.t('مُرجعة', 'Returned'),
      _ => '—',
    };

Tone custodyTone(CustodyStatus st) => switch (st) {
      CustodyStatus.pendingAcknowledgement => Tone.warning,
      CustodyStatus.acknowledged => Tone.success,
      _ => Tone.neutral,
    };

String custodyKindLabel(S s, CustodyKind k) => k == CustodyKind.shared ? s.t('مشتركة (قسم)', 'Shared (department)') : s.t('شخصية', 'Personal');

/// S-CUS: custody — mine, my team's (manager), all (store officer / HR) and shared-custody counts.
class CustodyPage extends ConsumerStatefulWidget {
  const CustodyPage({super.key});

  @override
  ConsumerState<CustodyPage> createState() => _CustodyPageState();
}

class _CustodyPageState extends ConsumerState<CustodyPage> {
  int _version = 0;
  CustodyStatus? _status;
  CustodyKind? _kind;
  CustodyCountResult? _countResult;

  void _bump() => setState(() => _version++);

  Future<void> _create() async {
    final s = S.of(context);
    ItemRefDto? item;
    EmployeeRefDto? employee;
    DepartmentRefDto? department;
    var kind = CustodyKind.personal;
    var deduct = true;
    final quantity = TextEditingController(text: '1');
    final note = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('إضافة عهدة', 'New custody'),
      fields: (setState) => [
        PickerField(
          label: s.item,
          value: item?.name,
          onTap: () async {
            final x = await showSearchPicker<ItemRefDto>(context,
                title: s.item, search: (q) => searchItems(ref, q, kind: ItemKind.returnable), label: (i) => i.name);
            if (x != null) setState(() => item = x);
          },
        ),
        SegmentedButton<CustodyKind>(
          segments: [
            ButtonSegment(value: CustodyKind.personal, label: Text(custodyKindLabel(s, CustodyKind.personal))),
            ButtonSegment(value: CustodyKind.shared, label: Text(custodyKindLabel(s, CustodyKind.shared))),
          ],
          selected: {kind},
          onSelectionChanged: (v) => setState(() => kind = v.first),
        ),
        if (kind == CustodyKind.personal)
          PickerField(
            label: s.employee,
            value: employee?.fullName,
            onTap: () async {
              final x = await showSearchPicker<EmployeeRefDto>(context,
                  title: s.employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
              if (x != null) setState(() => employee = x);
            },
          )
        else
          PickerField(
            label: s.department,
            value: department?.name,
            onTap: () async {
              final x = await showSearchPicker<DepartmentRefDto>(context, title: s.department, search: (q) => searchDepartments(ref, q), label: (d) => d.name);
              if (x != null) setState(() => department = x);
            },
          ),
        numberField(context, quantity, s.quantity, positive: true),
        SwitchListTile(
          contentPadding: EdgeInsets.zero,
          value: deduct,
          title: Text(s.t('خصم من رصيد المخزن', 'Deduct from stock')),
          subtitle: Text(s.t('أوقفه إذا كانت المادة عند الموظف أصلاً.', 'Turn off if the item is already with the holder.')),
          onChanged: (v) => setState(() => deduct = v),
        ),
        TextFormField(controller: note, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () async {
        if (item == null || (kind == CustodyKind.personal ? employee == null : department == null)) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).custody.Custody_Create(
                idempotencyKey: key,
                body: CustodyCreateInputDto(
                  itemId: item!.id,
                  kind: kind,
                  employeeId: kind == CustodyKind.personal ? employee!.id : null,
                  departmentId: kind == CustodyKind.shared ? department!.id : null,
                  quantity: parseNum(quantity)!,
                  deductFromStock: deduct,
                  note: optText(note),
                ),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) _bump();
  }

  Future<void> _return(CustodyDto c) async {
    final s = S.of(context);
    var restock = true;
    final note = TextEditingController();
    final ok = await showFormDialog(
      context,
      title: s.t('إرجاع العهدة', 'Return custody'),
      submitLabel: s.t('إرجاع', 'Return'),
      fields: (setState) => [
        Text('${qty(c.quantity)} × ${c.item.name}'),
        SwitchListTile(
          contentPadding: EdgeInsets.zero,
          value: restock,
          title: Text(s.t('إعادة للرصيد', 'Back to stock')),
          subtitle: Text(s.t('التالف لا يعود للرصيد، وتلزم ملاحظة.', 'Damaged items stay out of stock; a note is required.')),
          onChanged: (v) => setState(() => restock = v),
        ),
        TextFormField(
          controller: note,
          maxLength: 2000,
          decoration: InputDecoration(labelText: s.note),
          validator: (v) => !restock && (v ?? '').trim().isEmpty ? s.requiredField : null,
        ),
      ],
      onSubmit: () => runAction(context,
          () => ref.read(hrApiProvider).custody.Custody_Return(id: c.id, body: CustodyReturnInputDto(restock: restock, note: optText(note))), done: s.done),
    );
    if (ok) _bump();
  }

  Future<void> _ack(CustodyDto c) async {
    final s = S.of(context);
    final ok = await confirmAction(context,
        title: s.t('تأكيد استلام العهدة', 'Acknowledge custody'), message: '${qty(c.quantity)} × ${c.item.name}');
    if (!ok || !mounted) return;
    if (await runAction(context, () => ref.read(hrApiProvider).custody.Custody_Acknowledge(id: c.id), done: s.done)) _bump();
  }

  /// Shared-custody count for one department: one line per open shared custody.
  Future<void> _count() async {
    final s = S.of(context);
    final api = ref.read(hrApiProvider).custody;
    final shared = await api.Custody_GetTeam(kind: CustodyKind.shared, maxResultCount: 200);
    if (!mounted) return;
    final open = shared.items.where((c) => c.status != CustodyStatus.returned && c.department != null).toList();
    final departments = <String, DepartmentRefDto>{for (final c in open) c.department!.id: c.department!};
    if (departments.isEmpty) {
      showError(context, s.t('لا توجد عهد مشتركة قائمة في أقسامك.', 'No open shared custody in your departments.'));
      return;
    }
    var dept = departments.values.first;
    final counted = <String, TextEditingController>{};
    final note = TextEditingController();
    var date = DateTime.now();
    final key = const Uuid().v4();
    final f = Fmt(context);
    final ok = await showFormDialog(
      context,
      width: 640,
      title: s.t('جرد العهدة المشتركة', 'Shared custody count'),
      fields: (setState) {
        final lines = open.where((c) => c.department!.id == dept.id).toList();
        for (final c in lines) {
          counted.putIfAbsent(c.id, () => TextEditingController(text: qty(c.quantity)));
        }
        return [
          DropdownButtonFormField<String>(
            initialValue: dept.id,
            decoration: InputDecoration(labelText: s.department),
            items: [for (final d in departments.values) DropdownMenuItem(value: d.id, child: Text(d.name))],
            onChanged: (v) => setState(() => dept = departments[v]!),
          ),
          PickerField(label: s.date, value: f.date(date), icon: Icons.event, onTap: () async {
            final x = await pickDate(context, initial: date, last: DateTime.now());
            if (x != null) setState(() => date = x);
          }),
          for (final c in lines)
            Row(children: [
              Expanded(child: Text('${c.item.name} (${s.t('المتوقع', 'expected')} ${qty(c.quantity)})')),
              SizedBox(width: 140, child: numberField(context, counted[c.id]!, s.t('المعدود', 'Counted'), min: 0)),
            ]),
          TextFormField(controller: note, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
        ];
      },
      onSubmit: () {
        final lines = open.where((c) => c.department!.id == dept.id).toList();
        return runAction(
          context,
          () async {
            final r = await api.CustodyCount_Create(
              idempotencyKey: key,
              body: CustodyCountCreateInputDto(
                departmentId: dept.id,
                countDate: date,
                note: optText(note),
                lines: [for (final c in lines) CustodyCountLineInputDto(custodyId: c.id, countedQuantity: parseNum(counted[c.id]!)!)],
              ),
            );
            if (mounted && r.result == CustodyCountResult.discrepancy) {
              showError(context, s.t('يوجد فرق، وأُشعر مسؤول المخزن.', 'Discrepancy found; the store officer was notified.'));
            }
          },
          done: s.saved,
        );
      },
    );
    if (ok) _bump();
  }

  Widget _tile(BuildContext context, CustodyDto c, {bool canAck = false, bool canReturn = false}) {
    final s = S.of(context);
    final f = Fmt(context);
    return OpsTile(
      leading: c.kind == CustodyKind.shared ? Icons.domain_outlined : Icons.inventory_2_outlined,
      title: '${qty(c.quantity)} × ${c.item.name}',
      subtitle: [
        custodyKindLabel(s, c.kind),
        if (c.employee != null) c.employee!.fullName,
        if (c.department != null) c.department!.name,
        '${s.t('سُلّمت', 'Assigned')} ${f.date(c.assignedAt)}',
        if (c.acknowledgedBy != null) '${s.t('أكدها', 'Acknowledged by')} ${c.acknowledgedBy!.displayName}',
        if (c.returnedAt != null) '${s.t('أُرجعت', 'Returned')} ${f.date(c.returnedAt)}',
      ].join(' · '),
      trailing: StatusPill(custodyStatusLabel(s, c.status), tone: custodyTone(c.status)),
      actions: [
        if (canAck && c.status == CustodyStatus.pendingAcknowledgement)
          FilledButton.tonal(onPressed: () => _ack(c), child: Text(s.t('تأكيد الاستلام', 'Acknowledge'))),
        if (canReturn && c.status != CustodyStatus.returned)
          OutlinedButton(onPressed: () => _return(c), child: Text(s.t('إرجاع', 'Return'))),
      ],
    );
  }

  Widget _filters(BuildContext context, {bool withKind = true}) {
    final s = S.of(context);
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Wrap(spacing: 12, runSpacing: 8, children: [
        enumFilter<CustodyStatus>(context,
            label: s.status, value: _status, values: CustodyStatus.values, text: (v) => custodyStatusLabel(s, v), onChanged: (v) => setState(() => _status = v)),
        if (withKind)
          enumFilter<CustodyKind>(context,
              label: s.t('النوع', 'Kind'), value: _kind, values: CustodyKind.values, text: (v) => custodyKindLabel(s, v), onChanged: (v) => setState(() => _kind = v)),
      ]),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).custody;
    final canAck = session.can(HrOps.Custody_Acknowledge);
    final canReturn = session.can(HrOps.Custody_Return);

    final tabs = <(String, Widget)>[
      if (session.can(HrOps.Custody_GetMy))
        (
          s.t('عهدتي', 'My custody'),
          PagedList<CustodyDto>(
            key: ValueKey('my|$_version|$_status'),
            header: _filters(context, withKind: false),
            fetch: (skip, max) async {
              final r = await api.Custody_GetMy(skipCount: skip, maxResultCount: max, status: _status);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => _tile(context, c, canAck: canAck),
          )
        ),
      if (session.can(HrOps.Custody_GetTeam))
        (
          s.t('عهد الفريق', 'Team custody'),
          PagedList<CustodyDto>(
            key: ValueKey('team|$_version|$_status|$_kind'),
            header: _filters(context),
            fetch: (skip, max) async {
              final r = await api.Custody_GetTeam(skipCount: skip, maxResultCount: max, status: _status, kind: _kind);
              return (r.items, r.totalCount);
            },
            // the manager acknowledges shared custody of their departments
            itemBuilder: (context, c) => _tile(context, c, canAck: canAck && c.kind == CustodyKind.shared),
          )
        ),
      if (session.can(HrOps.Custody_GetList))
        (
          s.t('كل العهد', 'All custody'),
          PagedList<CustodyDto>(
            key: ValueKey('all|$_version|$_status|$_kind'),
            pageSize: 50,
            header: _filters(context),
            fetch: (skip, max) async {
              final r = await api.Custody_GetList(skipCount: skip, maxResultCount: max, status: _status, kind: _kind);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => _tile(context, c, canReturn: canReturn),
          )
        ),
      if (session.can(HrOps.CustodyCount_GetList))
        (
          s.t('الجرد', 'Counts'),
          PagedList<CustodyCountDto>(
            key: ValueKey('counts|$_version|$_countResult'),
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Align(
                alignment: AlignmentDirectional.centerStart,
                child: enumFilter<CustodyCountResult>(context,
                    label: s.t('النتيجة', 'Result'),
                    value: _countResult,
                    values: CustodyCountResult.values,
                    text: (v) => v == CustodyCountResult.matched ? s.t('مطابق', 'Matched') : s.t('يوجد فرق', 'Discrepancy'),
                    onChanged: (v) => setState(() => _countResult = v)),
              ),
            ),
            fetch: (skip, max) async {
              final r = await api.CustodyCount_GetList(skipCount: skip, maxResultCount: max, result: _countResult);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: HrCard(
                child: ExpansionTile(
                  tilePadding: EdgeInsets.zero,
                  title: Text('${c.department.name} · ${f.date(c.countDate)}'),
                  subtitle: Text('${c.countedBy.displayName}${c.note == null ? '' : ' · ${c.note}'}'),
                  trailing: StatusPill(c.result == CustodyCountResult.matched ? s.t('مطابق', 'Matched') : s.t('يوجد فرق', 'Discrepancy'),
                      tone: c.result == CustodyCountResult.matched ? Tone.success : Tone.danger),
                  children: [
                    for (final l in c.lines)
                      InfoRow(l.item.name, '${qty(l.countedQuantity)} / ${qty(l.expectedQuantity)}${l.note == null ? '' : ' · ${l.note}'}'),
                  ],
                ),
              ),
            ),
          )
        ),
    ];

    return HrPage(
      title: s.t('العهد', 'Custody'),
      actions: [
        if (session.can(HrOps.CustodyCount_Create))
          OutlinedButton.icon(onPressed: _count, icon: const Icon(Icons.fact_check_outlined), label: Text(s.t('جرد', 'Count'))),
        const SizedBox(width: 8),
        if (session.can(HrOps.Custody_Create)) FilledButton.icon(onPressed: _create, icon: const Icon(Icons.add), label: Text(s.t('عهدة جديدة', 'New custody'))),
      ],
      child: tabbedBody(context, tabs),
    );
  }
}
