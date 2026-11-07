import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../requests/request_widgets.dart';
import 'time_shared.dart';

String ledgerTypeLabel(S s, LeaveLedgerType t) => switch (t) {
      LeaveLedgerType.grant => s.t('استحقاق', 'Grant'),
      LeaveLedgerType.carryIn => s.t('ترحيل', 'Carry-in'),
      LeaveLedgerType.hold => s.t('حجز', 'Hold'),
      LeaveLedgerType.release => s.t('فك حجز', 'Release'),
      LeaveLedgerType.deduct => s.t('خصم', 'Deduct'),
      LeaveLedgerType.adjust => s.t('تعديل', 'Adjust'),
      LeaveLedgerType.reverse => s.t('تصحيح عكسي', 'Reverse'),
      LeaveLedgerType.$unknown => '—',
    };

/// HR leave administration for one employee: balance (LeaveBalance_GetByEmployee),
/// entitlement and adjustment, ledger, leave requests with reversal (decision 17).
class LeaveAdminPage extends ConsumerStatefulWidget {
  const LeaveAdminPage({super.key});

  @override
  ConsumerState<LeaveAdminPage> createState() => _LeaveAdminPageState();
}

class _LeaveAdminPageState extends ConsumerState<LeaveAdminPage> {
  EmployeeRefDto? _employee;
  LeaveBalanceDto? _balance;
  Object? _error;
  bool _loading = false;
  int _version = 0;

