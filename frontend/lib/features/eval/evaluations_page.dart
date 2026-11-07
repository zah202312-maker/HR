import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../api/runtime/api_client.dart' show ApiException;
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';

String evalStatusLabel(S s, EvaluationStatus st) => switch (st) {
      EvaluationStatus.draft => s.t('مسودة', 'Draft'),
      EvaluationStatus.submitted => s.t('مرفوع', 'Submitted'),
      EvaluationStatus.acknowledged => s.t('اطّلع عليه الموظف', 'Acknowledged'),
      _ => '—',
    };

Tone evalTone(EvaluationStatus st) => switch (st) {
      EvaluationStatus.draft => Tone.warning,
      EvaluationStatus.submitted => Tone.info,
      _ => Tone.success,
    };

String objectionLabel(S s, EvaluationObjectionStatus st) => switch (st) {
      EvaluationObjectionStatus.pending => s.t('اعتراض قيد البت', 'Objection pending'),
      EvaluationObjectionStatus.accepted => s.t('اعتراض مقبول', 'Objection accepted'),
      EvaluationObjectionStatus.rejected => s.t('اعتراض مرفوض', 'Objection rejected'),
      _ => '',
    };

String monthLabel(int year, int month) => '$year-${month.toString().padLeft(2, '0')}';

final currentCycleProvider = FutureProvider.autoDispose<EvaluationCycleDto?>((ref) async {
  try {
    return await ref.watch(hrApiProvider).evaluations.EvaluationCycle_GetCurrent();
  } on ApiException catch (e) {
    if (e.statusCode == 404) return null; // no cycle created yet
    rethrow;
  }
});

/// S-EVL: monthly evaluations — mine (employee), my team (department manager), company (HR).
class EvaluationsPage extends ConsumerStatefulWidget {
  const EvaluationsPage({super.key});

  @override
  ConsumerState<EvaluationsPage> createState() => _EvaluationsPageState();
}

class _EvaluationsPageState extends ConsumerState<EvaluationsPage> {
  EvaluationStatus? _teamStatus;
  EvaluationObjectionStatus? _objection;
  bool? _late;

