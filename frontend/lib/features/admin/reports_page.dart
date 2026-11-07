import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/download.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/pickers.dart';
import '../org/org_providers.dart' show searchDepartments;
import '../time/time_shared.dart' show pickEmployee;

typedef ReportPage = ({List<List<String>> rows, int total});

class ReportFilters {
  ReportFilters({required this.from, required this.to, this.employee, this.department});
  DateTime from;
  DateTime to;
  EmployeeRefDto? employee;
  DepartmentRefDto? department;
}

/// A tabular report: its contract operation, columns and filters.
class ReportDef {
  const ReportDef({
    required this.code,
    required this.operation,
    required this.title,
    required this.columns,
    required this.fetch,
    this.needsPeriod = true,
    this.teamScope = false,
    this.departmentFilter = false,
    this.endPeriod = false,
  });
  final ReportCode code;
  final String operation;
  final String Function(S s) title;
  final List<String> Function(S s) columns;
  final Future<ReportPage> Function(HrApi api, Fmt f, ReportFilters r, int skip, int max) fetch;
  final bool needsPeriod;
  final bool teamScope;
  final bool departmentFilter;

  /// The period filters the contract end date (endFrom/endTo) instead of from/to.
  final bool endPeriod;
}

String s0(String ar, String en, Fmt f) => f.isArabic ? ar : en;

String _n(num? v) => v == null ? '—' : (v % 1 == 0 ? v.toInt().toString() : v.toStringAsFixed(2));

