import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';

// Reviewer evaluations (decision 2026-11-11): the reviewer grades employees from weak to excellent on their own headings,
// with an optional reason per heading and a date they pick. No numbers and no average. The employee never sees them,
// and they go to the executive director one at a time or together.

String reviewerGradeLabel(S s, ReviewerGrade g) => switch (g) {
      ReviewerGrade.weak => s.t('ضعيف', 'Weak'),
      ReviewerGrade.acceptable => s.t('مقبول', 'Acceptable'),
      ReviewerGrade.average => s.t('متوسط', 'Average'),
      ReviewerGrade.good => s.t('جيد', 'Good'),
      ReviewerGrade.veryGood => s.t('جيد جدًا', 'Very good'),
      ReviewerGrade.excellent => s.t('ممتاز', 'Excellent'),
      ReviewerGrade.$unknown => '—',
    };

Tone reviewerGradeTone(ReviewerGrade g) => switch (g) {
      ReviewerGrade.weak => Tone.danger,
      ReviewerGrade.acceptable || ReviewerGrade.average => Tone.warning,
      ReviewerGrade.good || ReviewerGrade.veryGood || ReviewerGrade.excellent => Tone.success,
      ReviewerGrade.$unknown => Tone.neutral,
    };

/// The «تقييم جديد» form: a date, a grade and an optional reason per active heading, and notes. With [employee] the person is fixed
/// (opened from the employee file); otherwise the HR manager picks one. True when an evaluation was saved.
Future<bool> showReviewerEvaluationDialog(BuildContext context, WidgetRef ref, {EmployeeRefDto? employee}) async {
  final s = S.of(context);
  final api = ref.read(hrApiProvider).reviewerEvaluations;
  final List<ReviewerEvaluationCriterionDto> criteria;
  try {
    criteria = (await api.ReviewerEvaluationCriterion_GetList(isActive: true, maxResultCount: 200)).items;
  } catch (e) {
    if (context.mounted) showError(context, e);
    return false;
  }
  if (!context.mounted) return false;
  if (criteria.isEmpty) {
    showError(context, s.t('أضف عناوين التقييم أولاً من «عناوين تقييم المراجع».', 'Add evaluation headings first.'));
    return false;
  }

  var target = employee;
  final grades = <String, ReviewerGrade>{};
  final reasons = <String, TextEditingController>{for (final c in criteria) c.id: TextEditingController()};
  var date = DateTime.now();
  final f = Fmt(context);
  final notes = TextEditingController();
  final key = const Uuid().v4();
  return showFormDialog(
    context,
    width: 640,
    title: s.t('تقييم جديد', 'New evaluation'),
    submitLabel: s.t('حفظ التقييم', 'Save evaluation'),
    fields: (setState) => [
      PickerField(
        label: s.employee,
        value: target?.fullName,
        onTap: employee != null
            ? () {}
            : () async {
                // Every employee the reviewer may evaluate, not only department managers (decision 2026-11-11).
                final e = await showSearchPicker<EmployeeRefDto>(
                  context,
                  title: s.employee,
                  search: (q) async => (await api.ReviewerEvaluation_GetCandidates(search: q.isEmpty ? null : q, maxResultCount: 50)).items,
                  label: (e) => e.fullName,
                  subtitle: (e) => e.departmentName ?? '',
                );
                if (e != null) setState(() => target = e);
              },
      ),
      PickerField(
        label: s.t('تاريخ التقييم', 'Evaluation date'),
        value: f.date(date),
        icon: Icons.event,
        onTap: () async {
          final picked = await pickDate(context, initial: date, first: DateTime(2020), last: DateTime.now().add(const Duration(days: 366)));
          if (picked != null) setState(() => date = picked);
        },
      ),
      Text(s.t('التقدير لكل عنوان، والسبب اختياري', 'A grade for each heading; the reason is optional'), style: Theme.of(context).textTheme.labelLarge),
      for (final c in criteria) ...[
        DropdownButtonFormField<ReviewerGrade>(
          initialValue: grades[c.id],
          decoration: InputDecoration(labelText: c.name),
          items: [
            for (final g in ReviewerGrade.values.where((v) => v != ReviewerGrade.$unknown))
              DropdownMenuItem(value: g, child: Text(reviewerGradeLabel(s, g))),
          ],
          validator: (v) => v == null ? s.requiredField : null,
          onChanged: (v) => setState(() {
            if (v != null) grades[c.id] = v;
          }),
        ),
        TextFormField(
          controller: reasons[c.id],
          maxLines: 2,
          maxLength: 2000,
          decoration: InputDecoration(labelText: s.t('سبب التقدير (اختياري)', 'Reason (optional)')),
        ),
      ],
      TextFormField(
        controller: notes,
        maxLines: 5,
        maxLength: 4000,
        decoration: InputDecoration(labelText: s.t('الملاحظات', 'Notes'), alignLabelWithHint: true),
      ),
      Text(s.t('لا يرى الموظف هذا التقييم، ولا يُعدّل بعد الحفظ.', 'The employee does not see this evaluation, and it cannot be edited after saving.'),
          style: Theme.of(context).textTheme.labelSmall),
    ],
    onSubmit: () async {
      if (target == null) {
        showError(context, s.validation);
        return false;
      }
      return runAction(
        context,
        () => api.ReviewerEvaluation_Create(
          idempotencyKey: key,
          body: ReviewerEvaluationCreateDto(
            employeeId: target!.id,
            evaluationDate: date,
            items: [
              for (final c in criteria) ReviewerEvaluationItemInputDto(criterionId: c.id, grade: grades[c.id]!, reason: optText(reasons[c.id]!)),
            ],
            notes: optText(notes),
          ),
        ),
        done: s.saved,
      );
    },
  );
}

