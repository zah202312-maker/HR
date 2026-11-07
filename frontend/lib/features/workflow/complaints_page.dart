import 'package:dio/dio.dart' show MultipartFile;
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/download.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';

String complaintLevelLabel(S s, ComplaintLevel l) => switch (l) {
      ComplaintLevel.normal => s.t('عادية', 'Normal'),
      ComplaintLevel.confidential => s.t('سرية', 'Confidential'),
      ComplaintLevel.highlyConfidential => s.t('سرية للغاية', 'Highly confidential'),
      ComplaintLevel.$unknown => '—',
    };

String complaintStatusLabel(S s, ComplaintStatus st) => switch (st) {
      ComplaintStatus.pendingDepartmentManager => s.t('عند مدير القسم', 'With department manager'),
      ComplaintStatus.pendingHrManager => s.t('عند مدير الموارد البشرية', 'With HR manager'),
      ComplaintStatus.pendingExecutiveDirector => s.t('عند المدير التنفيذي', 'With executive director'),
      ComplaintStatus.closed => s.t('مغلقة', 'Closed'),
      ComplaintStatus.$unknown => '—',
    };

String handlerRoleLabel(S s, ComplaintHandlerRole r) => switch (r) {
      ComplaintHandlerRole.departmentManager => s.role(AppRole.departmentManager),
      ComplaintHandlerRole.hrManager => s.role(AppRole.hrManager),
      ComplaintHandlerRole.executiveDirector => s.role(AppRole.executiveDirector),
      ComplaintHandlerRole.$unknown => '—',
    };

String actionTypeLabel(S s, ComplaintActionType a) => switch (a) {
      ComplaintActionType.note => s.t('ملاحظة', 'Note'),
      ComplaintActionType.meeting => s.t('اجتماع', 'Meeting'),
      ComplaintActionType.investigation => s.t('تحقيق', 'Investigation'),
      ComplaintActionType.followUp => s.t('متابعة', 'Follow-up'),
      ComplaintActionType.submitted => s.t('تقديم', 'Submitted'),
      ComplaintActionType.levelRaised => s.t('رفع المستوى', 'Level raised'),
      ComplaintActionType.forwarded => s.t('تحويل', 'Forwarded'),
      ComplaintActionType.closed => s.t('إغلاق', 'Closed'),
      ComplaintActionType.$unknown => '—',
    };

Tone levelTone(ComplaintLevel l) => switch (l) {
      ComplaintLevel.normal => Tone.info,
      ComplaintLevel.confidential => Tone.warning,
      _ => Tone.danger,
    };

/// Complaint handler box for any approved assignee: probed, since an assignment can
/// grant access whatever the role (x-complaint-handler-assignee).
final handlerBoxVisibleProvider = FutureProvider.autoDispose<bool>((ref) async {
  final session = ref.watch(sessionProvider);
  if (session.can(HrOps.Complaint_GetAssignedList)) return true;
  try {
    final r = await ref.watch(hrApiProvider).complaints.Complaint_GetAssignedList(maxResultCount: 1);
    return r.totalCount > 0;
  } catch (_) {
    return false;
  }
});