  Future<void> _load() async {
    final e = _employee;
    if (e == null) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final b = await ref.read(hrApiProvider).leaveBalance.LeaveBalance_GetByEmployee(id: e.id);
      if (mounted) setState(() => _balance = b);
    } catch (err) {
      if (mounted) setState(() => _error = err);
    } finally {
      if (mounted) {
        setState(() {
          _loading = false;
          _version++;
        });
      }
    }
  }

  Future<void> _setEntitlement(LeaveBalanceDto b) async {
    final s = S.of(context);
    final days = TextEditingController(text: b.nextYearEntitlementDays?.toString() ?? b.currentYearEntitlementDays.toString());
    final reason = TextEditingController();
    final ok = await showFormDialog(
      context,
      title: s.t('تحديد الاستحقاق', 'Set entitlement'),
      fields: (_) => [
        Text(
          b.isEntitlementSet
              ? s.t('التغيير يبدأ من سنة الموظف القادمة. لتعديل السنة الحالية استخدمي «تعديل الرصيد».',
                  'The change starts next employee year. Use "Adjust" for the current year.')
              : s.t('أول تحديد يفتح سنة الموظف الحالية.', 'The first value opens the current employee year.'),
          style: Theme.of(context).textTheme.labelSmall,
        ),
        TextFormField(
          controller: days,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: InputDecoration(labelText: s.t('عدد الأيام', 'Days')),
          validator: (v) => (double.tryParse((v ?? '').trim()) ?? -1) < 0 ? s.requiredField : null,
        ),
        TextFormField(controller: reason, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).leaveBalance.LeaveBalance_SetEntitlement(
              id: _employee!.id,
              body: LeaveEntitlementUpdateDto(
                entitlementDays: double.parse(days.text.trim()),
                reason: reason.text.trim().isEmpty ? null : reason.text.trim(),
                concurrencyStamp: b.concurrencyStamp,
              ),
            ),
        done: s.saved,
      ),
    );
    if (ok) _load();
  }

  Future<void> _adjust() async {
    final s = S.of(context);
    final days = TextEditingController();
    final reason = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('تعديل الرصيد', 'Adjust balance'),
      fields: (_) => [
        TextFormField(
          controller: days,
          keyboardType: const TextInputType.numberWithOptions(decimal: true, signed: true),
          decoration: InputDecoration(labelText: s.t('الأيام (موجب للإضافة وسالب للخصم)', 'Days (+ add, - deduct)')),
          validator: (v) => (double.tryParse((v ?? '').trim()) ?? 0) == 0 ? s.requiredField : null,
        ),
        TextFormField(controller: reason, maxLines: 3, decoration: InputDecoration(labelText: s.reason), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).leaveBalance.LeaveBalance_Adjust(
            id: _employee!.id, idempotencyKey: key, body: LeaveBalanceAdjustDto(days: double.parse(days.text.trim()), reason: reason.text.trim())),
        done: s.saved,
      ),
    );
    if (ok) _load();
  }

  Future<void> _reverse(LeaveRequestDto l) async {
    final s = S.of(context);
    final reason = await askText(context, title: s.t('تصحيح إجازة معتمدة', 'Reverse approved leave'), label: s.reason);
    if (reason == null || !mounted) return;
    final ok = await runAction(
      context,
      () => ref.read(hrApiProvider).leaveRequest.LeaveRequest_Reverse(id: l.requestId, body: ReasonInputDto(reason: reason)),
      done: s.t('أُعيدت الأيام وأُلغي الطلب', 'Days returned; the request was cancelled'),
    );
    if (ok) _load();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider);
    final b = _balance;
    final e = _employee;
    return HrPage(
      title: s.t('أرصدة الإجازات', 'Leave balances'),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        SizedBox(
          width: 320,
          child: PickerField(
            label: s.employee,
            value: e?.fullName,
            onTap: () async {
              final picked = await pickEmployee(context, ref);
              if (picked != null) {
                setState(() {
                  _employee = picked;
                  _balance = null;
                });
                _load();
              }
            },
          ),
        ),
        const SizedBox(height: 12),
        if (e == null)
          Expanded(child: EmptyState(message: s.t('اختر موظفاً', 'Pick an employee'), icon: Icons.person_search))
        else if (_loading && b == null)
          const Expanded(child: Center(child: CircularProgressIndicator()))
        else if (_error != null)
          Expanded(child: ErrorState(error: _error!, onRetry: _load))
        else if (b != null)
          Expanded(
            child: DefaultTabController(
              length: 3,
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                HrCard(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                    Wrap(spacing: 24, runSpacing: 8, children: [
                      if (b.isEntitlementSet) ...[
                        _num(context, s.remaining, f.number(b.remainingDays), strong: true),
                        _num(context, s.entitlement, f.number(b.currentYearEntitlementDays)),
                        _num(context, s.carried, f.number(b.carriedDays)),
                        _num(context, s.t('التعديلات', 'Adjustments'), f.number(b.adjustmentDays)),
                        _num(context, s.used, f.number(b.usedDays)),
                        _num(context, s.held, f.number(b.heldDays)),
                        if (b.nextYearEntitlementDays != null) _num(context, s.t('استحقاق السنة القادمة', 'Next year'), f.number(b.nextYearEntitlementDays)),
                      ] else
                        Text(s.entitlementNotSet),
                    ]),
                    const SizedBox(height: 8),
                    Text('${f.date(b.yearStartDate)} → ${f.date(b.yearEndDate)}', style: Theme.of(context).textTheme.labelSmall),
                    const SizedBox(height: 12),
                    Wrap(spacing: 8, runSpacing: 8, children: [
                      if (session.can(HrOps.LeaveBalance_SetEntitlement))
                        FilledButton.icon(onPressed: () => _setEntitlement(b), icon: const Icon(Icons.event_available), label: Text(s.t('تحديد الاستحقاق', 'Set entitlement'))),
                      if (session.can(HrOps.LeaveBalance_Adjust) && b.isEntitlementSet)
                        OutlinedButton.icon(onPressed: _adjust, icon: const Icon(Icons.exposure), label: Text(s.t('تعديل الرصيد', 'Adjust'))),
                    ]),
                  ]),
                ),
                const SizedBox(height: 12),
                TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [
                  Tab(text: s.t('الإجازات', 'Leaves')),
                  Tab(text: s.t('حركات الرصيد', 'Ledger')),
                  Tab(text: s.attendance),
                ]),
                const SizedBox(height: 8),
                Expanded(
                  child: TabBarView(children: [
                    PagedList<LeaveRequestDto>(
                      key: ValueKey('leaves|${e.id}|$_version'),
                      fetch: (skip, max) async {
                        if (!session.can(HrOps.LeaveRequest_ListByEmployee)) return (<LeaveRequestDto>[], 0);
                        final r = await api.leaveRequest.LeaveRequest_ListByEmployee(id: e.id, skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
                        return (r.items, r.totalCount);
                      },
                      itemBuilder: (context, l) {
                        final tone = toneForStatus(l.status);
                        return HrCard(
                          onTap: () => context.go('/requests/${l.requestId}'),
                          child: Row(children: [
                            Expanded(
                              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                                Text(l.otherLeaveType?.name ?? s.leaveKindLabel(l.kind), style: const TextStyle(fontWeight: FontWeight.w600)),
                                Text(
                                  l.kind == LeaveKind.hourly
                                      ? '${f.date(l.date)} · ${l.fromTime}–${l.toTime}'
                                      : '${f.date(l.startDate)} → ${f.date(l.endDate)} · ${f.number(l.workingDays)} ${s.days}',
                                  style: TextStyle(color: context.hr.textSecondary),
                                ),
                              ]),
                            ),
                            StatusPill(s.requestStatus(l.status), tone: tone),
                            if (l.status == RequestStatus.approved && session.can(HrOps.LeaveRequest_Reverse))
                              IconButton(
                                tooltip: s.t('تصحيح عكسي', 'Reverse'),
                                onPressed: () => _reverse(l),
                                icon: const Icon(Icons.undo, color: HrColors.danger),
                              ),
                          ]),
                        );
                      },
                    ),
                    PagedList<LeaveLedgerEntryDto>(
                      key: ValueKey('ledger|${e.id}|$_version'),
                      fetch: (skip, max) async {
                        final r = await api.leaveBalance.LeaveLedger_GetByEmployee(id: e.id, skipCount: skip, maxResultCount: max);
                        return (r.items, r.totalCount);
                      },
                      itemBuilder: (context, x) => HrCard(
                        child: Row(children: [
                          SizedBox(width: 110, child: Text(ledgerTypeLabel(s, x.type), style: const TextStyle(fontWeight: FontWeight.w600))),
                          SizedBox(
                            width: 100,
                            child: Text(
                              x.days != null ? '${x.days! > 0 ? '+' : ''}${f.number(x.days)} ${s.days}' : '${f.number(x.hours)} ${s.hours}',
                              style: TextStyle(color: (x.days ?? x.hours ?? 0) < 0 ? HrColors.danger : HrColors.success),
                            ),
                          ),
                          Expanded(
                            child: Text([f.dateTime(x.creationTime), if (x.reason != null) x.reason!, if (x.createdBy != null) x.createdBy!.displayName].join(' · '),
                                style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                          ),
                        ]),
                      ),
                    ),
                    _EmployeeAttendanceTab(employeeId: e.id),
                  ]),
                ),
              ]),
            ),
          ),
      ]),
    );
  }

  Widget _num(BuildContext context, String label, String value, {bool strong = false}) => Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
          Text(value, style: TextStyle(fontSize: strong ? 22 : 16, fontWeight: FontWeight.w700, color: strong ? context.hr.accent : null)),
        ],
      );
}

