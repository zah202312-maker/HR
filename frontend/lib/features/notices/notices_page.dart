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
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/paged_list.dart';
import '../ops/ops_shared.dart';
import '../time/time_shared.dart';

/// «تبليغاتي»: notices sent to me by name, with acknowledgement (decision 2026-11-10).
class MyNoticesPage extends ConsumerStatefulWidget {
  const MyNoticesPage({super.key});

  @override
  ConsumerState<MyNoticesPage> createState() => _MyNoticesPageState();
}

class _MyNoticesPageState extends ConsumerState<MyNoticesPage> {
  bool? _acknowledged;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).notices;
    return HrPage(
      title: s.t('تبليغاتي', 'My notices'),
      // Whoever may send notices (HR manager, executive director, reviewer) starts one from here too (decision 2026-11-11).
      actions: [
        if (ref.watch(sessionProvider).can(HrOps.Notice_Create))
          FilledButton.icon(
            onPressed: () => context.go('/send-notice'),
            icon: const Icon(Icons.forward_to_inbox_outlined),
            label: Text(s.t('إرسال تبليغ', 'Send a notice')),
          ),
      ],
      child: PagedList<NoticeReceivedDto>(
        key: ValueKey('in|$_acknowledged'),
        emptyMessage: s.t('لا توجد تبليغات.', 'No notices.'),
        header: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Wrap(spacing: 8, children: [
            FilterChip(
              label: Text(s.t('غير المؤكدة', 'Not acknowledged')),
              selected: _acknowledged == false,
              onSelected: (v) => setState(() => _acknowledged = v ? false : null),
            ),
          ]),
        ),
        fetch: (skip, max) async {
          final r = await api.Notice_GetReceivedList(skipCount: skip, maxResultCount: max, isAcknowledged: _acknowledged);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, n) => OpsTile(
          leading: Icons.mark_email_unread_outlined,
          title: n.title,
          subtitle: '${n.sender.displayName} · ${f.dateTime(n.sentAt)}',
          trailing: n.acknowledgedAt == null
              ? StatusPill(s.t('بانتظار الاطلاع', 'Not acknowledged'), tone: Tone.warning)
              : StatusPill(s.t('اطّلعت', 'Acknowledged'), tone: Tone.success),
          onTap: () => context.go('/notices/${n.id}'),
        ),
      ),
    );
  }
}

final noticeProvider = FutureProvider.autoDispose.family<NoticeReceivedDto, String>((ref, id) => ref.watch(hrApiProvider).notices.Notice_Get(id: id));

/// One received notice, with «اطّلعت».
class NoticeDetailPage extends ConsumerWidget {
  const NoticeDetailPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrPage(
      title: s.t('تبليغ', 'Notice'),
      child: AsyncBody<NoticeReceivedDto>(
        value: ref.watch(noticeProvider(id)),
        onRetry: () => ref.invalidate(noticeProvider(id)),
        builder: (n) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              Text(n.title, style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: 4),
              Text('${n.sender.displayName} · ${f.dateTime(n.sentAt)}', style: TextStyle(color: context.hr.textSecondary)),
              const Divider(height: 24),
              SelectableText(n.content),
              const SizedBox(height: 16),
              if (n.acknowledgedAt == null)
                Align(
                  alignment: AlignmentDirectional.centerEnd,
                  child: FilledButton.icon(
                    onPressed: () async {
                      if (await runAction(context, () => ref.read(hrApiProvider).notices.Notice_Acknowledge(id: id), done: s.done)) {
                        ref.invalidate(noticeProvider(id));
                      }
                    },
                    icon: const Icon(Icons.done_all),
                    label: Text(s.t('اطّلعت', 'I have read it')),
                  ),
                )
              else
                Text('${s.t('اطّلعت في', 'Acknowledged on')} ${f.dateTime(n.acknowledgedAt)}', style: TextStyle(color: context.hr.textSecondary)),
            ]),
          ),
        ]),
      ),
    );
  }
}

/// «إرسال تبليغ»: the HR manager or the executive director writes a notice to employees chosen by name,
/// and follows who has read it.
class SendNoticePage extends ConsumerStatefulWidget {
  const SendNoticePage({super.key});

  @override
  ConsumerState<SendNoticePage> createState() => _SendNoticePageState();
}

class _SendNoticePageState extends ConsumerState<SendNoticePage> {
  int _version = 0;

