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
import '../../core/widgets/person_avatar.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import 'workflow_shared.dart';

/// Fault incidents: my incidents, inbox (manager: team; HR: company), create.
class IncidentsPage extends ConsumerStatefulWidget {
  const IncidentsPage({super.key});

  @override
  ConsumerState<IncidentsPage> createState() => _IncidentsPageState();
}

class _IncidentsPageState extends ConsumerState<IncidentsPage> {
  int _version = 0;

  Future<void> _create() async {
    final s = S.of(context);
    final description = TextEditingController();
    ItemRefDto? item;
    final key = const Uuid().v4();
    final canItems = ref.read(sessionProvider).can(HrOps.Item_Lookup);
    final ok = await showFormDialog(
      context,
      title: s.t('بلاغ عطل جديد', 'New fault incident'),
      fields: (setState) => [
        if (canItems)
          PickerField(
            label: s.t('الصنف المتعطل (اختياري)', 'Faulty item (optional)'),
            value: item?.name,
            onClear: () => setState(() => item = null),
            onTap: () async {
              final i = await showSearchPicker<ItemRefDto>(
                context,
                title: s.item,
                search: (q) async => (await ref.read(hrApiProvider).store.Item_Lookup(filter: q.isEmpty ? null : q, maxResultCount: 50)).items,
                label: (i) => i.name,
              );
              if (i != null) setState(() => item = i);
            },
          ),
        TextFormField(
          controller: description,
          maxLines: 5,
          maxLength: 4000,
          decoration: InputDecoration(labelText: s.t('وصف العطل', 'Describe the fault'), alignLabelWithHint: true),
          validator: (v) => requiredValidator(context, v),
        ),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).incidents.Incident_Create(
            idempotencyKey: key, body: IncidentCreateInputDto(itemId: item?.id, description: description.text.trim())),
        done: s.submitted,
      ),
      submitLabel: s.submit,
    );
    if (ok) setState(() => _version++);
  }

  Widget _tile(BuildContext context, IncidentListItemDto i, {bool showEmployee = false}) {
    final f = Fmt(context);
    return HrCard(
      onTap: () => context.go('/incidents/${i.id}'),
      child: Row(children: [
        Icon(Icons.build_circle_outlined, color: context.hr.accent),
        const SizedBox(width: 12),
        Expanded(
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            if (showEmployee) PersonName.employee(i.employee, style: const TextStyle(fontWeight: FontWeight.w600)),
            Text(i.descriptionPreview, maxLines: 2, overflow: TextOverflow.ellipsis),
            Text([if (i.item != null) i.item!.name, f.dateTime(i.creationTime)].join(' · '), style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
          ]),
        ),
        StatusPill(i.statusLabel, tone: incidentTone(i.status)),
      ]),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).incidents;
    final tabs = <(String, Widget)>[
      if (session.can(HrOps.Incident_GetMy))
        (
          s.t('بلاغاتي', 'My incidents'),
          PagedList<IncidentListItemDto>(
            key: ValueKey('my|$_version'),
            fetch: (skip, max) async {
              final r = await api.Incident_GetMy(skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, i) => _tile(context, i),
          )
        ),
      if (session.can(HrOps.Incident_GetInbox))
        (
          s.t('الوارد', 'Inbox'),
          PagedList<IncidentListItemDto>(
            key: ValueKey('inbox|$_version'),
            emptyMessage: s.nothingPending,
            fetch: (skip, max) async {
              final r = await api.Incident_GetInbox(skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, i) => _tile(context, i, showEmployee: true),
          )
        ),
    ];
    return HrPage(
      title: s.t('بلاغات الأعطال', 'Fault incidents'),
      actions: [
        if (session.can(HrOps.Incident_Create)) FilledButton.icon(onPressed: _create, icon: const Icon(Icons.add), label: Text(s.t('بلاغ جديد', 'New incident'))),
      ],
      child: tabs.isEmpty
          ? EmptyState(message: s.forbidden)
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

final incidentProvider = FutureProvider.autoDispose.family<IncidentDto, String>((ref, id) => ref.watch(hrApiProvider).incidents.Incident_Get(id: id));

/// Incident detail: manager forwards to HR; HR moves pendingHr → inProgress → resolved.
class IncidentDetailPage extends ConsumerWidget {
  const IncidentDetailPage({super.key, required this.id});
  final String id;

  Future<void> _forward(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    final note = await askText(context, title: s.t('تحويل إلى الموارد البشرية', 'Forward to HR'), label: s.note, mandatory: false);
    if (note == null || !context.mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).incidents.Incident_Forward(id: id, body: NoteInputDto(note: note.isEmpty ? null : note)), done: s.saved);
    if (ok) ref.invalidate(incidentProvider(id));
  }

  Future<void> _status(BuildContext context, WidgetRef ref, IncidentDto i, IncidentStatusTarget target) async {
    final s = S.of(context);
    final note = await askText(context,
        title: target == IncidentStatusTarget.resolved ? s.t('تم الحل', 'Resolved') : s.t('قيد المعالجة', 'In progress'), label: s.note, mandatory: false);
    if (note == null || !context.mounted) return;
    final ok = await runAction(
      context,
      () => ref.read(hrApiProvider).incidents.Incident_UpdateStatus(
          id: id, body: IncidentStatusUpdateInputDto(status: target, note: note.isEmpty ? null : note, concurrencyStamp: i.concurrencyStamp)),
      done: s.saved,
    );
    if (ok) ref.invalidate(incidentProvider(id));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.t('بلاغ عطل', 'Fault incident'),
      child: AsyncBody<IncidentDto>(
        value: ref.watch(incidentProvider(id)),
        onRetry: () => ref.invalidate(incidentProvider(id)),
        builder: (i) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              Row(children: [
                Expanded(child: PersonName.employee(i.employee, radius: 16, style: Theme.of(context).textTheme.titleMedium)),
                StatusPill(i.statusLabel, tone: incidentTone(i.status)),
              ]),
              const SizedBox(height: 8),
              if (i.item != null) InfoRow(s.item, i.item!.name),
              InfoRow(s.date, f.dateTime(i.creationTime)),
              if (i.currentHandlerRole != null) InfoRow(s.t('عند', 'With'), s.role(i.currentHandlerRole)),
              const SizedBox(height: 8),
              SelectableText(i.description),
              const SizedBox(height: 12),
              Wrap(spacing: 8, runSpacing: 8, children: [
                if (i.status == IncidentStatus.pendingManager && session.can(HrOps.Incident_Forward))
                  FilledButton.icon(onPressed: () => _forward(context, ref), icon: const Icon(Icons.forward), label: Text(s.t('تحويل للموارد البشرية', 'Forward to HR'))),
                if (i.status == IncidentStatus.pendingHr && session.can(HrOps.Incident_UpdateStatus))
                  OutlinedButton.icon(
                      onPressed: () => _status(context, ref, i, IncidentStatusTarget.inProgress),
                      icon: const Icon(Icons.engineering),
                      label: Text(s.t('قيد المعالجة', 'In progress'))),
                if ((i.status == IncidentStatus.pendingHr || i.status == IncidentStatus.inProgress) && session.can(HrOps.Incident_UpdateStatus))
                  FilledButton.icon(
                      onPressed: () => _status(context, ref, i, IncidentStatusTarget.resolved),
                      icon: const Icon(Icons.check),
                      label: Text(s.t('تم الحل', 'Resolved'))),
              ]),
            ]),
          ),
          const SizedBox(height: 12),
          TimelineCard(entries: i.timeline),
        ]),
      ),
    );
  }
}
