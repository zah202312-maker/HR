import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/pickers.dart';

String dayOfWeekLabel(S s, WorkShiftDayOfWeek d) => switch (d) {
      WorkShiftDayOfWeek.sunday => s.t('الأحد', 'Sunday'),
      WorkShiftDayOfWeek.monday => s.t('الاثنين', 'Monday'),
      WorkShiftDayOfWeek.tuesday => s.t('الثلاثاء', 'Tuesday'),
      WorkShiftDayOfWeek.wednesday => s.t('الأربعاء', 'Wednesday'),
      WorkShiftDayOfWeek.thursday => s.t('الخميس', 'Thursday'),
      WorkShiftDayOfWeek.friday => s.t('الجمعة', 'Friday'),
      WorkShiftDayOfWeek.saturday => s.t('السبت', 'Saturday'),
      WorkShiftDayOfWeek.$unknown => '—',
    };

const workWeek = <WorkShiftDayOfWeek>[
  WorkShiftDayOfWeek.saturday,
  WorkShiftDayOfWeek.sunday,
  WorkShiftDayOfWeek.monday,
  WorkShiftDayOfWeek.tuesday,
  WorkShiftDayOfWeek.wednesday,
  WorkShiftDayOfWeek.thursday,
  WorkShiftDayOfWeek.friday,
];

String shiftTypeLabel(S s, WorkShiftType t) => switch (t) {
      WorkShiftType.fixed => s.t('ثابت', 'Fixed'),
      WorkShiftType.rotating => s.t('متناوب', 'Rotating'),
      WorkShiftType.overnight => s.t('ليلي', 'Overnight'),
      WorkShiftType.$unknown => '—',
    };

Tone dayTone(AttendanceDayStatus st) => switch (st) {
      AttendanceDayStatus.present => Tone.success,
      AttendanceDayStatus.late || AttendanceDayStatus.incomplete => Tone.warning,
      AttendanceDayStatus.absent => Tone.danger,
      AttendanceDayStatus.onLeave || AttendanceDayStatus.holiday => Tone.info,
      _ => Tone.neutral,
    };

/// HH:mm field (contract time pattern).
class TimeField extends StatelessWidget {
  const TimeField({super.key, required this.label, required this.value, required this.onChanged});
  final String label;
  final String? value;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) => PickerField(
        label: label,
        value: value,
        icon: Icons.schedule,
        onTap: () async {
          final parts = (value ?? '08:00').split(':');
          final t = await showTimePicker(
            context: context,
            initialTime: TimeOfDay(hour: int.parse(parts[0]), minute: int.parse(parts[1])),
            builder: (context, child) => MediaQuery(data: MediaQuery.of(context).copyWith(alwaysUse24HourFormat: true), child: child!),
          );
          if (t != null) onChanged('${t.hour.toString().padLeft(2, '0')}:${t.minute.toString().padLeft(2, '0')}');
        },
      );
}

/// Month navigator used by attendance screens.
class MonthBar extends StatelessWidget {
  const MonthBar({super.key, required this.month, required this.onChanged});
  final DateTime month;
  final ValueChanged<DateTime> onChanged;

  @override
  Widget build(BuildContext context) => Row(mainAxisSize: MainAxisSize.min, children: [
        IconButton(onPressed: () => onChanged(DateTime(month.year, month.month - 1)), icon: const Icon(Icons.chevron_left)),
        Directionality(
          textDirection: TextDirection.ltr,
          child: Text('${month.year}-${month.month.toString().padLeft(2, '0')}', style: Theme.of(context).textTheme.titleMedium),
        ),
        IconButton(onPressed: () => onChanged(DateTime(month.year, month.month + 1)), icon: const Icon(Icons.chevron_right)),
      ]);
}

DateTime monthStart(DateTime m) => DateTime(m.year, m.month, 1);
DateTime monthEnd(DateTime m) => DateTime(m.year, m.month + 1, 0);

