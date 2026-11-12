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
import '../time/time_shared.dart' show pickEmployee;

// HR evaluations (decision 2026-11-10): the HR manager scores employees 1-10 on its own headings, the employee never
// sees them, and the unsent ones go to the executive director as a report.

String _avg(double v) => v % 1 == 0 ? v.toInt().toString() : v.toStringAsFixed(2);

/// The «تقييم جديد» form: a 1-10 score per active heading and notes. With [employee] the person is fixed
/// (opened from the employee file); otherwise the HR manager picks one. True when an evaluation was saved.
Future<bool> showHrEvaluationDialog(BuildContext context, WidgetRef ref, {EmployeeRefDto? employee}) async {
  final s = S.of(context);
  final api = ref.read(hrApiProvider).hrEvaluations;
  final List<HrEvaluationCriterionDto> criteria;
  try {
    criteria = (await api.HrEvaluationCriterion_GetList(isActive: true, maxResultCount: 200)).items;
  } catch (e) {
    if (context.mounted) showError(context, e);
    return false;
  }
  if (!context.mounted) return false;
  if (criteria.isEmpty) {
    showError(context, s.t('أضف عناوين التقييم أولاً من «عناوين تقييم الموارد».', 'Add evaluation headings first.'));
    return false;
  }

  var target = employee;
  final scores = <String, int>{};
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
                final e = await pickEmployee(context, ref);
                if (e != null) setState(() => target = e);
              },
      ),
      Text(s.t('الدرجة من 1 إلى 10 لكل عنوان', 'Score 1 to 10 for each heading'), style: Theme.of(context).textTheme.labelLarge),
      for (final c in criteria)
        DropdownButtonFormField<int>(
          initialValue: scores[c.id],
          decoration: InputDecoration(labelText: c.name),
          items: [for (var i = 1; i <= 10; i++) DropdownMenuItem(value: i, child: Text('$i'))],
          validator: (v) => v == null ? s.requiredField : null,
          onChanged: (v) => setState(() {
            if (v != null) scores[c.id] = v;
          }),
        ),
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
        () => api.HrEvaluation_Create(
          idempotencyKey: key,
          body: HrEvaluationCreateDto(
            employeeId: target!.id,
            scores: [for (final c in criteria) HrEvaluationScoreInputDto(criterionId: c.id, score: scores[c.id]!)],
            notes: optText(notes),
          ),
        ),
        done: s.saved,
      );
    },
  );
}

/// Opens one evaluation's details in a dialog.
Future<void> showHrEvaluationDetails(BuildContext context, HrEvaluationDto e) => showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: PersonName.employee(e.employee, radius: 16),
        content: SizedBox(width: 520, child: SingleChildScrollView(child: HrEvaluationView(evaluation: e))),
        actions: [TextButton(onPressed: () => Navigator.pop(ctx), child: Text(S.of(ctx).close))],
      ),
    );

/// «عناوين تقييم الموارد»: add, rename, reorder and switch off headings. Never deleted.
class HrEvaluationCriteriaPage extends ConsumerStatefulWidget {
  const HrEvaluationCriteriaPage({super.key});

  @override
  ConsumerState<HrEvaluationCriteriaPage> createState() => _HrEvaluationCriteriaPageState();
}

class _HrEvaluationCriteriaPageState extends ConsumerState<HrEvaluationCriteriaPage> {
  int _version = 0;

