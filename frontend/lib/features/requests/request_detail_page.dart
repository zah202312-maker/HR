import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/person_avatar.dart';
import 'request_widgets.dart';

final requestDetailProvider = FutureProvider.autoDispose.family<RequestDetailsDto, String>(
  (ref, id) => ref.watch(hrApiProvider).requests.Request_Get(id: id),
);

/// Request_Get with Request_Approve / Request_Reject / Request_Cancel.
/// Buttons follow canApprove / canReject / canCancel from the server, never a client rule.
class RequestDetailPage extends ConsumerStatefulWidget {
  const RequestDetailPage({super.key, required this.id});
  final String id;

  @override
  ConsumerState<RequestDetailPage> createState() => _RequestDetailPageState();
}

class _RequestDetailPageState extends ConsumerState<RequestDetailPage> {
  bool _busy = false;

  RequestsApi get _api => ref.read(hrApiProvider).requests;

  Future<void> _act(Future<RequestDetailsDto> Function() call, String done) async {
    setState(() => _busy = true);
    try {
      await call();
      if (!mounted) return;
      showDone(context, done);
      ref.invalidate(requestDetailProvider(widget.id));
    } catch (e) {
      if (mounted) showError(context, e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _approve(S s) async {
    final note = await askText(context, title: s.approve, label: s.note, mandatory: false);
    if (note == null) return;
    await _act(() => _api.Request_Approve(id: widget.id, body: NoteInputDto(note: note.isEmpty ? null : note)), s.approved);
  }

  Future<void> _reject(S s) async {
    final reason = await askText(context, title: s.reject, label: s.reason);
    if (reason == null) return;
    await _act(() => _api.Request_Reject(id: widget.id, body: ReasonInputDto(reason: reason)), s.rejected);
  }

  Future<void> _cancel(S s) async {
    final note = await askText(context, title: s.cancelRequest, label: s.note, mandatory: false);
    if (note == null) return;
    await _act(() => _api.Request_Cancel(id: widget.id, body: NoteInputDto(note: note.isEmpty ? null : note)), s.cancelled);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final value = ref.watch(requestDetailProvider(widget.id));
    return HrPage(
      title: value.valueOrNull?.type.name ?? s.details,
      child: AsyncBody<RequestDetailsDto>(
        value: value,
        onRetry: () => ref.invalidate(requestDetailProvider(widget.id)),
        builder: (r) {
          final tone = toneForStatus(r.status);
          final wide = MediaQuery.sizeOf(context).width >= 1024;
          final info = HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              Row(children: [
                Expanded(child: Text(r.detailsSummary ?? r.type.name, style: Theme.of(context).textTheme.titleMedium)),
                StatusPill(r.statusLabel, tone: tone, icon: iconForStatusTone(tone)),
              ]),
              const SizedBox(height: 12),
              InfoRow(s.employee, [r.employee.fullName, if (r.employee.departmentName != null) r.employee.departmentName!].join(' · '), leading: PersonAvatar.employee(r.employee, radius: 12)),
              if (r.isOnBehalf) InfoRow(s.submittedBy, '${r.submittedBy.displayName} (${s.onBehalf})'),
              InfoRow(s.date, f.dateTime(r.creationTime)),
              if (r.description != null) InfoRow(s.description, r.description!),
              if (r.rejectionReason != null) InfoRow(s.rejectionReason, r.rejectionReason!),
              if (r.canApprove || r.canReject || r.canCancel) ...[
                const SizedBox(height: 16),
                Wrap(spacing: 8, runSpacing: 8, children: [
                  if (r.canApprove)
                    FilledButton.icon(
                        onPressed: _busy ? null : () => _approve(s), icon: const Icon(Icons.check), label: Text(s.approve)),
                  if (r.canReject)
                    OutlinedButton.icon(
                        onPressed: _busy ? null : () => _reject(s),
                        icon: const Icon(Icons.close, color: HrColors.danger),
                        label: Text(s.reject)),
                  if (r.canCancel)
                    TextButton(onPressed: _busy ? null : () => _cancel(s), child: Text(s.cancelRequest)),
                ]),
              ],
            ]),
          );
          final steps = HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.steps),
              for (final st in r.steps)
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: CircleAvatar(radius: 14, child: Text('${st.order}', style: const TextStyle(fontSize: 12))),
                  title: st.approver == null ? Text(s.role(st.approverRole)) : PersonName.user(st.approver!),
                  subtitle: Text([
                    s.stepDecision(st.decision),
                    if (st.decidedBy != null) st.decidedBy!.displayName,
                    if (st.delegatedFrom != null) '${s.delegatedFrom}: ${st.delegatedFrom!.displayName}',
                    if (st.decidedAt != null) f.dateTime(st.decidedAt),
                    if (st.note != null) st.note!,
                  ].join(' · ')),
                ),
            ]),
          );
          final timeline = HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.timeline),
              for (final t in r.timeline)
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: Icon(Icons.circle, size: 10, color: context.hr.accent),
                  title: Text(t.action),
                  subtitle: Text([f.dateTime(t.occurredAt), if (t.actor != null) t.actor!.displayName, if (t.note != null) t.note!].join(' · ')),
                ),
            ]),
          );
          if (wide) {
            return SingleChildScrollView(
              padding: const EdgeInsets.only(bottom: 24),
              child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Expanded(flex: 3, child: Column(children: [info, const SizedBox(height: 12), timeline])),
                const SizedBox(width: 12),
                Expanded(flex: 2, child: steps),
              ]),
            );
          }
          return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
            info,
            const SizedBox(height: 12),
            steps,
            const SizedBox(height: 12),
            timeline,
          ]);
        },
      ),
    );
  }
}