/// Period totals (AttendancePeriodSummaryDto).
class AttendanceSummaryBar extends StatelessWidget {
  const AttendanceSummaryBar({super.key, required this.summary});
  final AttendancePeriodSummaryDto summary;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    Widget chip(String label, String v, Tone tone) => StatusPill('$label: $v', tone: tone);
    return Wrap(spacing: 8, runSpacing: 8, children: [
      chip(s.dayStatus(AttendanceDayStatus.present), '${summary.presentDays}', Tone.success),
      chip(s.dayStatus(AttendanceDayStatus.late), '${summary.lateDays} (${summary.totalLateMinutes} ${s.minutes})', Tone.warning),
      chip(s.dayStatus(AttendanceDayStatus.absent), '${summary.absentDays}', Tone.danger),
      chip(s.dayStatus(AttendanceDayStatus.incomplete), '${summary.incompleteDays}', Tone.warning),
      chip(s.dayStatus(AttendanceDayStatus.onLeave), f.number(summary.leaveDays), Tone.info),
      chip(s.worked, '${(summary.workedMinutes / 60).toStringAsFixed(1)} ${s.hours}', Tone.neutral),
      if (summary.overtimeMinutes > 0) chip(s.overtime, '${summary.overtimeMinutes} ${s.minutes}', Tone.neutral),
    ]);
  }
}

/// One attendance day row; tap opens the day detail (Attendance_GetDayDetail).
class AttendanceDayTile extends ConsumerWidget {
  const AttendanceDayTile({super.key, required this.day, this.showEmployee = false, this.trailing});
  final AttendanceDayDto day;
  final bool showEmployee;
  final Widget? trailing;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final canDetail = ref.watch(sessionProvider).can(HrOps.Attendance_GetDayDetail);
    return HrCard(
      onTap: canDetail ? () => context.push('/attendance/day/${day.id}') : null,
      child: Row(children: [
        SizedBox(
          width: showEmployee ? 180 : 100,
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            if (showEmployee) Text(day.employee.fullName, style: const TextStyle(fontWeight: FontWeight.w600), overflow: TextOverflow.ellipsis),
            Text('${f.weekday(day.date)} ${f.date(day.date)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
          ]),
        ),
        Expanded(
          child: Wrap(spacing: 16, runSpacing: 4, children: [
            Text('${s.checkIn}: ${f.time(day.checkIn)}'),
            Text('${s.checkOut}: ${f.time(day.checkOut)}'),
            if (day.lateMinutes > 0) Text('${s.lateLabel}: ${day.lateMinutes} ${s.minutes}'),
            if (day.workedMinutes > 0) Text('${s.worked}: ${(day.workedMinutes / 60).toStringAsFixed(1)} ${s.hours}'),
            if (day.isCorrected) Text(s.corrected, style: TextStyle(color: context.hr.accent)),
          ]),
        ),
        StatusPill(s.dayStatus(day.status), tone: dayTone(day.status)),
        if (trailing != null) trailing!,
      ]),
    );
  }
}

/// Shows a one-time secret (device key) with a copy button.
Future<void> showSecretOnce(BuildContext context, {required String title, required String secret}) {
  final s = S.of(context);
  return showHrDialog<void>(
    context: context,
    barrierDismissible: false,
    builder: (ctx) => AlertDialog(
      title: Text(title),
      content: SizedBox(
        width: 460,
        child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Text(s.t('يُعرض مرة واحدة فقط. انسخه وضعه في إعدادات الجهاز أو الوسيط المحلي الآن.',
              'Shown once only. Copy it into the device or local agent settings now.'),
              style: const TextStyle(color: HrColors.warning)),
          const SizedBox(height: 12),
          Directionality(textDirection: TextDirection.ltr, child: SelectableText(secret, style: const TextStyle(fontFamily: 'monospace'))),
        ]),
      ),
      actions: [
        TextButton.icon(onPressed: () => Clipboard.setData(ClipboardData(text: secret)), icon: const Icon(Icons.copy), label: Text(s.t('نسخ', 'Copy'))),
        FilledButton(onPressed: () => Navigator.pop(ctx), child: Text(s.done)),
      ],
    ),
  );
}

/// Employee choice for HR (Employee_Lookup) or a manager (Manager_GetMyTeam).
Future<EmployeeRefDto?> pickEmployee(BuildContext context, WidgetRef ref, {bool teamOnly = false}) {
  final s = S.of(context);
  final api = ref.read(hrApiProvider).employees;
  return showSearchPicker<EmployeeRefDto>(
    context,
    title: s.employee,
    search: (q) async {
      if (teamOnly) {
        final r = await api.Manager_GetMyTeam(search: q.isEmpty ? null : q, maxResultCount: 50);
        return [for (final m in r.items) EmployeeRefDto(id: m.id, fullName: m.fullName, departmentName: m.department.name)];
      }
      return (await api.Employee_Lookup(search: q.isEmpty ? null : q, maxResultCount: 50)).items;
    },
    label: (e) => e.fullName,
    subtitle: (e) => e.departmentName ?? '',
  );
}
