import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

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
import 'org_providers.dart';

String delegationScopeLabel(S s, DelegationScope sc) => switch (sc) {
      DelegationScope.allApprovals => s.t('كل الموافقات', 'All approvals'),
      DelegationScope.requests => s.t('الطلبات', 'Requests'),
      DelegationScope.memosAndIncidents => s.t('المطالعات والبلاغات', 'Memos and incidents'),
      DelegationScope.reviewReports => s.t('التقرير اليومي وغرفه', 'Daily report and rooms'),
      DelegationScope.$unknown => '—',
    };

String delegationStatusLabel(S s, DelegationStatus st) => switch (st) {
      DelegationStatus.scheduled => s.t('مجدول', 'Scheduled'),
      DelegationStatus.active => s.t('ساري', 'Active'),
      DelegationStatus.expired => s.t('منتهي', 'Expired'),
      DelegationStatus.cancelled => s.t('ملغى', 'Cancelled'),
      DelegationStatus.$unknown => '—',
    };

/// S-DEL-01: my delegations (given and received), create, cancel; all delegations for HR.
class DelegationsPage extends ConsumerStatefulWidget {
  const DelegationsPage({super.key});

  @override
  ConsumerState<DelegationsPage> createState() => _DelegationsPageState();
}

class _DelegationsPageState extends ConsumerState<DelegationsPage> {
  int _version = 0;
  DelegationDirection? _direction;