  Widget _objectionPill(S s, EvaluationObjectionStatus o) => o == EvaluationObjectionStatus.none || o == EvaluationObjectionStatus.$unknown
      ? const SizedBox.shrink()
      : Padding(
          padding: const EdgeInsetsDirectional.only(end: 6),
          child: StatusPill(objectionLabel(s, o), tone: o == EvaluationObjectionStatus.pending ? Tone.warning : Tone.neutral),
        );

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).evaluations;

    Widget cycleCard() => ref.watch(currentCycleProvider).when(
          loading: () => const LinearProgressIndicator(),
          error: (e, _) => ErrorState(error: e),
          data: (c) {
            if (c == null) return EmptyState(message: s.t('لم تُنشأ دورة تقييم بعد.', 'No evaluation cycle yet.'));
            final p = c.managerProgress;
            return Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: HrCard(
                child: Wrap(spacing: 24, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                  Text('${s.t('دورة', 'Cycle')} ${monthLabel(c.year, c.month)}', style: Theme.of(context).textTheme.titleMedium),
                  StatusPill(switch (c.status) {
                    EvaluationCycleStatus.open => s.t('مفتوحة', 'Open'),
                    EvaluationCycleStatus.closed => s.t('مغلقة', 'Closed'),
                    _ => s.t('مجدولة', 'Scheduled'),
                  }, tone: c.status == EvaluationCycleStatus.open ? Tone.success : Tone.neutral),
                  Text('${s.t('المهلة', 'Due')} ${f.dateTime(c.dueAt)}'),
                  if (p != null) Text('${s.t('المرفوع', 'Submitted')} ${p.submittedCount}/${p.expectedCount}'),
                  if (p != null && p.isLate) StatusPill(s.t('متأخر', 'Late'), tone: Tone.danger),
                  if (p != null && p.isCompleted) StatusPill(s.t('مكتمل', 'Completed'), tone: Tone.success),
                  if (p?.reportId != null) TextButton(onPressed: () => context.go('/department-reports/${p!.reportId}'), child: Text(s.t('التقرير الشهري', 'Monthly report'))),
                ]),
              ),
            );
          },
        );

    final tabs = <(String, Widget)>[
      if (session.can(HrOps.Evaluation_GetTeamList))
        (
          s.t('تقييم فريقي', 'My team'),
          PagedList<EvaluationManagerSummaryDto>(
            key: ValueKey('team|$_teamStatus'),
            header: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              if (session.can(HrOps.EvaluationCycle_GetCurrent)) cycleCard(),
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Align(
                  alignment: AlignmentDirectional.centerStart,
                  child: enumFilter<EvaluationStatus>(context,
                      label: s.status, value: _teamStatus, values: EvaluationStatus.values, text: (v) => evalStatusLabel(s, v), onChanged: (v) => setState(() => _teamStatus = v)),
                ),
              ),
            ]),
            fetch: (skip, max) async {
              final r = await api.Evaluation_GetTeamList(skipCount: skip, maxResultCount: max, status: _teamStatus);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, e) => OpsTile(
              leading: Icons.rate_review_outlined,
              title: e.employee.fullName,
              subtitle: [monthLabel(e.year, e.month), if (e.submittedAt != null) f.dateTime(e.submittedAt)].join(' · '),
              trailing: Row(mainAxisSize: MainAxisSize.min, children: [_objectionPill(s, e.objectionStatus), StatusPill(evalStatusLabel(s, e.status), tone: evalTone(e.status))]),
              onTap: () => context.go('/evaluations/${e.id}'),
            ),
          )
        ),
      if (session.can(HrOps.Evaluation_GetHrList))
        (
          s.t('تقييمات الشركة', 'Company'),
          PagedList<EvaluationHrSummaryDto>(
            key: ValueKey('hr|$_objection|$_late'),
            pageSize: 50,
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Wrap(spacing: 12, runSpacing: 8, children: [
                enumFilter<EvaluationObjectionStatus>(context,
                    label: s.t('الاعتراض', 'Objection'),
                    value: _objection,
                    values: EvaluationObjectionStatus.values,
                    text: (v) => v == EvaluationObjectionStatus.none ? s.t('بلا اعتراض', 'None') : objectionLabel(s, v),
                    onChanged: (v) => setState(() => _objection = v)),
                FilterChip(label: Text(s.t('المتأخرة', 'Late')), selected: _late == true, onSelected: (v) => setState(() => _late = v ? true : null)),
              ]),
            ),
            fetch: (skip, max) async {
              final r = await api.Evaluation_GetHrList(skipCount: skip, maxResultCount: max, objectionStatus: _objection, isLate: _late);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, e) => OpsTile(
              leading: Icons.assessment_outlined,
              title: e.employee.fullName,
              subtitle: [monthLabel(e.year, e.month), e.evaluator.displayName, if (e.employee.departmentName != null) e.employee.departmentName!].join(' · '),
              trailing: Row(mainAxisSize: MainAxisSize.min, children: [
                if (e.isLate) ...[StatusPill(s.t('متأخر', 'Late'), tone: Tone.danger), const SizedBox(width: 6)],
                _objectionPill(s, e.objectionStatus),
                StatusPill(evalStatusLabel(s, e.status), tone: evalTone(e.status)),
              ]),
              onTap: e.status == EvaluationStatus.draft ? null : () => context.go('/evaluations/${e.id}/hr'),
            ),
          )
        ),
      if (session.can(HrOps.Evaluation_GetMyList))
        (
          s.t('تقييماتي', 'My evaluations'),
          PagedList<EvaluationEmployeeSummaryDto>(
            key: const ValueKey('my'),
            header: session.can(HrOps.EvaluationAnnualSummary_GetMy) ? const _AnnualSummaryCard() : null,
            fetch: (skip, max) async {
              final r = await api.Evaluation_GetMyList(skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, e) => OpsTile(
              leading: Icons.star_outline,
              title: monthLabel(e.year, e.month),
              subtitle: f.dateTime(e.submittedAt),
              trailing: Row(mainAxisSize: MainAxisSize.min, children: [_objectionPill(s, e.objectionStatus), StatusPill(evalStatusLabel(s, e.status), tone: evalTone(e.status))]),
              onTap: () => context.go('/evaluations/my/${e.id}'),
            ),
          )
        ),
    ];
    return HrPage(title: s.t('التقييمات', 'Evaluations'), child: tabbedBody(context, tabs));
  }
}