  Future<void> _edit([HrEvaluationCriterionDto? c]) async {
    final s = S.of(context);
    final ar = TextEditingController(text: c?.nameAr);
    final en = TextEditingController(text: c?.nameEn);
    final order = TextEditingController(text: '${c?.sortOrder ?? 0}');
    var active = c?.isActive ?? true;
    final api = ref.read(hrApiProvider).hrEvaluations;
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
            ? api.HrEvaluationCriterion_Create(body: HrEvaluationCriterionCreateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: int.parse(order.text)))
            : api.HrEvaluationCriterion_Update(
                id: c.id,
                body: HrEvaluationCriterionUpdateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: int.parse(order.text), isActive: active, concurrencyStamp: c.concurrencyStamp),
              ),
        done: s.saved,
      ),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final api = ref.watch(hrApiProvider).hrEvaluations;
    return HrPage(
      title: s.t('عناوين تقييم الموارد', 'HR evaluation headings'),
      actions: [FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('عنوان جديد', 'New heading')))],
      child: PagedList<HrEvaluationCriterionDto>(
        key: ValueKey(_version),
        pageSize: 200,
        emptyMessage: s.t('لا توجد عناوين بعد. أضف مثل «السلوك المهني» أو «الالتزام بالدوام».', 'No headings yet. Add e.g. "Professional conduct".'),
        fetch: (skip, max) async {
          final r = await api.HrEvaluationCriterion_GetList(skipCount: skip, maxResultCount: max);
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
class HrEvaluationsPage extends ConsumerStatefulWidget {
  const HrEvaluationsPage({super.key});

  @override
  ConsumerState<HrEvaluationsPage> createState() => _HrEvaluationsPageState();
}

class _HrEvaluationsPageState extends ConsumerState<HrEvaluationsPage> {
  int _version = 0;
  bool _unsentOnly = false;

  Future<void> _evaluate() async {
    if (await showHrEvaluationDialog(context, ref)) setState(() => _version++);
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
        () => ref.read(hrApiProvider).hrEvaluations.HrEvaluationReport_Send(idempotencyKey: key, body: HrEvaluationReportCreateDto(note: optText(note))),
        done: s.t('أُرسل التقرير للمدير التنفيذي', 'Report sent to the executive director'),
      ),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).hrEvaluations;
    return HrPage(
      title: s.t('تقييم الموظفين', 'Employee evaluations'),
      actions: [
        OutlinedButton.icon(onPressed: _send, icon: const Icon(Icons.forward_to_inbox_outlined), label: Text(s.t('إرسال للمدير التنفيذي', 'Send to executive'))),
        const SizedBox(width: 8),
        FilledButton.icon(onPressed: _evaluate, icon: const Icon(Icons.add), label: Text(s.t('تقييم جديد', 'New evaluation'))),
      ],
      child: PagedList<HrEvaluationDto>(
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
          final r = await api.HrEvaluation_GetList(isSent: _unsentOnly ? false : null, skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, e) => OpsTile(
          leading: Icons.grading_outlined,
          avatar: PersonAvatar.employee(e.employee),
          title: e.employee.fullName,
          subtitle: '${e.employee.departmentName ?? ''} · ${f.dateTime(e.evaluatedAt)} · ${s.t('المعدل', 'Average')} ${_avg(e.averageScore)}/10',
          trailing: _SendButton(evaluation: e, onSent: () => setState(() => _version++)),
          onTap: () => showHrEvaluationDetails(context, e),
        ),
      ),
    );
  }
}

/// «إرسال» on each evaluation (decision 2026-11-11): asks for confirmation with the evaluation itself in view, then
/// sends that one alone to the executive director (HrEvaluation_Send). A sent evaluation shows «تم الإرسال», disabled.
class _SendButton extends ConsumerStatefulWidget {
  const _SendButton({required this.evaluation, required this.onSent});
  final HrEvaluationDto evaluation;
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
              HrEvaluationView(evaluation: e),
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
      () => ref.read(hrApiProvider).hrEvaluations.HrEvaluation_Send(id: e.id, idempotencyKey: key),
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

/// The scores, average and notes of one evaluation (HR manager and executive director).
class HrEvaluationView extends StatelessWidget {
  const HrEvaluationView({super.key, required this.evaluation});
  final HrEvaluationDto evaluation;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final e = evaluation;
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, mainAxisSize: MainAxisSize.min, children: [
      Text('${e.evaluatedBy.displayName} · ${f.dateTime(e.evaluatedAt)}', style: TextStyle(color: context.hr.textSecondary)),
      const Divider(height: 20),
      for (final x in e.scores) InfoRow(x.criterionName, '${x.score}/10'),
      InfoRow(s.t('المعدل', 'Average'), '${_avg(e.averageScore)}/10'),
      if (e.notes != null) ...[
        const SizedBox(height: 8),
        Text(s.t('الملاحظات', 'Notes'), style: Theme.of(context).textTheme.labelLarge),
        const SizedBox(height: 4),
        SelectableText(e.notes!),
      ],
    ]);
  }
}

/// «تقارير تقييم الموارد»: the executive director reads every report; the HR manager reads the ones they sent.
class HrEvaluationReportsPage extends ConsumerWidget {
  const HrEvaluationReportsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).hrEvaluations;
    return HrPage(
      title: s.t('تقارير تقييم الموارد', 'HR evaluation reports'),
      child: PagedList<HrEvaluationReportSummaryDto>(
        emptyMessage: s.t('لا توجد تقارير.', 'No reports.'),
        fetch: (skip, max) async {
          final r = await api.HrEvaluationReport_GetList(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, r) => OpsTile(
          leading: Icons.summarize_outlined,
          avatar: PersonAvatar.user(r.sentBy),
          title: '${r.sentBy.displayName} · ${f.dateTime(r.sentAt)}',
          subtitle: [
            '${r.evaluationsCount} ${s.t('تقييم', 'evaluations')}',
            '${f.date(r.firstEvaluatedAt)} → ${f.date(r.lastEvaluatedAt)}',
            if (r.note != null) r.note!,
          ].join(' · '),
          onTap: () => context.go('/hr-evaluation-reports/${r.id}'),
        ),
      ),
    );
  }
}

final hrEvaluationReportProvider =
    FutureProvider.autoDispose.family<HrEvaluationReportDto, String>((ref, id) => ref.watch(hrApiProvider).hrEvaluations.HrEvaluationReport_Get(id: id));

