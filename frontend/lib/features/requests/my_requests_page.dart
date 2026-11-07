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
import 'request_widgets.dart';

/// Request_GetMy: shows employeeStatus and statusLabel, never RequestStatus.
class MyRequestsPage extends ConsumerStatefulWidget {
  const MyRequestsPage({super.key});

  @override
  ConsumerState<MyRequestsPage> createState() => _MyRequestsPageState();
}

class _MyRequestsPageState extends ConsumerState<MyRequestsPage> {
  RequestEmployeeStatus? _status;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final api = ref.watch(hrApiProvider).requests;
    final f = Fmt(context);
    return HrPage(
      title: s.myRequests,
      actions: [
        if (ref.watch(sessionProvider).can(HrOps.Request_Submit))
          FilledButton.icon(
            onPressed: () => context.go('/requests/new'),
            icon: const Icon(Icons.add),
            label: Text(s.newRequest),
          ),
      ],
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(children: [
            _filter(s.all, null),
            for (final st in RequestEmployeeStatus.values.where((v) => v != RequestEmployeeStatus.$unknown))
              _filter(s.employeeStatus(st), st),
          ]),
        ),
        const SizedBox(height: 12),
        Expanded(
          child: PagedList<MyRequestDto>(
            key: ValueKey(_status),
            fetch: (skip, max) async {
              final r = await api.Request_GetMy(
                  skipCount: skip, maxResultCount: max, sorting: 'creationTime desc', employeeStatus: _status);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, r) {
              final tone = toneForEmployeeStatus(r.employeeStatus);
              return HrCard(
                onTap: () => context.go('/requests/${r.id}'),
                child: Row(children: [
                  Expanded(
                    child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                      Text(r.type.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                      const SizedBox(height: 4),
                      Text(r.detailsSummary, style: TextStyle(color: context.hr.textSecondary)),
                      const SizedBox(height: 6),
                      Text(
                        [
                          f.dateTime(r.creationTime),
                          if (r.isOnBehalf) '${s.submittedBy}: ${r.submittedBy.displayName}',
                          if (r.stockIssueStatusLabel != null) r.stockIssueStatusLabel!,
                        ].join(' · '),
                        style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary),
                      ),
                      if (r.rejectionReason != null) ...[
                        const SizedBox(height: 6),
                        Text('${s.rejectionReason}: ${r.rejectionReason}', style: const TextStyle(color: HrColors.danger)),
                      ],
                    ]),
                  ),
                  const SizedBox(width: 12),
                  StatusPill(r.statusLabel, tone: tone, icon: iconForStatusTone(tone)),
                ]),
              );
            },
          ),
        ),
      ]),
    );
  }

  Widget _filter(String label, RequestEmployeeStatus? value) => Padding(
        padding: const EdgeInsetsDirectional.only(end: 8),
        child: ChoiceChip(
          label: Text(label),
          selected: _status == value,
          onSelected: (_) => setState(() => _status = value),
        ),
      );
}