/// Opens one evaluation's details in a dialog.
Future<void> showReviewerEvaluationDetails(BuildContext context, ReviewerEvaluationDto e) => showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: PersonName.employee(e.employee, radius: 16),
        content: SizedBox(width: 520, child: SingleChildScrollView(child: ReviewerEvaluationView(evaluation: e))),
        actions: [TextButton(onPressed: () => Navigator.pop(ctx), child: Text(S.of(ctx).close))],
      ),
    );

/// «عناوين تقييم المراجع»: add, rename, reorder and switch off headings. Never deleted.
class ReviewerEvaluationCriteriaPage extends ConsumerStatefulWidget {
  const ReviewerEvaluationCriteriaPage({super.key});

  @override
  ConsumerState<ReviewerEvaluationCriteriaPage> createState() => _ReviewerEvaluationCriteriaPageState();
}

class _ReviewerEvaluationCriteriaPageState extends ConsumerState<ReviewerEvaluationCriteriaPage> {
  int _version = 0;

  Future<void> _edit([ReviewerEvaluationCriterionDto? c]) async {
    final s = S.of(context);
    final ar = TextEditingController(text: c?.nameAr);
    final en = TextEditingController(text: c?.nameEn);
    final order = TextEditingController(text: '${c?.sortOrder ?? 0}');
    var active = c?.isActive ?? true;
    final api = ref.read(hrApiProvider).reviewerEvaluations;
    final ok = await showFormDialog(
      context,
      title: c == null ? s.t('عنوان تقييم جديد', 'New evaluation heading') : s.t('تعديل العنوان', 'Edit heading'),
      fields: (setState) => [
        TextFormField(controller: ar, maxLength: 100, decoration: InputDecoration(labelText: s.t('الاسم بالعربي', 'Arabic name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, maxLength: 100, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزي', 'English name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(
          controller: order,
          keyboardType: TextInputType.number,
          decoration: InputDecoration(labelText: s.t('الترتيب', 'Order')),
          validator: (v) => int.tryParse(v ?? '') == null || int.parse(v!) < 0 ? s.validation : null,
        ),
        if (c != null)
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(s.t('فعّال', 'Active')),
            value: active,
            onChanged: (v) => setState(() => active = v),
          ),
      ],
      onSubmit: () => runAction(
        context,
        () => c == null
            ? api.ReviewerEvaluationCriterion_Create(body: ReviewerEvaluationCriterionCreateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: int.parse(order.text)))
            : api.ReviewerEvaluationCriterion_Update(
                id: c.id,
                body: ReviewerEvaluationCriterionUpdateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: int.parse(order.text), isActive: active, concurrencyStamp: c.concurrencyStamp),
              ),
        done: s.saved,
      ),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final api = ref.watch(hrApiProvider).reviewerEvaluations;
    return HrPage(
      title: s.t('عناوين تقييم المراجع', 'Reviewer evaluation headings'),
      actions: [FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('عنوان جديد', 'New heading')))],
      child: PagedList<ReviewerEvaluationCriterionDto>(
        key: ValueKey(_version),
        pageSize: 200,
        emptyMessage: s.t('لا توجد عناوين بعد. أضف مثل «السلوك المهني» أو «الالتزام بالدوام».', 'No headings yet. Add e.g. "Professional conduct".'),
        fetch: (skip, max) async {
          final r = await api.ReviewerEvaluationCriterion_GetList(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, c) => OpsTile(
          leading: Icons.checklist,
          title: c.name,
          subtitle: '${c.nameAr} · ${c.nameEn} · ${s.t('الترتيب', 'Order')} ${c.sortOrder}',
          trailing: StatusPill(c.isActive ? s.t('فعّال', 'Active') : s.t('معطّل', 'Inactive'), tone: c.isActive ? Tone.success : Tone.neutral),
          onTap: () => _edit(c),
        ),
      ),
    );
  }
}