class HrEvaluationReportDetailPage extends ConsumerWidget {
  const HrEvaluationReportDetailPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrPage(
      title: s.t('تقرير تقييم الموارد', 'HR evaluation report'),
      child: AsyncBody<HrEvaluationReportDto>(
        value: ref.watch(hrEvaluationReportProvider(id)),
        onRetry: () => ref.invalidate(hrEvaluationReportProvider(id)),
        builder: (r) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle('${r.sentBy.displayName} · ${f.dateTime(r.sentAt)}'),
              InfoRow(s.t('عدد التقييمات', 'Evaluations'), '${r.evaluationsCount}'),
              InfoRow(s.t('الفترة', 'Period'), '${f.date(r.firstEvaluatedAt)} → ${f.date(r.lastEvaluatedAt)}'),
              if (r.note != null) InfoRow(s.note, r.note!),
            ]),
          ),
          for (final e in r.evaluations) ...[
            const SizedBox(height: 12),
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(e.employee.fullName, trailing: StatusPill('${_avg(e.averageScore)}/10', tone: Tone.info)),
                if (e.employee.departmentName != null) Text(e.employee.departmentName!, style: TextStyle(color: context.hr.textSecondary)),
                const SizedBox(height: 8),
                HrEvaluationView(evaluation: e),
              ]),
            ),
          ],
        ]),
      ),
    );
  }
}

/// «تنبيهات التقييم» (decision 2026-11-11): at the end of each cycle the server alerts the HR manager about every
/// employee whose average on a heading was the threshold or lower. Read only; the HR manager acts from the other pages.
class HrEvaluationAlertsPage extends ConsumerWidget {
  const HrEvaluationAlertsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).hrEvaluations;
    return HrPage(
      title: s.t('تنبيهات التقييم', 'Evaluation alerts'),
      subtitle: s.t('موظفون معدلهم في عنوان أقل من المطلوب خلال الدورة.', 'Employees whose cycle average on a heading is below the required level.'),
      child: PagedList<HrEvaluationAlertDto>(
        emptyMessage: s.t('لا توجد تنبيهات.', 'No alerts.'),
        fetch: (skip, max) async {
          final r = await api.HrEvaluationAlert_GetList(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, a) => OpsTile(
          leading: Icons.trending_down,
          avatar: PersonAvatar.employee(a.employee),
          title: '${a.employee.fullName} · ${a.criterionName}',
          subtitle: [
            '${s.t('المعدل', 'Average')} ${_avg(a.averageScore)}/10',
            '${a.evaluationsCount} ${s.t('تقييم', 'evaluations')}',
            '${f.date(a.periodFrom)} → ${f.date(a.periodTo)}',
          ].join(' · '),
          onTap: () => context.go('/hr-evaluation-alerts/${a.id}'),
        ),
      ),
    );
  }
}

final hrEvaluationAlertProvider =
    FutureProvider.autoDispose.family<HrEvaluationAlertDto, String>((ref, id) => ref.watch(hrApiProvider).hrEvaluations.HrEvaluationAlert_Get(id: id));

/// Opened from the hrEvaluationLowAverage notification: who, which heading, the average and the cycle.
class HrEvaluationAlertDetailPage extends ConsumerWidget {
  const HrEvaluationAlertDetailPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrPage(
      title: s.t('تنبيه تقييم', 'Evaluation alert'),
      child: AsyncBody<HrEvaluationAlertDto>(
        value: ref.watch(hrEvaluationAlertProvider(id)),
        onRetry: () => ref.invalidate(hrEvaluationAlertProvider(id)),
        builder: (a) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(a.employee.fullName, trailing: StatusPill('${_avg(a.averageScore)}/10', tone: Tone.danger)),
              if (a.employee.departmentName != null) Text(a.employee.departmentName!, style: TextStyle(color: context.hr.textSecondary)),
              const SizedBox(height: 8),
              Text(
                s.t('لديه تقييم أقل من المطلوب في «${a.criterionName}».', 'Has an evaluation below the required level in "${a.criterionName}".'),
                style: Theme.of(context).textTheme.titleSmall,
              ),
              const Divider(height: 20),
              InfoRow(s.t('العنوان', 'Heading'), a.criterionName),
              InfoRow(s.t('المعدل', 'Average'), '${_avg(a.averageScore)}/10'),
              InfoRow(s.t('الحد المطلوب', 'Alert level'), '${_avg(a.threshold)} ${s.t('أو أقل', 'or lower')}'),
              InfoRow(s.t('عدد التقييمات', 'Evaluations'), '${a.evaluationsCount}'),
              InfoRow(s.t('الفترة', 'Period'), '${f.date(a.periodFrom)} → ${f.date(a.periodTo)}'),
              const SizedBox(height: 12),
              Align(
                alignment: AlignmentDirectional.centerStart,
                child: OutlinedButton.icon(
                  icon: const Icon(Icons.badge_outlined),
                  label: Text(s.t('فتح ملف الموظف', 'Open the employee file')),
                  onPressed: () => context.go('/employees/${a.employee.id}'),
                ),
              ),
            ]),
          ),
        ]),
      ),
    );
  }
}