/// Complaints: what I submitted (Complaint_GetMyList) and, for handlers, the handling box.
/// Lists never reveal complaints made against the viewer.
class ComplaintsPage extends ConsumerWidget {
  const ComplaintsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).complaints;
    final handler = ref.watch(handlerBoxVisibleProvider).valueOrNull ?? false;
    final tabs = <(String, Widget)>[
      if (session.can(HrOps.Complaint_GetMyList))
        (
          s.t('شكاواي', 'My complaints'),
          PagedList<ComplaintComplainantSummaryDto>(
            emptyMessage: s.t('لم تقدّم أي شكوى', 'You have not submitted any complaint'),
            fetch: (skip, max) async {
              final r = await api.Complaint_GetMyList(skipCount: skip, maxResultCount: max, sorting: 'submittedAt desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => HrCard(
              onTap: () => context.go('/complaints/my/${c.id}'),
              child: Row(children: [
                Icon(Icons.shield_outlined, color: context.hr.accent),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(c.subject, style: const TextStyle(fontWeight: FontWeight.w600)),
                    Text('${c.referenceNumber} · ${f.dateTime(c.submittedAt)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                  ]),
                ),
                StatusPill(complaintLevelLabel(s, c.level), tone: levelTone(c.level)),
                const SizedBox(width: 6),
                StatusPill(complaintStatusLabel(s, c.status), tone: c.status == ComplaintStatus.closed ? Tone.neutral : Tone.warning),
              ]),
            ),
          )
        ),
      if (handler)
        (
          s.t('صندوق المعالجة', 'Handling box'),
          PagedList<ComplaintHandlerSummaryDto>(
            emptyMessage: s.nothingPending,
            fetch: (skip, max) async {
              final r = await api.Complaint_GetAssignedList(skipCount: skip, maxResultCount: max, sorting: 'submittedAt desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => HrCard(
              onTap: () => context.go('/complaints/${c.id}'),
              child: Row(children: [
                Icon(Icons.gavel_outlined, color: context.hr.accent),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(c.referenceNumber, style: const TextStyle(fontWeight: FontWeight.w600)),
                    Text([f.dateTime(c.submittedAt), if (c.dueAt != null) '${s.t('المهلة', 'Due')}: ${f.dateTime(c.dueAt)}'].join(' · '),
                        style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                  ]),
                ),
                if (c.isLate) ...[StatusPill(s.t('متأخرة', 'Late'), tone: Tone.danger), const SizedBox(width: 6)],
                StatusPill(complaintLevelLabel(s, c.level), tone: levelTone(c.level)),
                const SizedBox(width: 6),
                StatusPill(complaintStatusLabel(s, c.status), tone: c.status == ComplaintStatus.closed ? Tone.neutral : Tone.warning),
              ]),
            ),
          )
        ),
    ];
    return HrPage(
      title: s.t('الشكاوى', 'Complaints'),
      actions: [
        if (session.can(HrOps.Complaint_Submit))
          FilledButton.icon(onPressed: () => context.go('/complaints/new'), icon: const Icon(Icons.add), label: Text(s.t('شكوى جديدة', 'New complaint'))),
      ],
      child: tabs.isEmpty
          ? EmptyState(message: s.empty)
          : tabs.length == 1
              ? tabs.first.$2
              : DefaultTabController(
                  length: tabs.length,
                  child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                    TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [for (final t in tabs) Tab(text: t.$1)]),
                    const SizedBox(height: 12),
                    Expanded(child: TabBarView(children: [for (final t in tabs) t.$2])),
                  ]),
                ),
    );
  }
}

/// Submit: target, routing preview (minimum level and route by role), level, subject, content.
class NewComplaintPage extends ConsumerStatefulWidget {
  const NewComplaintPage({super.key});

  @override
  ConsumerState<NewComplaintPage> createState() => _NewComplaintPageState();
}

class _NewComplaintPageState extends ConsumerState<NewComplaintPage> {
  final _form = GlobalKey<FormState>();
  final _subject = TextEditingController();
  final _content = TextEditingController();
  ComplaintTargetKind _target = ComplaintTargetKind.general;
  EmployeeRefDto? _respondent;
  ComplaintRoutingPreviewDto? _preview;
  ComplaintLevel? _level;
  bool _busy = false;
  final String _key = const Uuid().v4();

  @override
  void initState() {
    super.initState();
    _loadPreview();
  }

  @override
  void dispose() {
    _subject.dispose();
    _content.dispose();
    super.dispose();
  }

  Future<void> _loadPreview() async {
    if (_target == ComplaintTargetKind.employee && _respondent == null) {
      setState(() => _preview = null);
      return;
    }
    try {
      final p = await ref.read(hrApiProvider).complaints.Complaint_PreviewRouting(
          body: ComplaintRoutingPreviewInputDto(targetKind: _target, respondentId: _target == ComplaintTargetKind.employee ? _respondent?.id : null));
      if (!mounted) return;
      setState(() {
        _preview = p;
        if (_level == null || !p.selectableLevels.contains(_level)) _level = p.minimumLevel;
      });
    } catch (e) {
      if (mounted) showError(context, e);
    }
  }

