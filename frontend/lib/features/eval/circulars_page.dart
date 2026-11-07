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

String circularScopeLabel(S s, CircularScope sc) => switch (sc) {
      CircularScope.team => s.t('فريقي', 'My team'),
      CircularScope.departmentManagers => s.t('مديرو الأقسام', 'Department managers'),
      CircularScope.company => s.t('كل الشركة', 'Whole company'),
      _ => '—',
    };

/// S-CIR: circulars received (acknowledge) and sent (receipts), and sending a new one.
class CircularsPage extends ConsumerStatefulWidget {
  const CircularsPage({super.key});

  @override
  ConsumerState<CircularsPage> createState() => _CircularsPageState();
}

class _CircularsPageState extends ConsumerState<CircularsPage> {
  int _version = 0;
  bool? _acknowledged;

  Future<void> _send() async {
    final s = S.of(context);
    final session = ref.read(sessionProvider);
    // allowed scopes follow the role; the server answers ScopeNotAllowed otherwise
    final scopes = <CircularScope>[
      if (session.hasRole(AppRole.departmentManager)) CircularScope.team,
      if (session.hasRole(AppRole.hrManager) || session.hasRole(AppRole.executiveDirector)) ...[CircularScope.departmentManagers, CircularScope.company],
    ];
    if (scopes.isEmpty) scopes.addAll(CircularScope.values.where((v) => v != CircularScope.$unknown));
    var scope = scopes.first;
    final title = TextEditingController();
    final content = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      width: 620,
      title: s.t('تعميم جديد', 'New circular'),
      submitLabel: s.t('إرسال', 'Send'),
      fields: (setState) => [
        DropdownButtonFormField<CircularScope>(
          initialValue: scope,
          decoration: InputDecoration(labelText: s.t('المستلمون', 'Recipients')),
          items: [for (final x in scopes) DropdownMenuItem(value: x, child: Text(circularScopeLabel(s, x)))],
          onChanged: (v) => setState(() => scope = v ?? scope),
        ),
        TextFormField(controller: title, maxLength: 200, decoration: InputDecoration(labelText: s.t('العنوان', 'Title')), validator: (v) => requiredValidator(context, v)),
        TextFormField(
          controller: content,
          maxLines: 8,
          maxLength: 8000,
          decoration: InputDecoration(labelText: s.t('النص', 'Content'), alignLabelWithHint: true),
          validator: (v) => requiredValidator(context, v),
        ),
        Text(s.t('لا يُعدّل التعميم ولا يُحذف بعد الإرسال.', 'A circular cannot be edited or deleted after sending.'), style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).circulars.Circular_Create(
            idempotencyKey: key, body: CircularCreateDto(scope: scope, title: title.text.trim(), content: content.text.trim())),
        done: s.t('أُرسل التعميم', 'Circular sent'),
      ),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).circulars;
    final tabs = <(String, Widget)>[
      if (session.can(HrOps.Circular_GetReceivedList))
        (
          s.t('الواردة', 'Received'),
          PagedList<CircularReceivedDto>(
            key: ValueKey('in|$_version|$_acknowledged'),
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Wrap(spacing: 8, children: [
                FilterChip(
                  label: Text(s.t('غير المؤكدة', 'Unacknowledged')),
                  selected: _acknowledged == false,
                  onSelected: (v) => setState(() => _acknowledged = v ? false : null),
                ),
              ]),
            ),
            fetch: (skip, max) async {
              final r = await api.Circular_GetReceivedList(skipCount: skip, maxResultCount: max, isAcknowledged: _acknowledged, sorting: 'sentAt desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => OpsTile(
              leading: Icons.campaign_outlined,
              title: c.title,
              subtitle: '${c.sender.displayName} · ${f.dateTime(c.sentAt)}',
              trailing: c.acknowledgedAt == null
                  ? StatusPill(s.t('بانتظار التأكيد', 'Not acknowledged'), tone: c.acknowledgementRequired ? Tone.warning : Tone.info)
                  : StatusPill(s.t('مؤكد', 'Acknowledged'), tone: Tone.success),
              onTap: () => context.go('/circulars/${c.id}'),
            ),
          )
        ),
      if (session.can(HrOps.Circular_GetSentList))
        (
          s.t('المرسلة', 'Sent'),
          PagedList<CircularSentDto>(
            key: ValueKey('out|$_version'),
            fetch: (skip, max) async {
              final r = await api.Circular_GetSentList(skipCount: skip, maxResultCount: max, sorting: 'sentAt desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => OpsTile(
              leading: Icons.outgoing_mail,
              title: c.title,
              subtitle: '${circularScopeLabel(s, c.scope)} · ${f.dateTime(c.sentAt)}',
              trailing: Text('${c.acknowledgedCount}/${c.recipientsCount}', style: Theme.of(context).textTheme.titleMedium),
              onTap: () => _receipts(c),
            ),
          )
        ),
    ];
    return HrPage(
      title: s.t('التعاميم', 'Circulars'),
      actions: [
        if (session.can(HrOps.Circular_Create)) FilledButton.icon(onPressed: _send, icon: const Icon(Icons.send_outlined), label: Text(s.t('تعميم جديد', 'New circular'))),
      ],
      child: tabbedBody(context, tabs),
    );
  }

