import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
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
import '../org/org_providers.dart' show searchUsers;
import 'complaints_page.dart' show complaintLevelLabel;

String assignmentStatusLabel(S s, ComplaintHandlerAssignmentStatus st) => switch (st) {
      ComplaintHandlerAssignmentStatus.pendingApproval => s.t('بانتظار موافقة المدير التنفيذي', 'Awaiting executive approval'),
      ComplaintHandlerAssignmentStatus.approved => s.t('معتمد', 'Approved'),
      ComplaintHandlerAssignmentStatus.rejected => s.t('مرفوض', 'Rejected'),
      ComplaintHandlerAssignmentStatus.revoked => s.t('مسحوب', 'Revoked'),
      ComplaintHandlerAssignmentStatus.$unknown => '—',
    };

/// Complaint handler assignments: HR requests, the executive director approves, rejects or
/// revokes. No access is granted before approval. Contains no complaint data.
class ComplaintHandlersPage extends ConsumerStatefulWidget {
  const ComplaintHandlersPage({super.key});

  @override
  ConsumerState<ComplaintHandlersPage> createState() => _ComplaintHandlersPageState();
}

class _ComplaintHandlersPageState extends ConsumerState<ComplaintHandlersPage> {
  int _version = 0;
  ComplaintHandlerAssignmentStatus? _status;