  Future<void> _submit() async {
    final s = S.of(context);
    if (!_form.currentState!.validate() || _level == null) return;
    if (_target == ComplaintTargetKind.employee && _respondent == null) {
      showError(context, s.validation);
      return;
    }
    setState(() => _busy = true);
    try {
      final r = await ref.read(hrApiProvider).complaints.Complaint_Submit(
            idempotencyKey: _key,
            body: ComplaintCreateDto(
              targetKind: _target,
              respondentId: _target == ComplaintTargetKind.employee ? _respondent!.id : null,
              level: _level!,
              subject: _subject.text.trim(),
              content: _content.text.trim(),
            ),
          );
      if (!mounted) return;
      showDone(
          context,
          r.levelRaisedBySystem
              ? s.t('قُدّمت الشكوى ${r.referenceNumber}، ورُفع مستواها حسب هوية المشتكى عليه', 'Complaint ${r.referenceNumber} submitted; its level was raised for the respondent')
              : s.t('قُدّمت الشكوى ${r.referenceNumber}', 'Complaint ${r.referenceNumber} submitted'));
      context.go('/complaints/my/${r.id}');
    } catch (e) {
      if (mounted) showError(context, e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final p = _preview;
    return HrPage(
      title: s.t('شكوى جديدة', 'New complaint'),
      child: SingleChildScrollView(
        padding: const EdgeInsets.only(bottom: 24),
        child: Align(
          alignment: AlignmentDirectional.topStart,
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 720),
            child: HrCard(
              child: Form(
                key: _form,
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  Text(
                    s.t('الشكوى باسمك ويظهر اسمك للمعالج. لا يراها المشتكى عليه، ولا يطّلع مسؤول النظام على محتواها.',
                        'The complaint carries your name, visible to the handler. The respondent never sees it, and the system admin cannot read it.'),
                    style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary),
                  ),
                  const SizedBox(height: 12),
                  SegmentedButton<ComplaintTargetKind>(
                    segments: [
                      ButtonSegment(value: ComplaintTargetKind.general, label: Text(s.t('عامة', 'General'))),
                      ButtonSegment(value: ComplaintTargetKind.employee, label: Text(s.t('ضد شخص', 'Against a person'))),
                    ],
                    selected: {_target},
                    onSelectionChanged: (v) {
                      setState(() => _target = v.first);
                      _loadPreview();
                    },
                  ),
                  if (_target == ComplaintTargetKind.employee) ...[
                    const SizedBox(height: 12),
                    PickerField(
                      label: s.t('المشتكى عليه', 'Respondent'),
                      value: _respondent?.fullName,
                      onTap: () async {
                        final e = await showSearchPicker<EmployeeRefDto>(
                          context,
                          title: s.t('المشتكى عليه', 'Respondent'),
                          search: (q) async => (await ref.read(hrApiProvider).complaints.Complaint_GetRespondents(filter: q.isEmpty ? null : q, maxResultCount: 50)).items,
                          label: (e) => e.fullName,
                          subtitle: (e) => e.departmentName ?? '',
                        );
                        if (e != null) {
                          setState(() => _respondent = e);
                          _loadPreview();
                        }
                      },
                    ),
                  ],
                  if (p != null) ...[
                    const SizedBox(height: 12),
                    Text(s.t('المستوى', 'Level'), style: TextStyle(color: context.hr.textSecondary)),
                    const SizedBox(height: 6),
                    Wrap(spacing: 8, children: [
                      for (final l in p.selectableLevels)
                        ChoiceChip(label: Text(complaintLevelLabel(s, l)), selected: _level == l, onSelected: (_) => setState(() => _level = l)),
                    ]),
                    const SizedBox(height: 6),
                    for (final r in p.routes.where((r) => r.level == _level))
                      Text('${s.t('المسار', 'Route')}: ${r.handlerPath.map((h) => handlerRoleLabel(s, h)).join(' ← ')}',
                          style: Theme.of(context).textTheme.labelSmall),
                  ],
                  const SizedBox(height: 12),
                  TextFormField(controller: _subject, maxLength: 200, decoration: InputDecoration(labelText: s.t('الموضوع', 'Subject')), validator: (v) => requiredValidator(context, v)),
                  TextFormField(
                    controller: _content,
                    maxLines: 8,
                    maxLength: 8000,
                    decoration: InputDecoration(labelText: s.t('التفاصيل', 'Details'), alignLabelWithHint: true),
                    validator: (v) => requiredValidator(context, v),
                  ),
                  Text(s.t('تكدر تضيف مرفقات بعد التقديم من صفحة الشكوى.', 'You can add attachments after submitting, from the complaint page.'),
                      style: Theme.of(context).textTheme.labelSmall),
                  const SizedBox(height: 16),
                  FilledButton(onPressed: _busy || p == null ? null : _submit, child: Text(s.submit)),
                ]),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

Future<void> _uploadAttachment(BuildContext context, WidgetRef ref, String id, VoidCallback onDone) async {
  final s = S.of(context);
  final r = await FilePicker.platform.pickFiles(withData: true);
  final file = r?.files.single;
  if (file == null || file.bytes == null || !context.mounted) return;
  final ok = await runAction(
    context,
    () => ref.read(hrApiProvider).complaints.Complaint_UploadAttachment(
        id: id, idempotencyKey: const Uuid().v4(), body: ComplaintAttachmentUploadDto(file: MultipartFile.fromBytes(file.bytes!, filename: file.name))),
    done: s.t('رُفع المرفق', 'Attachment uploaded'),
  );
  if (ok) onDone();
}

Future<void> _download(BuildContext context, WidgetRef ref, String id, AttachmentDto a) async {
  await runAction(context, () async {
    final file = await ref.read(hrApiProvider).complaints.Complaint_DownloadAttachment(id: id, attachmentId: a.id);
    await saveBinary(file, fallbackName: a.fileName);
  });
}

Widget _attachments(BuildContext context, WidgetRef ref, String id, List<AttachmentDto> list, {VoidCallback? onUpload}) {
  final s = S.of(context);
  return HrCard(
    child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      SectionTitle(s.attachments,
          trailing: onUpload == null ? null : TextButton.icon(onPressed: onUpload, icon: const Icon(Icons.attach_file), label: Text(s.addAttachment))),
      if (list.isEmpty) Text(s.empty),
      for (final a in list)
        ListTile(
          contentPadding: EdgeInsets.zero,
          leading: const Icon(Icons.insert_drive_file_outlined),
          title: Text(a.fileName),
          trailing: IconButton(onPressed: () => _download(context, ref, id, a), icon: const Icon(Icons.download)),
        ),
    ]),
  );
}

final myComplaintProvider = FutureProvider.autoDispose.family<ComplaintComplainantDetailDto, String>(
  (ref, id) => ref.watch(hrApiProvider).complaints.Complaint_GetMyDetail(id: id),
);

/// Complainant view: status, final decision once closed, own attachments, raise level.
class MyComplaintPage extends ConsumerWidget {
  const MyComplaintPage({super.key, required this.id});
  final String id;

  Future<void> _raise(BuildContext context, WidgetRef ref, ComplaintComplainantDetailDto c) async {
    final s = S.of(context);
    final higher = ComplaintLevel.values.where((l) => l != ComplaintLevel.$unknown && l.index > c.level.index).toList();
    if (higher.isEmpty) return;
    var level = higher.first;
    final reason = TextEditingController();
    final ok = await showFormDialog(
      context,
      title: s.t('رفع مستوى الشكوى', 'Raise complaint level'),
      fields: (setState) => [
        Wrap(spacing: 8, children: [
          for (final l in higher) ChoiceChip(label: Text(complaintLevelLabel(s, l)), selected: level == l, onSelected: (_) => setState(() => level = l)),
        ]),
        TextFormField(controller: reason, maxLines: 2, decoration: InputDecoration(labelText: s.note)),
        Text(s.t('لا يمكن خفض المستوى بعد رفعه.', 'A raised level can never be lowered.'), style: const TextStyle(color: HrColors.warning)),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).complaints.Complaint_RaiseLevel(
            id: id, body: ComplaintRaiseLevelInputDto(level: level, reason: reason.text.trim().isEmpty ? null : reason.text.trim(), concurrencyStamp: c.concurrencyStamp)),
        done: s.saved,
      ),
    );
    if (ok) ref.invalidate(myComplaintProvider(id));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrPage(
      title: s.t('شكواي', 'My complaint'),
      child: AsyncBody<ComplaintComplainantDetailDto>(
        value: ref.watch(myComplaintProvider(id)),
        onRetry: () => ref.invalidate(myComplaintProvider(id)),
        builder: (c) {
          final open = c.status != ComplaintStatus.closed;
          return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                Row(children: [
                  Expanded(child: Text(c.subject, style: Theme.of(context).textTheme.titleMedium)),
                  StatusPill(complaintLevelLabel(s, c.level), tone: levelTone(c.level)),
                  const SizedBox(width: 6),
                  StatusPill(complaintStatusLabel(s, c.status), tone: open ? Tone.warning : Tone.neutral),
                ]),
                const SizedBox(height: 8),
                InfoRow(s.t('الرقم المرجعي', 'Reference'), c.referenceNumber),
                InfoRow(s.t('ضد', 'Against'), c.respondent?.fullName ?? s.t('عامة', 'General')),
                InfoRow(s.date, f.dateTime(c.submittedAt)),
                if (c.closedAt != null) InfoRow(s.t('أُغلقت', 'Closed'), f.dateTime(c.closedAt)),
                if (c.closureDecision != null) InfoRow(s.t('القرار النهائي', 'Final decision'), c.closureDecision!),
                const SizedBox(height: 8),
                SelectableText(c.content),
                if (open && c.level != ComplaintLevel.highlyConfidential) ...[
                  const SizedBox(height: 12),
                  Align(
                    alignment: AlignmentDirectional.centerStart,
                    child: OutlinedButton.icon(onPressed: () => _raise(context, ref, c), icon: const Icon(Icons.arrow_upward), label: Text(s.t('رفع المستوى', 'Raise level'))),
                  ),
                ],
              ]),
            ),
            const SizedBox(height: 12),
            _attachments(context, ref, id, c.attachments,
                onUpload: open ? () => _uploadAttachment(context, ref, id, () => ref.invalidate(myComplaintProvider(id))) : null),
          ]);
        },
      ),
    );
  }
}