  Future<void> _send() async {
    final s = S.of(context);
    final recipients = <EmployeeRefDto>[];
    final title = TextEditingController();
    final content = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      width: 620,
      title: s.t('تبليغ جديد', 'New notice'),
      submitLabel: s.t('إرسال', 'Send'),
      fields: (setState) => [
        FormField<List<EmployeeRefDto>>(
          validator: (_) => recipients.isEmpty ? s.t('اختر موظفًا واحدًا على الأقل.', 'Choose at least one employee.') : null,
          builder: (field) => InputDecorator(
            decoration: InputDecoration(labelText: s.t('المستلمون', 'Recipients'), errorText: field.errorText),
            child: Wrap(spacing: 8, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
              for (final e in recipients)
                InputChip(
                  label: Text(e.fullName),
                  tooltip: e.departmentName,
                  onDeleted: () => setState(() => recipients.remove(e)),
                ),
              if (recipients.length < 200)
                ActionChip(
                  avatar: const Icon(Icons.person_add_alt_1_outlined, size: 18),
                  label: Text(s.t('إضافة موظف', 'Add employee')),
                  onPressed: () async {
                    final picked = await pickEmployee(context, ref);
                    if (picked != null && !recipients.any((x) => x.id == picked.id)) {
                      setState(() => recipients.add(picked));
                      field.didChange(recipients);
                    }
                  },
                ),
            ]),
          ),
        ),
        TextFormField(controller: title, maxLength: 200, decoration: InputDecoration(labelText: s.t('عنوان التبليغ', 'Title')), validator: (v) => requiredValidator(context, v)),
        TextFormField(
          controller: content,
          maxLines: 8,
          maxLength: 8000,
          decoration: InputDecoration(labelText: s.t('نص التبليغ', 'Notice text'), alignLabelWithHint: true),
          validator: (v) => requiredValidator(context, v),
        ),
        Text(s.t('لا يُعدّل التبليغ ولا يُحذف بعد الإرسال.', 'A notice cannot be edited or deleted after sending.'), style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).notices.Notice_Create(
              idempotencyKey: key,
              body: NoticeCreateDto(recipientEmployeeIds: [for (final e in recipients) e.id], title: title.text.trim(), content: content.text.trim()),
            ),
        done: s.t('أُرسل التبليغ', 'Notice sent'),
      ),
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _receipts(NoticeSentDto n) async {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.read(hrApiProvider).notices;
    await showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(n.title),
        content: SizedBox(
          width: 560,
          height: 480,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            Text(n.content, maxLines: 4, overflow: TextOverflow.ellipsis),
            const Divider(),
            Expanded(
              child: PagedList<NoticeReceiptDto>(
                pageSize: 50,
                fetch: (skip, max) async {
                  final r = await api.Notice_GetReceipts(id: n.id, skipCount: skip, maxResultCount: max);
                  return (r.items, r.totalCount);
                },
                itemBuilder: (context, r) => ListTile(
                  dense: true,
                  leading: PersonAvatar.employee(r.recipient),
                  title: Text(r.recipient.fullName),
                  subtitle: Text(r.recipient.departmentName ?? ''),
                  trailing: r.acknowledgedAt == null
                      ? StatusPill(s.t('لم يطّلع', 'Not yet'), tone: Tone.warning)
                      : Text(f.dateTime(r.acknowledgedAt), style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                ),
              ),
            ),
          ]),
        ),
        actions: [TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.close))],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).notices;
    return HrPage(
      title: s.t('إرسال تبليغ', 'Send a notice'),
      actions: [
        if (session.can(HrOps.Notice_Create)) FilledButton.icon(onPressed: _send, icon: const Icon(Icons.send_outlined), label: Text(s.t('تبليغ جديد', 'New notice'))),
      ],
      child: PagedList<NoticeSentDto>(
        key: ValueKey('out|$_version'),
        emptyMessage: s.t('لم ترسل أي تبليغ بعد.', 'You have not sent any notice yet.'),
        fetch: (skip, max) async {
          final r = await api.Notice_GetSentList(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, n) => OpsTile(
          leading: Icons.outgoing_mail,
          title: n.title,
          subtitle: f.dateTime(n.sentAt),
          trailing: Tooltip(
            message: s.t('اطّلع / المستلمون', 'Read / recipients'),
            child: Text('${n.acknowledgedCount}/${n.recipientsCount}', style: Theme.of(context).textTheme.titleMedium),
          ),
          onTap: () => _receipts(n),
        ),
      ),
    );
  }
}
