import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/pickers.dart';
import 'time_shared.dart';

/// S-ATT-02 / S-ATT-03: team attendance for a department manager (Attendance_GetTeam)
/// and one employee's attendance for HR or the manager (Attendance_GetByEmployee).
class AttendanceAdminPage extends ConsumerStatefulWidget {
  const AttendanceAdminPage({super.key});

  @override
  ConsumerState<AttendanceAdminPage> createState() => _AttendanceAdminPageState();
}

class _AttendanceAdminPageState extends ConsumerState<AttendanceAdminPage> {
  DateTime _month = DateTime(DateTime.now().year, DateTime.now().month);
  EmployeeRefDto? _employee;
  AttendanceDayStatus? _status;
  Future<AttendanceDayPagedResultDto>? _future;
  String? _futureKey;

  Future<AttendanceDayPagedResultDto>? _query(bool isTeam) {
    final api = ref.read(hrApiProvider).attendance;
    final key = '$isTeam|$_month|${_employee?.id}|$_status';
    if (key == _futureKey) return _future;
    _futureKey = key;
    if (!isTeam && _employee == null) return _future = null;
    _future = isTeam && _employee == null
        ? api.Attendance_GetTeam(fromDate: monthStart(_month), toDate: monthEnd(_month), status: _status, maxResultCount: 200, sorting: 'date desc')
        : api.Attendance_GetByEmployee(
            id: _employee!.id, fromDate: monthStart(_month), toDate: monthEnd(_month), status: _status, maxResultCount: 62, sorting: 'date desc');
    return _future;
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final isTeam = session.can(HrOps.Attendance_GetTeam) && !session.can(HrOps.Employee_List);
    final future = _query(isTeam);
    return HrPage(
      title: isTeam ? s.t('حضور الفريق', 'Team attendance') : s.t('حضور الموظفين', 'Employee attendance'),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Wrap(spacing: 12, runSpacing: 12, crossAxisAlignment: WrapCrossAlignment.center, children: [
          SizedBox(
            width: 280,
            child: PickerField(
              label: isTeam ? s.t('موظف (الكل إذا فارغ)', 'Employee (all if empty)') : s.employee,
              value: _employee?.fullName,
              onClear: () => setState(() => _employee = null),
              onTap: () async {
                final e = await pickEmployee(context, ref, teamOnly: isTeam);
                if (e != null) setState(() => _employee = e);
              },
            ),
          ),
          MonthBar(month: _month, onChanged: (m) => setState(() => _month = m)),
          DropdownButton<AttendanceDayStatus?>(
            value: _status,
            hint: Text(s.status),
            items: [
              DropdownMenuItem(value: null, child: Text(s.all)),
              for (final st in AttendanceDayStatus.values.where((v) => v != AttendanceDayStatus.$unknown))
                DropdownMenuItem(value: st, child: Text(s.dayStatus(st))),
            ],
            onChanged: (v) => setState(() => _status = v),
          ),
        ]),
        const SizedBox(height: 12),
        Expanded(
          child: future == null
              ? EmptyState(message: s.t('اختر موظفاً لعرض حضوره', 'Pick an employee to see attendance'), icon: Icons.person_search)
              : FutureBuilder<AttendanceDayPagedResultDto>(
                  future: future,
                  builder: (context, snap) {
                    if (snap.hasError) {
                      return ErrorState(error: snap.error!, onRetry: () => setState(() => _futureKey = null));
                    }
                    if (!snap.hasData) return const Center(child: CircularProgressIndicator());
                    final data = snap.data!;
                    return ListView.separated(
                      padding: const EdgeInsets.only(bottom: 24),
                      itemCount: data.items.length + 1,
                      separatorBuilder: (_, __) => const SizedBox(height: 8),
                      itemBuilder: (context, i) => i == 0
                          ? AttendanceSummaryBar(summary: data.summary)
                          : AttendanceDayTile(day: data.items[i - 1], showEmployee: isTeam && _employee == null),
                    );
                  },
                ),
        ),
      ]),
    );
  }
}

final dayDetailProvider = FutureProvider.autoDispose.family<AttendanceDayDetailDto, String>(
  (ref, id) => ref.watch(hrApiProvider).attendance.Attendance_GetDayDetail(id: id),
);

/// S-ATT-04: raw punches (never changed) and the full correction history of one day.
class AttendanceDayDetailPage extends ConsumerWidget {
  const AttendanceDayDetailPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final value = ref.watch(dayDetailProvider(id));
    return HrPage(
      title: s.t('تفاصيل يوم الحضور', 'Attendance day'),
      child: AsyncBody<AttendanceDayDetailDto>(
        value: value,
        onRetry: () => ref.invalidate(dayDetailProvider(id)),
        builder: (d) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          AttendanceDayTile(day: d.day, showEmployee: true),
          const SizedBox(height: 12),
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.t('الحساب', 'Calculation')),
              if (d.day.shift != null) InfoRow(s.shift, d.day.shift!.name),
              InfoRow(s.t('الدوام المجدول', 'Scheduled'), '${f.time(d.day.scheduledStart)} – ${f.time(d.day.scheduledEnd)}'),
              InfoRow(s.t('الدخول الأصلي', 'Original check-in'), f.time(d.originalCheckIn)),
              InfoRow(s.t('الخروج الأصلي', 'Original check-out'), f.time(d.originalCheckOut)),
              InfoRow(s.lateLabel, '${d.day.lateMinutes} ${s.minutes}'),
              InfoRow(s.worked, '${d.day.workedMinutes} ${s.minutes}'),
              InfoRow(s.overtime, '${d.day.overtimeMinutes} ${s.minutes}'),
              if (d.day.hourlyLeaveMinutes != null) InfoRow(s.t('إجازة زمنية', 'Hourly leave'), '${d.day.hourlyLeaveMinutes} ${s.minutes}'),
            ]),
          ),
          const SizedBox(height: 12),
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.t('العمليات الخام', 'Raw punches')),
              if (d.rawPunches.isEmpty) Text(s.empty),
              for (final p in d.rawPunches)
                ListTile(
                  dense: true,
                  contentPadding: EdgeInsets.zero,
                  leading: Icon(Icons.fingerprint, color: context.hr.accent),
                  title: Text(f.dateTime(p.punchTime)),
                  subtitle: Text([p.source.value, if (p.deviceName != null) p.deviceName!].join(' · ')),
                ),
            ]),
          ),
          const SizedBox(height: 12),
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.t('سجل التصحيح', 'Correction history')),
              if (d.corrections.isEmpty) Text(s.empty),
              for (final c in d.corrections)
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.edit_calendar_outlined, color: HrColors.warning),
                  title: Text('${s.punch(c.punchType)}: ${f.time(c.oldValue)} → ${f.time(c.newValue)}'),
                  subtitle: Text([
                    c.reason,
                    c.requestedBy.displayName,
                    if (c.approvedBy != null) '${s.approve}: ${c.approvedBy!.displayName}',
                    f.dateTime(c.appliedAt),
                    if (c.affectsClosedMonth && c.settlementPeriod != null) '${s.t('تسوية في', 'Settled in')} ${c.settlementPeriod}',
                  ].join(' · ')),
                ),
            ]),
          ),
        ]),
      ),
    );
  }
}
