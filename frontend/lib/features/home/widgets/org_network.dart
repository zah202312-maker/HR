import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../../../core/l10n/strings.dart';
import '../../../core/theme/motion.dart';
import '../../../core/theme/tokens.dart';
import '../../../core/widgets/hr_logo.dart';

/// Abstract organizational network for the welcome hero (viewBox 440 × 280):
/// the project mark at the top, three departments, three employees under each.
/// Intro ≈ 2.5 s (logo spring → department links → department nodes → employee links → employees),
/// then only light continuous motion: pulses along a few links, a halo around the mark, node drift.
/// Static (complete, no motion) under reduced motion or when [static] is set (small screens).
class OrgNetwork extends StatefulWidget {
  const OrgNetwork({super.key, this.static = false, this.maxWidth = 480});
  final bool static;
  final double maxWidth;

  static const viewBox = Size(440, 280);

  @override
  State<OrgNetwork> createState() => _OrgNetworkState();
}

class _OrgNetworkState extends State<OrgNetwork> with TickerProviderStateMixin {
  late final AnimationController _intro = AnimationController(vsync: this, duration: HrMotion.netTotal);
  late final AnimationController _pulse = AnimationController(vsync: this, duration: HrMotion.netPulse);
  late final AnimationController _float = AnimationController(vsync: this, duration: HrMotion.netFloat);
  Offset _pointer = Offset.zero;
  bool _configured = false;
  bool _isStatic = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    final s = widget.static || HrMotion.reduced(context);
    if (_configured && s == _isStatic) return;
    _configured = true;
    _isStatic = s;
    if (s) {
      _intro.value = 1;
      _pulse.stop();
      _float.stop();
      _pulse.value = 0;
      _float.value = 0;
    } else {
      // Intro plays once; continuous motion starts after it, never restarting the scene.
      _intro.forward().whenComplete(() {
        if (!mounted || _isStatic) return;
        _pulse.repeat();
        _float.repeat();
      });
    }
  }

  @override
  void dispose() {
    _intro.dispose();
    _pulse.dispose();
    _float.dispose();
    super.dispose();
  }

  void _hover(PointerEvent e, Size size) {
    if (_isStatic || !mounted || size.isEmpty) return;
    _setPointer(Offset(
      ((e.localPosition.dx / size.width) * 2 - 1).clamp(-1.0, 1.0),
      ((e.localPosition.dy / size.height) * 2 - 1).clamp(-1.0, 1.0),
    ));
  }

  /// Pointer callbacks run inside the mouse tracker's update; rebuild on the next frame.
  void _setPointer(Offset p) {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted && p != _pointer) setState(() => _pointer = p);
    });
    WidgetsBinding.instance.scheduleFrame();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final c = context.hr;
    final dark = Theme.of(context).brightness == Brightness.dark;
    return RepaintBoundary(child: Semantics(
      container: true,
      label: 'HR Organizational Network',
      value: s.t('رسم تجريدي: الإدارة في الأعلى، وثلاثة أقسام، وتحت كل قسم ثلاثة موظفين.',
          'Abstract drawing: management at the top, three departments, three employees under each.'),
      child: ExcludeSemantics(
        child: ConstrainedBox(
          constraints: BoxConstraints(maxWidth: widget.maxWidth),
          child: AspectRatio(
            aspectRatio: OrgNetwork.viewBox.width / OrgNetwork.viewBox.height,
            child: LayoutBuilder(builder: (context, box) {
              final size = box.biggest;
              final k = size.width / OrgNetwork.viewBox.width;
              return MouseRegion(
                onHover: (e) => _hover(e, size),
                onExit: (_) => _setPointer(Offset.zero),
                child: TweenAnimationBuilder<Offset>(
                  tween: Tween(end: _pointer),
                  duration: const Duration(milliseconds: 420),
                  curve: HrMotion.enter,
                  builder: (context, p, _) {
                    const deg = math.pi / 180;
                    return Transform(
                      alignment: Alignment.center,
                      transform: Matrix4.identity()
                        ..setEntry(3, 2, HrMotion.netPerspective)
                        ..rotateX(-p.dy * 3 * deg)
                        ..rotateY(p.dx * 4 * deg),
                      child: AnimatedBuilder(
                        animation: Listenable.merge([_intro, _pulse, _float]),
                        builder: (context, _) {
                          final ms = _intro.value * HrMotion.netTotal.inMilliseconds;
                          final geo = _NetGeometry(floatT: _isStatic ? null : _float.value, pointer: p);
                          return Stack(clipBehavior: Clip.none, children: [
                            Positioned.fill(
                              child: CustomPaint(
                                isComplex: true,
                                willChange: !_isStatic,
                                painter: _NetPainter(
                                  geo: geo,
                                  ms: ms,
                                  pulse: _isStatic ? null : _pulse.value,
                                  k: k,
                                  accent: c.accent,
                                  surface: c.surface,
                                  dark: dark,
                                ),
                              ),
                            ),
                            _logo(geo, ms, k),
                          ]);
                        },
                      ),
                    );
                  },
                ),
              );
            }),
          ),
        ),
      ),
    ));
  }

  /// The mark inside the center container: scale 0.7 → 1, −12° → 0, opacity 0 → 1 (spring, 0.9 s),
  /// then still. Its height leaves ≥25% clear space inside the container.
  Widget _logo(_NetGeometry geo, double ms, double k) {
    final t = timelineValue(ms, 0, HrMotion.netLogoIn, HrMotion.spring);
    final o = timelineValue(ms, 0, const Duration(milliseconds: 400));
    const h = _NetGeometry.logoHeight;
    final center = geo.center;
    final box = h * 1.6; // layout box around the mark; the image keeps its own ratio inside
    final scale = HrMotion.netLogoScaleFrom + (1 - HrMotion.netLogoScaleFrom) * t;
    final turn = HrMotion.netLogoTurnFrom * (1 - t) * math.pi / 180;
    return Positioned(
      left: (center.dx - box / 2) * k,
      top: (center.dy - box / 2) * k,
      width: box * k,
      height: box * k,
      child: Opacity(
        opacity: o,
        child: Transform.rotate(
          angle: turn,
          child: Transform.scale(scale: scale, child: Center(child: HrMark(height: h * k, semanticLabel: null))),
        ),
      ),
    );
  }
}

