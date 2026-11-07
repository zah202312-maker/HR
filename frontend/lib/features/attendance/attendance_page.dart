import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../api/runtime/json.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';

final myAttendanceProvider = FutureProvider.autoDispose.family<List<AttendanceDayDto>, DateTime>((ref, month) async {
  final from = DateTime(month.year, month.month, 1);
  final to = DateTime(month.year, month.month + 1, 0);
  final r = await ref.watch(hrApiProvider).attendance.Attendance_GetMy(
        fromDate: from,
        toDate: to,
        maxResultCount: 62,
        sorting: 'date desc',
      );
  return r.items;
});

/// S-ATT-01: Attendance_GetMy by month. There is no check-in API; data comes from devices.
class AttendancePage extends ConsumerStatefulWidget {
  const AttendancePage({super.key});

  @override
  ConsumerState<AttendancePage> createState() => _AttendancePageState();
}

class _AttendancePageState extends ConsumerState<AttendancePage> {
  DateTime _month = DateTime(DateTime.now().year, DateTime.now().month);

  Tone _tone(AttendanceDayStatus st) => switch (st) {
        AttendanceDayStatus.present => Tone.success,
        AttendanceDayStatus.late || AttendanceDayStatus.incomplete => Tone.warning,
        AttendanceDayStatus.absent => Tone.danger,
        AttendanceDayStatus.onLeave || AttendanceDayStatus.holiday => Tone.info,
        _ => Tone.neutral,
      };

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final canCorrect = ref.watch(sessionProvider).can(HrOps.AttendanceCorrection_Create);
    final value = ref.watch(myAttendanceProvider(_month));
    final monthLabel = '${_month.year}-${_month.month.toString().padLeft(2, '0')}';
    return HrPage(
      title: s.attendance,
      actions: [
        if (canCorrect)
          FilledButton.icon(
            onPressed: () => context.go('/attendance/correction'),
            icon: const Icon(Icons.fingerprint),
            label: Text(s.correction),
          ),
      ],
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Row(children: [
          IconButton(
            onPressed: () => setState(() => _month = DateTime(_month.year, _month.month - 1)),
            icon: const Icon(Icons.chevron_left),
            tooltip: s.from,
          ),
          Directionality(textDirection: TextDirection.ltr, child: Text(monthLabel, style: Theme.of(context).textTheme.titleMedium)),
          IconButton(
            onPressed: () => setState(() => _month = DateTime(_month.year, _month.month + 1)),
            icon: const Icon(Icons.chevron_right),
            tooltip: s.to,
          ),
        ]),
        const SizedBox(height: 8),
        Expanded(
          child: AsyncBody<List<AttendanceDayDto>>(
            value: value,
            onRetry: () => ref.invalidate(myAttendanceProvider(_month)),
            builder: (days) {
              if (days.isEmpty) return const EmptyState(icon: Icons.event_busy);
              return ListView.separated(
                padding: const EdgeInsets.only(bottom: 24),
                itemCount: days.length,
                separatorBuilder: (_, __) => const SizedBox(height: 8),
                itemBuilder: (context, i) {
                  final d = days[i];
                  final tone = _tone(d.status);
                  final needsCorrection = canCorrect &&
                      (d.status == AttendanceDayStatus.incomplete ||
                          d.status == AttendanceDayStatus.absent ||
                          d.status == AttendanceDayStatus.late);
                  return HrCard(
                    onTap: ref.watch(sessionProvider).can(HrOps.Attendance_GetDayDetail) ? () => context.push('/attendance/day/${d.id}') : null,
                    child: Row(children: [
                      SizedBox(
                        width: 96,
                        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                          Text(f.weekday(d.date), style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                          Text(f.date(d.date), style: const TextStyle(fontWeight: FontWeight.w600)),
                        ]),
                      ),
                      Expanded(
                        child: Wrap(spacing: 16, runSpacing: 4, children: [
                          Text('${s.checkIn}: ${f.time(d.checkIn)}'),
                          Text('${s.checkOut}: ${f.time(d.checkOut)}'),
                          if (d.lateMinutes > 0) Text('${s.lateLabel}: ${d.lateMinutes} ${s.minutes}'),
                          if (d.workedMinutes > 0) Text('${s.worked}: ${(d.workedMinutes / 60).toStringAsFixed(1)} ${s.hours}'),
                          if (d.overtimeMinutes > 0) Text('${s.overtime}: ${d.overtimeMinutes} ${s.minutes}'),
                          if (d.isCorrected) Text(s.corrected, style: TextStyle(color: context.hr.accent)),
                        ]),
                      ),
                      StatusPill(s.dayStatus(d.status), tone: tone),
                      if (needsCorrection)
                        IconButton(
                          tooltip: s.correction,
                          onPressed: () => context.go('/attendance/correction?date=${hrDateToJson(d.date)}'),
                          icon: const Icon(Icons.edit_calendar_outlined),
                        ),
                    ]),
                  );
                },
              );
            },
          ),
        ),
      ]),
    );
  }
}