/// «تقييم الموظفين»: evaluate, list, and send the unsent ones to the executive director.
class ReviewerEvaluationsPage extends ConsumerStatefulWidget {
  const ReviewerEvaluationsPage({super.key});

  @override
  ConsumerState<ReviewerEvaluationsPage> createState() => _ReviewerEvaluationsPageState();
}

class _ReviewerEvaluationsPageState extends ConsumerState<ReviewerEvaluationsPage> {
  int _version = 0;
  bool _unsentOnly = false;

  Future<void> _evaluate() async {
    if (await showReviewerEvaluationDialog(context, ref)) setState(() => _version++);
  }

  Future<void> _send() async {
    final s = S.of(context);
    final note = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('إرسال تقييمات الأسبوع', "Send this week's evaluations"),
      submitLabel: s.t('إرسال', 'Send'),
      fields: (setState) => [
        Text(s.t('يُرسل كل تقييم لم يُرسل بعد إلى المدير التنفيذي في تقرير واحد.', 'Every unsent evaluation goes to the executive director in one report.')),
        TextFormField(controller: note, maxLines: 3, maxLength: 2000, decoration: InputDecoration(labelText: s.t('ملاحظة (اختياري)', 'Note (optional)'))),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).reviewerEvaluations.ReviewerEvaluationReport_Send(idempotencyKey: key, body: ReviewerEvaluationReportCreateDto(note: optText(note))),
        done: s.t('أُرسل التقرير للمدير التنفيذي', 'Report sent to the executive director'),
      ),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).reviewerEvaluations;
    return HrPage(
      title: s.t('تقييمات المراجع', 'Reviewer evaluations'),
      actions: [
        OutlinedButton.icon(onPressed: _send, icon: const Icon(Icons.forward_to_inbox_outlined), label: Text(s.t('إرسال للمدير التنفيذي', 'Send to executive'))),
        const SizedBox(width: 8),
        FilledButton.icon(onPressed: _evaluate, icon: const Icon(Icons.add), label: Text(s.t('تقييم جديد', 'New evaluation'))),
      ],
      child: PagedList<ReviewerEvaluationDto>(
        key: ValueKey('$_version|$_unsentOnly'),
        emptyMessage: s.t('لا توجد تقييمات.', 'No evaluations.'),
        header: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Wrap(spacing: 8, children: [
            FilterChip(
              label: Text(s.t('غير المرسلة', 'Not sent')),
              selected: _unsentOnly,
              onSelected: (v) => setState(() => _unsentOnly = v),
            ),
          ]),
        ),
        fetch: (skip, max) async {
          final r = await api.ReviewerEvaluation_GetList(isSent: _unsentOnly ? false : null, skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, e) => OpsTile(
          leading: Icons.grading_outlined,
          avatar: PersonAvatar.employee(e.employee),
          title: e.employee.fullName,
          subtitle: '${e.employee.departmentName ?? ''} · ${s.t('تاريخ التقييم', 'Evaluation date')} ${f.date(e.evaluationDate)}',
          trailing: _SendButton(evaluation: e, onSent: () => setState(() => _version++)),
          onTap: () => showReviewerEvaluationDetails(context, e),
        ),
      ),
    );
  }
}

