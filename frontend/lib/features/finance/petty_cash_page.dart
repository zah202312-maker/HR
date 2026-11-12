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
import '../ops/ops_shared.dart';
import '../org/org_providers.dart' show searchEmployees;
import 'finance_shared.dart';

// Petty cash (decision 2026-11-11): separate from the salary. Types, a fixed monthly amount per employee,
// and a monthly list the accountant marks as delivered.

String _period(DateTime d) => '${d.year.toString().padLeft(4, '0')}-${d.month.toString().padLeft(2, '0')}';

String _typeName(BuildContext context, PettyCashTypeDto t) => Localizations.localeOf(context).languageCode == 'ar' ? t.nameAr : t.nameEn;

class PettyCashTab extends ConsumerStatefulWidget {
  const PettyCashTab({super.key});

  @override
  ConsumerState<PettyCashTab> createState() => _PettyCashTabState();
}

class _PettyCashTabState extends ConsumerState<PettyCashTab> {
  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return tabbedBody(context, [
      (s.t('تسليم الشهر', 'This month'), const _MonthView()),
      (s.t('نثريات الموظفين', 'Employee petty cash'), const _AssignmentsView()),
      (s.t('أنواع النثرية', 'Types'), const _TypesView()),
    ]);
  }
}

// ---------------------------------------------------------------- month

class _MonthView extends ConsumerStatefulWidget {
  const _MonthView();

  @override
  ConsumerState<_MonthView> createState() => _MonthViewState();
}

class _MonthViewState extends ConsumerState<_MonthView> {
  DateTime _month = DateTime(DateTime.now().year, DateTime.now().month);
  PettyCashEntryStatus? _status;
  int _version = 0;
  final Set<String> _selected = {};
  PettyCashMonthDto? _last;

  void _bump() => setState(() {
        _version++;
        _selected.clear();
      });

  Future<void> _deliver(List<String> ids) async {
    final s = S.of(context);
    final picked = await pickDate(context, initial: DateTime.now(), last: DateTime.now());
    if (picked == null || !mounted) return;
    if (await runAction(context, () => ref.read(hrApiProvider).finance.PettyCashEntry_Deliver(body: PettyCashDeliveryInputDto(entryIds: ids, deliveredOn: picked)), done: s.done)) {
      _bump();
    }
  }

  Future<void> _changeAmount(PettyCashEntryDto e) async {
    final s = S.of(context);
    final amount = TextEditingController(text: qty(e.amount));
    final ok = await showFormDialog(
      context,
      title: '${e.employee.fullName} · ${e.type.name}',
      fields: (_) => [numberField(context, amount, s.t('مبلغ هذا الشهر', 'Amount this month'), positive: true)],
      onSubmit: () => runAction(
          context,
          () => ref.read(hrApiProvider).finance.PettyCashEntry_Update(id: e.id, body: PettyCashEntryUpdateInputDto(amount: parseNum(amount)!, concurrencyStamp: e.concurrencyStamp)),
          done: s.saved),
    );
    if (ok) _bump();
  }

