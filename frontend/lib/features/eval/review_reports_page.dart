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
import '../reviewer_evaluations/reviewer_evaluations_page.dart' show reviewerGradeLabel, reviewerGradeTone;

Widget reviewStatusPill(S s, ReviewReportStatus status, DateTime? viewedAt) {
  if (status == ReviewReportStatus.draft) return StatusPill(s.t('مسودة', 'Draft'), tone: Tone.warning);
  if (viewedAt != null) return StatusPill(s.t('اطّلع عليه التنفيذي', 'Seen by the executive'), tone: Tone.success, icon: Icons.done_all);
  return StatusPill(s.t('مرفوع', 'Submitted'), tone: Tone.info);
}

/// The grades a reviewer can pick, without the client's placeholder for unknown values.
final reviewGradeChoices = [for (final g in ReviewerGrade.values) if (g != ReviewerGrade.$unknown) g];

/// A room row is complete once both the backend and the frontend work carry a grade (the contract's submit rule).
bool reviewTeamGraded(ReviewerGrade? backend, ReviewerGrade? frontend) => backend != null && frontend != null;

Widget reviewGradePill(S s, ReviewerGrade? g) =>
    g == null ? StatusPill(s.t('بدون تقدير', 'Not graded'), tone: Tone.neutral) : StatusPill(reviewerGradeLabel(s, g), tone: reviewerGradeTone(g));

/// The reviewers who delegated their daily report to me, right now (decision 2026-11-11). Empty for most people.
final standInForProvider = FutureProvider.autoDispose<List<UserRefDto>>((ref) async {
  final session = ref.watch(sessionProvider);
  if (!session.can(HrOps.Delegation_GetMy)) return const <UserRefDto>[];
  try {
    final r = await ref.watch(hrApiProvider).delegations.Delegation_GetMy(
        direction: DelegationDirection.received, status: DelegationStatus.active, maxResultCount: 50);
    final seen = <String>{};
    return [for (final d in r.items) if (d.scope == DelegationScope.reviewReports && seen.add(d.fromUser.id)) d.fromUser];
  } catch (_) {
    return const <UserRefDto>[];
  }
});

/// S-RVR: the daily report (decision 2026-11-11). The reviewer writes one a day with a full text and a row per room,
/// then sends it to the executive director, who reads the submitted ones.
class ReviewReportsPage extends ConsumerStatefulWidget {
  const ReviewReportsPage({super.key});

  @override
  ConsumerState<ReviewReportsPage> createState() => _ReviewReportsPageState();
}

class _ReviewReportsPageState extends ConsumerState<ReviewReportsPage> {
  ReviewReportStatus? _status;
  DateTime? _from;
  DateTime? _to;

  Future<void> _create() async {
    final s = S.of(context);
    final today = DateTime.now();
    var day = DateTime(today.year, today.month, today.day);
    // A stand-in writes for a reviewer who delegated to them; a reviewer writes their own (null).
    final own = ref.read(sessionProvider).can(HrOps.ReviewReport_Create);
    final standInFor = ref.read(standInForProvider).valueOrNull ?? const <UserRefDto>[];
    UserRefDto? onBehalfOf = own ? null : (standInFor.isEmpty ? null : standInFor.first);
    final content = TextEditingController();
    String? createdId;

    await showFormDialog(
      context,
      title: s.t('تقرير يومي جديد', 'New daily report'),
      submitLabel: s.t('إنشاء', 'Create'),
      fields: (setState) {
        final f = Fmt(context);
        return [
          if (standInFor.isNotEmpty)
            DropdownButtonFormField<UserRefDto?>(
              initialValue: onBehalfOf,
              decoration: InputDecoration(labelText: s.t('عن المراجع', 'On behalf of the reviewer')),
              items: [
                if (own) DropdownMenuItem<UserRefDto?>(value: null, child: Text(s.t('تقريري', 'My own report'))),
                for (final u in standInFor) DropdownMenuItem<UserRefDto?>(value: u, child: PersonName.user(u)),
              ],
              onChanged: (v) => setState(() => onBehalfOf = v),
            ),
          PickerField(
            label: s.t('يوم التقرير', 'Report day'),
            value: f.date(day),
            icon: Icons.calendar_month_outlined,
            onTap: () async {
              final picked = await pickDate(context, initial: day, first: DateTime(2020), last: today);
              if (picked != null) setState(() => day = picked);
            },
          ),
          Text(s.t('يأخذ التقرير غرفك الحالية كما هي، صف لكل غرفة.', 'The report takes your current rooms as they are, one row per room.'),
              style: TextStyle(color: context.hr.textSecondary)),
          TextFormField(
            controller: content,
            minLines: 4,
            maxLines: 10,
            maxLength: 10000,
            decoration: InputDecoration(labelText: s.t('نص التقرير (يمكن كتابته لاحقًا)', 'Report text (can be written later)')),
          ),
        ];
      },
      onSubmit: () async {
        return runAction(context, () async {
          final r = await ref.read(hrApiProvider).reviewReports.ReviewReport_Create(
              body: ReviewReportCreateDto(reportDate: day, content: optText(content), onBehalfOfUserId: onBehalfOf?.id));
          createdId = r.id;
        });
      },
    );
    content.dispose();
    if (createdId != null && mounted) context.go('/review-reports/$createdId');
  }