class _EmployeeAttendanceTab extends ConsumerStatefulWidget {
  const _EmployeeAttendanceTab({required this.employeeId});
  final String employeeId;

  @override
  ConsumerState<_EmployeeAttendanceTab> createState() => _EmployeeAttendanceTabState();
}

class _EmployeeAttendanceTabState extends ConsumerState<_EmployeeAttendanceTab> {
  DateTime _month = DateTime(DateTime.now().year, DateTime.now().month);

  @override
  Widget build(BuildContext context) {
    final api = ref.watch(hrApiProvider).attendance;
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      Align(alignment: AlignmentDirectional.centerStart, child: MonthBar(month: _month, onChanged: (m) => setState(() => _month = m))),
      Expanded(
        child: FutureBuilder<AttendanceDayPagedResultDto>(
          key: ValueKey(_month),
          future: api.Attendance_GetByEmployee(
              id: widget.employeeId, fromDate: monthStart(_month), toDate: monthEnd(_month), maxResultCount: 62, sorting: 'date desc'),
          builder: (context, snap) {
            if (snap.hasError) return ErrorState(error: snap.error!);
            if (!snap.hasData) return const Center(child: CircularProgressIndicator());
            final d = snap.data!;
            return ListView.separated(
              itemCount: d.items.length + 1,
              separatorBuilder: (_, __) => const SizedBox(height: 8),
              itemBuilder: (context, i) => i == 0 ? AttendanceSummaryBar(summary: d.summary) : AttendanceDayTile(day: d.items[i - 1]),
            );
          },
        ),
      ),
    ]);
  }
}

