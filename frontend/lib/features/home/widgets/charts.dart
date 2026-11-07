import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../../../core/l10n/strings.dart';
import '../../../core/theme/motion.dart';
import '../../../core/theme/tokens.dart';

/// 0 → 1 once over [duration] (draw curve); 1 at once under reduced motion.
class DrawProgress extends StatelessWidget {
  const DrawProgress({super.key, required this.builder, this.duration = HrMotion.chartDraw, this.delay = Duration.zero});
  final Widget Function(BuildContext context, double t) builder;
  final Duration duration;
  final Duration delay;

  @override
  Widget build(BuildContext context) {
    if (HrMotion.reduced(context)) return builder(context, 1);
    return RepaintBoundary(child: _animated(context));
  }

  Widget _animated(BuildContext context) {
    final total = duration + delay;
    final startFrac = total.inMilliseconds == 0 ? 0.0 : delay.inMilliseconds / total.inMilliseconds;
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: total,
      builder: (context, v, _) {
        final local = startFrac >= 1 ? 1.0 : ((v - startFrac) / (1 - startFrac)).clamp(0.0, 1.0);
        return builder(context, HrMotion.draw.transform(local));
      },
    );
  }
}

Path _partial(Path p, double t) {
  if (t >= 1) return p;
  final out = Path();
  if (t <= 0) return out;
  for (final m in p.computeMetrics()) {
    out.addPath(m.extractPath(0, m.length * t), Offset.zero);
  }
  return out;
}

Path _smoothLine(List<Offset> pts) {
  final path = Path();
  if (pts.isEmpty) return path;
  path.moveTo(pts.first.dx, pts.first.dy);
  for (var i = 1; i < pts.length; i++) {
    final a = pts[i - 1];
    final b = pts[i];
    final mx = (a.dx + b.dx) / 2;
    path.cubicTo(mx, a.dy, mx, b.dy, b.dx, b.dy);
  }
  return path;
}

/// Small trend line for KPI cards; drawn once like the network links.
class Sparkline extends StatelessWidget {
  const Sparkline({super.key, required this.values, this.height = 36, this.color});
  final List<double> values;
  final double height;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    final col = color ?? context.hr.accent;
    return SizedBox(
      height: height,
      child: Directionality(
        textDirection: TextDirection.ltr, // time runs left → right in both languages
        child: DrawProgress(
          duration: const Duration(milliseconds: 1100),
          delay: const Duration(milliseconds: 250),
          builder: (context, t) => CustomPaint(size: Size.infinite, painter: _SparkPainter(values, col, t)),
        ),
      ),
    );
  }
}

class _SparkPainter extends CustomPainter {
  _SparkPainter(this.values, this.color, this.t);
  final List<double> values;
  final Color color;
  final double t;

  @override
  void paint(Canvas canvas, Size size) {
    if (values.length < 2) return;
    final maxV = values.reduce(math.max);
    final minV = values.reduce(math.min);
    final span = (maxV - minV).abs() < 1e-9 ? 1.0 : maxV - minV;
    final pts = [
      for (var i = 0; i < values.length; i++)
        Offset(size.width * i / (values.length - 1), size.height - 3 - (size.height - 6) * (values[i] - minV) / span),
    ];
    final line = _smoothLine(pts);
    final area = Path.from(line)
      ..lineTo(size.width, size.height)
      ..lineTo(0, size.height)
      ..close();
    canvas.save();
    canvas.clipRect(Rect.fromLTWH(0, 0, size.width * t, size.height));
    canvas.drawPath(
      area,
      Paint()
        ..shader = LinearGradient(begin: Alignment.topCenter, end: Alignment.bottomCenter, colors: [color.withValues(alpha: .22), color.withValues(alpha: 0)])
            .createShader(Offset.zero & size),
    );
    canvas.restore();
    canvas.drawPath(
      _partial(line, t),
      Paint()
        ..color = color
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2
        ..strokeCap = StrokeCap.round,
    );
  }

  @override
  bool shouldRepaint(_SparkPainter old) => old.t != t || old.values != values || old.color != color;
}

/// Card frame for charts, with a table alternative for screen readers and keyboard users.
class ChartCard extends StatefulWidget {
  const ChartCard({super.key, required this.title, required this.chart, required this.table, this.subtitle, this.semanticsSummary});
  final String title;
  final String? subtitle;
  final Widget chart;
  final List<(String, String)> table;
  final String? semanticsSummary;

  @override
  State<ChartCard> createState() => _ChartCardState();
}