final reportDefs = <ReportDef>[
  ReportDef(
    code: ReportCode.teamAttendance,
    operation: HrOps.Report_GetTeamAttendance,
    teamScope: true,
    title: (s) => s.t('حضور الفريق', 'Team attendance'),
    columns: (s) => [s.employee, s.date, s.checkIn, s.checkOut, s.lateLabel, s.status],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetTeamAttendance(from: r.from, to: r.to, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (rows: [for (final i in x.items) [i.employee.fullName, f.date(i.date), f.time(i.checkIn), f.time(i.checkOut), '${i.lateMinutes}', i.statusName]], total: x.totalCount);
    },
  ),
  ReportDef(
    code: ReportCode.teamLatenessAbsence,
    operation: HrOps.Report_GetTeamLatenessAbsence,
    teamScope: true,
    title: (s) => s.t('تأخر وغياب الفريق', 'Team lateness and absence'),
    columns: (s) => [s.employee, s.t('مرات التأخر', 'Late count'), s.t('دقائق التأخر', 'Late minutes'), s.t('أيام الغياب', 'Absent days')],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetTeamLatenessAbsence(from: r.from, to: r.to, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (rows: [for (final i in x.items) [i.employee.fullName, '${i.lateCount}', '${i.lateMinutes}', '${i.absentDays}']], total: x.totalCount);
    },
  ),
  ReportDef(
    code: ReportCode.teamLeaves,
    operation: HrOps.Report_GetTeamLeaves,
    teamScope: true,
    title: (s) => s.t('إجازات الفريق', 'Team leaves'),
    columns: (s) => [s.employee, s.leaveKind, s.from, s.to, s.days, s.hours, s.status],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetTeamLeaves(from: r.from, to: r.to, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (rows: [for (final i in x.items) [i.employee.fullName, i.leaveTypeName, f.dateTime(i.from), f.dateTime(i.to), _n(i.days), _n(i.hours), i.statusName]], total: x.totalCount);
    },
  ),
  ReportDef(
    code: ReportCode.teamPending,
    operation: HrOps.Report_GetTeamPending,
    teamScope: true,
    needsPeriod: false,
    title: (s) => s.t('طلبات الفريق المعلقة', 'Team pending requests'),
    columns: (s) => [s.employee, s.requestType, s.status, s.date],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetTeamPending(employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (rows: [for (final i in x.items) [i.employee.fullName, i.requestTypeName, i.statusName, f.dateTime(i.creationTime)]], total: x.totalCount);
    },
  ),
  ReportDef(
    code: ReportCode.teamCustody,
    operation: HrOps.Report_GetTeamCustody,
    teamScope: true,
    needsPeriod: false,
    title: (s) => s.t('عهد الفريق', 'Team custody'),
    columns: (s) => [s.item, s.t('النوع', 'Kind'), s.employee, s.t('التسليم', 'Assigned'), s.t('الاستلام', 'Acknowledged'), s.status],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetTeamCustody(employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (
        rows: [
          for (final i in x.items)
            [i.itemName, i.kind == ReportCustodyKind.shared ? s0('مشتركة', 'Shared', f) : s0('شخصية', 'Personal', f), i.employee?.fullName ?? '—', f.date(i.assignedAt), f.date(i.acknowledgedAt), i.statusName]
        ],
        total: x.totalCount
      );
    },
  ),
  ReportDef(
    code: ReportCode.teamEvaluations,
    operation: HrOps.Report_GetTeamEvaluations,
    teamScope: true,
    needsPeriod: false,
    title: (s) => s.t('تقييمات الفريق', 'Team evaluations'),
    columns: (s) => [s.employee, s.t('الشهر', 'Month'), s.status, s.t('الدرجات', 'Grades')],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetTeamEvaluations(employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (
        rows: [
          for (final i in x.items)
            [i.employee.fullName, '${i.year}-${i.month.toString().padLeft(2, '0')}', switch (i.status) { EvaluationStatus.draft => s0('مسودة', 'Draft', f), EvaluationStatus.acknowledged => s0('اطّلع عليه', 'Acknowledged', f), _ => s0('مرفوع', 'Submitted', f) }, i.scores.map((g) => '${g.criterionName}: ${g.gradeName}').join('، ')]
        ],
        total: x.totalCount
      );
    },
  ),
  ReportDef(
    code: ReportCode.evaluationConsistency,
    operation: HrOps.Report_GetEvaluationConsistency,
    needsPeriod: false,
    departmentFilter: true,
    title: (s) => s.t('اتساق التقييمات', 'Evaluation consistency'),
    columns: (s) => [s.manager, s.department, s.t('المرفوع', 'Submitted'), s.t('نسبة الإنجاز', 'Completion'), s.t('درجات مختلفة', 'Distinct grades'), s.t('درجة واحدة للجميع', 'Same grade for all')],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetEvaluationConsistency(departmentId: r.department?.id, skipCount: skip, maxResultCount: max);
      return (
        rows: [
          for (final i in x.items)
            [i.manager.displayName, i.department?.name ?? '—', '${i.submittedCount}/${i.expectedCount}', '${(i.completionRatio * 100).round()}%', '${i.distinctGradesUsed}', i.sameGradeForAll ? '✓' : '—']
        ],
        total: x.totalCount
      );
    },
  ),
  ReportDef(
    code: ReportCode.hrAttendance,
    operation: HrOps.Report_GetHrAttendance,
    departmentFilter: true,
    title: (s) => s.t('حضور الشركة', 'Company attendance'),
    columns: (s) => [s.employee, s.t('أيام الدوام', 'Scheduled'), s.dayStatus(AttendanceDayStatus.present), s.dayStatus(AttendanceDayStatus.absent),
          s.t('مرات التأخر', 'Late count'), s.t('دقائق التأخر', 'Late minutes'), s.dayStatus(AttendanceDayStatus.onLeave)],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetHrAttendance(
          from: r.from, to: r.to, departmentId: r.department?.id, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (
        rows: [
          for (final i in x.items)
            [i.employee.fullName, '${i.scheduledWorkingDays}', '${i.presentDays}', '${i.absentDays}', '${i.lateCount}', '${i.lateMinutes}', _n(i.leaveDays)]
        ],
        total: x.totalCount
      );
    },
  ),
  ReportDef(
    code: ReportCode.hrLeaveBalances,
    operation: HrOps.Report_GetHrLeaveBalances,
    departmentFilter: true,
    needsPeriod: false,
    title: (s) => s.t('أرصدة الإجازات', 'Leave balances'),
    columns: (s) => [s.employee, s.t('بداية السنة', 'Year start'), s.entitlement, s.carried, s.used, s.held, s.remaining, s.t('ساعات زمنية', 'Hourly used')],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetHrLeaveBalances(departmentId: r.department?.id, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (
        rows: [
          for (final i in x.items)
            [i.employee.fullName, f.date(i.leaveYearStart), _n(i.entitlementDays), _n(i.carriedInDays), _n(i.usedDays), _n(i.heldDays), _n(i.remainingDays), _n(i.usedHours)]
        ],
        total: x.totalCount
      );
    },
  ),
  ReportDef(
    code: ReportCode.hrRequests,
    operation: HrOps.Report_GetHrRequests,
    departmentFilter: true,
    title: (s) => s.t('طلبات الشركة', 'Company requests'),
    columns: (s) => [s.employee, s.requestType, s.status, s.date, s.t('أُغلق', 'Closed')],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetHrRequests(
          from: r.from, to: r.to, departmentId: r.department?.id, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (rows: [for (final i in x.items) [i.employee.fullName, i.requestTypeName, i.statusName, f.dateTime(i.creationTime), f.dateTime(i.closedAt)]], total: x.totalCount);
    },
  ),
  ReportDef(
    code: ReportCode.hrContracts,
    operation: HrOps.Report_GetHrContracts,
    departmentFilter: true,
    endPeriod: true,
    title: (s) => s.t('العقود (حسب تاريخ الانتهاء)', 'Contracts (by end date)'),
    columns: (s) => [s.employee, s.t('النوع', 'Type'), s.startDate, s.endDate, s.status],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetHrContracts(
          endFrom: r.from, endTo: r.to, departmentId: r.department?.id, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (rows: [for (final i in x.items) [i.employee.fullName, i.typeName, f.date(i.startDate), f.date(i.endDate), i.statusName]], total: x.totalCount);
    },
  ),
  ReportDef(
    code: ReportCode.hrWarnings,
    operation: HrOps.Report_GetHrWarnings,
    departmentFilter: true,
    title: (s) => s.t('الإنذارات', 'Warnings'),
    columns: (s) => [s.employee, s.t('المستوى', 'Level'), s.t('تاريخ الواقعة', 'Incident date'), s.status],
    fetch: (api, f, r, skip, max) async {
      final x = await api.reports.Report_GetHrWarnings(
          from: r.from, to: r.to, departmentId: r.department?.id, employeeId: r.employee?.id, skipCount: skip, maxResultCount: max);
      return (rows: [for (final i in x.items) [i.employee.fullName, i.levelName, f.date(i.incidentDate), i.statusName]], total: x.totalCount);
    },
  ),
];

