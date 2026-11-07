import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';
import 'evaluations_page.dart' show evalStatusLabel, monthLabel;

Widget completionPill(S s, DepartmentCompletionDto c) => StatusPill(
      c.isCompleted ? s.t('منجز', 'Completed') : (c.isLate ? s.t('متأخر', 'Late') : s.t('جارٍ', 'In progress')),
      tone: c.isCompleted ? Tone.success : (c.isLate ? Tone.danger : Tone.warning),
    );

/// S-DRP: monthly department reports — the manager's own, all for HR.
class DepartmentReportsPage extends ConsumerStatefulWidget {
  const DepartmentReportsPage({super.key});

  @override
  ConsumerState<DepartmentReportsPage> createState() => _DepartmentReportsPageState();
}

class _DepartmentReportsPageState extends ConsumerState<DepartmentReportsPage> {
  DepartmentReportStatus? _status;
  bool? _late;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).departmentReports;
    return HrPage(
      title: s.t('التقارير الشهرية للأقسام', 'Monthly department reports'),
      child: PagedList<DepartmentReportSummaryDto>(
        key: ValueKey('$_status|$_late'),
        header: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Wrap(spacing: 12, runSpacing: 8, children: [
            enumFilter<DepartmentReportStatus>(context,
                label: s.status,
                value: _status,
                values: DepartmentReportStatus.values,
                text: (v) => v == DepartmentReportStatus.draft ? s.t('مسودة', 'Draft') : s.t('مرفوع', 'Submitted'),
                onChanged: (v) => setState(() => _status = v)),
            FilterChip(label: Text(s.t('المتأخرة', 'Late')), selected: _late == true, onSelected: (v) => setState(() => _late = v ? true : null)),
          ]),
        ),
        fetch: (skip, max) async {
          final r = await api.DepartmentReport_GetList(skipCount: skip, maxResultCount: max, status: _status, isLate: _late);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, r) => OpsTile(
          leading: Icons.summarize_outlined,
          title: '${monthLabel(r.year, r.month)} · ${r.manager.displayName}',
          subtitle: [
            if (r.department != null) r.department!.name,
            '${s.t('التقييمات', 'Evaluations')} ${r.completion.evaluationsSubmitted}/${r.completion.evaluationsExpected}',
            if (r.submittedAt != null) f.dateTime(r.submittedAt),
          ].join(' · '),
          trailing: completionPill(s, r.completion),
          onTap: () => context.go('/department-reports/${r.id}'),
        ),
      ),
    );
  }
}

/// One monthly report: frozen snapshot, manager notes, submit.
class DepartmentReportPage extends ConsumerStatefulWidget {
  const DepartmentReportPage({super.key, required this.id});
  final String id;

  @override
  ConsumerState<DepartmentReportPage> createState() => _DepartmentReportPageState();
}