/// Annual summary: how many times each grade was given per criterion, no percentages.
class _AnnualSummaryCard extends ConsumerStatefulWidget {
  const _AnnualSummaryCard({this.employeeId});
  final String? employeeId;

  @override
  ConsumerState<_AnnualSummaryCard> createState() => _AnnualSummaryCardState();
}

class _AnnualSummaryCardState extends ConsumerState<_AnnualSummaryCard> {
  int _year = DateTime.now().year;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final api = ref.watch(hrApiProvider).evaluations;
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: HrCard(
        child: FutureBuilder<EvaluationAnnualSummaryDto>(
          key: ValueKey(_year),
          future: widget.employeeId == null
              ? api.EvaluationAnnualSummary_GetMy(year: _year)
              : api.EvaluationAnnualSummary_GetForEmployee(employeeId: widget.employeeId!, year: _year),
          builder: (context, snap) {
            final header = Row(children: [
              Expanded(child: Text(s.t('الملخص السنوي', 'Annual summary'), style: Theme.of(context).textTheme.titleMedium)),
              IconButton(onPressed: () => setState(() => _year--), icon: const Icon(Icons.chevron_left)),
              Text('$_year'),
              IconButton(onPressed: () => setState(() => _year++), icon: const Icon(Icons.chevron_right)),
            ]);
            if (snap.hasError) return Column(children: [header, ErrorState(error: snap.error!)]);
            if (!snap.hasData) return Column(children: [header, const LinearProgressIndicator()]);
            final d = snap.data!;
            return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              header,
              Text('${s.t('أشهر مقيّمة', 'Months evaluated')} ${d.evaluatedMonthsCount} · ${s.t('اطلاع', 'Acknowledged')} ${d.acknowledgedCount} · ${s.t('اعتراضات', 'Objections')} ${d.objectionsCount}',
                  style: TextStyle(color: context.hr.textSecondary)),
              const SizedBox(height: 8),
              for (final c in d.criteria)
                InfoRow(c.criterionName, c.gradeCounts.where((g) => g.count > 0).map((g) => '${g.gradeName} × ${g.count}').join('، ')),
            ]);
          },
        ),
      ),
    );
  }
}

Widget _attendanceCard(BuildContext context, EvaluationAttendanceSummaryDto a) {
  final s = S.of(context);
  final f = Fmt(context);
  return HrCard(
    child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      SectionTitle(s.t('بيانات الحضور للفترة', 'Attendance for the period'),
          trailing: Text('${f.date(a.periodFrom)} → ${f.date(a.periodTo)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12))),
      Wrap(spacing: 24, runSpacing: 8, children: [
        Text('${s.t('أيام الدوام', 'Scheduled')} ${a.scheduledWorkingDays}'),
        Text('${s.dayStatus(AttendanceDayStatus.present)} ${a.presentDays}'),
        Text('${s.dayStatus(AttendanceDayStatus.absent)} ${a.absentDays}'),
        Text('${s.t('مرات التأخر', 'Late')} ${a.lateCount} (${a.lateMinutes} ${s.minutes})'),
        Text('${s.dayStatus(AttendanceDayStatus.onLeave)} ${qty(a.leaveDays)}'),
        if (a.isFrozen) StatusPill(s.t('بيانات مثبتة', 'Frozen'), tone: Tone.neutral),
      ]),
    ]),
  );
}