/// Reports hub: one card per report the user may read (x-permission of its operation).
class ReportsPage extends ConsumerWidget {
  const ReportsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final defs = reportDefs.where((d) => session.can(d.operation)).toList();
    final review = session.can(HrOps.Report_GetReviewDashboard);
    return HrPage(
      title: s.t('التقارير', 'Reports'),
      child: ListView(padding: const EdgeInsets.only(bottom: 24), children: [
        Wrap(spacing: 12, runSpacing: 12, children: [
          if (review)
            _ReportCard(title: s.t('لوحة المراجع', 'Reviewer dashboard'), icon: Icons.insights, onTap: () => context.go('/reports/review')),
          for (final d in defs)
            _ReportCard(title: d.title(s), icon: Icons.table_chart_outlined, onTap: () => context.go('/reports/${d.code.value}')),
        ]),
        if (defs.isEmpty && !review) EmptyState(message: s.forbidden),
      ]),
    );
  }
}

class _ReportCard extends StatelessWidget {
  const _ReportCard({required this.title, required this.icon, required this.onTap});
  final String title;
  final IconData icon;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => SizedBox(
        width: 260,
        child: HrCard(
          onTap: onTap,
          child: Row(children: [
            Icon(icon, color: context.hr.accent),
            const SizedBox(width: 12),
            Expanded(child: Text(title, style: const TextStyle(fontWeight: FontWeight.w600))),
          ]),
        ),
      );
}

/// One tabular report with its filters, paging and export (Report_Export, PDF or Excel).
class ReportViewPage extends ConsumerStatefulWidget {
  const ReportViewPage({super.key, required this.code});
  final String code;

  @override
  ConsumerState<ReportViewPage> createState() => _ReportViewPageState();
}