class _NetGeometry {
  _NetGeometry({required this.floatT, required this.pointer});
  final double? floatT;
  final Offset pointer;

  static const centerBase = Offset(220, 60);
  static const containerR = 40.0;
  static const logoHeight = 34.0;
  static const deptY = 152.0;
  static const deptR = 18.0;
  static const deptX = [92.0, 220.0, 348.0];
  static const empY = 240.0;
  static const empR = 7.0;
  static const empSpread = 42.0;

  // parallax depth per layer (px in viewBox units at full pointer deflection)
  static const _depthCenter = 2.0;
  static const _depthDept = 5.0;
  static const _depthEmp = 8.0;

  double _drift(int i) {
    final t = floatT;
    if (t == null) return 0;
    return HrMotion.netFloatDistance * math.sin(2 * math.pi * (t + i * .137));
  }

  Offset get center => centerBase + pointer * _depthCenter;
  Offset dept(int d) => Offset(deptX[d], deptY + _drift(d + 1)) + pointer * _depthDept;
  Offset emp(int d, int e) => Offset(deptX[d] + (e - 1) * empSpread, empY + _drift(4 + d * 3 + e)) + pointer * _depthEmp;

  /// Department link: leaves from the container's outer edge (never across the mark).
  Path deptLink(int d) {
    final a = center;
    final b = dept(d);
    final dir = (b - a);
    final start = a + dir / dir.distance * (containerR + 5);
    final end = b - Offset(0, deptR + 2);
    return Path()
      ..moveTo(start.dx, start.dy)
      ..cubicTo(start.dx + (end.dx - start.dx) * .15, start.dy + 40, end.dx, end.dy - 46, end.dx, end.dy);
  }

  Path empLink(int d, int e) {
    final a = dept(d) + const Offset(0, deptR + 2);
    final b = emp(d, e) - const Offset(0, empR + 2);
    return Path()
      ..moveTo(a.dx, a.dy)
      ..cubicTo(a.dx, a.dy + 30, b.dx, b.dy - 30, b.dx, b.dy);
  }

  static int empLinkDelay(int d, int e) =>
      HrMotion.netEmployeeLinkStart + e * HrMotion.netEmployeeLinkStep + d * HrMotion.netEmployeeGroupStep;
}

class _NetPainter extends CustomPainter {
  _NetPainter({required this.geo, required this.ms, required this.pulse, required this.k, required this.accent, required this.surface, required this.dark});
  final _NetGeometry geo;
  final double ms;
  final double? pulse;
  final double k;
  final Color accent;
  final Color surface;
  final bool dark;

  static Path _partial(Path p, double t) {
    if (t >= 1) return p;
    final out = Path();
    if (t <= 0) return out;
    for (final m in p.computeMetrics()) {
      out.addPath(m.extractPath(0, m.length * t), Offset.zero);
    }
    return out;
  }