Widget _responsesCard(BuildContext context, List<EvaluationResponseDto> responses, {void Function(EvaluationResponseDto r)? onDecide}) {
  final s = S.of(context);
  final f = Fmt(context);
  if (responses.isEmpty) return const SizedBox.shrink();
  return HrCard(
    child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      SectionTitle(s.t('ردود الموظف', 'Employee responses')),
      for (final r in responses)
        Padding(
          padding: const EdgeInsets.only(bottom: 10),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              StatusPill(r.type == EvaluationResponseType.objection ? s.t('اعتراض', 'Objection') : s.t('تعليق', 'Comment'),
                  tone: r.type == EvaluationResponseType.objection ? Tone.warning : Tone.info),
              const SizedBox(width: 8),
              Text(f.dateTime(r.creationTime), style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
              const Spacer(),
              if (r.type == EvaluationResponseType.objection && r.decision == null && onDecide != null)
                FilledButton.tonal(onPressed: () => onDecide(r), child: Text(s.t('البت', 'Decide'))),
            ]),
            const SizedBox(height: 4),
            Text(r.content),
            if (r.decision != null)
              Padding(
                padding: const EdgeInsets.only(top: 4),
                child: Text(
                  '${r.decision == EvaluationObjectionDecision.accepted ? s.t('قُبل', 'Accepted') : s.t('رُفض', 'Rejected')}'
                  '${r.decidedBy == null ? '' : ' · ${r.decidedBy!.displayName}'}'
                  '${r.decisionReason == null ? '' : ' · ${r.decisionReason}'}',
                  style: TextStyle(color: context.hr.textSecondary),
                ),
              ),
          ]),
        ),
    ]),
  );
}

Widget _scoresCard(BuildContext context, List<EvaluationSubmittedScoreDto> scores, String? notes) {
  final s = S.of(context);
  return HrCard(
    child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      SectionTitle(s.t('الدرجات', 'Grades')),
      for (final x in scores) InfoRow(x.criterionName, x.gradeName),
      if (notes != null && notes.isNotEmpty) ...[const Divider(), InfoRow(s.t('ملاحظات للموظف', 'Notes to employee'), notes)],
    ]),
  );
}

final myEvaluationProvider =
    FutureProvider.autoDispose.family<EvaluationEmployeeDto, String>((ref, id) => ref.watch(hrApiProvider).evaluations.Evaluation_GetMyDetail(id: id));

/// The employee's view: grades, notes, acknowledge, comment or object (window decided by the server).
class MyEvaluationPage extends ConsumerWidget {
  const MyEvaluationPage({super.key, required this.id});
  final String id;

  Future<void> _respond(BuildContext context, WidgetRef ref, EvaluationResponseType type) async {
    final s = S.of(context);
    final key = const Uuid().v4();
    final text = await askText(context,
        title: type == EvaluationResponseType.objection ? s.t('اعتراض على التقييم', 'Object to the evaluation') : s.t('تعليق', 'Comment'), label: s.description);
    if (text == null || !context.mounted) return;
    final ok = await runAction(
        context,
        () => ref.read(hrApiProvider).evaluations.Evaluation_AddResponse(
            id: id, idempotencyKey: key, body: EvaluationResponseCreateDto(type: type, content: text)),
        done: s.submitted);
    if (ok) ref.invalidate(myEvaluationProvider(id));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrPage(
      title: s.t('تقييمي', 'My evaluation'),
      child: AsyncBody<EvaluationEmployeeDto>(
        value: ref.watch(myEvaluationProvider(id)),
        onRetry: () => ref.invalidate(myEvaluationProvider(id)),
        builder: (e) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Wrap(spacing: 16, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
              Text(monthLabel(e.year, e.month), style: Theme.of(context).textTheme.titleMedium),
              Text('${s.t('المقيّم', 'Evaluator')} ${e.evaluator.displayName}'),
              Text(f.dateTime(e.submittedAt)),
              StatusPill(evalStatusLabel(s, e.status), tone: evalTone(e.status)),
              if (e.objectionStatus != EvaluationObjectionStatus.none) StatusPill(objectionLabel(s, e.objectionStatus), tone: Tone.warning),
            ]),
          ),
          const SizedBox(height: 12),
          _scoresCard(context, e.scores, e.employeeNotes),
          const SizedBox(height: 12),
          _responsesCard(context, e.responses),
          const SizedBox(height: 12),
          Wrap(spacing: 8, runSpacing: 8, children: [
            if (e.acknowledgedAt == null)
              FilledButton.icon(
                onPressed: () async {
                  if (await runAction(context, () => ref.read(hrApiProvider).evaluations.Evaluation_Acknowledge(id: id), done: s.done)) {
                    ref.invalidate(myEvaluationProvider(id));
                  }
                },
                icon: const Icon(Icons.visibility_outlined),
                label: Text(s.t('اطلعت', 'Acknowledge')),
              ),
            OutlinedButton.icon(
                onPressed: () => _respond(context, ref, EvaluationResponseType.comment), icon: const Icon(Icons.comment_outlined), label: Text(s.t('تعليق', 'Comment'))),
            if (e.objectionStatus != EvaluationObjectionStatus.pending)
              OutlinedButton.icon(
                  onPressed: () => _respond(context, ref, EvaluationResponseType.objection), icon: const Icon(Icons.gavel_outlined), label: Text(s.t('اعتراض', 'Object'))),
          ]),
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Text(s.t('الاطلاع لا يعني الموافقة، ويبقى التعليق والاعتراض متاحين.', 'Acknowledging is not agreeing; you can still comment or object.'),
                style: Theme.of(context).textTheme.labelSmall),
          ),
        ]),
      ),
    );
  }
}

