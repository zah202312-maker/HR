import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../home/home_page.dart' show myBalanceProvider;
import '../requests/request_widgets.dart';

/// S-LV-01 + S-LV-03: LeaveBalance_GetMy and LeaveRequest_ListMy.
class LeavesPage extends ConsumerWidget {
  const LeavesPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).leaveRequest;
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.leaves,
      actions: [
        if (session.can(HrOps.LeaveRequest_Create))
          FilledButton.icon(onPressed: () => context.go('/leaves/new'), icon: const Icon(Icons.add), label: Text(s.newLeave)),
      ],
      child: PagedList<LeaveRequestDto>(
        header: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          SectionTitle(s.myBalance),
          const _BalanceCard(),
          const SizedBox(height: 12),
          SectionTitle(s.myLeaves),
        ]),
        fetch: (skip, max) async {
          if (!session.can(HrOps.LeaveRequest_ListMy)) return (<LeaveRequestDto>[], 0);
          final r = await api.LeaveRequest_ListMy(skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, l) {
          final tone = toneForStatus(l.status);
          final period = l.kind == LeaveKind.hourly
              ? '${f.date(l.date)} · ${l.fromTime ?? ''}–${l.toTime ?? ''}'
              : '${f.date(l.startDate)} → ${f.date(l.endDate)}';
          final amount = l.kind == LeaveKind.hourly
              ? '${f.number(l.requestedHours)} ${s.hours}'
              : '${f.number(l.workingDays)} ${s.days}';
          return HrCard(
            onTap: () => context.go('/requests/${l.requestId}'),
            child: Row(children: [
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(l.otherLeaveType?.name ?? s.leaveKindLabel(l.kind), style: const TextStyle(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 4),
                  Text('$period · $amount', style: TextStyle(color: context.hr.textSecondary)),
                  const SizedBox(height: 4),
                  Text(s.balanceEffect(l.balanceEffect),
                      style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary)),
                ]),
              ),
              StatusPill(s.requestStatus(l.status), tone: tone, icon: iconForStatusTone(tone)),
            ]),
          );
        },
      ),
    );
  }
}

class _BalanceCard extends ConsumerWidget {
  const _BalanceCard();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    return AsyncBody<LeaveBalanceDto?>(
      value: ref.watch(myBalanceProvider),
      onRetry: () => ref.invalidate(myBalanceProvider),
      builder: (b) {
        if (b == null) return const SizedBox.shrink();
        if (!b.isEntitlementSet) return HrCard(child: Text(s.entitlementNotSet));
        final width = MediaQuery.sizeOf(context).width;
        final cols = width >= 1024 ? 4 : 2;
        Widget cell(String label, num v, {bool strong = false}) => Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(label, style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                const SizedBox(height: 4),
                Text(f.number(v),
                    style: TextStyle(
                      fontSize: strong ? 24 : 18,
                      fontWeight: FontWeight.w700,
                      color: strong ? context.hr.accent : null,
                      fontFeatures: const [FontFeature.tabularFigures()],
                    )),
              ],
            );
        final cells = [
          cell(s.remaining, b.remainingDays, strong: true),
          cell(s.entitlement, b.currentYearEntitlementDays),
          cell(s.carried, b.carriedDays),
          cell(s.used, b.usedDays),
          cell(s.held, b.heldDays),
          if (b.hourly.isConfigured) cell(s.hourlyQuota, b.hourly.remainingHours),
        ];
        return HrCard(
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            LayoutBuilder(builder: (context, c) {
              final w = (c.maxWidth - 16 * (cols - 1)) / cols;
              return Wrap(spacing: 16, runSpacing: 16, children: [for (final x in cells) SizedBox(width: w, child: x)]);
            }),
            const Divider(height: 24),
            Text('${f.date(b.yearStartDate)} → ${f.date(b.yearEndDate)} · ${s.nextYearStart}: ${f.date(b.nextYearStartDate)}',
                style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary)),
          ]),
        );
      },
    );
  }
}