class _ReportViewPageState extends ConsumerState<ReportViewPage> {
  late final ReportDef? _def = reportDefs.where((d) => d.code.value == widget.code).firstOrNull;
  final _filters = ReportFilters(
    from: DateTime(DateTime.now().year, DateTime.now().month, 1),
    to: DateTime.now(),
  );
  int _skip = 0;
  static const _page = 50;
  Future<ReportPage>? _future;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_future == null && _def != null) _future = _load();
  }

  Future<ReportPage> _load() => _def!.fetch(ref.read(hrApiProvider), Fmt(context), _filters, _skip, _page);

  void _run() {
    if (_def == null) return;
    setState(() => _future = _load());
  }

  Future<void> _export(ReportExportFormat format) async {
    final d = _def!;
    await runAction(context, () async {
      final file = await ref.read(hrApiProvider).reports.Report_Export(
            code: d.code,
            format: format,
            from: d.needsPeriod && !d.endPeriod ? _filters.from : null,
            to: d.needsPeriod && !d.endPeriod ? _filters.to : null,
            endFrom: d.endPeriod ? _filters.from : null,
            endTo: d.endPeriod ? _filters.to : null,
            employeeId: _filters.employee?.id,
            departmentId: _filters.department?.id,
          );
      await saveBinary(file, fallbackName: '${d.code.value}.${format == ReportExportFormat.pdf ? 'pdf' : 'xlsx'}');
    });
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final d = _def;
    if (d == null) return HrPage(title: s.t('التقارير', 'Reports'), child: EmptyState(message: s.notFound));
    final canExport = ref.watch(sessionProvider).can(HrOps.Report_Export);
    return HrPage(
      title: d.title(s),
      actions: [
        if (canExport) ...[
          OutlinedButton.icon(onPressed: () => _export(ReportExportFormat.pdf), icon: const Icon(Icons.picture_as_pdf_outlined), label: const Text('PDF')),
          const SizedBox(width: 8),
          OutlinedButton.icon(onPressed: () => _export(ReportExportFormat.excel), icon: const Icon(Icons.grid_on), label: const Text('Excel')),
        ],
      ],
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Wrap(spacing: 12, runSpacing: 12, crossAxisAlignment: WrapCrossAlignment.center, children: [
          if (d.needsPeriod) ...[
            SizedBox(
              width: 180,
              child: PickerField(label: s.from, value: f.date(_filters.from), icon: Icons.event, onTap: () async {
                final x = await pickDate(context, initial: _filters.from);
                if (x != null) setState(() => _filters.from = x);
              }),
            ),
            SizedBox(
              width: 180,
              child: PickerField(label: s.to, value: f.date(_filters.to), icon: Icons.event, onTap: () async {
                final x = await pickDate(context, initial: _filters.to);
                if (x != null) setState(() => _filters.to = x);
              }),
            ),
          ],
          SizedBox(
            width: 240,
            child: PickerField(
              label: s.employee,
              value: _filters.employee?.fullName,
              onClear: () => setState(() => _filters.employee = null),
              onTap: () async {
                final e = await pickEmployee(context, ref, teamOnly: d.teamScope);
                if (e != null) setState(() => _filters.employee = e);
              },
            ),
          ),
          if (d.departmentFilter)
            SizedBox(
              width: 220,
              child: PickerField(
                label: s.department,
                value: _filters.department?.name,
                onClear: () => setState(() => _filters.department = null),
                onTap: () async {
                  final x = await showSearchPicker<DepartmentRefDto>(context, title: s.department, search: (q) => searchDepartments(ref, q), label: (x) => x.name);
                  if (x != null) setState(() => _filters.department = x);
                },
              ),
            ),
          FilledButton.icon(
            onPressed: () {
              _skip = 0;
              _run();
            },
            icon: const Icon(Icons.search),
            label: Text(s.t('عرض', 'Show')),
          ),
        ]),
        const SizedBox(height: 12),
        Expanded(
          child: FutureBuilder<ReportPage>(
            future: _future,
            builder: (context, snap) {
              if (snap.hasError) return ErrorState(error: snap.error!, onRetry: _run);
              if (!snap.hasData) return const Center(child: CircularProgressIndicator());
              final data = snap.data!;
              if (data.rows.isEmpty) return const EmptyState();
              final cols = d.columns(s);
              return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                Expanded(
                  child: HrCard(
                    padding: EdgeInsets.zero,
                    child: SingleChildScrollView(
                      child: SingleChildScrollView(
                        scrollDirection: Axis.horizontal,
                        child: DataTable(
                          columns: [for (final c in cols) DataColumn(label: Text(c, style: const TextStyle(fontWeight: FontWeight.w700)))],
                          rows: [for (final r in data.rows) DataRow(cells: [for (final c in r) DataCell(Text(c))])],
                        ),
                      ),
                    ),
                  ),
                ),
                Row(children: [
                  Text('${_skip + 1}–${_skip + data.rows.length} / ${data.total}'),
                  const Spacer(),
                  IconButton(
                    onPressed: _skip == 0 ? null : () {
                      _skip = _skip - _page < 0 ? 0 : _skip - _page;
                      _run();
                    },
                    icon: const Icon(Icons.chevron_left),
                  ),
                  IconButton(
                    onPressed: _skip + data.rows.length >= data.total ? null : () {
                      _skip += _page;
                      _run();
                    },
                    icon: const Icon(Icons.chevron_right),
                  ),
                ]),
              ]);
            },
          ),
        ),
      ]),
    );
  }
}