  Future<void> _create() async {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.read(sessionProvider);
    final reason = TextEditingController();
    UserRefDto? to;
    UserRefDto? from;
    DateTime? starts = DateTime.now().add(const Duration(minutes: 5));
    DateTime? ends;
    final hrOnBehalf = session.can(HrOps.Delegation_List) && session.can(HrOps.UserAccount_Lookup);
    // Which scopes this person may delegate: approvers all approval scopes, the accountant the requests step, the reviewer
    // the follow-up reports only (decision 2026-11-11). HR, delegating for someone absent, sees every scope.
    final approver = [AppRole.departmentManager, AppRole.hrManager, AppRole.executiveDirector, AppRole.systemAdmin].any(session.hasRole);
    final scopes = <DelegationScope>[
      if (approver || hrOnBehalf) ...[DelegationScope.allApprovals, DelegationScope.requests, DelegationScope.memosAndIncidents]
      else if (session.hasRole(AppRole.accountant)) DelegationScope.requests,
      if (session.hasRole(AppRole.reviewer) || hrOnBehalf) DelegationScope.reviewReports,
    ];
    if (scopes.isEmpty) return;
    var scope = scopes.first;
    final ok = await showFormDialog(
      context,
      title: s.t('تفويض جديد', 'New delegation'),
      fields: (setState) => [
        if (hrOnBehalf)
          PickerField(
            label: s.t('عن المعتمد الغائب (للموارد البشرية، اختياري)', 'For an absent approver (HR, optional)'),
            value: from?.displayName,
            onClear: () => setState(() => from = null),
            onTap: () async {
              final u = await showSearchPicker<UserRefDto>(context, title: s.t('المعتمد', 'Approver'), search: (q) => searchUsers(ref, q), label: (u) => u.displayName);
              if (u != null) setState(() => from = u);
            },
          ),
        PickerField(
          label: s.t('المفوَّض', 'Delegate'),
          value: to?.displayName,
          onTap: () async {
            final u = await showSearchPicker<UserRefDto>(
              context,
              title: s.t('المفوَّض', 'Delegate'),
              search: (q) async => (await ref.read(hrApiProvider).delegations.Delegation_GetCandidates(search: q.isEmpty ? null : q, maxResultCount: 50)).items,
              label: (u) => u.displayName,
            );
            if (u != null) setState(() => to = u);
          },
        ),
        DropdownButtonFormField<DelegationScope>(
          initialValue: scope,
          decoration: InputDecoration(labelText: s.t('النطاق', 'Scope')),
          items: [
            for (final sc in scopes) DropdownMenuItem(value: sc, child: Text(delegationScopeLabel(s, sc))),
          ],
          onChanged: (v) => setState(() => scope = v ?? scope),
        ),
        PickerField(
          label: s.from,
          value: starts == null ? null : f.dateTime(starts),
          icon: Icons.event,
          onTap: () async {
            final d = await pickDateTime(context, initial: starts);
            if (d != null) setState(() => starts = d);
          },
        ),
        PickerField(
          label: s.to,
          value: ends == null ? null : f.dateTime(ends),
          icon: Icons.event,
          onTap: () async {
            final d = await pickDateTime(context, initial: ends ?? starts?.add(const Duration(days: 1)));
            if (d != null) setState(() => ends = d);
          },
        ),
        TextFormField(controller: reason, maxLines: 3, maxLength: 2000, decoration: InputDecoration(labelText: s.reason), validator: (v) => requiredValidator(context, v)),
        Text(
          s.t('لا يشمل التفويض الشكاوى ولا المالية ولا التقييم. ويبقى صاحب الصلاحية قادراً على القرار.',
              'Delegation never covers complaints, finance or evaluations. The original approver can still decide.'),
          style: Theme.of(context).textTheme.labelSmall,
        ),
      ],
      onSubmit: () async {
        if (to == null || starts == null || ends == null || !ends!.isAfter(starts!)) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).delegations.Delegation_Create(
                body: DelegationCreateDto(
                  fromUserId: from?.id,
                  toUserId: to!.id,
                  scope: scope,
                  startsAt: starts!,
                  endsAt: ends!,
                  reason: reason.text.trim(),
                ),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _cancel(DelegationDto d) async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('إلغاء التفويض', 'Cancel delegation'), message: d.toUser.displayName)) return;
    if (!mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).delegations.Delegation_Cancel(id: d.id), done: s.cancelled);
    if (ok) setState(() => _version++);
  }

  Widget _tile(DelegationDto d, {required bool canCancel}) {
    final s = S.of(context);
    final f = Fmt(context);
    final tone = switch (d.status) {
      DelegationStatus.active => Tone.success,
      DelegationStatus.scheduled => Tone.info,
      _ => Tone.neutral,
    };
    return HrCard(
      child: Row(children: [
        Icon(Icons.swap_horizontal_circle_outlined, color: context.hr.accent),
        const SizedBox(width: 12),
        Expanded(
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Wrap(crossAxisAlignment: WrapCrossAlignment.center, spacing: 6, children: [
              PersonName.user(d.fromUser, style: const TextStyle(fontWeight: FontWeight.w600)),
              const Text('←'),
              PersonName.user(d.toUser, style: const TextStyle(fontWeight: FontWeight.w600)),
            ]),
            Text('${delegationScopeLabel(s, d.scope)} · ${f.dateTime(d.startsAt)} → ${f.dateTime(d.endsAt)}',
                style: TextStyle(color: context.hr.textSecondary)),
            Text(d.reason, style: Theme.of(context).textTheme.labelSmall),
          ]),
        ),
        StatusPill(delegationStatusLabel(s, d.status), tone: tone),
        if (canCancel && (d.status == DelegationStatus.active || d.status == DelegationStatus.scheduled))
          IconButton(tooltip: s.t('إلغاء', 'Cancel'), onPressed: () => _cancel(d), icon: const Icon(Icons.cancel_outlined, color: HrColors.danger)),
      ]),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).delegations;
    final myId = session.me?.userId;
    final canAll = session.can(HrOps.Delegation_List);
    final mine = Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      Wrap(spacing: 8, children: [
        ChoiceChip(label: Text(s.all), selected: _direction == null, onSelected: (_) => setState(() => _direction = null)),
        ChoiceChip(label: Text(s.t('أعطيتها', 'Given')), selected: _direction == DelegationDirection.given, onSelected: (_) => setState(() => _direction = DelegationDirection.given)),
        ChoiceChip(label: Text(s.t('وصلتني', 'Received')), selected: _direction == DelegationDirection.received, onSelected: (_) => setState(() => _direction = DelegationDirection.received)),
      ]),
      const SizedBox(height: 12),
      Expanded(
        child: PagedList<DelegationDto>(
          key: ValueKey('my|$_version|$_direction'),
          fetch: (skip, max) async {
            final r = await api.Delegation_GetMy(direction: _direction, skipCount: skip, maxResultCount: max);
            return (r.items, r.totalCount);
          },
          itemBuilder: (context, d) => _tile(d, canCancel: d.fromUser.id == myId && session.can(HrOps.Delegation_Cancel)),
        ),
      ),
    ]);
    return HrPage(
      title: s.t('التفويض', 'Delegations'),
      actions: [
        if (session.can(HrOps.Delegation_Create))
          FilledButton.icon(onPressed: _create, icon: const Icon(Icons.add), label: Text(s.t('تفويض جديد', 'New delegation'))),
      ],
      child: !canAll
          ? mine
          : DefaultTabController(
              length: 2,
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [
                  Tab(text: s.t('تفويضاتي', 'Mine')),
                  Tab(text: s.t('كل التفويضات', 'All delegations')),
                ]),
                const SizedBox(height: 12),
                Expanded(
                  child: TabBarView(children: [
                    mine,
                    PagedList<DelegationDto>(
                      key: ValueKey('all|$_version'),
                      fetch: (skip, max) async {
                        final r = await api.Delegation_List(skipCount: skip, maxResultCount: max);
                        return (r.items, r.totalCount);
                      },
                      itemBuilder: (context, d) => _tile(d, canCancel: false),
                    ),
                  ]),
                ),
              ]),
            ),
    );
  }
}
