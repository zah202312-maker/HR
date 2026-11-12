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
import '../hr_evaluations/hr_evaluations_page.dart' show showHrEvaluationDialog, showHrEvaluationDetails;
import '../ops/custody_page.dart' show custodyStatusLabel, custodyTone;
import '../ops/ops_shared.dart';
import '../workflow/complaints_page.dart' show complaintLevelLabel, complaintStatusLabel, levelTone;
import 'appreciation_page.dart';
import 'contracts_page.dart' show contractStatusLabel, contractTone;
import 'warnings_page.dart' show warningLevelLabel, warningStatusLabel, warningTone;

/// One tab of the employee file's records: contracts, warnings, appreciation letters, custody or complaints.
/// Each is shown only to whoever holds its permission (decision 2026-11-10: a tab each, not one long page).
class EmployeeRecordTab {
  const EmployeeRecordTab(this.label, this.body);
  final String label;
  final Widget body;
}

/// The record tabs this session may open, in display order.
/// [withComplaints] is false on the direct manager's view of a team member, which never shows complaints.
/// [employee] enables the HR evaluations tab (HR manager only), whose «add» fixes this person (decision 2026-11-10).
List<EmployeeRecordTab> employeeRecordTabs(S s, Session session, String employeeId, {bool withComplaints = true, EmployeeRefDto? employee}) => [
      if (employee != null && session.can(HrOps.HrEvaluation_GetList))
        EmployeeRecordTab(s.t('التقييمات', 'Evaluations'), _HrEvaluationsList(employee: employee)),
      if (session.can(HrOps.Contract_List)) EmployeeRecordTab(s.t('العقود', 'Contracts'), _ContractsList(employeeId: employeeId)),
      if (session.can(HrOps.Warning_GetEmployeeFile)) EmployeeRecordTab(s.t('الإنذارات', 'Warnings'), _WarningsList(employeeId: employeeId)),
      if (session.can(HrOps.AppreciationLetter_GetEmployeeFile))
        EmployeeRecordTab(s.t('كتب الشكر', 'Appreciation letters'), _AppreciationList(employeeId: employeeId)),
      if (session.can(HrOps.Custody_GetList)) EmployeeRecordTab(s.t('العهد', 'Custody'), _CustodyList(employeeId: employeeId)),
      if (withComplaints && session.can(HrOps.Complaint_GetAssignedList)) EmployeeRecordTab(s.t('الشكاوى', 'Complaints'), _ComplaintsList(employeeId: employeeId)),
    ];

/// Loads once and lists the items, or a dash when there are none.
class _RecordList<T> extends StatefulWidget {
  const _RecordList({super.key, required this.load, required this.item, this.note});
  final Future<List<T>> Function() load;
  final Widget Function(BuildContext context, T item) item;
  final String? note;

  @override
  State<_RecordList<T>> createState() => _RecordListState<T>();
}

class _RecordListState<T> extends State<_RecordList<T>> {
  late Future<List<T>> _future = widget.load();

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<T>>(
      future: _future,
      builder: (context, snap) {
        if (snap.hasError) return ErrorState(error: snap.error!, onRetry: () => setState(() => _future = widget.load()));
        if (!snap.hasData) return const Align(alignment: Alignment.topCenter, child: LinearProgressIndicator());
        return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          if (widget.note != null)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Text(widget.note!, style: Theme.of(context).textTheme.bodySmall?.copyWith(color: context.hr.textSecondary)),
            ),
          if (snap.data!.isEmpty)
            EmptyState(message: S.of(context).t('لا يوجد شيء هنا', 'Nothing here'))
          else
            for (final x in snap.data!) widget.item(context, x),
        ]);
      },
    );
  }
}

/// This employee's HR evaluations, newest first, with «إضافة تقييم» for this person. Never shown to the employee.
class _HrEvaluationsList extends ConsumerStatefulWidget {
  const _HrEvaluationsList({required this.employee});
  final EmployeeRefDto employee;

  @override
  ConsumerState<_HrEvaluationsList> createState() => _HrEvaluationsListState();
}