/// System admin: other leave types (sick, maternity, bereavement...). No delete; deactivate instead.
class OtherLeaveTypesPage extends ConsumerStatefulWidget {
  const OtherLeaveTypesPage({super.key});

  @override
  ConsumerState<OtherLeaveTypesPage> createState() => _OtherLeaveTypesPageState();
}

class _OtherLeaveTypesPageState extends ConsumerState<OtherLeaveTypesPage> {
  int _version = 0;

  String _category(S s, OtherLeaveEligibleCategory c) => switch (c) {
        OtherLeaveEligibleCategory.all => s.t('الجميع', 'Everyone'),
        OtherLeaveEligibleCategory.male => s.t('الذكور', 'Male'),
        OtherLeaveEligibleCategory.female => s.t('الإناث', 'Female'),
        OtherLeaveEligibleCategory.$unknown => '—',
      };

  Future<void> _edit([OtherLeaveTypeDto? t]) async {
    final s = S.of(context);
    final ar = TextEditingController(text: t?.nameAr ?? '');
    final en = TextEditingController(text: t?.nameEn ?? '');
    final max = TextEditingController(text: t?.maxDays?.toString() ?? '');
    var paid = t?.isPaid ?? true;
    var attach = t?.requiresAttachment ?? false;
    var deducts = t?.deductsFromAnnualBalance ?? false;
    var active = t?.isActive ?? true;
    var category = t?.eligibleCategory ?? OtherLeaveEligibleCategory.all;
    final key = const Uuid().v4();
    final api = ref.read(hrApiProvider).otherLeaveType;
    final ok = await showFormDialog(
      context,
      title: t == null ? s.t('نوع إجازة جديد', 'New leave type') : s.t('تعديل نوع الإجازة', 'Edit leave type'),
      fields: (setState) => [
        TextFormField(controller: ar, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Name (Arabic)')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Name (English)')), validator: (v) => requiredValidator(context, v)),
        TextFormField(
          controller: max,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: InputDecoration(labelText: s.t('الحد الأقصى بالسنة (فارغ = بلا حد)', 'Max days per year (empty = no limit)')),
        ),
        DropdownButtonFormField<OtherLeaveEligibleCategory>(
          initialValue: category,
          decoration: InputDecoration(labelText: s.t('يستحقها', 'Eligible')),
          items: [
            for (final c in OtherLeaveEligibleCategory.values.where((v) => v != OtherLeaveEligibleCategory.$unknown))
              DropdownMenuItem(value: c, child: Text(_category(s, c))),
          ],
          onChanged: (v) => setState(() => category = v ?? category),
        ),
        SwitchListTile(contentPadding: EdgeInsets.zero, title: Text(s.t('مدفوعة', 'Paid')), value: paid, onChanged: (v) => setState(() => paid = v)),
        SwitchListTile(contentPadding: EdgeInsets.zero, title: Text(s.t('تتطلب مرفقاً', 'Requires attachment')), value: attach, onChanged: (v) => setState(() => attach = v)),
        SwitchListTile(contentPadding: EdgeInsets.zero, title: Text(s.t('تخصم من الرصيد السنوي', 'Deducts from annual balance')), value: deducts, onChanged: (v) => setState(() => deducts = v)),
        SwitchListTile(contentPadding: EdgeInsets.zero, title: Text(s.t('مفعّل', 'Active')), value: active, onChanged: (v) => setState(() => active = v)),
      ],
      onSubmit: () {
        final maxDays = double.tryParse(max.text.trim());
        return runAction(
          context,
          () => t == null
              ? api.OtherLeaveType_Create(
                  idempotencyKey: key,
                  body: OtherLeaveTypeCreateDto(
                      nameAr: ar.text.trim(),
                      nameEn: en.text.trim(),
                      isPaid: paid,
                      maxDays: maxDays,
                      requiresAttachment: attach,
                      eligibleCategory: category,
                      deductsFromAnnualBalance: deducts,
                      isActive: active))
              : api.OtherLeaveType_Update(
                  id: t.id,
                  body: OtherLeaveTypeUpdateDto(
                      nameAr: ar.text.trim(),
                      nameEn: en.text.trim(),
                      isPaid: paid,
                      maxDays: maxDays,
                      requiresAttachment: attach,
                      eligibleCategory: category,
                      deductsFromAnnualBalance: deducts,
                      isActive: active,
                      concurrencyStamp: t.concurrencyStamp)),
          done: s.saved,
        );
      },
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).otherLeaveType;
    return HrPage(
      title: s.t('أنواع الإجازات الأخرى', 'Other leave types'),
      actions: [FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('نوع جديد', 'New type')))],
      child: PagedList<OtherLeaveTypeDto>(
        key: ValueKey(_version),
        fetch: (skip, max) async {
          final r = await api.OtherLeaveType_List(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, t) => HrCard(
          onTap: () => _edit(t),
          child: Row(children: [
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(t.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                Text(
                  [
                    t.isPaid ? s.t('مدفوعة', 'Paid') : s.t('غير مدفوعة', 'Unpaid'),
                    t.maxDays == null ? s.t('بلا حد', 'No limit') : '${f.number(t.maxDays)} ${s.days}',
                    _category(s, t.eligibleCategory),
                    if (t.requiresAttachment) s.t('بمرفق', 'Attachment'),
                    if (t.deductsFromAnnualBalance) s.t('تخصم من السنوي', 'Deducts annual'),
                  ].join(' · '),
                  style: TextStyle(color: context.hr.textSecondary, fontSize: 12),
                ),
              ]),
            ),
            StatusPill(t.isActive ? s.t('مفعّل', 'Active') : s.t('معطّل', 'Inactive'), tone: t.isActive ? Tone.success : Tone.neutral),
          ]),
        ),
      ),
    );
  }
}