  @override
  void paint(Canvas canvas, Size size) {
    canvas.save();
    canvas.scale(k);

    final link = Paint()
      ..color = accent.withValues(alpha: .4)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.5
      ..strokeCap = StrokeCap.round;

    // 1) department links
    for (var d = 0; d < 3; d++) {
      final t = timelineValue(ms, HrMotion.netDeptLinkDelays[d], HrMotion.netLinkDraw, Curves.easeOut);
      canvas.drawPath(_partial(geo.deptLink(d), t), link);
    }
    // 2) employee links
    for (var d = 0; d < 3; d++) {
      for (var e = 0; e < 3; e++) {
        final t = timelineValue(ms, _NetGeometry.empLinkDelay(d, e), HrMotion.netEmployeeLinkDraw, Curves.easeOut);
        canvas.drawPath(_partial(geo.empLink(d, e), t), link);
      }
    }

    // 3) center container: glow (fixed blur, breathing opacity), halo ring, container
    final logoT = timelineValue(ms, 0, HrMotion.netLogoIn, HrMotion.spring);
    final logoO = timelineValue(ms, 0, const Duration(milliseconds: 400));
    final c = geo.center;
    final r = _NetGeometry.containerR * (HrMotion.netLogoScaleFrom + (1 - HrMotion.netLogoScaleFrom) * logoT);
    final wave = pulse == null ? .5 : .5 - .5 * math.cos(2 * math.pi * pulse!); // 0 → 1 → 0 per cycle
    final glowStrength = pulse == null ? .5 : .35 + .5 * wave;
    final glowR = r + 18;
    final glowA = (dark ? .35 : .22) * glowStrength * logoO;
    canvas.drawCircle(
      c,
      glowR,
      Paint()
        ..shader = RadialGradient(
          colors: [accent.withValues(alpha: glowA), accent.withValues(alpha: 0)],
          stops: [r / glowR, 1],
        ).createShader(Rect.fromCircle(center: c, radius: glowR)),
    );
    final haloScale = 1 + (HrMotion.netHaloScale - 1) * wave;
    final haloOpacity = 1 - (1 - HrMotion.netHaloMinOpacity) * wave;
    canvas.drawCircle(
      c,
      (r + 7) * haloScale,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1
        ..color = accent.withValues(alpha: .45 * haloOpacity * logoO),
    );
    canvas.drawCircle(c, r, Paint()..color = surface.withValues(alpha: logoO));
    canvas.drawCircle(
      c,
      r,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.5
        ..color = accent.withValues(alpha: .9 * logoO),
    );

    // 4) department nodes (scale 0.4 → 1, spring)
    for (var d = 0; d < 3; d++) {
      final start = HrMotion.netDeptLinkDelays[d] + 600;
      final t = timelineValue(ms, start, HrMotion.netDeptNodeIn, HrMotion.spring);
      if (t <= 0) continue;
      final o = timelineValue(ms, start, const Duration(milliseconds: 250));
      final p = geo.dept(d);
      final rr = _NetGeometry.deptR * (HrMotion.netDeptNodeScaleFrom + (1 - HrMotion.netDeptNodeScaleFrom) * t);
      canvas.drawCircle(p, rr + 3, Paint()..color = accent.withValues(alpha: .10 * o));
      canvas.drawCircle(p, rr, Paint()..color = surface.withValues(alpha: o));
      canvas.drawCircle(
        p,
        rr,
        Paint()
          ..style = PaintingStyle.stroke
          ..strokeWidth = 1.5
          ..color = accent.withValues(alpha: .85 * o),
      );
      canvas.drawCircle(p, rr * .34, Paint()..color = accent.withValues(alpha: .9 * o));
    }

    // 5) employee nodes, last
    for (var d = 0; d < 3; d++) {
      for (var e = 0; e < 3; e++) {
        final start = _NetGeometry.empLinkDelay(d, e) + 420;
        final t = timelineValue(ms, start, HrMotion.netEmployeeNodeIn, HrMotion.spring);
        if (t <= 0) continue;
        final p = geo.emp(d, e);
        final rr = _NetGeometry.empR * (.5 + .5 * t);
        canvas.drawCircle(p, rr, Paint()..color = surface.withValues(alpha: t.clamp(0, 1)));
        canvas.drawCircle(
          p,
          rr,
          Paint()
            ..style = PaintingStyle.stroke
            ..strokeWidth = 1.2
            ..color = accent.withValues(alpha: .7 * t.clamp(0, 1)),
        );
        canvas.drawCircle(p, rr * .45, Paint()..color = accent.withValues(alpha: .55 * t.clamp(0, 1)));
      }
    }

    // 6) light pulses on selected links (linear, 5 s, fade at both ends)
    final pv = pulse;
    if (pv != null) {
      final paths = [geo.deptLink(0), geo.deptLink(2), geo.empLink(1, 1), geo.deptLink(1)];
      const phases = [0.0, .5, .25, .75];
      for (var i = 0; i < paths.length; i++) {
        final t = (pv + phases[i]) % 1.0;
        final metrics = paths[i].computeMetrics().toList();
        if (metrics.isEmpty) continue;
        final m = metrics.first;
        final tan = m.getTangentForOffset(m.length * t);
        if (tan == null) continue;
        final fade = math.sin(math.pi * t); // 0 → 1 → 0
        canvas.drawCircle(
          tan.position,
          7,
          Paint()
            ..shader = RadialGradient(colors: [accent.withValues(alpha: .55 * fade), accent.withValues(alpha: 0)])
                .createShader(Rect.fromCircle(center: tan.position, radius: 7)),
        );
        canvas.drawCircle(tan.position, 2.2, Paint()..color = accent.withValues(alpha: fade));
      }
    }
    canvas.restore();
  }

  @override
  bool shouldRepaint(_NetPainter old) => true;
}