class _ChartCardState extends State<ChartCard> {
  bool _table = false;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final c = context.hr;
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: c.surface,
        borderRadius: BorderRadius.circular(HrColors.radiusCard),
        border: Border.all(color: c.border),
        boxShadow: c.shadow,
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Row(children: [
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Semantics(header: true, child: Text(widget.title, style: Theme.of(context).textTheme.titleMedium)),
              if (widget.subtitle != null)
                Text(widget.subtitle!, style: TextStyle(color: c.textSecondary, fontSize: 13)),
            ]),
          ),
          IconButton(
            tooltip: _table ? s.t('عرض كرسم', 'Show chart') : s.t('عرض كجدول', 'Show as table'),
            onPressed: () => setState(() => _table = !_table),
            icon: Icon(_table ? Icons.insert_chart_outlined : Icons.table_rows_outlined, color: c.textSecondary),
          ),
        ]),
        const SizedBox(height: 12),
        if (_table)
          Column(children: [
            for (final (k, v) in widget.table)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 6),
                child: Row(children: [
                  Expanded(child: Text(k, style: TextStyle(color: c.textSecondary))),
                  Text(v, style: const TextStyle(fontWeight: FontWeight.w600, fontFeatures: [FontFeature.tabularFigures()])),
                ]),
              ),
          ])
        else
          Semantics(label: widget.semanticsSummary ?? widget.title, child: ExcludeSemantics(child: widget.chart)),
      ]),
    );
  }
}

/// Line chart (one series), drawn with the dashoffset technique.
class LineChart extends StatelessWidget {
  const LineChart({super.key, required this.values, required this.labels, this.height = 200, this.unit = ''});
  final List<double> values;
  final List<String> labels; // same length as values; a few are shown
  final double height;
  final String unit;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    final text = TextStyle(color: c.textSecondary, fontSize: 11, fontFeatures: const [FontFeature.tabularFigures()]);
    return SizedBox(
      height: height,
      child: Directionality(
        textDirection: TextDirection.ltr,
        child: DrawProgress(
          builder: (context, t) => CustomPaint(
            size: Size.infinite,
            painter: _LinePainter(values: values, labels: labels, t: t, accent: c.accent, grid: c.border, text: text, unit: unit),
          ),
        ),
      ),
    );
  }
}

class _LinePainter extends CustomPainter {
  _LinePainter({required this.values, required this.labels, required this.t, required this.accent, required this.grid, required this.text, required this.unit});
  final List<double> values;
  final List<String> labels;
  final double t;
  final Color accent;
  final Color grid;
  final TextStyle text;
  final String unit;

  void _label(Canvas canvas, String s, Offset at, {bool center = true}) {
    final tp = TextPainter(text: TextSpan(text: s, style: text), textDirection: TextDirection.ltr)..layout();
    tp.paint(canvas, at - Offset(center ? tp.width / 2 : 0, 0));
  }

  @override
  void paint(Canvas canvas, Size size) {
    const left = 32.0, bottom = 22.0, top = 8.0;
    final w = size.width - left - 8;
    final h = size.height - bottom - top;
    final maxV = values.isEmpty ? 1.0 : math.max(1.0, values.reduce(math.max));
    final niceMax = (maxV / 2).ceil() * 2.0;
    final gridPaint = Paint()
      ..color = grid
      ..strokeWidth = 1;
    for (var i = 0; i <= 4; i++) {
      final y = top + h - h * i / 4;
      canvas.drawLine(Offset(left, y), Offset(left + w, y), gridPaint);
      _label(canvas, '${(niceMax * i / 4).round()}$unit', Offset(0, y - 7), center: false);
    }
    if (values.length < 2) return;
    final pts = [
      for (var i = 0; i < values.length; i++) Offset(left + w * i / (values.length - 1), top + h - h * values[i] / niceMax),
    ];
    final step = math.max(1, (values.length / 6).ceil());
    for (var i = 0; i < labels.length; i += step) {
      _label(canvas, labels[i], Offset(pts[i].dx, top + h + 6));
    }
    final line = _smoothLine(pts);
    final area = Path.from(line)
      ..lineTo(pts.last.dx, top + h)
      ..lineTo(pts.first.dx, top + h)
      ..close();
    canvas.save();
    canvas.clipRect(Rect.fromLTWH(left, 0, w * t, size.height));
    canvas.drawPath(
      area,
      Paint()
        ..shader = LinearGradient(begin: Alignment.topCenter, end: Alignment.bottomCenter, colors: [accent.withValues(alpha: .24), accent.withValues(alpha: 0)])
            .createShader(Rect.fromLTWH(0, top, size.width, h)),
    );
    canvas.restore();
    canvas.drawPath(
      _partial(line, t),
      Paint()
        ..color = accent
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2.5
        ..strokeCap = StrokeCap.round,
    );
  }

  @override
  bool shouldRepaint(_LinePainter old) => old.t != t || old.values != values || old.accent != accent;
}