final handlerComplaintProvider = FutureProvider.autoDispose.family<ComplaintHandlerDetailDto, String>(
  (ref, id) => ref.watch(hrApiProvider).complaints.Complaint_GetDetail(id: id),
);

/// Handler view: content, actions (append-only), forward, raise level, close, access log.
/// When a transition returns callerRetainsAccess = false the screen leaves immediately.
class HandlerComplaintPage extends ConsumerWidget {
  const HandlerComplaintPage({super.key, required this.id});
  final String id;

  void _after(BuildContext context, WidgetRef ref, ComplaintTransitionResultDto r) {
    if (!r.callerRetainsAccess) {
      ref.invalidate(handlerComplaintProvider(id));
      context.go('/complaints');
    } else {
      ref.invalidate(handlerComplaintProvider(id));
    }
  }

  Future<void> _addAction(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    var type = ComplaintManualActionType.note;
    final note = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('إضافة إجراء', 'Add action'),
      fields: (setState) => [
        DropdownButtonFormField<ComplaintManualActionType>(
          initialValue: type,
          decoration: InputDecoration(labelText: s.t('النوع', 'Type')),
          items: [
            for (final t in ComplaintManualActionType.values.where((v) => v != ComplaintManualActionType.$unknown))
              DropdownMenuItem(value: t, child: Text(actionTypeLabel(s, ComplaintActionType.fromJson(t.value)))),
          ],
          onChanged: (v) => setState(() => type = v ?? type),
        ),
        TextFormField(controller: note, maxLines: 5, maxLength: 4000, decoration: InputDecoration(labelText: s.notes), validator: (v) => requiredValidator(context, v)),
        Text(s.t('الإجراء لا يُعدّل ولا يُحذف بعد حفظه.', 'Actions cannot be edited or deleted once saved.'), style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).complaints.Complaint_AddAction(id: id, idempotencyKey: key, body: ComplaintActionCreateDto(type: type, note: note.text.trim())),
        done: s.saved,
      ),
    );
    if (ok) ref.invalidate(handlerComplaintProvider(id));
  }

  Future<void> _forward(BuildContext context, WidgetRef ref, ComplaintHandlerDetailDto c) async {
    final s = S.of(context);
    final note = await askText(context, title: s.t('تحويل إلى مدير الموارد البشرية', 'Forward to HR manager'), label: s.note, mandatory: false);
    if (note == null || !context.mounted) return;
    ComplaintTransitionResultDto? r;
    await runAction(context, () async {
      r = await ref.read(hrApiProvider).complaints.Complaint_Forward(
          id: id, body: ComplaintForwardInputDto(note: note.isEmpty ? null : note, concurrencyStamp: c.concurrencyStamp));
    }, done: s.saved);
    if (r != null && context.mounted) _after(context, ref, r!);
  }

  Future<void> _close(BuildContext context, WidgetRef ref, ComplaintHandlerDetailDto c) async {
    final s = S.of(context);
    final decision = await askText(context, title: s.t('إغلاق الشكوى بقرار', 'Close with a decision'), label: s.t('القرار', 'Decision'), maxLength: 4000);
    if (decision == null || !context.mounted) return;
    ComplaintTransitionResultDto? r;
    await runAction(context, () async {
      r = await ref.read(hrApiProvider).complaints.Complaint_Close(id: id, body: ComplaintCloseInputDto(decision: decision, concurrencyStamp: c.concurrencyStamp));
    }, done: s.saved);
    if (r != null && context.mounted) _after(context, ref, r!);
  }

  Future<void> _raise(BuildContext context, WidgetRef ref, ComplaintHandlerDetailDto c) async {
    final s = S.of(context);
    final higher = ComplaintLevel.values.where((l) => l != ComplaintLevel.$unknown && l.index > c.level.index).toList();
    if (higher.isEmpty) return;
    var level = higher.first;
    final reason = TextEditingController();
    ComplaintTransitionResultDto? r;
    await showFormDialog(
      context,
      title: s.t('رفع المستوى', 'Raise level'),
      fields: (setState) => [
        Wrap(spacing: 8, children: [
          for (final l in higher) ChoiceChip(label: Text(complaintLevelLabel(s, l)), selected: level == l, onSelected: (_) => setState(() => level = l)),
        ]),
        TextFormField(controller: reason, maxLines: 2, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () => runAction(context, () async {
        r = await ref.read(hrApiProvider).complaints.Complaint_RaiseLevel(
            id: id, body: ComplaintRaiseLevelInputDto(level: level, reason: reason.text.trim().isEmpty ? null : reason.text.trim(), concurrencyStamp: c.concurrencyStamp));
      }, done: s.saved),
    );
    if (r != null && context.mounted) _after(context, ref, r!);
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).complaints;
    return HrPage(
      title: s.t('معالجة شكوى', 'Complaint handling'),
      child: AsyncBody<ComplaintHandlerDetailDto>(
        value: ref.watch(handlerComplaintProvider(id)),
        onRetry: () => ref.invalidate(handlerComplaintProvider(id)),
        builder: (c) {
          final open = c.status != ComplaintStatus.closed;
          return DefaultTabController(
            length: 2,
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [
                Tab(text: s.details),
                Tab(text: s.t('سجل الاطلاع', 'Access log')),
              ]),
              const SizedBox(height: 12),
              Expanded(
                child: TabBarView(children: [
                  ListView(padding: const EdgeInsets.only(bottom: 24), children: [
                    HrCard(
                      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                        Row(children: [
                          Expanded(child: Text(c.subject, style: Theme.of(context).textTheme.titleMedium)),
                          if (c.isLate) ...[StatusPill(s.t('متأخرة', 'Late'), tone: Tone.danger), const SizedBox(width: 6)],
                          StatusPill(complaintLevelLabel(s, c.level), tone: levelTone(c.level)),
                          const SizedBox(width: 6),
                          StatusPill(complaintStatusLabel(s, c.status), tone: open ? Tone.warning : Tone.neutral),
                        ]),
                        const SizedBox(height: 8),
                        InfoRow(s.t('الرقم المرجعي', 'Reference'), c.referenceNumber),
                        InfoRow(s.t('مقدم الشكوى', 'Complainant'), [c.complainant.fullName, if (c.complainant.departmentName != null) c.complainant.departmentName!].join(' · ')),
                        InfoRow(s.t('ضد', 'Against'), c.respondent?.fullName ?? s.t('عامة', 'General')),
                        InfoRow(s.t('الحد الأدنى للمستوى', 'Minimum level'), complaintLevelLabel(s, c.minimumLevel)),
                        if (c.handler != null) InfoRow(s.t('المعالج الحالي', 'Current handler'), c.handler!.displayName),
                        InfoRow(s.date, f.dateTime(c.submittedAt)),
                        if (c.dueAt != null) InfoRow(s.t('المهلة', 'Due'), f.dateTime(c.dueAt)),
                        if (c.closureDecision != null) InfoRow(s.t('القرار', 'Decision'), c.closureDecision!),
                        const SizedBox(height: 8),
                        SelectableText(c.content),
                        if (open) ...[
                          const SizedBox(height: 12),
                          Wrap(spacing: 8, runSpacing: 8, children: [
                            FilledButton.icon(onPressed: () => _addAction(context, ref), icon: const Icon(Icons.add_comment_outlined), label: Text(s.t('إجراء', 'Action'))),
                            if (session.can(HrOps.Complaint_Forward) && c.status == ComplaintStatus.pendingDepartmentManager)
                              OutlinedButton.icon(onPressed: () => _forward(context, ref, c), icon: const Icon(Icons.forward), label: Text(s.t('تحويل', 'Forward'))),
                            if (c.level != ComplaintLevel.highlyConfidential)
                              OutlinedButton.icon(onPressed: () => _raise(context, ref, c), icon: const Icon(Icons.arrow_upward), label: Text(s.t('رفع المستوى', 'Raise level'))),
                            if (c.status != ComplaintStatus.pendingDepartmentManager)
                              TextButton.icon(onPressed: () => _close(context, ref, c), icon: const Icon(Icons.lock_outline, color: HrColors.danger), label: Text(s.t('إغلاق بقرار', 'Close'))),
                          ]),
                        ],
                      ]),
                    ),
                    const SizedBox(height: 12),
                    HrCard(
                      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                        SectionTitle(s.t('الإجراءات', 'Actions')),
                        if (c.actions.isEmpty) Text(s.empty),
                        for (final a in c.actions)
                          ListTile(
                            contentPadding: EdgeInsets.zero,
                            leading: Icon(Icons.event_note, color: context.hr.accent),
                            title: Text('${actionTypeLabel(s, a.type)} · ${a.actor.displayName}'),
                            subtitle: Text([f.dateTime(a.creationTime), if (a.note != null) a.note!].join('\n')),
                          ),
                      ]),
                    ),
                    const SizedBox(height: 12),
                    _attachments(context, ref, id, c.attachments,
                        onUpload: open ? () => _uploadAttachment(context, ref, id, () => ref.invalidate(handlerComplaintProvider(id))) : null),
                  ]),
                  PagedList<ComplaintAccessLogEntryDto>(
                    fetch: (skip, max) async {
                      final r = await api.Complaint_GetAccessLog(id: id, skipCount: skip, maxResultCount: max);
                      return (r.items, r.totalCount);
                    },
                    itemBuilder: (context, e) => HrCard(
                      child: Row(children: [
                        Expanded(child: Text(e.user.displayName)),
                        Text(e.action.value, style: TextStyle(color: context.hr.textSecondary)),
                        const SizedBox(width: 12),
                        Text(f.dateTime(e.at)),
                      ]),
                    ),
                  ),
                ]),
              ),
            ]),
          );
        },
      ),
    );
  }
}

/// Opens a complaint from a secureItemUpdateForOwner notification: tries the complainant
/// view first; a 404 there means the user is a handler (contract rule).
class ComplaintOpenPage extends ConsumerStatefulWidget {
  const ComplaintOpenPage({super.key, required this.id});
  final String id;

  @override
  ConsumerState<ComplaintOpenPage> createState() => _ComplaintOpenPageState();
}

class _ComplaintOpenPageState extends ConsumerState<ComplaintOpenPage> {
  @override
  void initState() {
    super.initState();
    _route();
  }

  Future<void> _route() async {
    try {
      await ref.read(hrApiProvider).complaints.Complaint_GetMyDetail(id: widget.id);
      if (mounted) context.go('/complaints/my/${widget.id}');
    } catch (_) {
      if (mounted) context.go('/complaints/${widget.id}');
    }
  }

  @override
  Widget build(BuildContext context) => const Center(child: CircularProgressIndicator());
}