/// Reviewer dashboard: complaint counts only (no names or breakdown), and monthly completion.
class ReviewDashboardPage extends ConsumerStatefulWidget {
  const ReviewDashboardPage({super.key});

  @override
  ConsumerState<ReviewDashboardPage> createState() => _ReviewDashboardPageState();
}

class _ReviewDashboardPageState extends ConsumerState<ReviewDashboardPage> {
  DateTime _month = DateTime(DateTime.now().year, DateTime.now().month);

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).reports;
    return HrPage(
      title: s.t('لوحة المراجع', 'Reviewer dashboard'),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Row(children: [
          IconButton(onPressed: () => setState(() => _month = DateTime(_month.year, _month.month - 1)), icon: const Icon(Icons.chevron_left)),
          Text('${_month.year}-${_month.month.toString().padLeft(2, '0')}', style: Theme.of(context).textTheme.titleMedium),
          IconButton(onPressed: () => setState(() => _month = DateTime(_month.year, _month.month + 1)), icon: const Icon(Icons.chevron_right)),
        ]),
        Expanded(
          child: FutureBuilder<ReviewDashboardDto>(
            key: ValueKey(_month),
            future: api.Report_GetReviewDashboard(year: _month.year, month: _month.month),
            builder: (context, snap) {
              if (snap.hasError) return ErrorState(error: snap.error!);
              if (!snap.hasData) return const Center(child: CircularProgressIndicator());
              final d = snap.data!;
              final c = d.complaintCounts;
              return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
                Wrap(spacing: 12, runSpacing: 12, children: [
                  SizedBox(width: 220, child: KpiTile(label: s.t('شكاوى سرية مقدمة', 'Confidential submitted'), value: '${c.confidentialSubmittedCount}', icon: Icons.shield_outlined)),
                  SizedBox(width: 220, child: KpiTile(label: s.t('سرية للغاية مقدمة', 'Highly confidential submitted'), value: '${c.highlyConfidentialSubmittedCount}', icon: Icons.shield)),
                  SizedBox(
                    width: 220,
                    child: KpiTile(
                      label: s.t('شكاوى متأخرة الآن', 'Late complaints now'),
                      value: '${c.lateNormalCount + c.lateConfidentialCount + c.lateHighlyConfidentialCount}',
                      icon: Icons.timer_off_outlined,
                      caption: '${c.lateNormalCount} / ${c.lateConfidentialCount} / ${c.lateHighlyConfidentialCount} · ${f.dateTime(c.asOf)}',
                    ),
                  ),
                  SizedBox(width: 220, child: KpiTile(label: s.t('أقسام مكتملة', 'Departments completed'), value: '${d.completedDepartmentsCount}', icon: Icons.task_alt)),
                  SizedBox(width: 220, child: KpiTile(label: s.t('أقسام متأخرة', 'Departments late'), value: '${d.lateDepartmentsCount}', icon: Icons.warning_amber)),
                ]),
                const SizedBox(height: 16),
                for (final x in d.departments)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 8),
                    child: HrCard(
                      child: Row(children: [
                        Expanded(child: Text('${x.manager.displayName}${x.department == null ? '' : ' · ${x.department!.name}'}')),
                        Text('${x.completion.evaluationsSubmitted}/${x.completion.evaluationsExpected}'),
                        const SizedBox(width: 12),
                        StatusPill(
                          x.completion.isCompleted ? s.t('مكتمل', 'Completed') : (x.completion.isLate ? s.t('متأخر', 'Late') : s.t('جارٍ', 'In progress')),
                          tone: x.completion.isCompleted ? Tone.success : (x.completion.isLate ? Tone.danger : Tone.warning),
                        ),
                      ]),
                    ),
                  ),
              ]);
            },
          ),
        ),
      ]),
    );
  }
}