  Future<void> _pickRange(bool from) async {
    final picked = await pickDate(context,
        initial: (from ? _from : _to) ?? DateTime.now(), first: DateTime(2020), last: DateTime.now().add(const Duration(days: 1)));
    if (picked != null) setState(() => from ? _from = picked : _to = picked);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final standInFor = ref.watch(standInForProvider).valueOrNull ?? const <UserRefDto>[];
    final canWrite = session.can(HrOps.ReviewReport_Create) || standInFor.isNotEmpty;
    final api = ref.watch(hrApiProvider).reviewReports;
    return HrPage(
      title: s.t('التقرير اليومي', 'Daily report'),
      subtitle: canWrite
          ? s.t('قيّم عمل كل غرفة يوميًا (الباك والفرونت والملاحظات) وارفعه إلى المدير التنفيذي',
              'Grade each room every day (backend, frontend, notes) and send it to the executive director')
          : s.t('التقارير اليومية المرفوعة من المتابع', 'Daily reports submitted by the reviewer'),
      actions: [
        if (canWrite)
          OutlinedButton.icon(
              onPressed: () => context.go('/review-rooms'), icon: const Icon(Icons.meeting_room_outlined), label: Text(s.t('الغرف', 'Rooms'))),
        if (canWrite) FilledButton.icon(onPressed: _create, icon: const Icon(Icons.add), label: Text(s.t('تقرير جديد', 'New report'))),
      ],
      child: PagedList<ReviewReportSummaryDto>(
        key: ValueKey('$_status|$_from|$_to'),
        emptyMessage: s.t('لا توجد تقارير بعد', 'No reports yet'),
        header: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
            SizedBox(
              width: 180,
              child: PickerField(
                  label: s.t('من يوم', 'From'), value: _from == null ? '—' : f.date(_from), icon: Icons.event_outlined, onTap: () => _pickRange(true)),
            ),
            SizedBox(
              width: 180,
              child: PickerField(
                  label: s.t('إلى يوم', 'To'), value: _to == null ? '—' : f.date(_to), icon: Icons.event_outlined, onTap: () => _pickRange(false)),
            ),
            if (_from != null || _to != null)
              TextButton(onPressed: () => setState(() => _from = _to = null), child: Text(s.t('مسح التاريخ', 'Clear dates'))),
            if (canWrite)
              enumFilter<ReviewReportStatus>(context,
                  label: s.status,
                  value: _status,
                  values: ReviewReportStatus.values,
                  text: (v) => v == ReviewReportStatus.draft ? s.t('مسودة', 'Draft') : s.t('مرفوع', 'Submitted'),
                  onChanged: (v) => setState(() => _status = v)),
          ]),
        ),
        fetch: (skip, max) async {
          final r = await api.ReviewReport_GetList(
              skipCount: skip, maxResultCount: max, dateFrom: _from, dateTo: _to, status: canWrite ? _status : null, sorting: 'reportDate desc');
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, r) => OpsTile(
          leading: Icons.today_outlined,
          title: '${s.t('التقرير اليومي', 'Daily report')} · ${f.date(r.reportDate)}',
          subtitle: [
            r.reviewer.displayName,
            if (r.preparedBy.id != r.reviewer.id) '${s.t('أعدّه', 'Prepared by')} ${r.preparedBy.displayName}',
            '${s.t('الغرف', 'Rooms')} ${r.teamsCount}',
            if (r.submittedAt != null) '${s.t('رُفع', 'Submitted')} ${f.dateTime(r.submittedAt)}',
          ].join(' · '),
          trailing: reviewStatusPill(s, r.status, r.viewedAt),
          onTap: () => context.go('/review-reports/${r.id}'),
        ),
      ),
    );
  }
}