  Future<void> _undo(PettyCashEntryDto e) async {
    final s = S.of(context);
    final reason = await askText(context, title: s.t('إلغاء التسليم', 'Undo delivery'), label: s.reason);
    if (reason == null || reason.isEmpty || !mounted) return;
    if (await runAction(context, () => ref.read(hrApiProvider).finance.PettyCashEntry_UndoDelivery(id: e.id, body: ReasonInputDto(reason: reason)), done: s.done)) _bump();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final canManage = ref.watch(sessionProvider).can(HrOps.PettyCashEntry_Deliver);
    final api = ref.watch(hrApiProvider).finance;
    final now = DateTime(DateTime.now().year, DateTime.now().month);
    return PagedList<PettyCashEntryDto>(
      key: ValueKey('pc-month|${_period(_month)}|$_status|$_version'),
      pageSize: 100,
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
            IconButton(onPressed: () => setState(() => _month = DateTime(_month.year, _month.month - 1)), icon: const Icon(Icons.chevron_right)),
            Text(_period(_month), style: Theme.of(context).textTheme.titleMedium),
            IconButton(
                onPressed: _month.isBefore(now) ? () => setState(() => _month = DateTime(_month.year, _month.month + 1)) : null, icon: const Icon(Icons.chevron_left)),
            enumFilter<PettyCashEntryStatus>(context,
                label: s.status,
                value: _status,
                values: PettyCashEntryStatus.values.where((x) => x.value.isNotEmpty).toList(),
                text: (v) => v == PettyCashEntryStatus.delivered ? s.t('مسلَّمة', 'Delivered') : s.t('غير مسلَّمة', 'Pending'),
                onChanged: (v) => setState(() => _status = v)),
            if (canManage && _selected.isNotEmpty)
              FilledButton.icon(onPressed: () => _deliver(_selected.toList()), icon: const Icon(Icons.payments_outlined), label: Text(s.t('تسليم المحدد (${_selected.length})', 'Deliver selected (${_selected.length})'))),
          ]),
          if (_last != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Wrap(spacing: 16, children: [
                Text('${s.t('المجموع', 'Total')} ${money(_last!.totalAmount, _last!.currency)}'),
                Text('${s.t('المسلَّم', 'Delivered')} ${money(_last!.deliveredAmount, _last!.currency)}'),
                Text('${s.t('الباقي', 'Pending')} ${money(_last!.pendingAmount, _last!.currency)}'),
              ]),
            ),
        ]),
      ),
      emptyMessage: s.t('ماكو نثريات بهذا الشهر.', 'No petty cash this month.'),
      fetch: (skip, max) async {
        final r = await api.PettyCashEntry_GetMonth(period: _period(_month), status: _status, skipCount: skip, maxResultCount: max);
        if (skip == 0) WidgetsBinding.instance.addPostFrameCallback((_) => mounted ? setState(() => _last = r) : null);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, e) {
        final pending = e.status == PettyCashEntryStatus.pending;
        return OpsTile(
          avatar: PersonAvatar.employee(e.employee),
          title: '${e.employee.fullName} · ${e.type.name}',
          subtitle: pending
              ? s.t('غير مسلَّمة', 'Pending')
              : '${s.t('سُلّمت', 'Delivered')} ${f.date(e.deliveredOn)}${e.deliveredBy == null ? '' : ' · ${e.deliveredBy!.displayName}'}',
          trailing: Row(mainAxisSize: MainAxisSize.min, children: [
            Text(money(e.amount, e.currency), style: const TextStyle(fontWeight: FontWeight.w700)),
            const SizedBox(width: 8),
            if (pending && canManage)
              Checkbox(value: _selected.contains(e.id), onChanged: (v) => setState(() => v == true ? _selected.add(e.id) : _selected.remove(e.id)))
            else
              StatusPill(pending ? s.t('غير مسلَّمة', 'Pending') : s.t('مسلَّمة', 'Delivered'), tone: pending ? Tone.warning : Tone.success),
          ]),
          actions: [
            if (pending && canManage) ...[
              FilledButton.tonal(onPressed: () => _deliver([e.id]), child: Text(s.t('سلّمتها', 'Delivered'))),
              TextButton(onPressed: () => _changeAmount(e), child: Text(s.t('تغيير المبلغ', 'Change amount'))),
            ],
            if (!pending && canManage) TextButton(onPressed: () => _undo(e), child: Text(s.t('إلغاء التسليم', 'Undo'))),
          ],
        );
      },
    );
  }
}

// ---------------------------------------------------------------- assignments

class _AssignmentsView extends ConsumerStatefulWidget {
  const _AssignmentsView();

  @override
  ConsumerState<_AssignmentsView> createState() => _AssignmentsViewState();
}

class _AssignmentsViewState extends ConsumerState<_AssignmentsView> {
  int _version = 0;
  EmployeeRefDto? _employee;

