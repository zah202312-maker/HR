import 'package:flutter/material.dart';

import '../../../core/theme/tokens.dart';
import '../../../core/widgets/motion_widgets.dart';
import 'charts.dart';

/// KPI: label, counted-up number, caption, optional trend line and status (icon + text, never color alone).
class KpiCard extends StatelessWidget {
  const KpiCard({
    super.key,
    required this.icon,
    required this.label,
    required this.value,
    this.decimals = 0,
    this.unit = '',
    this.caption,
    this.trend,
    this.status,
    this.onTap,
  });
  final IconData icon;
  final String label;
  final num? value;
  final int decimals;
  final String unit;
  final String? caption;
  final List<double>? trend;
  final (String text, IconData icon, Color color)? status;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    return Semantics(
      button: onTap != null,
      label: '$label ${value ?? ''} $unit',
      child: TiltSpotlightCard(
        onTap: onTap,
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, mainAxisSize: MainAxisSize.min, children: [
          Row(children: [
            Container(
              width: 36,
              height: 36,
              decoration: BoxDecoration(color: c.glow, borderRadius: BorderRadius.circular(HrColors.radiusControl)),
              child: Icon(icon, color: c.accent, size: 20),
            ),
            const SizedBox(width: 10),
            Expanded(child: Text(label, style: TextStyle(color: c.textSecondary, fontWeight: FontWeight.w600, fontSize: 13))),
            if (status != null) _StatusPill(status!),
          ]),
          const SizedBox(height: 14),
          Row(crossAxisAlignment: CrossAxisAlignment.end, children: [
            if (value == null)
              const Skeleton(width: 72, height: 30)
            else
              CountUp(value: value!, decimals: decimals, style: const TextStyle(fontSize: 30, fontWeight: FontWeight.w700, height: 1.1)),
            if (unit.isNotEmpty && value != null) ...[
              const SizedBox(width: 6),
              Text(unit, style: TextStyle(color: c.textSecondary, fontSize: 13)),
            ],
          ]),
          if (caption != null) ...[
            const SizedBox(height: 4),
            Text(caption!, maxLines: 2, overflow: TextOverflow.ellipsis, style: TextStyle(color: c.textSecondary, fontSize: 12)),
          ],
          if (trend != null && trend!.length > 1) ...[
            const SizedBox(height: 10),
            Sparkline(values: trend!),
          ],
        ]),
      ),
    );
  }
}

class _StatusPill extends StatelessWidget {
  const _StatusPill(this.s);
  final (String, IconData, Color) s;

  @override
  Widget build(BuildContext context) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
        decoration: BoxDecoration(color: s.$3.withValues(alpha: .12), borderRadius: BorderRadius.circular(HrColors.radiusBadge)),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          Icon(s.$2, size: 13, color: s.$3),
          const SizedBox(width: 4),
          // text in the primary color keeps AA contrast; the hue is carried by the icon and tint
          Text(s.$1, style: TextStyle(color: context.hr.textPrimary, fontSize: 11, fontWeight: FontWeight.w700)),
        ]),
      );
}
