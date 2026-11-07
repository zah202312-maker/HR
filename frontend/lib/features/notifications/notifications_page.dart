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
import 'unread_count.dart';

/// S-NOT-01: Notification_List / Notification_MarkRead. Title and body come from the server.
class NotificationsPage extends ConsumerStatefulWidget {
  const NotificationsPage({super.key});

  @override
  ConsumerState<NotificationsPage> createState() => _NotificationsPageState();
}

class _NotificationsPageState extends ConsumerState<NotificationsPage> {
  bool _unreadOnly = false;
  final Set<String> _readNow = <String>{};

  Future<void> _open(NotificationDto n) async {
    if (!n.isRead && !_readNow.contains(n.id)) {
      try {
        await ref.read(hrApiProvider).notifications.Notification_MarkRead(id: n.id);
        setState(() => _readNow.add(n.id));
        ref.invalidate(unreadCountProvider);
      } catch (e) {
        if (mounted) showError(context, e);
      }
    }
    if (!mounted) return;
    final id = n.resourceId;
    if (id == null) return;
    switch (n.resourceType) {
      case NotificationResourceType.request:
        context.go('/requests/$id');
      case NotificationResourceType.memo:
        context.go('/memos/$id');
      case NotificationResourceType.incident:
        context.go('/incidents/$id');
      case NotificationResourceType.secureItem:
        // One code for complainant and handler: the open page tries my/{id} first.
        context.go('/complaints/open/$id');
      case NotificationResourceType.stockIssue:
        context.go('/store');
      case NotificationResourceType.custody:
        context.go('/custody');
      case NotificationResourceType.circular:
        context.go('/circulars/$id');
      case NotificationResourceType.evaluation:
        // employee, manager or HR: the open page picks the view the caller may read
        context.go('/evaluations/open/$id');
      case NotificationResourceType.contract:
        context.go(ref.read(sessionProvider).can(HrOps.Contract_Get) ? '/contracts/$id' : '/contracts');
      case NotificationResourceType.warning:
        const own = {
          NotificationTemplateCode.warningNotified,
          NotificationTemplateCode.warningDecided,
          NotificationTemplateCode.warningObjectionResolved,
        };
        context.go(own.contains(n.templateCode) ? '/warnings/my/$id' : '/warnings/$id');
      case NotificationResourceType.appreciationLetter:
        context.go('/appreciation');
      case NotificationResourceType.payrollRun:
        context.go('/finance/payroll/$id');
      case NotificationResourceType.payslip:
        context.go('/payslips');
      case NotificationResourceType.adjustmentSuggestion:
        context.go('/finance');
      case NotificationResourceType.reviewReport:
        context.go('/review-reports/$id');
      default:
        break;
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).notifications;
    return HrPage(
      title: s.notifications,
      subtitle: s.markAllHint,
      actions: [
        FilterChip(
          label: Text(s.onlyUnread),
          selected: _unreadOnly,
          onSelected: (v) => setState(() => _unreadOnly = v),
        ),
      ],
      child: PagedList<NotificationDto>(
        key: ValueKey(_unreadOnly),
        fetch: (skip, max) async {
          final r = await api.Notification_List(
              skipCount: skip, maxResultCount: max, isRead: _unreadOnly ? false : null, sorting: 'creationTime desc');
          return (r.items, r.totalCount);
        },
        emptyMessage: s.empty,
        itemBuilder: (context, n) {
          final read = n.isRead || _readNow.contains(n.id);
          return HrCard(
            onTap: () => _open(n),
            child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Padding(
                padding: const EdgeInsets.only(top: 6),
                child: Icon(Icons.circle, size: 10, color: read ? Colors.transparent : context.hr.accent),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(n.title, style: TextStyle(fontWeight: read ? FontWeight.w500 : FontWeight.w700)),
                  const SizedBox(height: 4),
                  Text(n.body, style: TextStyle(color: context.hr.textSecondary)),
                  const SizedBox(height: 6),
                  Text(f.dateTime(n.creationTime), style: Theme.of(context).textTheme.labelSmall),
                ]),
              ),
              if (n.count != null && n.count! > 1) StatusPill('${n.count}', tone: Tone.info),
            ]),
          );
        },
      ),
    );
  }
}
