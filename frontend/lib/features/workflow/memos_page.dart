import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../leaves/new_leave_page.dart' show otherLeaveTypesProvider;
import '../time/time_shared.dart' show TimeField;
import 'workflow_shared.dart';

/// Memos: my memos (Memo_GetMy), memos waiting for me (Memo_GetInbox), create.
class MemosPage extends ConsumerStatefulWidget {
  const MemosPage({super.key});

  @override
  ConsumerState<MemosPage> createState() => _MemosPageState();
}

class _MemosPageState extends ConsumerState<MemosPage> {
  int _version = 0;

  Future<void> _create() async {
    final s = S.of(context);
    final content = TextEditingController();
    var kind = MemoKind.problem;
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('مطالعة جديدة', 'New memo'),
      fields: (setState) => [
        SegmentedButton<MemoKind>(
          segments: [
            ButtonSegment(value: MemoKind.problem, label: Text(memoKindLabel(s, MemoKind.problem))),
            ButtonSegment(value: MemoKind.leave, label: Text(memoKindLabel(s, MemoKind.leave))),
          ],
          selected: {kind},
          onSelectionChanged: (v) => setState(() => kind = v.first),
        ),
        TextFormField(
          controller: content,
          maxLines: 6,
          maxLength: 4000,
          decoration: InputDecoration(labelText: s.t('نص المطالعة', 'Memo text'), alignLabelWithHint: true),
          validator: (v) => requiredValidator(context, v),
        ),
        Text(s.t('تصل أولاً إلى مديرك المباشر.', 'It goes to your direct manager first.'), style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).memos.Memo_Create(idempotencyKey: key, body: MemoCreateInputDto(kind: kind, content: content.text.trim())),
        done: s.submitted,
      ),
      submitLabel: s.submit,
    );
    if (ok) setState(() => _version++);
  }

  Widget _tile(BuildContext context, MemoListItemDto m, {bool showEmployee = false}) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrCard(
      onTap: () => context.go('/memos/${m.id}'),
      child: Row(children: [
        Icon(m.kind == MemoKind.leave ? Icons.beach_access_outlined : Icons.report_problem_outlined, color: context.hr.accent),
        const SizedBox(width: 12),
        Expanded(
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            if (showEmployee) Text(m.employee.fullName, style: const TextStyle(fontWeight: FontWeight.w600)),
            Text(m.contentPreview, maxLines: 2, overflow: TextOverflow.ellipsis),
            Text('${memoKindLabel(s, m.kind)} · ${f.dateTime(m.creationTime)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
          ]),
        ),
        StatusPill(m.statusLabel, tone: memoTone(m.status)),
      ]),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).memos;
    final canInbox = session.can(HrOps.Memo_GetInbox);
    final canMine = session.can(HrOps.Memo_GetMy);
    final mine = PagedList<MemoListItemDto>(
      key: ValueKey('my|$_version'),
      fetch: (skip, max) async {
        final r = await api.Memo_GetMy(skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, m) => _tile(context, m),
    );
    final inbox = PagedList<MemoListItemDto>(
      key: ValueKey('inbox|$_version'),
      emptyMessage: s.nothingPending,
      fetch: (skip, max) async {
        final r = await api.Memo_GetInbox(skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, m) => _tile(context, m, showEmployee: true),
    );
    final tabs = <(String, Widget)>[
      if (canMine) (s.t('مطالعاتي', 'My memos'), mine),
      if (canInbox) (s.t('بانتظاري', 'Awaiting me'), inbox),
    ];
    return HrPage(
      title: s.t('المطالعات', 'Memos'),
      actions: [
        if (session.can(HrOps.Memo_Create)) FilledButton.icon(onPressed: _create, icon: const Icon(Icons.add), label: Text(s.t('مطالعة جديدة', 'New memo'))),
      ],
      child: tabs.length == 1
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

final memoProvider = FutureProvider.autoDispose.family<MemoDto, String>((ref, id) => ref.watch(hrApiProvider).memos.Memo_Get(id: id));

/// Memo detail with forward (manager → HR), convert (to leave request or incident) and close.
/// The server decides who may act; the client only offers the actions the status allows.
class MemoDetailPage extends ConsumerWidget {
  const MemoDetailPage({super.key, required this.id});
  final String id;

  Future<void> _forward(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    final note = await askText(context, title: s.t('تحويل إلى الموارد البشرية', 'Forward to HR'), label: s.note, mandatory: false);
    if (note == null || !context.mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).memos.Memo_Forward(id: id, body: NoteInputDto(note: note.isEmpty ? null : note)), done: s.saved);
    if (ok) ref.invalidate(memoProvider(id));
  }

  Future<void> _close(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    final reason = await askText(context, title: s.t('إغلاق المطالعة', 'Close memo'), label: s.reason);
    if (reason == null || !context.mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).memos.Memo_Close(id: id, body: ReasonInputDto(reason: reason)), done: s.saved);
    if (ok) ref.invalidate(memoProvider(id));
  }

  Future<void> _convert(BuildContext context, WidgetRef ref, MemoDto m) async {
    final s = S.of(context);
    final f = Fmt(context);
    final note = TextEditingController();
    final toLeave = m.kind == MemoKind.leave;
    var kind = LeaveKind.annual;
    DateTime? start, end, day;
    String? from, to, otherTypeId;
    final ok = await showFormDialog(
      context,
      title: toLeave ? s.t('تحويل إلى طلب إجازة', 'Convert to leave request') : s.t('تحويل إلى بلاغ', 'Convert to incident'),
      fields: (setState) => [
        if (toLeave) ...[
          SegmentedButton<LeaveKind>(
            segments: [for (final k in const [LeaveKind.annual, LeaveKind.hourly, LeaveKind.other]) ButtonSegment(value: k, label: Text(s.leaveKindLabel(k)))],
            selected: {kind},
            onSelectionChanged: (v) => setState(() => kind = v.first),
          ),
          if (kind == LeaveKind.other)
            Consumer(builder: (context, ref, _) {
              final types = ref.watch(otherLeaveTypesProvider).valueOrNull ?? const <OtherLeaveTypeOptionDto>[];
              return DropdownButtonFormField<String>(
                initialValue: otherTypeId,
                decoration: InputDecoration(labelText: s.otherLeaveType),
                items: [for (final t in types) DropdownMenuItem(value: t.id, child: Text(t.name))],
                onChanged: (v) => setState(() => otherTypeId = v),
              );
            }),
          if (kind == LeaveKind.hourly) ...[
            PickerField(label: s.date, value: day == null ? null : f.date(day), icon: Icons.event, onTap: () async {
              final d = await pickDate(context, initial: day);
              if (d != null) setState(() => day = d);
            }),
            TimeField(label: s.fromTime, value: from, onChanged: (v) => setState(() => from = v)),
            TimeField(label: s.toTime, value: to, onChanged: (v) => setState(() => to = v)),
          ] else ...[
            PickerField(label: s.startDate, value: start == null ? null : f.date(start), icon: Icons.event, onTap: () async {
              final d = await pickDate(context, initial: start);
              if (d != null) setState(() => start = d);
            }),
            PickerField(label: s.endDate, value: end == null ? null : f.date(end), icon: Icons.event, onTap: () async {
              final d = await pickDate(context, initial: end ?? start);
              if (d != null) setState(() => end = d);
            }),
          ],
        ],
        TextFormField(controller: note, maxLines: 2, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).memos.Memo_Convert(
              id: id,
              body: MemoConvertInputDto(
                target: toLeave ? MemoConvertTarget.leaveRequest : MemoConvertTarget.incident,
                leave: toLeave
                    ? MemoLeaveConversionDto(
                        kind: kind,
                        otherLeaveTypeId: kind == LeaveKind.other ? otherTypeId : null,
                        startDate: kind == LeaveKind.hourly ? null : start,
                        endDate: kind == LeaveKind.hourly ? null : end,
                        leaveDate: kind == LeaveKind.hourly ? day : null,
                        fromTime: kind == LeaveKind.hourly ? from : null,
                        toTime: kind == LeaveKind.hourly ? to : null,
                      )
                    : null,
                note: note.text.trim().isEmpty ? null : note.text.trim(),
              ),
            ),
        done: s.saved,
      ),
    );
    if (ok) ref.invalidate(memoProvider(id));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final value = ref.watch(memoProvider(id));
    return HrPage(
      title: s.t('مطالعة', 'Memo'),
      child: AsyncBody<MemoDto>(
        value: value,
        onRetry: () => ref.invalidate(memoProvider(id)),
        builder: (m) {
          final mine = m.employee.id == session.me?.employee?.id;
          final pending = m.status == MemoStatus.pendingManager || m.status == MemoStatus.pendingHr || m.status == MemoStatus.pendingExecutive;
          return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                Row(children: [
                  Expanded(child: Text(m.employee.fullName, style: Theme.of(context).textTheme.titleMedium)),
                  StatusPill(m.statusLabel, tone: memoTone(m.status)),
                ]),
                const SizedBox(height: 8),
                InfoRow(s.t('النوع', 'Kind'), memoKindLabel(s, m.kind)),
                InfoRow(s.date, f.dateTime(m.creationTime)),
                if (m.currentHandlerRole != null) InfoRow(s.t('عند', 'With'), s.role(m.currentHandlerRole)),
                if (m.closeReason != null) InfoRow(s.reason, m.closeReason!),
                const SizedBox(height: 8),
                SelectableText(m.content),
                if (m.convertedRequestId != null)
                  TextButton.icon(
                      onPressed: () => context.go('/requests/${m.convertedRequestId}'),
                      icon: const Icon(Icons.open_in_new),
                      label: Text(s.t('طلب الإجازة الناتج', 'Resulting leave request'))),
                if (m.convertedIncidentId != null)
                  TextButton.icon(
                      onPressed: () => context.go('/incidents/${m.convertedIncidentId}'),
                      icon: const Icon(Icons.open_in_new),
                      label: Text(s.t('البلاغ الناتج', 'Resulting incident'))),
                if (!mine && pending) ...[
                  const SizedBox(height: 12),
                  Wrap(spacing: 8, runSpacing: 8, children: [
                    if (session.can(HrOps.Memo_Forward) && m.status != MemoStatus.pendingHr)
                      OutlinedButton.icon(onPressed: () => _forward(context, ref), icon: const Icon(Icons.forward), label: Text(s.t('تحويل للموارد البشرية', 'Forward to HR'))),
                    if (session.can(HrOps.Memo_Convert))
                      FilledButton.icon(
                          onPressed: () => _convert(context, ref, m),
                          icon: const Icon(Icons.transform),
                          label: Text(m.kind == MemoKind.leave ? s.t('تحويل لطلب إجازة', 'To leave request') : s.t('تحويل لبلاغ', 'To incident'))),
                    if (session.can(HrOps.Memo_Close) && m.status != MemoStatus.pendingManager)
                      TextButton.icon(onPressed: () => _close(context, ref), icon: const Icon(Icons.close, color: HrColors.danger), label: Text(s.t('إغلاق', 'Close'))),
                  ]),
                ],
              ]),
            ),
            const SizedBox(height: 12),
            TimelineCard(entries: m.timeline),
          ]);
        },
      ),
    );
  }
}