/// «إرسال» on each evaluation (decision 2026-11-11): asks for confirmation with the evaluation itself in view, then
/// sends that one alone to the executive director (ReviewerEvaluation_Send). A sent evaluation shows «تم الإرسال», disabled.
class _SendButton extends ConsumerStatefulWidget {
  const _SendButton({required this.evaluation, required this.onSent});
  final ReviewerEvaluationDto evaluation;
  final VoidCallback onSent;

  @override
  ConsumerState<_SendButton> createState() => _SendButtonState();
}

class _SendButtonState extends ConsumerState<_SendButton> {
  bool _busy = false;
  bool _sent = false;

  Future<void> _confirmAndSend() async {
    final s = S.of(context);
    final e = widget.evaluation;
    final yes = await showHrDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(s.t('إرسال التقييم', 'Send the evaluation')),
        content: SizedBox(
          width: 520,
          child: SingleChildScrollView(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, mainAxisSize: MainAxisSize.min, children: [
              Text(
                s.t('هل أنت متأكد من إرسال هذا التقييم إلى المدير التنفيذي؟', 'Are you sure you want to send this evaluation to the executive director?'),
                style: Theme.of(ctx).textTheme.titleSmall,
              ),
              const SizedBox(height: 12),
              PersonName.employee(e.employee, radius: 16, style: const TextStyle(fontWeight: FontWeight.w600)),
              const SizedBox(height: 8),
              ReviewerEvaluationView(evaluation: e),
            ]),
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(s.t('لا', 'No'))),
          FilledButton.icon(
            onPressed: () => Navigator.pop(ctx, true),
            icon: const Icon(Icons.send_outlined),
            label: Text(s.t('نعم، أرسل', 'Yes, send')),
          ),
        ],
      ),
    );
    if (yes != true || !mounted) return;

    setState(() => _busy = true);
    final key = const Uuid().v4();
    final ok = await runAction(
      context,
      () => ref.read(hrApiProvider).reviewerEvaluations.ReviewerEvaluation_Send(id: e.id, idempotencyKey: key),
      done: s.t('أُرسل التقييم إلى المدير التنفيذي', 'Evaluation sent to the executive director'),
    );
    if (!mounted) return;
    setState(() {
      _busy = false;
      _sent = ok;
    });
    if (ok) widget.onSent();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    if (widget.evaluation.reportId != null || _sent) {
      return FilledButton.tonalIcon(
        onPressed: null,
        icon: const Icon(Icons.check_circle_outline, size: 18),
        label: Text(s.t('تم الإرسال', 'Sent')),
      );
    }
    return FilledButton.icon(
      onPressed: _busy ? null : _confirmAndSend,
      icon: _busy
          ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
          : const Icon(Icons.send_outlined, size: 18),
      label: Text(s.t('إرسال', 'Send')),
    );
  }
}