/// Internal notes: visible to their author and HR only, never to the employee.
class _InternalNotesCard extends ConsumerStatefulWidget {
  const _InternalNotesCard({required this.evaluationId, required this.submitted, required this.canWrite});
  final String evaluationId;
  final bool submitted;
  final bool canWrite;

  @override
  ConsumerState<_InternalNotesCard> createState() => _InternalNotesCardState();
}

class _InternalNotesCardState extends ConsumerState<_InternalNotesCard> {
  int _version = 0;
  final _draft = TextEditingController();
  bool _draftLoaded = false;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).evaluations;
    return HrCard(
      child: FutureBuilder<EvaluationInternalNotePagedResultDto>(
        key: ValueKey(_version),
        future: api.EvaluationInternalNote_GetList(id: widget.evaluationId, maxResultCount: 200),
        builder: (context, snap) {
          final title = SectionTitle(s.t('ملاحظات داخلية (لا يراها الموظف)', 'Internal notes (hidden from the employee)'));
          if (snap.hasError) return Column(children: [title, ErrorState(error: snap.error!)]);
          if (!snap.hasData) return Column(children: [title, const LinearProgressIndicator()]);
          final notes = snap.data!.items;
          final draft = notes.where((n) => n.isDraft).firstOrNull;
          if (!_draftLoaded) {
            _draft.text = draft?.content ?? '';
            _draftLoaded = true;
          }
          return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            title,
            for (final n in notes.where((n) => !n.isDraft))
              Padding(
                padding: const EdgeInsets.only(bottom: 8),
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text('${n.author.displayName} · ${f.dateTime(n.creationTime)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                  Text(n.content),
                ]),
              ),
            if (widget.canWrite && !widget.submitted) ...[
              TextField(
                controller: _draft,
                maxLines: 3,
                maxLength: 4000,
                decoration: InputDecoration(labelText: s.t('مسودة الملاحظة (تثبت عند الرفع)', 'Draft note (fixed on submit)')),
              ),
              Align(
                alignment: AlignmentDirectional.centerEnd,
                child: TextButton(
                  onPressed: () => runAction(
                      context,
                      () => api.EvaluationInternalNote_SaveDraft(
                          id: widget.evaluationId, body: EvaluationInternalNoteDraftDto(content: _draft.text.trim().isEmpty ? null : _draft.text.trim())),
                      done: s.saved),
                  child: Text(s.t('حفظ المسودة', 'Save draft')),
                ),
              ),
            ],
            if (widget.canWrite && widget.submitted)
              Align(
                alignment: AlignmentDirectional.centerEnd,
                child: OutlinedButton.icon(
                  onPressed: () async {
                    final key = const Uuid().v4();
                    final text = await askText(context, title: s.t('ملاحظة داخلية', 'Internal note'), label: s.note, maxLength: 4000);
                    if (text == null || !context.mounted) return;
                    if (await runAction(context,
                        () => api.EvaluationInternalNote_Add(id: widget.evaluationId, idempotencyKey: key, body: EvaluationInternalNoteCreateDto(content: text)),
                        done: s.saved)) {
                      setState(() => _version++);
                    }
                  },
                  icon: const Icon(Icons.note_add_outlined),
                  label: Text(s.t('إضافة ملاحظة', 'Add note')),
                ),
              ),
          ]);
        },
      ),
    );
  }
}