class _HrEvaluationsListState extends ConsumerState<_HrEvaluationsList> {
  int _version = 0;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider);
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      if (session.can(HrOps.HrEvaluation_Create))
        Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Align(
            alignment: AlignmentDirectional.centerStart,
            child: FilledButton.icon(
              onPressed: () async {
                if (await showHrEvaluationDialog(context, ref, employee: widget.employee)) setState(() => _version++);
              },
              icon: const Icon(Icons.add),
              label: Text(s.t('إضافة تقييم', 'Add evaluation')),
            ),
          ),
        ),
      Expanded(
        child: _RecordList<HrEvaluationDto>(
          key: ValueKey(_version),
          note: s.t('تقييمات الموارد البشرية لهذا الموظف. لا يراها الموظف.', 'HR evaluations of this employee. The employee does not see them.'),
          load: () => api.hrEvaluations.HrEvaluation_GetList(employeeId: widget.employee.id, maxResultCount: 200).then((r) => r.items),
          item: (context, e) => OpsTile(
            leading: Icons.grading_outlined,
            title: '${s.t('المعدل', 'Average')} ${e.averageScore % 1 == 0 ? e.averageScore.toInt() : e.averageScore.toStringAsFixed(2)}/10',
            subtitle: [
              f.dateTime(e.evaluatedAt),
              e.evaluatedBy.displayName,
              for (final x in e.scores) '${x.criterionName} ${x.score}',
            ].join(' · '),
            trailing: e.reportId == null
                ? StatusPill(s.t('لم يُرسل', 'Not sent'), tone: Tone.warning)
                : StatusPill(s.t('أُرسل للتنفيذي', 'Sent'), tone: Tone.success),
            onTap: () => showHrEvaluationDetails(context, e),
          ),
        ),
      ),
    ]);
  }
}

class _ContractsList extends ConsumerWidget {
  const _ContractsList({required this.employeeId});
  final String employeeId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider);
    return _RecordList<ContractDto>(
      load: () => api.contracts.Contract_List(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
      item: (context, c) => OpsTile(
        leading: Icons.description_outlined,
        title: c.contractType,
        subtitle: '${f.date(c.startDate)} → ${f.date(c.endDate)}',
        trailing: StatusPill(contractStatusLabel(s, c.status), tone: contractTone(c.status)),
        onTap: () => context.go('/contracts/${c.id}'),
      ),
    );
  }
}

class _WarningsList extends ConsumerWidget {
  const _WarningsList({required this.employeeId});
  final String employeeId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider);
    return _RecordList<WarningFileEntryDto>(
      load: () => api.warnings.Warning_GetEmployeeFile(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
      item: (context, w) => OpsTile(
        leading: Icons.report_outlined,
        title: '${warningLevelLabel(s, w.level)} · ${f.date(w.incidentDate)}',
        subtitle: [w.decisionNote, if (w.employeeResponse != null) '${s.t('الرد', 'Response')}: ${w.employeeResponse!.content}'].join(' · '),
        trailing: StatusPill(warningStatusLabel(s, w.status), tone: warningTone(w.status)),
      ),
    );
  }
}

class _AppreciationList extends ConsumerWidget {
  const _AppreciationList({required this.employeeId});
  final String employeeId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final api = ref.watch(hrApiProvider);
    return _RecordList<AppreciationLetterDto>(
      load: () => api.appreciationLetters.AppreciationLetter_GetEmployeeFile(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
      item: (context, l) => AppreciationTile(letter: l),
    );
  }
}

class _CustodyList extends ConsumerWidget {
  const _CustodyList({required this.employeeId});
  final String employeeId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider);
    return _RecordList<CustodyDto>(
      load: () => api.custody.Custody_GetList(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
      item: (context, c) => OpsTile(
        leading: Icons.inventory_2_outlined,
        title: '${qty(c.quantity)} × ${c.item.name}',
        subtitle: f.date(c.assignedAt),
        trailing: StatusPill(custodyStatusLabel(s, c.status), tone: custodyTone(c.status)),
      ),
    );
  }
}

/// Complaints this employee filed, limited by the server to the ones the viewer may handle (ComplaintAccessPolicy).
/// Opening one is logged in the complaint's access log like any other read.
class _ComplaintsList extends ConsumerWidget {
  const _ComplaintsList({required this.employeeId});
  final String employeeId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).complaints;
    return _RecordList<ComplaintHandlerSummaryDto>(
      note: s.t('تظهر هنا الشكاوى التي قدّمها الموظف ويسمح لك بها نطاق صلاحيتك فقط، وكل فتح لشكوى يُسجَّل.',
          'Only the complaints this employee filed that your access allows appear here, and each one you open is logged.'),
      load: () => api
          .Complaint_GetAssignedList(complainantEmployeeId: employeeId, maxResultCount: 50, sorting: 'submittedAt desc')
          .then((r) => r.items),
      item: (context, c) => HrCard(
        onTap: () => context.go('/complaints/${c.id}'),
        child: Row(children: [
          Icon(Icons.gavel_outlined, color: context.hr.accent),
          const SizedBox(width: 12),
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(c.referenceNumber, style: const TextStyle(fontWeight: FontWeight.w600)),
              Text(f.dateTime(c.submittedAt), style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
            ]),
          ),
          if (c.isLate) ...[StatusPill(s.t('متأخرة', 'Late'), tone: Tone.danger), const SizedBox(width: 6)],
          StatusPill(complaintLevelLabel(s, c.level), tone: levelTone(c.level)),
          const SizedBox(width: 6),
          StatusPill(complaintStatusLabel(s, c.status), tone: c.status == ComplaintStatus.closed ? Tone.neutral : Tone.warning),
        ]),
      ),
    );
  }
}