/// Department manager: team leave calendar (LeaveCalendar_GetTeam), approved and pending.
class TeamLeaveCalendarPage extends ConsumerStatefulWidget {
  const TeamLeaveCalendarPage({super.key});

  @override
  ConsumerState<TeamLeaveCalendarPage> createState() => _TeamLeaveCalendarPageState();
}

class _TeamLeaveCalendarPageState extends ConsumerState<TeamLeaveCalendarPage> {
  DateTime _month = DateTime(DateTime.now().year, DateTime.now().month);
  bool _pending = true;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).leaveCalendar;
    return HrPage(
      title: s.t('تقويم إجازات الفريق', 'Team leave calendar'),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Wrap(spacing: 12, crossAxisAlignment: WrapCrossAlignment.center, children: [
          MonthBar(month: _month, onChanged: (m) => setState(() => _month = m)),
          FilterChip(label: Text(s.t('يشمل المعلقة', 'Include pending')), selected: _pending, onSelected: (v) => setState(() => _pending = v)),
        ]),
        const SizedBox(height: 12),
        Expanded(
          child: PagedList<LeaveCalendarEntryDto>(
            key: ValueKey('$_month|$_pending'),
            pageSize: 100,
            fetch: (skip, max) async {
              final r = await api.LeaveCalendar_GetTeam(
                  fromDate: monthStart(_month), toDate: monthEnd(_month), includePending: _pending, skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => HrCard(
              onTap: () => context.go('/requests/${c.requestId}'),
              child: Row(children: [
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(c.employee.fullName, style: const TextStyle(fontWeight: FontWeight.w600)),
                    Text(
                      '${c.otherLeaveTypeName ?? s.leaveKindLabel(c.kind)} · ${f.date(c.startDate)}${c.endDate == c.startDate ? '' : ' → ${f.date(c.endDate)}'}'
                      '${c.fromTime != null ? ' · ${c.fromTime}–${c.toTime}' : ''}',
                      style: TextStyle(color: context.hr.textSecondary),
                    ),
                  ]),
                ),
                StatusPill(s.requestStatus(c.status), tone: toneForStatus(c.status)),
              ]),
            ),
          ),
        ),
      ]),
    );
  }
}