/// Manager form: verbal scale per criterion (no percentages), notes to the employee, draft and submit.
class EvaluationFormPage extends ConsumerStatefulWidget {
  const EvaluationFormPage({super.key, required this.id});
  final String id;

  @override
  ConsumerState<EvaluationFormPage> createState() => _EvaluationFormPageState();
}

class _EvaluationFormPageState extends ConsumerState<EvaluationFormPage> {
  EvaluationManagerDto? _e;
  Object? _error;
  final _grades = <String, String?>{};
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
      _bind(await ref.read(hrApiProvider).evaluations.Evaluation_GetManagerForm(id: widget.id));
    } catch (e) {
      if (mounted) setState(() => _error = e);
    }
  }

  void _bind(EvaluationManagerDto e) {
    if (!mounted) return;
    setState(() {
      _e = e;
      _grades
        ..clear()
        ..addEntries(e.scores.map((x) => MapEntry(x.criterionId, x.gradeCode)));
      _notes.text = e.employeeNotes ?? '';
    });
  }

  Future<bool> _saveDraft({bool quiet = false}) async {
    final s = S.of(context);
    final e = _e!;
    setState(() => _busy = true);
    final ok = await runAction(context, () async {
      _bind(await ref.read(hrApiProvider).evaluations.Evaluation_SaveDraft(
            id: e.id,
            body: EvaluationSaveDraftDto(
              scores: [for (final x in _grades.entries) if (x.value != null) EvaluationScoreInputDto(criterionId: x.key, gradeCode: x.value!)],
              employeeNotes: _notes.text.trim().isEmpty ? null : _notes.text.trim(),
              concurrencyStamp: e.concurrencyStamp,
            ),
          ));
    }, done: quiet ? null : s.saved);
    if (mounted) setState(() => _busy = false);
    return ok;
  }

  Future<void> _submit() async {
    final s = S.of(context);
    if (_grades.values.any((g) => g == null)) {
      showError(context, s.t('قيّم كل المعايير قبل الرفع.', 'Grade every criterion before submitting.'));
      return;
    }
    if (!await confirmAction(context,
        title: s.t('رفع التقييم', 'Submit evaluation'), message: s.t('بعد الرفع لا يمكن التعديل، ويُشعر الموظف.', 'It cannot be edited after submitting; the employee is notified.'))) {
      return;
    }
    if (!mounted || !await _saveDraft(quiet: true) || !mounted) return;
    setState(() => _busy = true);
    await runAction(context, () async {
      _bind(await ref.read(hrApiProvider).evaluations.Evaluation_Submit(id: _e!.id, body: EvaluationSubmitInputDto(concurrencyStamp: _e!.concurrencyStamp)));
    }, done: s.submitted);
    if (mounted) setState(() => _busy = false);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final e = _e;
    final session = ref.watch(sessionProvider);
    if (_error != null) return HrPage(title: s.t('التقييم', 'Evaluation'), child: ErrorState(error: _error!, onRetry: _load));
    if (e == null) return HrPage(title: s.t('التقييم', 'Evaluation'), child: const Center(child: CircularProgressIndicator()));
    final editable = e.isEditable && !_busy;
    return HrPage(
      title: '${e.employee.fullName} · ${monthLabel(e.year, e.month)}',
      actions: [StatusPill(evalStatusLabel(s, e.status), tone: evalTone(e.status))],
      child: ListView(padding: const EdgeInsets.only(bottom: 24), children: [
        _attendanceCard(context, e.attendance),
        const SizedBox(height: 12),
        HrCard(
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            SectionTitle(s.t('المعايير', 'Criteria')),
            for (final c in e.scores)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(c.criterionName, style: const TextStyle(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 6),
                  Wrap(spacing: 6, runSpacing: 6, children: [
                    for (final g in e.availableGrades)
                      ChoiceChip(
                        label: Text(g.name),
                        selected: _grades[c.criterionId] == g.code,
                        onSelected: editable ? (_) => setState(() => _grades[c.criterionId] = g.code) : null,
                      ),
                  ]),
                ]),
              ),
            TextField(
              controller: _notes,
              enabled: editable,
              maxLines: 3,
              maxLength: 4000,
              decoration: InputDecoration(labelText: s.t('ملاحظات يراها الموظف', 'Notes the employee sees')),
            ),
            if (e.isEditable)
              Row(mainAxisAlignment: MainAxisAlignment.end, children: [
                TextButton(onPressed: editable ? () => _saveDraft() : null, child: Text(s.t('حفظ مسودة', 'Save draft'))),
                const SizedBox(width: 8),
                FilledButton(onPressed: editable ? _submit : null, child: Text(s.t('رفع التقييم', 'Submit'))),
              ]),
          ]),
        ),
        const SizedBox(height: 12),
        _responsesCard(context, e.responses),
        const SizedBox(height: 12),
        if (session.can(HrOps.EvaluationInternalNote_GetList))
          _InternalNotesCard(
            evaluationId: e.id,
            submitted: e.status != EvaluationStatus.draft,
            canWrite: session.can(HrOps.EvaluationInternalNote_Add),
          ),
        if (session.can(HrOps.EvaluationAnnualSummary_GetForEmployee)) ...[
          const SizedBox(height: 12),
          _AnnualSummaryCard(employeeId: e.employee.id),
        ],
      ]),
    );
  }
}