class _TeamEdit {
  _TeamEdit(ReviewReportTeamDto t)
      : backend = t.backendGrade,
        frontend = t.frontendGrade,
        backendNotes = TextEditingController(text: t.backendNotes ?? ''),
        frontendNotes = TextEditingController(text: t.frontendNotes ?? ''),
        notes = TextEditingController(text: t.notes ?? '');

  ReviewerGrade? backend;
  ReviewerGrade? frontend;
  final TextEditingController backendNotes;
  final TextEditingController frontendNotes;
  final TextEditingController notes;

  void reset(ReviewReportTeamDto t) {
    backend = t.backendGrade;
    frontend = t.frontendGrade;
    backendNotes.text = t.backendNotes ?? '';
    frontendNotes.text = t.frontendNotes ?? '';
    notes.text = t.notes ?? '';
  }

  void dispose() {
    backendNotes.dispose();
    frontendNotes.dispose();
    notes.dispose();
  }
}

/// One daily report: the full text, then a card per room (lead, members) graded for backend and frontend work.
class ReviewReportPage extends ConsumerStatefulWidget {
  const ReviewReportPage({super.key, required this.id});
  final String id;

  @override
  ConsumerState<ReviewReportPage> createState() => _ReviewReportPageState();
}

class _ReviewReportPageState extends ConsumerState<ReviewReportPage> {
  static const _pageSize = 200;