  Future<void> _edit([PettyCashAssignmentDto? a]) async {
    final s = S.of(context);
    final api = ref.read(hrApiProvider).finance;
    EmployeeRefDto? employee = a?.employee;
    PettyCashTypeDto? type;
    final amount = TextEditingController(text: a == null ? '' : qty(a.amount));
    final note = TextEditingController(text: a?.note);
    var from = a?.fromPeriod ?? _period(DateTime.now());
    String? to = a?.toPeriod;
    final key = const Uuid().v4();

    Future<String?> pickMonth(String? initial) async {
      final d = await pickDate(context, initial: initial == null ? DateTime.now() : DateTime.parse('$initial-01'));
      return d == null ? null : _period(d);
    }

    final ok = await showFormDialog(
      context,
      title: a == null ? s.t('نثرية شهرية لموظف', 'Monthly petty cash') : '${a.employee.fullName} · ${a.type.name}',
      fields: (setState) => [
        if (a == null) ...[
          PickerField(
            label: s.employee,
            value: employee?.fullName,
            onTap: () async {
              final x = await showSearchPicker<EmployeeRefDto>(context, title: s.employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
              if (x != null) setState(() => employee = x);
            },
          ),
          const SizedBox(height: 8),
          PickerField(
            label: s.t('نوع النثرية', 'Type'),
            value: type == null ? null : _typeName(context, type!),
            onTap: () async {
              final x = await showSearchPicker<PettyCashTypeDto>(context,
                  title: s.t('نوع النثرية', 'Type'),
                  search: (q) async => (await api.PettyCashType_GetList(isActive: true, maxResultCount: 200)).items.where((t) => _typeName(context, t).contains(q)).toList(),
                  label: (t) => _typeName(context, t));
              if (x != null) setState(() => type = x);
            },
          ),
          const SizedBox(height: 8),
          PickerField(label: s.t('من شهر', 'From month'), value: from, icon: Icons.event, onTap: () async {
            final m = await pickMonth(from);
            if (m != null) setState(() => from = m);
          }),
        ],
        numberField(context, amount, s.t('المبلغ الشهري', 'Monthly amount'), positive: true,
            helper: a == null ? null : s.t('المبلغ الجديد ينطبق على الأشهر غير المسلَّمة.', 'Applies to months not delivered yet.')),
        PickerField(
          label: s.t('إلى شهر (اختياري، لإيقافها)', 'Until month (optional, to stop)'),
          value: to,
          icon: Icons.event_busy,
          onClear: () => setState(() => to = null),
          onTap: () async {
            final m = await pickMonth(to ?? from);
            if (m != null) setState(() => to = m);
          },
        ),
        TextFormField(controller: note, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () async {
        if (a == null && (employee == null || type == null)) {
          showError(context, s.validation);
          return false;
        }
        return runAction(context, () async {
          if (a == null) {
            await api.PettyCashAssignment_Create(
                idempotencyKey: key,
                body: PettyCashAssignmentCreateInputDto(employeeId: employee!.id, typeId: type!.id, amount: parseNum(amount)!, fromPeriod: from, toPeriod: to, note: optText(note)));
          } else {
            await api.PettyCashAssignment_Update(
                id: a.id, body: PettyCashAssignmentUpdateInputDto(amount: parseNum(amount)!, toPeriod: to, note: optText(note), concurrencyStamp: a.concurrencyStamp));
          }
        }, done: s.saved);
      },
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final canManage = ref.watch(sessionProvider).can(HrOps.PettyCashAssignment_Create);
    final api = ref.watch(hrApiProvider).finance;
    final current = _period(DateTime.now());
    return PagedList<PettyCashAssignmentDto>(
      key: ValueKey('pc-assign|$_version|${_employee?.id}'),
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
          if (canManage) FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('إضافة نثرية لموظف', 'Add petty cash'))),
          SizedBox(
            width: 240,
            child: PickerField(
              label: s.employee,
              value: _employee?.fullName,
              onClear: () => setState(() => _employee = null),
              onTap: () async {
                final x = await showSearchPicker<EmployeeRefDto>(context, title: s.employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName);
                if (x != null) setState(() => _employee = x);
              },
            ),
          ),
        ]),
      ),
      fetch: (skip, max) async {
        final r = await api.PettyCashAssignment_GetList(employeeId: _employee?.id, skipCount: skip, maxResultCount: max);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, a) {
        final stopped = a.toPeriod != null && a.toPeriod!.compareTo(current) < 0;
        return OpsTile(
          avatar: PersonAvatar.employee(a.employee),
          title: '${a.employee.fullName} · ${a.type.name}',
          subtitle: [
            '${s.t('من', 'From')} ${a.fromPeriod}',
            if (a.toPeriod != null) '${s.t('إلى', 'To')} ${a.toPeriod}',
            if (a.note != null) a.note!,
          ].join(' · '),
          trailing: Row(mainAxisSize: MainAxisSize.min, children: [
            Text('${money(a.amount, a.currency)} / ${s.t('شهر', 'month')}', style: const TextStyle(fontWeight: FontWeight.w700)),
            const SizedBox(width: 8),
            StatusPill(stopped ? s.t('متوقفة', 'Stopped') : s.t('سارية', 'Active'), tone: stopped ? Tone.neutral : Tone.success),
          ]),
          onTap: canManage ? () => _edit(a) : null,
        );
      },
    );
  }
}