final hrEvaluationProvider =
    FutureProvider.autoDispose.family<EvaluationHrDto, String>((ref, id) => ref.watch(hrApiProvider).evaluations.Evaluation_GetHrView(id: id));

/// HR view of a submitted evaluation, with objection decisions and internal notes.
class EvaluationHrPage extends ConsumerWidget {
  const EvaluationHrPage({super.key, required this.id});
  final String id;

  Future<void> _decide(BuildContext context, WidgetRef ref, EvaluationHrDto e, EvaluationResponseDto r) async {
    final s = S.of(context);
    var decision = EvaluationObjectionDecision.rejected;
    final reason = TextEditingController();
    final grades = <String, String>{for (final x in e.scores) x.criterionId: x.gradeCode};
    final original = Map<String, String>.of(grades);
    // the grade scale comes from the scores already given (names) plus the active scale when readable
    final scale = <String, String>{for (final x in e.scores) x.gradeCode: x.gradeName};
    if (ref.read(sessionProvider).can(HrOps.EvaluationGrade_GetList)) {
      try {
        final g = await ref.read(hrApiProvider).evaluationAdmin.EvaluationGrade_GetList(isActive: true, maxResultCount: 50);
        for (final x in g.items) {
          scale[x.code] = x.name;
        }
      } catch (_) {}
    }
    if (!context.mounted) return;
    final ok = await showFormDialog(
      context,
      width: 600,
      title: s.t('البت في الاعتراض', 'Decide objection'),
      fields: (setState) => [
        Text(r.content),
        SegmentedButton<EvaluationObjectionDecision>(
          segments: [
            ButtonSegment(value: EvaluationObjectionDecision.rejected, label: Text(s.t('رفض', 'Reject'))),
            ButtonSegment(value: EvaluationObjectionDecision.accepted, label: Text(s.t('قبول', 'Accept'))),
          ],
          selected: {decision},
          onSelectionChanged: (v) => setState(() => decision = v.first),
        ),
        if (decision == EvaluationObjectionDecision.accepted)
          for (final x in e.scores)
            DropdownButtonFormField<String>(
              initialValue: grades[x.criterionId],
              decoration: InputDecoration(labelText: x.criterionName),
              items: [for (final g in scale.entries) DropdownMenuItem(value: g.key, child: Text(g.value))],
              onChanged: (v) => setState(() => grades[x.criterionId] = v ?? grades[x.criterionId]!),
            ),
        TextFormField(
          controller: reason,
          maxLines: 3,
          maxLength: 2000,
          decoration: InputDecoration(labelText: s.t('السبب (يظهر للموظف ومديره)', 'Reason (shown to the employee and manager)')),
          validator: (v) => requiredValidator(context, v),
        ),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).evaluations.Evaluation_DecideObjection(
              id: e.id,
              rid: r.id,
              body: EvaluationObjectionDecideDto(
                decision: decision,
                reason: reason.text.trim(),
                concurrencyStamp: e.concurrencyStamp,
                scores: decision == EvaluationObjectionDecision.accepted
                    ? [for (final x in grades.entries) if (original[x.key] != x.value) EvaluationScoreInputDto(criterionId: x.key, gradeCode: x.value)]
                    : null,
              ),
            ),
        done: s.saved,
      ),
    );
    if (ok) ref.invalidate(hrEvaluationProvider(id));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.t('التقييم', 'Evaluation'),
      child: AsyncBody<EvaluationHrDto>(
        value: ref.watch(hrEvaluationProvider(id)),
        onRetry: () => ref.invalidate(hrEvaluationProvider(id)),
        builder: (e) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Wrap(spacing: 16, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
              Text(e.employee.fullName, style: Theme.of(context).textTheme.titleMedium),
              Text(monthLabel(e.year, e.month)),
              Text('${s.t('المقيّم', 'Evaluator')} ${e.evaluator.displayName}'),
              Text(f.dateTime(e.submittedAt)),
              StatusPill(evalStatusLabel(s, e.status), tone: evalTone(e.status)),
              if (e.objectionStatus != EvaluationObjectionStatus.none) StatusPill(objectionLabel(s, e.objectionStatus), tone: Tone.warning),
            ]),
          ),
          const SizedBox(height: 12),
          _attendanceCard(context, e.attendance),
          const SizedBox(height: 12),
          _scoresCard(context, e.scores, e.employeeNotes),
          const SizedBox(height: 12),
          _responsesCard(context, e.responses,
              onDecide: session.can(HrOps.Evaluation_DecideObjection) ? (r) => _decide(context, ref, e, r) : null),
          const SizedBox(height: 12),
          if (session.can(HrOps.EvaluationInternalNote_GetList)) _InternalNotesCard(evaluationId: e.id, submitted: true, canWrite: false),
          if (session.can(HrOps.EvaluationAnnualSummary_GetForEmployee)) ...[
            const SizedBox(height: 12),
            _AnnualSummaryCard(employeeId: e.employee.id),
          ],
        ]),
      ),
    );
  }
}