  ReviewReportDto? _r;
  Object? _error;
  bool _busy = false;
  final _content = TextEditingController();
  final Map<String, _TeamEdit> _edits = <String, _TeamEdit>{};
  final _formKey = GlobalKey<FormState>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _content.dispose();
    for (final e in _edits.values) {
      e.dispose();
    }
    super.dispose();
  }

  Future<void> _load() async {
    setState(() => _error = null);
    try {
      _bind(await ref.read(hrApiProvider).reviewReports.ReviewReport_Get(id: widget.id));
    } catch (e) {
      if (mounted) setState(() => _error = e);
    }
  }

  void _bind(ReviewReportDto r) {
    if (!mounted) return;
    setState(() {
      _r = r;
      _content.text = r.content ?? '';
      // A report keeps its rooms for life, so the editors are reused and only their values are refreshed.
      for (final t in r.teams) {
        final edit = _edits[t.room.id];
        if (edit == null) {
          _edits[t.room.id] = _TeamEdit(t);
        } else {
          edit.reset(t);
        }
      }
    });
  }

  /// Saves the draft a page of rooms at a time (the contract caps one call at 500 rows).
  Future<void> _persist() async {
    final api = ref.read(hrApiProvider).reviewReports;
    var r = _r!;
    final ids = _edits.keys.toList();
    for (var i = 0; i < ids.length || i == 0; i += _pageSize) {
      final page = ids.skip(i).take(_pageSize);
      r = await api.ReviewReport_Update(
        id: r.id,
        body: ReviewReportUpdateDto(
          reportDate: r.reportDate,
          content: optText(_content),
          teams: [
            for (final id in page)
              ReviewReportTeamInputDto(
                roomId: id,
                backendGrade: _edits[id]!.backend,
                backendNotes: optText(_edits[id]!.backendNotes),
                frontendGrade: _edits[id]!.frontend,
                frontendNotes: optText(_edits[id]!.frontendNotes),
                notes: optText(_edits[id]!.notes),
              ),
          ],
          concurrencyStamp: r.concurrencyStamp,
        ),
      );
      if (ids.isEmpty) break;
    }
    _bind(r);
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    final s = S.of(context);
    setState(() => _busy = true);
    await runAction(context, _persist, done: s.saved);
    if (mounted) setState(() => _busy = false);
  }

  Future<void> _submit() async {
    final s = S.of(context);
    if (!_formKey.currentState!.validate()) return;
    if (_content.text.trim().isEmpty) {
      showError(context, s.t('اكتب نص التقرير قبل الرفع', 'Write the report text before submitting'));
      return;
    }
    if (_edits.values.any((e) => !reviewTeamGraded(e.backend, e.frontend))) {
      showError(context, s.t('اختر تقدير الباك والفرونت لكل غرفة قبل الرفع', 'Choose the backend and frontend grades for every room before submitting'));
      return;
    }
    if (!await confirmAction(context,
        title: s.t('رفع التقرير', 'Submit report'),
        message: s.t('يصل التقرير إلى المدير التنفيذي، ولا يتعدل بعد الرفع.', 'The report goes to the executive director and cannot change afterwards.'))) {
      return;
    }
    if (!mounted) return;
    setState(() => _busy = true);
    await runAction(context, () async {
      await _persist();
      _bind(await ref.read(hrApiProvider).reviewReports.ReviewReport_Submit(id: _r!.id, body: ReviewReportSubmitDto(concurrencyStamp: _r!.concurrencyStamp)));
    }, done: s.t('رُفع التقرير إلى المدير التنفيذي', 'Report sent to the executive director'));
    if (mounted) setState(() => _busy = false);
  }

  Future<void> _delete() async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('حذف المسودة', 'Delete draft'), message: s.t('تُحذف المسودة ويتحرر يومها.', 'The draft is deleted and its day is freed.'), destructive: true)) {
      return;
    }
    if (!mounted) return;
    setState(() => _busy = true);
    final ok = await runAction(context, () => ref.read(hrApiProvider).reviewReports.ReviewReport_Delete(id: _r!.id), done: s.t('حُذفت المسودة', 'Draft deleted'));
    if (!mounted) return;
    setState(() => _busy = false);
    if (ok) context.go('/review-reports');
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final r = _r;
    final title = s.t('التقرير اليومي', 'Daily report');
    if (_error != null) return HrPage(title: title, child: ErrorState(error: _error!, onRetry: _load));
    if (r == null) return HrPage(title: title, child: const Center(child: CircularProgressIndicator()));

    final session = ref.watch(sessionProvider);
    // The reviewer, or their stand-in during the delegation (decision 2026-11-11).
    final standIn = (ref.watch(standInForProvider).valueOrNull ?? const <UserRefDto>[]).any((u) => u.id == r.reviewer.id);
    final writer = session.can(HrOps.ReviewReport_Update) || standIn;
    final editable = r.status == ReviewReportStatus.draft && writer;
    final graded = editable
        ? _edits.values.where((e) => reviewTeamGraded(e.backend, e.frontend)).length
        : r.teams.where((t) => reviewTeamGraded(t.backendGrade, t.frontendGrade)).length;

    return HrPage(
      title: '$title · ${f.date(r.reportDate)}',
      subtitle: [
        r.reviewer.displayName,
        if (r.preparedBy.id != r.reviewer.id) '${s.t('أعدّه', 'Prepared by')} ${r.preparedBy.displayName}',
      ].join(' · '),
      actions: [reviewStatusPill(s, r.status, r.viewedAt)],
      child: Form(
        key: _formKey,
        child: ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          Wrap(spacing: 12, runSpacing: 12, children: [
            SizedBox(
              width: 240,
              child: KpiTile(
                label: s.t('الغرف المقيّمة', 'Rooms graded'),
                value: '$graded/${r.teams.length}',
                icon: Icons.meeting_room_outlined,
              ),
            ),
            SizedBox(
              width: 240,
              child: KpiTile(
                label: s.t('الرفع', 'Submission'),
                value: r.submittedAt == null ? s.t('لم يُرفع', 'Not yet') : f.dateTime(r.submittedAt),
                icon: Icons.send_outlined,
                caption: r.viewedAt == null ? null : '${s.t('اطّلع التنفيذي', 'Executive viewed')} ${f.dateTime(r.viewedAt)}',
              ),
            ),
          ]),
          const SizedBox(height: 12),
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.t('نص التقرير', 'Report text')),
              if (editable)
                TextFormField(
                  controller: _content,
                  enabled: !_busy,
                  minLines: 6,
                  maxLines: 20,
                  maxLength: 10000,
                  decoration: InputDecoration(hintText: s.t('اكتب تقرير اليوم كاملًا', 'Write the full report for the day')),
                )
              else
                SelectableText(r.content ?? '—'),
            ]),
          ),
          const SizedBox(height: 12),
          SectionTitle(s.t('تقييم الغرف حسب مسؤول الفريق', 'Rooms by team lead')),
          if (r.teams.isEmpty) EmptyState(message: s.t('لا توجد غرف', 'No rooms')),
          for (final t in r.teams) _teamCard(context, t, editable),
          if (editable) ...[
            const SizedBox(height: 12),
            Wrap(alignment: WrapAlignment.end, spacing: 8, runSpacing: 8, children: [
              TextButton.icon(
                onPressed: _busy ? null : _delete,
                icon: const Icon(Icons.delete_outline, color: HrColors.danger),
                label: Text(s.t('حذف المسودة', 'Delete draft'), style: const TextStyle(color: HrColors.danger)),
              ),
              OutlinedButton(onPressed: _busy ? null : _save, child: Text(s.save)),
              FilledButton.icon(
                onPressed: _busy ? null : _submit,
                icon: const Icon(Icons.send_outlined),
                label: Text(s.t('رفع إلى المدير التنفيذي', 'Send to the executive director')),
              ),
            ]),
          ],
        ]),
      ),
    );
  }

  Widget _gradeField(S s, String label, ReviewerGrade? value, ValueChanged<ReviewerGrade?> onChanged) => DropdownButtonFormField<ReviewerGrade?>(
        initialValue: value,
        decoration: InputDecoration(labelText: label),
        items: [
          DropdownMenuItem<ReviewerGrade?>(value: null, child: Text(s.t('بدون تقدير', 'Not graded'))),
          for (final g in reviewGradeChoices) DropdownMenuItem<ReviewerGrade?>(value: g, child: Text(reviewerGradeLabel(s, g))),
        ],
        onChanged: _busy ? null : onChanged,
      );

  Widget _teamCard(BuildContext context, ReviewReportTeamDto t, bool editable) {
    final s = S.of(context);
    final edit = _edits[t.room.id];
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: HrCard(
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Row(children: [
            Icon(Icons.meeting_room_outlined, color: context.hr.accent),
            const SizedBox(width: 12),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(t.room.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                const SizedBox(height: 2),
                Wrap(spacing: 6, crossAxisAlignment: WrapCrossAlignment.center, children: [
                  Text('${s.t('مسؤول الفريق', 'Team lead')}:', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                  PersonName.employee(t.manager, style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                  if ((t.department?.name ?? t.manager.departmentName) != null)
                    Text('· ${t.department?.name ?? t.manager.departmentName}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                ]),
              ]),
            ),
          ]),
          if (t.members.isNotEmpty) ...[
            const SizedBox(height: 8),
            Wrap(spacing: 6, runSpacing: 6, children: [
              for (final m in t.members) Chip(visualDensity: VisualDensity.compact, label: PersonName.employee(m, radius: 10)),
            ]),
          ],
          const SizedBox(height: 12),
          if (editable && edit != null) ...[
            FieldGrid(children: [
              _gradeField(s, s.t('تقييم عمل الباك', 'Backend work'), edit.backend, (v) => setState(() => edit.backend = v)),
              _gradeField(s, s.t('تقييم عمل الفرونت', 'Frontend work'), edit.frontend, (v) => setState(() => edit.frontend = v)),
              TextFormField(
                controller: edit.backendNotes,
                enabled: !_busy,
                maxLines: 3,
                maxLength: 2000,
                decoration: InputDecoration(labelText: s.t('شرح تقييم الباك', 'About the backend work')),
              ),
              TextFormField(
                controller: edit.frontendNotes,
                enabled: !_busy,
                maxLines: 3,
                maxLength: 2000,
                decoration: InputDecoration(labelText: s.t('شرح تقييم الفرونت', 'About the frontend work')),
              ),
            ]),
            const SizedBox(height: 8),
            TextFormField(
              controller: edit.notes,
              enabled: !_busy,
              maxLines: 3,
              maxLength: 2000,
              decoration: InputDecoration(labelText: s.t('ملاحظات', 'Notes')),
            ),
          ] else ...[
            _readOnlyGrade(context, s.t('عمل الباك', 'Backend work'), t.backendGrade, t.backendNotes),
            _readOnlyGrade(context, s.t('عمل الفرونت', 'Frontend work'), t.frontendGrade, t.frontendNotes),
            InfoRow(s.t('ملاحظات', 'Notes'), t.notes ?? '—'),
          ],
        ]),
      ),
    );
  }

  Widget _readOnlyGrade(BuildContext context, String label, ReviewerGrade? grade, String? notes) {
    final s = S.of(context);
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Wrap(spacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
          Text(label, style: const TextStyle(fontWeight: FontWeight.w600)),
          reviewGradePill(s, grade),
        ]),
        if (notes != null) Padding(padding: const EdgeInsets.only(top: 4), child: Text(notes, style: TextStyle(color: context.hr.textSecondary))),
      ]),
    );
  }
}