  Future<void> _receipts(CircularSentDto c) async {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.read(hrApiProvider).circulars;
    await showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(c.title),
        content: SizedBox(
          width: 560,
          height: 480,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            Text(c.content, maxLines: 4, overflow: TextOverflow.ellipsis),
            const Divider(),
            Expanded(
              child: PagedList<CircularReceiptDto>(
                pageSize: 50,
                fetch: (skip, max) async {
                  final r = await api.Circular_GetReceipts(id: c.id, skipCount: skip, maxResultCount: max);
                  return (r.items, r.totalCount);
                },
                itemBuilder: (context, r) => ListTile(
                  dense: true,
                  title: Text(r.recipient.fullName),
                  subtitle: Text(r.recipient.departmentName ?? ''),
                  trailing: r.acknowledgedAt == null
                      ? StatusPill(s.t('لم يؤكد', 'Pending'), tone: Tone.warning)
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
}

final circularProvider = FutureProvider.autoDispose.family<CircularReceivedDto, String>((ref, id) => ref.watch(hrApiProvider).circulars.Circular_Get(id: id));

/// One received circular, with acknowledgement.
class CircularDetailPage extends ConsumerWidget {
  const CircularDetailPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrPage(
      title: s.t('تعميم', 'Circular'),
      child: AsyncBody<CircularReceivedDto>(
        value: ref.watch(circularProvider(id)),
        onRetry: () => ref.invalidate(circularProvider(id)),
        builder: (c) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              Text(c.title, style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: 4),
              Text('${c.sender.displayName} · ${circularScopeLabel(s, c.scope)} · ${f.dateTime(c.sentAt)}', style: TextStyle(color: context.hr.textSecondary)),
              const Divider(height: 24),
              SelectableText(c.content),
              const SizedBox(height: 16),
              if (c.acknowledgedAt == null)
                Align(
                  alignment: AlignmentDirectional.centerEnd,
                  child: FilledButton.icon(
                    onPressed: () async {
                      if (await runAction(context, () => ref.read(hrApiProvider).circulars.Circular_Acknowledge(id: id), done: s.done)) {
                        ref.invalidate(circularProvider(id));
                      }
                    },
                    icon: const Icon(Icons.done_all),
                    label: Text(s.t('تأكيد الاستلام', 'Acknowledge')),
                  ),
                )
              else
                Text('${s.t('أكدت الاستلام', 'Acknowledged')} ${f.dateTime(c.acknowledgedAt)}', style: TextStyle(color: context.hr.textSecondary)),
            ]),
          ),
        ]),
      ),
    );
  }
}