  Future<void> _request() async {
    final s = S.of(context);
    final f = Fmt(context);
    UserRefDto? user;
    var level = ComplaintLevel.normal;
    DateTime? from = DateTime.now();
    DateTime? to;
    final reason = TextEditingController();
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('طلب تعيين معالج', 'Request a handler assignment'),
      fields: (setState) => [
        PickerField(
          label: s.t('المستخدم', 'User'),
          value: user?.displayName,
          onTap: () async {
            final u = await showSearchPicker<UserRefDto>(context, title: s.t('المستخدم', 'User'), search: (q) => searchUsers(ref, q), label: (u) => u.displayName);
            if (u != null) setState(() => user = u);
          },
        ),
        Wrap(spacing: 8, children: [
          for (final l in ComplaintLevel.values.where((v) => v != ComplaintLevel.$unknown))
            ChoiceChip(label: Text(complaintLevelLabel(s, l)), selected: level == l, onSelected: (_) => setState(() => level = l)),
        ]),
        PickerField(label: s.from, value: from == null ? null : f.dateTime(from), icon: Icons.event, onTap: () async {
          final d = await pickDateTime(context, initial: from);
          if (d != null) setState(() => from = d);
        }),
        PickerField(
          label: s.t('إلى (فارغ = حتى السحب)', 'To (empty = until revoked)'),
          value: to == null ? null : f.dateTime(to),
          icon: Icons.event,
          onClear: () => setState(() => to = null),
          onTap: () async {
            final d = await pickDateTime(context, initial: to);
            if (d != null) setState(() => to = d);
          },
        ),
        TextFormField(controller: reason, maxLines: 3, decoration: InputDecoration(labelText: s.reason), validator: (v) => requiredValidator(context, v)),
        Text(s.t('مسؤول النظام لا يكون معالجاً، والتعيين يحتاج موافقة المدير التنفيذي.', 'A system admin can never handle complaints; the executive director must approve.'),
            style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () async {
        if (user == null || from == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).complaintHandlers.ComplaintHandler_Request(
              idempotencyKey: key,
              body: ComplaintHandlerAssignmentCreateDto(userId: user!.id, level: level, from: from!, to: to, reason: reason.text.trim())),
          done: s.submitted,
        );
      },
      submitLabel: s.submit,
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _decide(ComplaintHandlerAssignmentDto a, String action) async {
    final s = S.of(context);
    final api = ref.read(hrApiProvider).complaintHandlers;
    if (action == 'approve') {
      if (!await confirmAction(context, title: s.approve, message: '${a.user.displayName} · ${complaintLevelLabel(s, a.level)}')) return;
      if (!mounted) return;
      final ok = await runAction(context, () => api.ComplaintHandler_Approve(id: a.id, body: ComplaintHandlerAssignmentApproveDto(concurrencyStamp: a.concurrencyStamp)), done: s.approved);
      if (ok) setState(() => _version++);
      return;
    }
    final reason = await askText(context, title: action == 'reject' ? s.reject : s.t('سحب التعيين', 'Revoke'), label: s.reason);
    if (reason == null || !mounted) return;
    final body = ComplaintHandlerAssignmentReasonDto(reason: reason, concurrencyStamp: a.concurrencyStamp);
    final ok = await runAction(
      context,
      () => action == 'reject' ? api.ComplaintHandler_Reject(id: a.id, body: body) : api.ComplaintHandler_Revoke(id: a.id, body: body),
      done: s.saved,
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final canDecide = session.can(HrOps.ComplaintHandler_Approve);
    final api = ref.watch(hrApiProvider).complaintHandlers;
    return HrPage(
      title: s.t('معالجو الشكاوى', 'Complaint handlers'),
      actions: [
        if (session.can(HrOps.ComplaintHandler_Request))
          FilledButton.icon(onPressed: _request, icon: const Icon(Icons.person_add_alt), label: Text(s.t('طلب تعيين', 'Request'))),
      ],
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Wrap(spacing: 8, children: [
          ChoiceChip(label: Text(s.all), selected: _status == null, onSelected: (_) => setState(() => _status = null)),
          for (final st in ComplaintHandlerAssignmentStatus.values.where((v) => v != ComplaintHandlerAssignmentStatus.$unknown))
            ChoiceChip(label: Text(assignmentStatusLabel(s, st)), selected: _status == st, onSelected: (_) => setState(() => _status = st)),
        ]),
        const SizedBox(height: 12),
        Expanded(
          child: PagedList<ComplaintHandlerAssignmentDto>(
            key: ValueKey('$_version|$_status'),
            fetch: (skip, max) async {
              final r = await api.ComplaintHandler_GetList(status: _status, skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, a) => HrCard(
              child: Row(children: [
                Icon(Icons.gavel_outlined, color: context.hr.accent),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text('${a.user.displayName} · ${complaintLevelLabel(s, a.level)}', style: const TextStyle(fontWeight: FontWeight.w600)),
                    Text('${f.dateTime(a.from)} → ${a.to == null ? s.t('حتى السحب', 'until revoked') : f.dateTime(a.to)}',
                        style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                    Text('${a.reason} · ${s.t('طلبه', 'Requested by')} ${a.requestedBy.displayName}', style: Theme.of(context).textTheme.labelSmall),
                    if (a.decisionReason != null) Text(a.decisionReason!, style: Theme.of(context).textTheme.labelSmall),
                  ]),
                ),
                StatusPill(assignmentStatusLabel(s, a.status),
                    tone: switch (a.status) {
                      ComplaintHandlerAssignmentStatus.approved => Tone.success,
                      ComplaintHandlerAssignmentStatus.pendingApproval => Tone.warning,
                      _ => Tone.neutral,
                    }),
                if (canDecide && a.status == ComplaintHandlerAssignmentStatus.pendingApproval) ...[
                  IconButton(tooltip: s.approve, onPressed: () => _decide(a, 'approve'), icon: const Icon(Icons.check_circle_outline, color: HrColors.success)),
                  IconButton(tooltip: s.reject, onPressed: () => _decide(a, 'reject'), icon: const Icon(Icons.cancel_outlined, color: HrColors.danger)),
                ],
                if (canDecide && a.status == ComplaintHandlerAssignmentStatus.approved)
                  IconButton(tooltip: s.t('سحب', 'Revoke'), onPressed: () => _decide(a, 'revoke'), icon: const Icon(Icons.remove_circle_outline, color: HrColors.danger)),
              ]),
            ),
          ),
        ),
      ]),
    );
  }
}
