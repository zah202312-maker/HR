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
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';

String reviewPeriodLabel(S s, ReviewReportPeriodType t) => switch (t) {
      ReviewReportPeriodType.daily => s.t('يومي', 'Daily'),
      ReviewReportPeriodType.weekly => s.t('أسبوعي', 'Weekly'),
      ReviewReportPeriodType.monthly => s.t('شهري', 'Monthly'),
      _ => '—',
    };

Widget reviewStatusPill(S s, ReviewReportStatus status, DateTime? viewedAt) {
  if (status == ReviewReportStatus.draft) return StatusPill(s.t('مسودة', 'Draft'), tone: Tone.warning);
  if (viewedAt != null) return StatusPill(s.t('اطّلع عليه التنفيذي', 'Seen by the executive'), tone: Tone.success, icon: Icons.done_all);
  return StatusPill(s.t('مرفوع', 'Submitted'), tone: Tone.info);
}

/// The contract's period rules: one day, seven days, or a whole calendar month.
(DateTime, DateTime) reviewPeriodRange(ReviewReportPeriodType type, DateTime start) {
  final d = DateTime(start.year, start.month, start.day);
  return switch (type) {
    ReviewReportPeriodType.weekly => (d, d.add(const Duration(days: 6))),
    ReviewReportPeriodType.monthly => (DateTime(d.year, d.month, 1), DateTime(d.year, d.month + 1, 0)),
    _ => (d, d),
  };
}

Color _completionColor(int? p) {
  if (p == null) return HrColors.info;
  if (p >= 80) return HrColors.success;
  if (p >= 50) return HrColors.warning;
  return HrColors.danger;
}

/// S-RVR: the reviewer's follow-up reports. The reviewer writes and submits; the executive director reads submitted ones.
class ReviewReportsPage extends ConsumerStatefulWidget {
  const ReviewReportsPage({super.key});

  @override
  ConsumerState<ReviewReportsPage> createState() => _ReviewReportsPageState();
}

class _ReviewReportsPageState extends ConsumerState<ReviewReportsPage> {
  ReviewReportPeriodType? _type;
  ReviewReportStatus? _status;

