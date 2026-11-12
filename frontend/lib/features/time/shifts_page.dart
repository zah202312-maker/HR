import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../org/org_providers.dart';
import 'time_shared.dart';

/// S-SHF-01: WorkShift_List (all templates, active and inactive).
class ShiftsPage extends ConsumerWidget {
  const ShiftsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final api = ref.watch(hrApiProvider).workShift;
    final canManage = ref.watch(sessionProvider).can(HrOps.WorkShift_Create);
    return HrPage(
      title: s.t('قوالب الدوام', 'Shift templates'),
      actions: [
        if (canManage)
          FilledButton.icon(onPressed: () => context.go('/shifts/new'), icon: const Icon(Icons.add), label: Text(s.t('قالب جديد', 'New template'))),
      ],
      child: PagedList<WorkShiftDto>(
        fetch: (skip, max) async {
          final r = await api.WorkShift_List(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, w) => HrCard(
          onTap: canManage ? () => context.go('/shifts/${w.id}') : null,
          child: Row(children: [
            Icon(w.type == WorkShiftType.overnight ? Icons.nightlight_outlined : Icons.schedule, color: context.hr.accent),
            const SizedBox(width: 12),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(w.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                Text(
                  [
                    shiftTypeLabel(s, w.type),
                    '${s.t('السماح', 'Grace')} ${w.graceMinutes} ${s.minutes}',
                    for (final d in w.days.take(7))
                      '${d.dayOfWeek != null ? dayOfWeekLabel(s, d.dayOfWeek!) : '#${d.cycleDayNumber}'} ${d.startTime}–${d.endTime}',
                  ].join(' · '),
                  style: TextStyle(color: context.hr.textSecondary, fontSize: 12),
                ),
              ]),
            ),
            StatusPill('${w.assignedEmployeeCount} ${s.t('موظف', 'employees')}', tone: Tone.info),
            const SizedBox(width: 6),
            StatusPill(w.isActive ? s.t('مفعّل', 'Active') : s.t('معطّل', 'Inactive'), tone: w.isActive ? Tone.success : Tone.neutral),
          ]),
        ),
      ),
    );
  }
}

class _DayRow {
  _DayRow({this.dayOfWeek, this.cycleDayNumber, this.start = '08:00', this.end = '16:00', this.crossesMidnight = false});
  WorkShiftDayOfWeek? dayOfWeek;
  int? cycleDayNumber;
  String start;
  String end;
  bool crossesMidnight;
}

/// S-SHF-02: WorkShift_Create / WorkShift_Update. Days by weekday for fixed and overnight,
/// by cycle day for rotating. The server validates the schedule (Shift:InvalidSchedule).
class ShiftEditorPage extends ConsumerStatefulWidget {
  const ShiftEditorPage({super.key, this.id});
  final String? id;

  @override
  ConsumerState<ShiftEditorPage> createState() => _ShiftEditorPageState();
}

class _ShiftEditorPageState extends ConsumerState<ShiftEditorPage> {
  final _form = GlobalKey<FormState>();
  final _ar = TextEditingController();
  final _en = TextEditingController();
  final _grace = TextEditingController(text: '10');
  final _afterHours = TextEditingController();
  final _cycle = TextEditingController(text: '7');
  WorkShiftType _type = WorkShiftType.fixed;
  WorkShiftOvertimePolicy _overtime = WorkShiftOvertimePolicy.none;
  bool _active = true;
  final List<_DayRow> _days = [];
  WorkShiftDto? _loaded;
  bool _loading = false;
  bool _busy = false;
  final String _idempotencyKey = const Uuid().v4();

