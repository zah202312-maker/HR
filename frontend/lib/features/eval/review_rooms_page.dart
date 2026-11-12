import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/person_avatar.dart';
import '../../core/widgets/pickers.dart';
import 'review_reports_page.dart' show standInForProvider;

/// The department managers a room may be built around, each with the people linked to them.
final reviewRoomCandidatesProvider = FutureProvider.autoDispose<List<ReviewRoomCandidateDto>>(
    (ref) => ref.watch(hrApiProvider).reviewRooms.ReviewRoom_GetCandidates());

final reviewRoomsProvider = FutureProvider.autoDispose<List<ReviewRoomDto>>(
    (ref) async => (await ref.watch(hrApiProvider).reviewRooms.ReviewRoom_GetList(maxResultCount: 200)).items);

/// The members a room keeps when its lead changes: only the people linked to the new lead.
List<String> reviewRoomMembersFor(ReviewRoomCandidateDto? lead, Iterable<String> picked) {
  if (lead == null) return const <String>[];
  final linked = {for (final m in lead.members) m.id};
  return [for (final id in picked) if (linked.contains(id)) id];
}

/// S-RVM «إنشاء غرف» (decision 2026-11-11): the reviewer picks a department manager and some of the people linked to
/// them. The daily report takes one row per room, ordered by the room lead.
class ReviewRoomsPage extends ConsumerWidget {
  const ReviewRoomsPage({super.key});

  Future<void> _edit(BuildContext context, WidgetRef ref, {ReviewRoomDto? room}) async {
    final s = S.of(context);
    final List<ReviewRoomCandidateDto> candidates;
    try {
      candidates = await ref.read(reviewRoomCandidatesProvider.future);
    } catch (e) {
      if (context.mounted) showError(context, e);
      return;
    }
    if (!context.mounted) return;

    final own = ref.read(sessionProvider).can(HrOps.ReviewRoom_Create);
    final standInFor = ref.read(standInForProvider).valueOrNull ?? const <UserRefDto>[];
    UserRefDto? onBehalfOf = own ? null : (standInFor.isEmpty ? null : standInFor.first);
    final name = TextEditingController(text: room?.name ?? '');
    ReviewRoomCandidateDto? lead = room == null ? null : candidates.where((c) => c.manager.id == room.manager.id).firstOrNull;
    var members = <String>{...reviewRoomMembersFor(lead, room?.members.map((m) => m.id) ?? const <String>[])};

    final saved = await showFormDialog(
      context,
      title: room == null ? s.t('غرفة جديدة', 'New room') : s.t('تعديل الغرفة', 'Edit room'),
      submitLabel: room == null ? s.t('إنشاء', 'Create') : s.save,
      fields: (setState) => [
        if (room == null && standInFor.isNotEmpty)
          DropdownButtonFormField<UserRefDto?>(
            initialValue: onBehalfOf,
            decoration: InputDecoration(labelText: s.t('عن المراجع', 'On behalf of the reviewer')),
            items: [
              if (own) DropdownMenuItem<UserRefDto?>(value: null, child: Text(s.t('غرفي', 'My own rooms'))),
              for (final u in standInFor) DropdownMenuItem<UserRefDto?>(value: u, child: PersonName.user(u)),
            ],
            onChanged: (v) => setState(() => onBehalfOf = v),
          ),
        TextFormField(
          controller: name,
          maxLength: 128,
          decoration: InputDecoration(labelText: s.t('اسم الغرفة', 'Room name')),
          validator: (v) => (v ?? '').trim().isEmpty ? s.t('اكتب اسم الغرفة', 'Enter the room name') : null,
        ),
        DropdownButtonFormField<ReviewRoomCandidateDto>(
          initialValue: lead,
          isExpanded: true,
          decoration: InputDecoration(labelText: s.t('مدير القسم (مسؤول الفريق)', 'Department manager (team lead)')),
          items: [
            for (final c in candidates)
              DropdownMenuItem(
                value: c,
                child: Text([c.manager.fullName, if (c.department != null) c.department!.name].join(' · '), overflow: TextOverflow.ellipsis),
              ),
          ],
          validator: (v) => v == null ? s.t('اختر مدير القسم', 'Choose the department manager') : null,
          onChanged: (v) => setState(() {
            lead = v;
            // A new lead starts with everyone linked to them; the reviewer then unticks whoever is out.
            members = {for (final m in v?.members ?? const <EmployeeRefDto>[]) m.id};
          }),
        ),
        if (lead != null) ...[
          Row(children: [
            Expanded(child: Text(s.t('الأفراد', 'Members'), style: const TextStyle(fontWeight: FontWeight.w600))),
            Text('${members.length}/${lead!.members.length}', style: TextStyle(color: context.hr.textSecondary)),
          ]),
          if (lead!.members.isEmpty)
            Text(s.t('لا يوجد موظفون مرتبطون بهذا المدير', 'Nobody is linked to this manager'), style: TextStyle(color: context.hr.textSecondary))
          else
            Wrap(spacing: 6, runSpacing: 6, children: [
              for (final m in lead!.members)
                FilterChip(
                  label: PersonName.employee(m, radius: 10),
                  selected: members.contains(m.id),
                  onSelected: (on) => setState(() {
                    on ? members.add(m.id) : members.remove(m.id);
                  }),
                ),
            ]),
        ],
      ],
      onSubmit: () async {
        if (members.isEmpty) {
          showError(context, s.t('اختر فردًا واحدًا على الأقل', 'Pick at least one member'));
          return false;
        }
        final api = ref.read(hrApiProvider).reviewRooms;
        return runAction(context, () async {
          if (room == null) {
            await api.ReviewRoom_Create(
                body: ReviewRoomCreateDto(
                    onBehalfOfUserId: onBehalfOf?.id, name: name.text.trim(), managerEmployeeId: lead!.manager.id, memberEmployeeIds: members.toList()));
          } else {
            await api.ReviewRoom_Update(
                id: room.id,
                body: ReviewRoomUpdateDto(
                    name: name.text.trim(), managerEmployeeId: lead!.manager.id, memberEmployeeIds: members.toList(), concurrencyStamp: room.concurrencyStamp));
          }
        }, done: s.saved);
      },
    );
    name.dispose();
    if (saved) ref.invalidate(reviewRoomsProvider);
  }