  Future<void> _create() async {
    final s = S.of(context);
    var type = ReviewReportPeriodType.weekly;
    var start = DateTime.now();
    final summary = TextEditingController();
    String? createdId;

    await showFormDialog(
      context,
      title: s.t('تقرير متابعة جديد', 'New follow-up report'),
      submitLabel: s.t('إنشاء', 'Create'),
      fields: (setState) {
        final (from, to) = reviewPeriodRange(type, start);
        final f = Fmt(context);
        return [
          DropdownButtonFormField<ReviewReportPeriodType>(
            initialValue: type,
            decoration: InputDecoration(labelText: s.t('نوع التقرير', 'Report type')),
            items: [
              for (final t in ReviewReportPeriodType.values.where((v) => v != ReviewReportPeriodType.$unknown))
                DropdownMenuItem(value: t, child: Text(reviewPeriodLabel(s, t))),
            ],
            onChanged: (v) => setState(() => type = v ?? type),
          ),
          PickerField(
            label: type == ReviewReportPeriodType.monthly ? s.t('الشهر', 'Month') : s.t('بداية الفترة', 'Period start'),
            value: f.date(from),
            icon: Icons.calendar_month_outlined,
            onTap: () async {
              final picked = await pickDate(context, initial: start, first: DateTime(2020), last: DateTime.now().add(const Duration(days: 366)));
              if (picked != null) setState(() => start = picked);
            },
          ),
          Text('${s.t('الفترة', 'Period')}: ${f.date(from)} → ${f.date(to)}', style: TextStyle(color: context.hr.textSecondary)),
          TextFormField(
            controller: summary,
            maxLines: 3,
            maxLength: 4000,
            decoration: InputDecoration(labelText: s.t('ملخص عام (اختياري)', 'General summary (optional)')),
          ),
        ];
      },
      onSubmit: () async {
        final (from, to) = reviewPeriodRange(type, start);
        return runAction(context, () async {
          final r = await ref.read(hrApiProvider).reviewReports.ReviewReport_Create(
              body: ReviewReportCreateDto(periodType: type, periodFrom: from, periodTo: to, summary: optText(summary)));
          createdId = r.id;
        });
      },
    );
    summary.dispose();
    if (createdId != null && mounted) context.go('/review-reports/$createdId');
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final canWrite = session.can(HrOps.ReviewReport_Create);
    final api = ref.watch(hrApiProvider).reviewReports;
    return HrPage(
      title: s.t('تقارير متابعة الفرق', 'Team follow-up reports'),
      subtitle: canWrite
          ? s.t('قيّم إنجاز كل فريق وارفع التقرير إلى المدير التنفيذي', 'Assess each team and send the report to the executive director')
          : s.t('تقارير المراجع المرفوعة', 'Reports submitted by the reviewer'),
      actions: [
        if (canWrite) FilledButton.icon(onPressed: _create, icon: const Icon(Icons.add), label: Text(s.t('تقرير جديد', 'New report'))),
      ],
      child: PagedList<ReviewReportSummaryDto>(
        key: ValueKey('$_type|$_status'),
        emptyMessage: s.t('لا توجد تقارير بعد', 'No reports yet'),
        header: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Wrap(spacing: 12, runSpacing: 8, children: [
            enumFilter<ReviewReportPeriodType>(context,
                label: s.t('النوع', 'Type'),
                value: _type,
                values: ReviewReportPeriodType.values,
                text: (v) => reviewPeriodLabel(s, v),
                onChanged: (v) => setState(() => _type = v)),
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
              skipCount: skip, maxResultCount: max, periodType: _type, status: canWrite ? _status : null, sorting: 'periodFrom desc');
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, r) => OpsTile(
          leading: Icons.insights_outlined,
          title: '${reviewPeriodLabel(s, r.periodType)} · ${f.date(r.periodFrom)} → ${f.date(r.periodTo)}',
          subtitle: [
            r.reviewer.displayName,
            '${s.t('الفرق', 'Teams')} ${r.teamsCount}',
            if (r.averageCompletion != null) '${s.t('متوسط الإنجاز', 'Average completion')} ${r.averageCompletion}%',
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
      : percent = TextEditingController(text: t.completionPercent?.toString() ?? ''),
        notes = TextEditingController(text: t.notes ?? ''),
        projects = TextEditingController(text: t.projectNotes ?? '');

  final TextEditingController percent;
  final TextEditingController notes;
  final TextEditingController projects;

  int? get value => int.tryParse(percent.text.trim());

  void dispose() {
    percent.dispose();
    notes.dispose();
    projects.dispose();
  }
}

/// One report: a card per team (manager and members), the reviewer's rate and notes, then submit to the executive.
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
  final _summary = TextEditingController();
  final Map<String, _TeamEdit> _edits = <String, _TeamEdit>{};
  final _formKey = GlobalKey<FormState>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _summary.dispose();
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
      _summary.text = r.summary ?? '';
      // A report keeps its teams for life, so the editors are reused and only their text is refreshed.
      for (final t in r.teams) {
        final edit = _edits[t.manager.id];
        if (edit == null) {
          _edits[t.manager.id] = _TeamEdit(t);
        } else {
          edit.percent.text = t.completionPercent?.toString() ?? '';
          edit.notes.text = t.notes ?? '';
          edit.projects.text = t.projectNotes ?? '';
        }
      }
    });
  }

  /// Saves the draft a page of teams at a time (the contract caps one call at 500 rows).
  Future<void> _persist() async {
    final api = ref.read(hrApiProvider).reviewReports;
    var r = _r!;
    final ids = _edits.keys.toList();
    for (var i = 0; i < ids.length || i == 0; i += _pageSize) {
      final page = ids.skip(i).take(_pageSize);
      r = await api.ReviewReport_Update(
        id: r.id,
        body: ReviewReportUpdateDto(
          periodType: r.periodType,
          periodFrom: r.periodFrom,
          periodTo: r.periodTo,
          summary: optText(_summary),
          teams: [
            for (final id in page)
              ReviewReportTeamInputDto(
                managerEmployeeId: id,
                completionPercent: _edits[id]!.value,
                notes: optText(_edits[id]!.notes),
                projectNotes: optText(_edits[id]!.projects),
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
    if (_edits.values.any((e) => e.value == null)) {
      showError(context, s.t('اكتب نسبة الإنجاز لكل فريق قبل الرفع', 'Enter a completion rate for every team before submitting'));
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
    if (!await confirmAction(context, title: s.t('حذف المسودة', 'Delete draft'), message: s.t('تُحذف المسودة نهائيًا.', 'The draft will be deleted.'), destructive: true)) {
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
    final title = s.t('تقرير متابعة', 'Follow-up report');
    if (_error != null) return HrPage(title: title, child: ErrorState(error: _error!, onRetry: _load));
    if (r == null) return HrPage(title: title, child: const Center(child: CircularProgressIndicator()));

    final session = ref.watch(sessionProvider);
    final editable = r.status == ReviewReportStatus.draft && session.can(HrOps.ReviewReport_Update);
    final assessed = r.teams.where((t) => t.completionPercent != null).length;

    return HrPage(
      title: '$title · ${reviewPeriodLabel(s, r.periodType)}',
      subtitle: '${f.date(r.periodFrom)} → ${f.date(r.periodTo)} · ${r.reviewer.displayName}',
      actions: [reviewStatusPill(s, r.status, r.viewedAt)],
      child: Form(
        key: _formKey,
        child: ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          Wrap(spacing: 12, runSpacing: 12, children: [
            SizedBox(
              width: 240,
              child: KpiTile(
                label: s.t('متوسط الإنجاز', 'Average completion'),
                value: r.averageCompletion == null ? '—' : '${r.averageCompletion}%',
                icon: Icons.speed_outlined,
              ),
            ),
            SizedBox(
              width: 240,
              child: KpiTile(
                label: s.t('الفرق المقيّمة', 'Teams assessed'),
                value: '$assessed/${r.teams.length}',
                icon: Icons.groups_2_outlined,
              ),
            ),
            SizedBox(
              width: 240,
              child: KpiTile(
                label: s.t('الرفع', 'Submission'),
                value: r.submittedAt == null ? s.t('لم يُرفع', 'Not yet') : f.date(r.submittedAt),
                icon: Icons.send_outlined,
                caption: r.viewedAt == null ? null : '${s.t('اطّلع التنفيذي', 'Executive viewed')} ${f.dateTime(r.viewedAt)}',
              ),
            ),
          ]),
          const SizedBox(height: 12),
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.t('الملخص العام', 'General summary')),
              if (editable)
                TextFormField(controller: _summary, enabled: !_busy, maxLines: 4, maxLength: 4000)
              else
                Text(r.summary ?? '—'),
            ]),
          ),
          const SizedBox(height: 12),
          SectionTitle(s.t('تقييم الفرق', 'Team assessment')),
          if (r.teams.isEmpty) EmptyState(message: s.t('لا توجد فرق', 'No teams')),
          for (final t in r.teams) _teamCard(context, t, editable),
          if (editable) ...[
            const SizedBox(height: 12),
            Wrap(alignment: WrapAlignment.end, spacing: 8, runSpacing: 8, children: [
              if (session.can(HrOps.ReviewReport_Delete))
                TextButton.icon(
                  onPressed: _busy ? null : _delete,
                  icon: const Icon(Icons.delete_outline, color: HrColors.danger),
                  label: Text(s.t('حذف المسودة', 'Delete draft'), style: const TextStyle(color: HrColors.danger)),
                ),
              OutlinedButton(onPressed: _busy ? null : _save, child: Text(s.save)),
              if (session.can(HrOps.ReviewReport_Submit))
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

  Widget _teamCard(BuildContext context, ReviewReportTeamDto t, bool editable) {
    final s = S.of(context);
    final edit = _edits[t.manager.id];
    final percent = editable ? edit?.value : t.completionPercent;
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: HrCard(
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Row(children: [
            Icon(Icons.groups_outlined, color: context.hr.accent),
            const SizedBox(width: 12),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(t.department?.name ?? t.manager.departmentName ?? '—', style: const TextStyle(fontWeight: FontWeight.w600)),
                Text('${s.t('المدير', 'Manager')}: ${t.manager.fullName} · ${s.t('الأفراد', 'Members')} ${t.membersCount}',
                    style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
              ]),
            ),
            if (!editable) StatusPill(percent == null ? '—' : '$percent%', tone: percent == null ? Tone.neutral : (percent >= 80 ? Tone.success : (percent >= 50 ? Tone.warning : Tone.danger))),
          ]),
          const SizedBox(height: 10),
          ClipRRect(
            borderRadius: BorderRadius.circular(6),
            child: LinearProgressIndicator(
              value: (percent ?? 0) / 100,
              minHeight: 8,
              color: _completionColor(percent),
              backgroundColor: context.hr.glow,
            ),
          ),
          const SizedBox(height: 12),
          if (editable && edit != null) ...[
            SizedBox(
              width: 200,
              child: TextFormField(
                controller: edit.percent,
                enabled: !_busy,
                keyboardType: TextInputType.number,
                inputFormatters: [FilteringTextInputFormatter.digitsOnly, LengthLimitingTextInputFormatter(3)],
                decoration: InputDecoration(labelText: s.t('نسبة الإنجاز %', 'Completion %'), suffixText: '%'),
                onChanged: (_) => setState(() {}),
                validator: (v) {
                  if ((v ?? '').trim().isEmpty) return null;
                  final n = int.tryParse(v!.trim());
                  return n == null || n < 0 || n > 100 ? '0–100' : null;
                },
              ),
            ),
            const SizedBox(height: 8),
            FieldGrid(children: [
              TextFormField(
                controller: edit.notes,
                enabled: !_busy,
                maxLines: 3,
                maxLength: 2000,
                decoration: InputDecoration(labelText: s.t('ملاحظات على الفريق ومديره', 'Notes on the team and its manager')),
              ),
              TextFormField(
                controller: edit.projects,
                enabled: !_busy,
                maxLines: 3,
                maxLength: 2000,
                decoration: InputDecoration(labelText: s.t('ملاحظات المشاريع', 'Project notes')),
              ),
            ]),
          ] else ...[
            InfoRow(s.t('ملاحظات', 'Notes'), t.notes ?? '—'),
            InfoRow(s.t('المشاريع', 'Projects'), t.projectNotes ?? '—'),
          ],
        ]),
      ),
    );
  }
}