  @override
  void initState() {
    super.initState();
    if (widget.id == null) {
      for (final d in workWeek.take(5)) {
        _days.add(_DayRow(dayOfWeek: d));
      }
    } else {
      _load();
    }
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final w = await ref.read(hrApiProvider).workShift.WorkShift_Get(id: widget.id!);
      _loaded = w;
      _ar.text = w.nameAr;
      _en.text = w.nameEn;
      _grace.text = '${w.graceMinutes}';
      _afterHours.text = w.overtimeAfterHours?.toString() ?? '';
      _cycle.text = '${w.rotationCycleDays ?? 7}';
      _type = w.type;
      _overtime = w.overtimePolicy;
      _active = w.isActive;
      _days
        ..clear()
        ..addAll(w.days.map((d) => _DayRow(
              dayOfWeek: d.dayOfWeek,
              cycleDayNumber: d.cycleDayNumber,
              start: d.startTime,
              end: d.endTime,
              crossesMidnight: d.crossesMidnight,
            )));
    } catch (e) {
      if (mounted) showError(context, e);
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  void dispose() {
    for (final c in [_ar, _en, _grace, _afterHours, _cycle]) {
      c.dispose();
    }
    super.dispose();
  }

  bool get _rotating => _type == WorkShiftType.rotating;

  List<WorkShiftDayDto> _dayDtos() => [
        for (final d in _days)
          WorkShiftDayDto(
            dayOfWeek: _rotating ? null : d.dayOfWeek,
            cycleDayNumber: _rotating ? d.cycleDayNumber : null,
            startTime: d.start,
            endTime: d.end,
            crossesMidnight: d.crossesMidnight,
          ),
      ];

  Future<void> _save() async {
    final s = S.of(context);
    if (!_form.currentState!.validate() || _days.isEmpty) return;
    setState(() => _busy = true);
    final api = ref.read(hrApiProvider).workShift;
    final grace = int.parse(_grace.text.trim());
    final after = _overtime == WorkShiftOvertimePolicy.afterDailyHours ? double.tryParse(_afterHours.text.trim()) : null;
    final cycle = _rotating ? int.tryParse(_cycle.text.trim()) : null;
    final ok = await runAction(context, () async {
      if (_loaded == null) {
        await api.WorkShift_Create(
          idempotencyKey: _idempotencyKey,
          body: WorkShiftCreateDto(
            nameAr: _ar.text.trim(),
            nameEn: _en.text.trim(),
            type: _type,
            graceMinutes: grace,
            overtimePolicy: _overtime,
            overtimeAfterHours: after,
            rotationCycleDays: cycle,
            days: _dayDtos(),
          ),
        );
      } else {
        await api.WorkShift_Update(
          id: _loaded!.id,
          body: WorkShiftUpdateDto(
            nameAr: _ar.text.trim(),
            nameEn: _en.text.trim(),
            type: _type,
            graceMinutes: grace,
            overtimePolicy: _overtime,
            overtimeAfterHours: after,
            rotationCycleDays: cycle,
            days: _dayDtos(),
            isActive: _active,
            concurrencyStamp: _loaded!.concurrencyStamp,
          ),
        );
      }
    }, done: s.saved);
    if (!mounted) return;
    setState(() => _busy = false);
    if (ok) {
      ref.invalidate(activeShiftsProvider);
      context.go('/shifts');
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    if (_loading) return const Center(child: CircularProgressIndicator());
    return HrPage(
      title: widget.id == null ? s.t('قالب دوام جديد', 'New shift template') : s.t('تعديل قالب الدوام', 'Edit shift template'),
      child: SingleChildScrollView(
        padding: const EdgeInsets.only(bottom: 24),
        child: Form(
          key: _form,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            HrCard(
              child: FieldGrid(children: [
                TextFormField(controller: _ar, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Name (Arabic)')), validator: (v) => requiredValidator(context, v)),
                TextFormField(controller: _en, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Name (English)')), validator: (v) => requiredValidator(context, v)),
                DropdownButtonFormField<WorkShiftType>(
                  initialValue: _type,
                  decoration: InputDecoration(labelText: s.t('النوع', 'Type')),
                  items: [
                    for (final t in WorkShiftType.values.where((v) => v != WorkShiftType.$unknown))
                      DropdownMenuItem(value: t, child: Text(shiftTypeLabel(s, t))),
                  ],
                  onChanged: (v) => setState(() {
                    _type = v ?? _type;
                    for (var i = 0; i < _days.length; i++) {
                      _days[i].cycleDayNumber ??= i + 1;
                      _days[i].dayOfWeek ??= workWeek[i % 7];
                    }
                  }),
                ),
                TextFormField(
                  controller: _grace,
                  keyboardType: TextInputType.number,
                  decoration: InputDecoration(labelText: s.t('دقائق السماح بالتأخير', 'Grace minutes')),
                  validator: (v) {
                    final n = int.tryParse((v ?? '').trim());
                    return n == null || n < 0 || n > 240 ? s.t('من 0 إلى 240', '0 to 240') : null;
                  },
                ),
                DropdownButtonFormField<WorkShiftOvertimePolicy>(
                  initialValue: _overtime,
                  decoration: InputDecoration(labelText: s.t('العمل الإضافي', 'Overtime')),
                  items: [
                    DropdownMenuItem(value: WorkShiftOvertimePolicy.none, child: Text(s.t('لا يُحسب', 'Not counted'))),
                    DropdownMenuItem(value: WorkShiftOvertimePolicy.afterShiftEnd, child: Text(s.t('بعد نهاية فترة العمل', 'After work period end'))),
                    DropdownMenuItem(value: WorkShiftOvertimePolicy.afterDailyHours, child: Text(s.t('بعد عدد ساعات يومية', 'After daily hours'))),
                  ],
                  onChanged: (v) => setState(() => _overtime = v ?? _overtime),
                ),
                if (_overtime == WorkShiftOvertimePolicy.afterDailyHours)
                  TextFormField(
                    controller: _afterHours,
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    decoration: InputDecoration(labelText: s.t('بعد كم ساعة', 'After how many hours')),
                    validator: (v) => double.tryParse((v ?? '').trim()) == null ? s.requiredField : null,
                  ),
                if (_rotating)
                  TextFormField(
                    controller: _cycle,
                    keyboardType: TextInputType.number,
                    decoration: InputDecoration(labelText: s.t('طول الدورة بالأيام', 'Cycle length (days)')),
                    validator: (v) {
                      final n = int.tryParse((v ?? '').trim());
                      return n == null || n < 2 || n > 366 ? s.t('من 2 إلى 366', '2 to 366') : null;
                    },
                  ),
                if (_loaded != null)
                  SwitchListTile(
                    contentPadding: EdgeInsets.zero,
                    title: Text(s.t('مفعّل', 'Active')),
                    value: _active,
                    onChanged: (v) => setState(() => _active = v),
                  ),
              ]),
            ),
            const SizedBox(height: 12),
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(
                  s.t('أيام الدوام', 'Working days'),
                  trailing: TextButton.icon(
                    onPressed: () => setState(() => _days.add(_DayRow(
                          dayOfWeek: _rotating ? null : workWeek[_days.length % 7],
                          cycleDayNumber: _rotating ? _days.length + 1 : null,
                        ))),
                    icon: const Icon(Icons.add),
                    label: Text(s.t('إضافة يوم', 'Add day')),
                  ),
                ),
                for (var i = 0; i < _days.length; i++)
                  Padding(
                    key: ObjectKey(_days[i]),
                    padding: const EdgeInsets.only(bottom: 10),
                    child: Wrap(spacing: 10, runSpacing: 10, crossAxisAlignment: WrapCrossAlignment.center, children: [
                      SizedBox(
                        width: 170,
                        child: _rotating
                            ? TextFormField(
                                initialValue: '${_days[i].cycleDayNumber ?? i + 1}',
                                keyboardType: TextInputType.number,
                                decoration: InputDecoration(labelText: s.t('يوم الدورة', 'Cycle day')),
                                onChanged: (v) => _days[i].cycleDayNumber = int.tryParse(v),
                              )
                            : DropdownButtonFormField<WorkShiftDayOfWeek>(
                                initialValue: _days[i].dayOfWeek,
                                decoration: InputDecoration(labelText: s.t('اليوم', 'Day')),
                                items: [for (final d in workWeek) DropdownMenuItem(value: d, child: Text(dayOfWeekLabel(s, d)))],
                                onChanged: (v) => setState(() => _days[i].dayOfWeek = v),
                              ),
                      ),
                      SizedBox(width: 140, child: TimeField(label: s.from, value: _days[i].start, onChanged: (v) => setState(() => _days[i].start = v))),
                      SizedBox(width: 140, child: TimeField(label: s.to, value: _days[i].end, onChanged: (v) => setState(() => _days[i].end = v))),
                      FilterChip(
                        label: Text(s.t('تعبر منتصف الليل', 'Crosses midnight')),
                        selected: _days[i].crossesMidnight,
                        onSelected: (v) => setState(() => _days[i].crossesMidnight = v),
                      ),
                      IconButton(
                        onPressed: _days.length > 1 ? () => setState(() => _days.removeAt(i)) : null,
                        icon: const Icon(Icons.remove_circle_outline, color: HrColors.danger),
                      ),
                    ]),
                  ),
              ]),
            ),
            const SizedBox(height: 16),
            Align(
              alignment: AlignmentDirectional.centerStart,
              child: FilledButton.icon(onPressed: _busy ? null : _save, icon: const Icon(Icons.save), label: Text(s.save)),
            ),
          ]),
        ),
      ),
    );
  }
}