// ---------------------------------------------------------------- types

class _TypesView extends ConsumerStatefulWidget {
  const _TypesView();

  @override
  ConsumerState<_TypesView> createState() => _TypesViewState();
}

class _TypesViewState extends ConsumerState<_TypesView> {
  int _version = 0;

  Future<void> _edit([PettyCashTypeDto? t]) async {
    final s = S.of(context);
    final api = ref.read(hrApiProvider).finance;
    final ar = TextEditingController(text: t?.nameAr);
    final en = TextEditingController(text: t?.nameEn);
    var active = t?.isActive ?? true;
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: t == null ? s.t('نوع نثرية جديد', 'New type') : s.t('تعديل النوع', 'Edit type'),
      fields: (setState) => [
        TextFormField(controller: ar, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالعربية (مثل: طعام)', 'Arabic name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'English name')), validator: (v) => requiredValidator(context, v)),
        if (t != null) SwitchListTile(contentPadding: EdgeInsets.zero, value: active, title: Text(s.t('مفعّل', 'Active')), onChanged: (v) => setState(() => active = v)),
      ],
      onSubmit: () => runAction(context, () async {
        if (t == null) {
          await api.PettyCashType_Create(idempotencyKey: key, body: PettyCashTypeCreateInputDto(nameAr: ar.text.trim(), nameEn: en.text.trim()));
        } else {
          await api.PettyCashType_Update(id: t.id, body: PettyCashTypeUpdateInputDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), isActive: active, concurrencyStamp: t.concurrencyStamp));
        }
      }, done: s.saved),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final canManage = ref.watch(sessionProvider).can(HrOps.PettyCashType_Create);
    return PagedList<PettyCashTypeDto>(
      key: ValueKey('pc-types|$_version'),
      header: canManage
          ? Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Align(
                  alignment: AlignmentDirectional.centerStart,
                  child: FilledButton.tonalIcon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('نوع جديد', 'New type')))),
            )
          : null,
      fetch: (skip, max) async {
        final r = await ref.read(hrApiProvider).finance.PettyCashType_GetList(skipCount: skip, maxResultCount: max);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, t) => OpsTile(
        leading: Icons.local_dining_outlined,
        title: _typeName(context, t),
        trailing: t.isActive ? null : StatusPill(s.t('موقوف', 'Inactive'), tone: Tone.neutral),
        onTap: canManage ? () => _edit(t) : null,
      ),
    );
  }
}
