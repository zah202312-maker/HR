import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import 'request_widgets.dart';

/// Request_GetInbox: the unified "awaiting my action" box, including delegated items.
class InboxPage extends ConsumerWidget {
  const InboxPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).requests;
    return HrPage(
      title: s.inbox,
      child: PagedList<RequestInboxItemDto>(
        fetch: (skip, max) async {
          final r = await api.Request_GetInbox(skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
          return (r.items, r.totalCount);
        },
        emptyMessage: s.nothingPending,
        itemBuilder: (context, r) {
          final tone = toneForStatus(r.status);
          return HrCard(
            onTap: () => context.go('/requests/${r.id}'),
            child: Row(children: [
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text('${r.type.name} — ${r.employee.fullName}', style: const TextStyle(fontWeight: FontWeight.w600)),
                  if (r.detailsSummary != null) ...[
                    const SizedBox(height: 4),
                    Text(r.detailsSummary!, style: TextStyle(color: context.hr.textSecondary)),
                  ],
                  const SizedBox(height: 6),
                  Text(
                    [
                      if (r.employee.departmentName != null) r.employee.departmentName!,
                      '${s.waitingSince}: ${f.dateTime(r.waitingSince)}',
                      if (r.isOnBehalf) '${s.submittedBy}: ${r.submittedBy.displayName}',
                      if (r.delegatedFrom != null) '${s.delegatedFrom}: ${r.delegatedFrom!.displayName}',
                      if (r.actionKind == RequestInboxActionKind.stockIssue) s.stockIssueFlow,
                    ].join(' · '),
                    style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary),
                  ),
                ]),
              ),
              const SizedBox(width: 12),
              StatusPill(s.requestStatus(r.status), tone: tone, icon: iconForStatusTone(tone)),
            ]),
          );
        },
      ),
    );
  }
}