/// The grades, reasons and notes of one evaluation (reviewer and executive director). No numbers.
class ReviewerEvaluationView extends StatelessWidget {
  const ReviewerEvaluationView({super.key, required this.evaluation});
  final ReviewerEvaluationDto evaluation;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final e = evaluation;
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, mainAxisSize: MainAxisSize.min, children: [
      Text('${s.t('تاريخ التقييم', 'Evaluation date')} ${f.date(e.evaluationDate)} · ${e.evaluatedBy.displayName}',
          style: TextStyle(color: context.hr.textSecondary)),
      const Divider(height: 20),
      for (final x in e.items)
        Padding(
          padding: const EdgeInsets.symmetric(vertical: 6),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Expanded(child: Text(x.criterionName, style: const TextStyle(fontWeight: FontWeight.w600))),
              StatusPill(reviewerGradeLabel(s, x.grade), tone: reviewerGradeTone(x.grade)),
            ]),
            if (x.reason != null) ...[
              const SizedBox(height: 4),
              SelectableText(x.reason!, style: TextStyle(color: context.hr.textSecondary)),
            ],
          ]),
        ),
      if (e.notes != null) ...[
        const SizedBox(height: 8),
        Text(s.t('الملاحظات', 'Notes'), style: Theme.of(context).textTheme.labelLarge),
        const SizedBox(height: 4),
        SelectableText(e.notes!),
      ],
    ]);
  }
}

/// «تقارير تقييم المراجع»: the executive director reads every report; the HR manager reads the ones they sent.
class ReviewerEvaluationReportsPage extends ConsumerWidget {
  const ReviewerEvaluationReportsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).reviewerEvaluations;
    return HrPage(
      title: s.t('تقارير تقييم المراجع', 'Reviewer evaluation reports'),
      child: PagedList<ReviewerEvaluationReportSummaryDto>(
        emptyMessage: s.t('لا توجد تقارير.', 'No reports.'),
        fetch: (skip, max) async {
          final r = await api.ReviewerEvaluationReport_GetList(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, r) => OpsTile(
          leading: Icons.summarize_outlined,
          avatar: PersonAvatar.user(r.sentBy),
          title: '${r.sentBy.displayName} · ${f.dateTime(r.sentAt)}',
          subtitle: [
            '${r.evaluationsCount} ${s.t('تقييم', 'evaluations')}',
            '${f.date(r.firstEvaluationDate)} → ${f.date(r.lastEvaluationDate)}',
            if (r.note != null) r.note!,
          ].join(' · '),
          onTap: () => context.go('/reviewer-evaluation-reports/${r.id}'),
        ),
      ),
    );
  }
}

final reviewerEvaluationReportProvider =
    FutureProvider.autoDispose.family<ReviewerEvaluationReportDto, String>((ref, id) => ref.watch(hrApiProvider).reviewerEvaluations.ReviewerEvaluationReport_Get(id: id));

class ReviewerEvaluationReportDetailPage extends ConsumerWidget {
  const ReviewerEvaluationReportDetailPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrPage(
      title: s.t('تقرير تقييم المراجع', 'Reviewer evaluation report'),
      child: AsyncBody<ReviewerEvaluationReportDto>(
        value: ref.watch(reviewerEvaluationReportProvider(id)),
        onRetry: () => ref.invalidate(reviewerEvaluationReportProvider(id)),
        builder: (r) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle('${r.sentBy.displayName} · ${f.dateTime(r.sentAt)}'),
              InfoRow(s.t('عدد التقييمات', 'Evaluations'), '${r.evaluationsCount}'),
              InfoRow(s.t('الفترة', 'Period'), '${f.date(r.firstEvaluationDate)} → ${f.date(r.lastEvaluationDate)}'),
              if (r.note != null) InfoRow(s.note, r.note!),
            ]),
          ),
          for (final e in r.evaluations) ...[
            const SizedBox(height: 12),
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(e.employee.fullName),
                if (e.employee.departmentName != null) Text(e.employee.departmentName!, style: TextStyle(color: context.hr.textSecondary)),
                const SizedBox(height: 8),
                ReviewerEvaluationView(evaluation: e),
              ]),
            ),
          ],
        ]),
      ),
    );
  }
}