/// Notification target: opens the employee view, the manager form or the HR view,
/// whichever the caller can read (the others answer 404 by design).
class EvaluationOpenPage extends ConsumerStatefulWidget {
  const EvaluationOpenPage({super.key, required this.id});
  final String id;

  @override
  ConsumerState<EvaluationOpenPage> createState() => _EvaluationOpenPageState();
}

class _EvaluationOpenPageState extends ConsumerState<EvaluationOpenPage> {
  Object? _error;

  @override
  void initState() {
    super.initState();
    _resolve();
  }

  Future<void> _resolve() async {
    final session = ref.read(sessionProvider);
    final api = ref.read(hrApiProvider).evaluations;
    final attempts = <(String, Future<Object> Function())>[
      if (session.can(HrOps.Evaluation_GetMyDetail)) ('/evaluations/my/${widget.id}', () => api.Evaluation_GetMyDetail(id: widget.id)),
      if (session.can(HrOps.Evaluation_GetManagerForm)) ('/evaluations/${widget.id}', () => api.Evaluation_GetManagerForm(id: widget.id)),
      if (session.can(HrOps.Evaluation_GetHrView)) ('/evaluations/${widget.id}/hr', () => api.Evaluation_GetHrView(id: widget.id)),
    ];
    Object? last;
    for (final (path, probe) in attempts) {
      try {
        await probe();
        if (mounted) context.go(path);
        return;
      } on ApiException catch (e) {
        if (e.statusCode != 404) {
          last = e;
          break;
        }
        last = e;
      }
    }
    if (mounted) setState(() => _error = last ?? S.of(context).notFound);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return HrPage(
      title: s.t('التقييم', 'Evaluation'),
      child: _error == null ? const Center(child: CircularProgressIndicator()) : ErrorState(error: _error!),
    );
  }
}
