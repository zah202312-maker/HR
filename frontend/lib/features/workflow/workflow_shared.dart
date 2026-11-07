import 'package:flutter/material.dart';

import '../../api/generated/models.dart';
import '../../core/l10n/strings.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';

/// Timeline card (TimelineEntryDto). Action codes come from the contract; labels are localized here.
class TimelineCard extends StatelessWidget {
  const TimelineCard({super.key, required this.entries});
  final List<TimelineEntryDto> entries;

  String _action(S s, String a) => switch (a) {
        'submitted' => s.t('قُدّم', 'Submitted'),
        'forwardedToHr' => s.t('حُوّل إلى الموارد البشرية', 'Forwarded to HR'),
        'convertedToRequest' => s.t('تحوّل إلى طلب إجازة', 'Converted to leave request'),
        'convertedToIncident' => s.t('تحوّل إلى بلاغ', 'Converted to incident'),
        'closed' => s.t('أُغلق', 'Closed'),
        'inProgress' => s.t('قيد المعالجة', 'In progress'),
        'resolved' => s.t('تم الحل', 'Resolved'),
        _ => a,
      };

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    return HrCard(
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        SectionTitle(s.timeline),
        for (final t in entries)
          ListTile(
            contentPadding: EdgeInsets.zero,
            leading: Icon(Icons.circle, size: 10, color: context.hr.accent),
            title: Text(_action(s, t.action)),
            subtitle: Text([f.dateTime(t.occurredAt), if (t.actor != null) t.actor!.displayName, if (t.note != null) t.note!].join(' · ')),
          ),
      ]),
    );
  }
}

Tone memoTone(MemoStatus st) => switch (st) {
      MemoStatus.closed => Tone.neutral,
      MemoStatus.convertedToIncident || MemoStatus.convertedToRequest => Tone.success,
      _ => Tone.warning,
    };

Tone incidentTone(IncidentStatus st) => switch (st) {
      IncidentStatus.resolved => Tone.success,
      IncidentStatus.inProgress => Tone.info,
      _ => Tone.warning,
    };

String memoKindLabel(S s, MemoKind k) => k == MemoKind.leave ? s.t('إجازة', 'Leave') : s.t('مشكلة', 'Problem');