/// Labeled bars that grow from the baseline (scaleY from 0), staggered.
class BarsChart extends StatelessWidget {
  const BarsChart({super.key, required this.bars, this.height = 180});
  final List<(String label, double value, Color color, IconData icon)> bars;
  final double height;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    final maxV = bars.isEmpty ? 1.0 : math.max(1.0, bars.map((b) => b.$2).reduce(math.max));
    return SizedBox(
      height: height + 56,
      child: Row(crossAxisAlignment: CrossAxisAlignment.end, children: [
        for (var i = 0; i < bars.length; i++)
          Expanded(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 6),
              child: Column(mainAxisAlignment: MainAxisAlignment.end, children: [
                Text(_fmt(bars[i].$2), style: const TextStyle(fontWeight: FontWeight.w700, fontFeatures: [FontFeature.tabularFigures()])),
                const SizedBox(height: 4),
                DrawProgress(
                  duration: const Duration(milliseconds: 900),
                  delay: Duration(milliseconds: 90 * i),
                  builder: (context, t) => Container(
                    height: math.max(4, height * bars[i].$2 / maxV),
                    alignment: Alignment.bottomCenter,
                    child: Transform(
                      alignment: Alignment.bottomCenter,
                      transform: Matrix4.diagonal3Values(1, t, 1),
                      child: Container(
                        decoration: BoxDecoration(
                          color: bars[i].$3.withValues(alpha: .85),
                          borderRadius: const BorderRadius.vertical(top: Radius.circular(8)),
                        ),
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 6),
                Row(mainAxisAlignment: MainAxisAlignment.center, mainAxisSize: MainAxisSize.min, children: [
                  Icon(bars[i].$4, size: 14, color: bars[i].$3),
                  const SizedBox(width: 4),
                  Flexible(
                    child: Text(bars[i].$1,
                        maxLines: 1, overflow: TextOverflow.ellipsis, style: TextStyle(color: c.textSecondary, fontSize: 12)),
                  ),
                ]),
              ]),
            ),
          ),
      ]),
    );
  }

  static String _fmt(double v) => v % 1 == 0 ? v.toInt().toString() : v.toStringAsFixed(1);
}

/// Donut whose segments grow by stroke length from 0; legend carries text and values.
class DonutChart extends StatelessWidget {
  const DonutChart({super.key, required this.segments, required this.centerValue, required this.centerLabel, this.size = 168});
  final List<(String label, double value, Color color)> segments;
  final String centerValue;
  final String centerLabel;
  final double size;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    return Wrap(
      alignment: WrapAlignment.center,
      crossAxisAlignment: WrapCrossAlignment.center,
      spacing: 24,
      runSpacing: 16,
      children: [
        SizedBox(
          width: size,
          height: size,
          child: Stack(alignment: Alignment.center, children: [
            DrawProgress(
              builder: (context, t) => CustomPaint(size: Size.square(size), painter: _DonutPainter(segments, t, c.border)),
            ),
            Column(mainAxisSize: MainAxisSize.min, children: [
              Text(centerValue,
                  style: const TextStyle(fontSize: 26, fontWeight: FontWeight.w700, fontFeatures: [FontFeature.tabularFigures()])),
              Text(centerLabel, style: TextStyle(color: c.textSecondary, fontSize: 12)),
            ]),
          ]),
        ),
        Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
          for (final seg in segments)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 4),
              child: Row(mainAxisSize: MainAxisSize.min, children: [
                Container(width: 10, height: 10, decoration: BoxDecoration(color: seg.$3, borderRadius: BorderRadius.circular(3))),
                const SizedBox(width: 8),
                Text(seg.$1, style: TextStyle(color: c.textSecondary)),
                const SizedBox(width: 8),
                Text(BarsChart._fmt(seg.$2),
                    style: const TextStyle(fontWeight: FontWeight.w700, fontFeatures: [FontFeature.tabularFigures()])),
              ]),
            ),
        ]),
      ],
    );
  }
}

class _DonutPainter extends CustomPainter {
  _DonutPainter(this.segments, this.t, this.track);
  final List<(String, double, Color)> segments;
  final double t;
  final Color track;

  @override
  void paint(Canvas canvas, Size size) {
    const stroke = 16.0;
    final rect = (Offset.zero & size).deflate(stroke / 2);
    canvas.drawArc(rect, 0, 2 * math.pi, false, Paint()
      ..color = track
      ..style = PaintingStyle.stroke
      ..strokeWidth = stroke);
    final total = segments.fold<double>(0, (a, s) => a + s.$2);
    if (total <= 0) return;
    var start = -math.pi / 2;
    const gap = 0.04;
    for (final s in segments) {
      final sweep = 2 * math.pi * s.$2 / total;
      final drawn = math.max(0.0, (sweep - gap) * t);
      if (drawn > 0) {
        canvas.drawArc(rect, start, drawn, false, Paint()
          ..color = s.$3
          ..style = PaintingStyle.stroke
          ..strokeWidth = stroke
          ..strokeCap = StrokeCap.round);
      }
      start += sweep;
    }
  }

  @override
  bool shouldRepaint(_DonutPainter old) => old.t != t || old.segments != segments;
}