class _DepartmentReportPageState extends ConsumerState<DepartmentReportPage> {
  DepartmentReportDto? _r;
  Object? _error;
  final _notes = TextEditingController();
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _error = null);
    try {
      _bind(await ref.read(hrApiProvider).departmentReports.DepartmentReport_Get(id: widget.id));
    } catch (e) {
      if (mounted) setState(() => _error = e);
    }
  }

  void _bind(DepartmentReportDto r) {
    if (!mounted) return;
    setState(() {
      _r = r;
      _notes.text = r.notes ?? '';
    });
  }

  Future<void> _save({bool quiet = false}) async {
    final s = S.of(context);
    setState(() => _busy = true);
    await runAction(context, () async {
      _bind(await ref.read(hrApiProvider).departmentReports.DepartmentReport_UpdateNotes(
          id: _r!.id, body: DepartmentReportNotesPatchDto(notes: _notes.text.trim().isEmpty ? null : _notes.text.trim(), concurrencyStamp: _r!.concurrencyStamp)));
    }, done: quiet ? null : s.saved);
    if (mounted) setState(() => _busy = false);
  }

  Future<void> _submit() async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('رفع التقرير', 'Submit report'), message: s.t('لا تتعدل الملاحظات بعد الرفع.', 'Notes cannot change after submitting.'))) return;
    if (!mounted) return;
    if ((_r!.notes ?? '') != _notes.text.trim()) await _save(quiet: true);
    if (!mounted) return;
    setState(() => _busy = true);
    await runAction(context, () async {
      _bind(await ref.read(hrApiProvider).departmentReports.DepartmentReport_Submit(id: _r!.id, body: DepartmentReportSubmitDto(concurrencyStamp: _r!.concurrencyStamp)));
    }, done: s.submitted);
    if (mounted) setState(() => _busy = false);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final r = _r;
    final title = s.t('التقرير الشهري', 'Monthly report');
    if (_error != null) return HrPage(title: title, child: ErrorState(error: _error!, onRetry: _load));
    if (r == null) return HrPage(title: title, child: const Center(child: CircularProgressIndicator()));
    final snap = r.snapshot;
    final canEdit = r.status == DepartmentReportStatus.draft && ref.watch(sessionProvider).can(HrOps.DepartmentReport_UpdateNotes);
    return HrPage(
      title: '$title · ${monthLabel(r.year, r.month)}',
      subtitle: [r.manager.displayName, if (r.department != null) r.department!.name].join(' · '),
      actions: [completionPill(s, r.completion)],
      child: ListView(padding: const EdgeInsets.only(bottom: 24), children: [
        Text('${f.date(snap.periodFrom)} → ${f.date(snap.periodTo)} · ${s.t('ثُبّتت', 'Frozen')} ${f.dateTime(snap.frozenAt)}',
            style: TextStyle(color: context.hr.textSecondary)),
        const SizedBox(height: 12),
        Wrap(spacing: 12, runSpacing: 12, children: [
          SizedBox(
            width: 260,
            child: KpiTile(
              label: s.t('التقييمات', 'Evaluations'),
              value: '${snap.evaluations.submittedCount}/${snap.evaluations.expectedCount}',
              icon: Icons.rate_review_outlined,
              caption: '${s.t('اطلاع', 'Acknowledged')} ${snap.evaluations.acknowledgedCount} · ${s.t('اعتراضات معلقة', 'Pending objections')} ${snap.evaluations.pendingObjectionsCount}',
            ),
          ),
          SizedBox(
            width: 260,
            child: KpiTile(
              label: s.t('العهدة', 'Custody'),
              value: '${snap.custody.personalItemsCount + snap.custody.sharedItemsCount}',
              icon: Icons.inventory_2_outlined,
              caption: '${s.t('غير مؤكدة', 'Unacknowledged')} ${snap.custody.unacknowledgedItemsCount} · ${s.t('آخر جرد', 'Last count')} ${f.date(snap.custody.lastCountDate)}',
            ),
          ),
          SizedBox(
            width: 260,
            child: KpiTile(
              label: s.t('التعاميم', 'Circulars'),
              value: '${snap.circulars.circularsSentCount}',
              icon: Icons.campaign_outlined,
              caption: '${s.t('تأكيد', 'Acknowledged')} ${snap.circulars.acknowledgedCount}/${snap.circulars.recipientsCount}',
            ),
          ),
        ]),
        if (snap.evaluations.gradeCounts.isNotEmpty) ...[
          const SizedBox(height: 12),
          HrCard(
            child: Wrap(spacing: 8, runSpacing: 8, children: [
              for (final g in snap.evaluations.gradeCounts) Chip(label: Text('${g.gradeName} × ${g.count}')),
            ]),
          ),
        ],
        const SizedBox(height: 12),
        HrCard(
          padding: EdgeInsets.zero,
          child: SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            child: DataTable(
              columns: [
                DataColumn(label: Text(s.employee)),
                DataColumn(label: Text(s.dayStatus(AttendanceDayStatus.present))),
                DataColumn(label: Text(s.dayStatus(AttendanceDayStatus.absent))),
                DataColumn(label: Text(s.t('تأخر', 'Late'))),
                DataColumn(label: Text(s.t('دقائق', 'Minutes'))),
                DataColumn(label: Text(s.leaves)),
                DataColumn(label: Text(s.t('طلبات معلقة', 'Pending'))),
                DataColumn(label: Text(s.t('التقييم', 'Evaluation'))),
              ],
              rows: [
                for (final e in snap.employees)
                  DataRow(cells: [
                    DataCell(Text(e.employee.fullName)),
                    DataCell(Text('${e.presentDays}')),
                    DataCell(Text('${e.absentDays}')),
                    DataCell(Text('${e.lateCount}')),
                    DataCell(Text('${e.lateMinutes}')),
                    DataCell(Text(qty(e.leaveDays))),
                    DataCell(Text('${e.pendingRequestsCount}')),
                    DataCell(Text(e.evaluationStatus == null ? '—' : evalStatusLabel(s, e.evaluationStatus!))),
                  ]),
              ],
            ),
          ),
        ),
        const SizedBox(height: 12),
        HrCard(
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            SectionTitle(s.t('ملاحظات المدير', 'Manager notes')),
            if (canEdit) ...[
              TextField(controller: _notes, enabled: !_busy, maxLines: 5, maxLength: 4000),
              Row(mainAxisAlignment: MainAxisAlignment.end, children: [
                TextButton(onPressed: _busy ? null : () => _save(), child: Text(s.save)),
                const SizedBox(width: 8),
                if (ref.watch(sessionProvider).can(HrOps.DepartmentReport_Submit))
                  FilledButton(onPressed: _busy ? null : _submit, child: Text(s.t('رفع التقرير', 'Submit report'))),
              ]),
            ] else
              Text(r.notes ?? '—'),
            if (r.submittedAt != null)
              Padding(
                padding: const EdgeInsets.only(top: 8),
                child: Text('${s.t('رُفع', 'Submitted')} ${f.dateTime(r.submittedAt)}', style: TextStyle(color: context.hr.textSecondary)),
              ),
          ]),
        ),
      ]),
    );
  }
}