  Future<void> _delete(BuildContext context, WidgetRef ref, ReviewRoomDto room) async {
    final s = S.of(context);
    if (!await confirmAction(context,
        title: s.t('حذف الغرفة', 'Delete room'),
        message: s.t('لا تدخل الغرفة في التقارير الجديدة، وتبقى التقارير السابقة كما هي.', 'The room leaves new reports; earlier reports stay as they are.'),
        destructive: true)) {
      return;
    }
    if (!context.mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).reviewRooms.ReviewRoom_Delete(id: room.id), done: s.t('حُذفت الغرفة', 'Room deleted'));
    if (ok) ref.invalidate(reviewRoomsProvider);
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final rooms = ref.watch(reviewRoomsProvider);
    final standInFor = ref.watch(standInForProvider).valueOrNull ?? const <UserRefDto>[];
    return HrPage(
      title: s.t('إنشاء غرف', 'Rooms'),
      subtitle: s.t('كل غرفة مدير قسم وأفراده، وتُقيَّم يوميًا في التقرير اليومي', 'Each room is a department manager and their people, graded every day in the daily report'),
      actions: [
        FilledButton.icon(onPressed: () => _edit(context, ref), icon: const Icon(Icons.add), label: Text(s.t('غرفة جديدة', 'New room'))),
      ],
      child: rooms.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => ErrorState(error: e, onRetry: () => ref.invalidate(reviewRoomsProvider)),
        data: (items) => items.isEmpty
            ? EmptyState(message: s.t('لا توجد غرف بعد. أنشئ غرفة لكل فريق.', 'No rooms yet. Create one for each team.'))
            : ListView(padding: const EdgeInsets.only(bottom: 24), children: [
                for (final room in items)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 8),
                    child: HrCard(
                      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                        Row(children: [
                          Icon(Icons.meeting_room_outlined, color: context.hr.accent),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                              Text(room.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                              const SizedBox(height: 2),
                              Wrap(spacing: 6, crossAxisAlignment: WrapCrossAlignment.center, children: [
                                Text('${s.t('مسؤول الفريق', 'Team lead')}:', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                                PersonName.employee(room.manager, style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                                if (room.department != null)
                                  Text('· ${room.department!.name}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                                if (standInFor.isNotEmpty) Text('· ${room.reviewer.displayName}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                              ]),
                            ]),
                          ),
                          IconButton(
                              tooltip: s.t('تعديل', 'Edit'), onPressed: () => _edit(context, ref, room: room), icon: const Icon(Icons.edit_outlined)),
                          IconButton(
                              tooltip: s.t('حذف', 'Delete'),
                              onPressed: () => _delete(context, ref, room),
                              icon: const Icon(Icons.delete_outline, color: HrColors.danger)),
                        ]),
                        const SizedBox(height: 8),
                        Wrap(spacing: 6, runSpacing: 6, children: [
                          for (final m in room.members) Chip(visualDensity: VisualDensity.compact, label: PersonName.employee(m, radius: 10)),
                        ]),
                      ]),
                    ),
                  ),
              ]),
      ),
    );
  }
}
