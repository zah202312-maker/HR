import 'dart:math' as math;
import 'dart:ui' show ImageFilter;

import 'package:flutter/material.dart';
import 'package:flutter/semantics.dart';
import 'package:intl/intl.dart' show NumberFormat;

import '../theme/motion.dart';
import '../theme/tokens.dart';

/// Opacity + short vertical shift on first build (cards, pages). Plays once; never on rebuilds.
class FadeSlideIn extends StatefulWidget {
  const FadeSlideIn({
    super.key,
    required this.child,
    this.delay = Duration.zero,
    this.duration = HrMotion.cardIn,
    this.offset = const Offset(0, HrMotion.cardShift),
    this.curve = HrMotion.enter,
  });
  final Widget child;
  final Duration delay;
  final Duration duration;

  /// Logical pixels; x follows the reading direction (positive = from the start side).
  final Offset offset;
  final Curve curve;

  @override
  State<FadeSlideIn> createState() => _FadeSlideInState();
}

class _FadeSlideInState extends State<FadeSlideIn> with SingleTickerProviderStateMixin {
  late final AnimationController _c = AnimationController(vsync: this, duration: widget.duration);
  bool _started = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_started) return;
    _started = true;
    if (HrMotion.reduced(context)) {
      _c.value = 1;
    } else {
      Future.delayed(widget.delay, () {
        if (mounted) _c.forward();
      });
    }
  }

  @override
  void dispose() {
    _c.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final rtl = Directionality.of(context) == TextDirection.rtl;
    final curved = CurvedAnimation(parent: _c, curve: widget.curve);
    return AnimatedBuilder(
      animation: curved,
      child: widget.child,
      builder: (context, child) {
        final t = curved.value;
        final dx = widget.offset.dx * (1 - t) * (rtl ? 1 : -1);
        return Opacity(
          opacity: t.clamp(0.0, 1.0),
          child: Transform.translate(offset: Offset(dx, widget.offset.dy * (1 - t)), child: child),
        );
      },
    );
  }
}

/// Counts from 0 to [value] once, formatted for the current language (tabular figures).
class CountUp extends StatelessWidget {
  const CountUp({super.key, required this.value, this.style, this.decimals = 0, this.duration = HrMotion.countUp, this.suffix = ''});
  final num value;
  final TextStyle? style;
  final int decimals;
  final Duration duration;
  final String suffix;

  @override
  Widget build(BuildContext context) {
    final lang = Localizations.localeOf(context).languageCode;
    final fmt = NumberFormat.decimalPatternDigits(locale: lang, decimalDigits: decimals);
    final st = (style ?? DefaultTextStyle.of(context).style).copyWith(fontFeatures: const [FontFeature.tabularFigures()]);
    if (HrMotion.reduced(context)) return Text('${fmt.format(value)}$suffix', style: st);
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: value.toDouble()),
      duration: duration,
      curve: HrMotion.enter,
      builder: (context, v, _) => Text('${fmt.format(decimals == 0 ? v.round() : v)}$suffix', style: st),
    );
  }
}

/// Small press feedback (scale) for buttons and tappable cards.
class PressScale extends StatefulWidget {
  const PressScale({super.key, required this.child, this.scale = 0.97});
  final Widget child;
  final double scale;

  @override
  State<PressScale> createState() => _PressScaleState();
}

class _PressScaleState extends State<PressScale> {
  bool _down = false;

  @override
  Widget build(BuildContext context) => Listener(
        onPointerDown: (_) => setState(() => _down = true),
        onPointerUp: (_) => setState(() => _down = false),
        onPointerCancel: (_) => setState(() => _down = false),
        child: AnimatedScale(
          scale: _down && !HrMotion.reduced(context) ? widget.scale : 1,
          duration: HrMotion.press,
          curve: HrMotion.enter,
          child: widget.child,
        ),
      );
}

/// The primary button follows the pointer slightly (≤10 px horizontally, ≤8 px vertically).
class Magnetic extends StatefulWidget {
  const Magnetic({super.key, required this.child});
  final Widget child;

  @override
  State<Magnetic> createState() => _MagneticState();
}

class _MagneticState extends State<Magnetic> {
  Offset _o = Offset.zero;

  @override
  Widget build(BuildContext context) {
    if (HrMotion.reduced(context)) return widget.child;
    return MouseRegion(
      onHover: (e) {
        if (!mounted) return;
        final box = context.findRenderObject();
        if (box is! RenderBox || !box.hasSize || box.size.isEmpty) return;
        final s = box.size;
        final nx = ((e.localPosition.dx / s.width) * 2 - 1).clamp(-1.0, 1.0);
        final ny = ((e.localPosition.dy / s.height) * 2 - 1).clamp(-1.0, 1.0);
        final next = Offset(nx * HrMotion.magneticX, ny * HrMotion.magneticY);
        // pointer callbacks run inside the mouse tracker's update: rebuild on the next frame
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (mounted) setState(() => _o = next);
        });
        WidgetsBinding.instance.scheduleFrame();
      },
      onExit: (_) {
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (mounted) setState(() => _o = Offset.zero);
        });
        WidgetsBinding.instance.scheduleFrame();
      },
      child: TweenAnimationBuilder<Offset>(
        tween: Tween(end: _o),
        duration: const Duration(milliseconds: 260),
        curve: HrMotion.enter,
        builder: (context, o, child) => Transform.translate(offset: o, child: child),
        child: PressScale(child: widget.child),
      ),
    );
  }
}

/// Card surface that lifts on hover, tilts toward the pointer (≤7°) and shows a soft spotlight.
class TiltSpotlightCard extends StatefulWidget {
  const TiltSpotlightCard({super.key, required this.child, this.onTap, this.padding = const EdgeInsets.all(20)});
  final Widget child;
  final VoidCallback? onTap;
  final EdgeInsetsGeometry padding;

  @override
  State<TiltSpotlightCard> createState() => _TiltSpotlightCardState();
}

class _TiltSpotlightCardState extends State<TiltSpotlightCard> {
  Offset? _p; // -1..1
  Offset _local = Offset.zero;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    final reduced = HrMotion.reduced(context);
    final p = _p ?? Offset.zero;
    final hovering = _p != null;
    const maxRad = HrMotion.maxTiltDegrees * math.pi / 180;
    final card = AnimatedContainer(
      duration: const Duration(milliseconds: 220),
      curve: HrMotion.enter,
      decoration: BoxDecoration(
        color: c.surface,
        borderRadius: BorderRadius.circular(HrColors.radiusCard),
        border: Border.all(color: hovering ? c.accent.withValues(alpha: .45) : c.border),
        boxShadow: hovering
            ? [BoxShadow(color: c.accent.withValues(alpha: .14), blurRadius: 24, offset: const Offset(0, 10))]
            : c.shadow,
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(HrColors.radiusCard),
        child: Stack(children: [
          if (hovering && !reduced)
            Positioned.fill(
              child: IgnorePointer(
                child: CustomPaint(painter: _SpotlightPainter(_local, c.accent)),
              ),
            ),
          Material(
            type: MaterialType.transparency,
            child: InkWell(onTap: widget.onTap, child: Padding(padding: widget.padding, child: widget.child)),
          ),
        ]),
      ),
    );
    if (reduced) return RepaintBoundary(child: card);
    return RepaintBoundary(child: MouseRegion(
      onHover: (e) {
        if (!mounted) return;
        final box = context.findRenderObject();
        if (box is! RenderBox || !box.hasSize || box.size.isEmpty) return;
        final s = box.size;
        final local = e.localPosition;
        final p = Offset((local.dx / s.width) * 2 - 1, (local.dy / s.height) * 2 - 1);
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (!mounted) return;
          setState(() {
            _local = local;
            _p = p;
          });
        });
        WidgetsBinding.instance.scheduleFrame();
      },
      onExit: (_) {
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (mounted) setState(() => _p = null);
        });
        WidgetsBinding.instance.scheduleFrame();
      },
      child: TweenAnimationBuilder<Offset>(
        tween: Tween(end: p),
        duration: const Duration(milliseconds: 240),
        curve: HrMotion.enter,
        child: card,
        builder: (context, v, child) => Transform(
          alignment: Alignment.center,
          transform: Matrix4.identity()
            ..setEntry(3, 2, HrMotion.netPerspective)
            ..translateByDouble(0.0, hovering ? -HrMotion.hoverLift : 0.0, 0.0, 1.0)
            ..rotateX(-v.dy * maxRad * .6)
            ..rotateY(v.dx * maxRad * .6),
          child: child,
        ),
      ),
    ));
  }
}

class _SpotlightPainter extends CustomPainter {
  _SpotlightPainter(this.at, this.color);
  final Offset at;
  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    final r = size.shortestSide * .9;
    canvas.drawCircle(
      at,
      r,
      Paint()
        ..shader = RadialGradient(colors: [color.withValues(alpha: .12), color.withValues(alpha: 0)])
            .createShader(Rect.fromCircle(center: at, radius: r)),
    );
  }

  @override
  bool shouldRepaint(_SpotlightPainter old) => old.at != at || old.color != color;
}

/// Loading placeholder with the content's geometry and a moving shine (only while loading).
class Skeleton extends StatefulWidget {
  const Skeleton({super.key, this.width, this.height = 16, this.radius = HrColors.radiusControl});
  final double? width;
  final double height;
  final double radius;

  @override
  State<Skeleton> createState() => _SkeletonState();
}

class _SkeletonState extends State<Skeleton> with SingleTickerProviderStateMixin {
  late final AnimationController _c = AnimationController(vsync: this, duration: HrMotion.shimmer);

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (HrMotion.reduced(context)) {
      _c.stop();
    } else if (!_c.isAnimating) {
      _c.repeat();
    }
  }

  @override
  void dispose() {
    _c.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    final base = c.border.withValues(alpha: .55);
    final shine = c.surface.withValues(alpha: .9);
    return RepaintBoundary(child: AnimatedBuilder(
      animation: _c,
      builder: (context, _) => Container(
        width: widget.width,
        height: widget.height,
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(widget.radius),
          gradient: LinearGradient(
            begin: Alignment(-3 + 4 * _c.value, 0),
            end: Alignment(-1 + 4 * _c.value, 0),
            colors: [base, shine, base],
          ),
        ),
      ),
    ));
  }
}

/// Slow emerald light blobs behind the app (translate only, 15–25 s cycles).
class AmbientBackground extends StatefulWidget {
  const AmbientBackground({super.key, required this.child});
  final Widget child;

  @override
  State<AmbientBackground> createState() => _AmbientBackgroundState();
}

class _AmbientBackgroundState extends State<AmbientBackground> with SingleTickerProviderStateMixin {
  late final AnimationController _c = AnimationController(vsync: this, duration: HrMotion.ambient);

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (HrMotion.reduced(context)) {
      _c.stop();
    } else if (!_c.isAnimating) {
      _c.repeat();
    }
  }

  @override
  void dispose() {
    _c.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final dark = Theme.of(context).brightness == Brightness.dark;
    final a = dark ? 1.0 : .6;
    Widget blob(Color color, double size) => IgnorePointer(
          child: Container(
            width: size,
            height: size,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              gradient: RadialGradient(colors: [color.withValues(alpha: color.a * a), color.withValues(alpha: 0)]),
            ),
          ),
        );
    // The drifting light lives on its own layer: without these boundaries every animation frame
    // repainted the whole app, which on GPUs without acceleration made pages look blank and frozen.
    return Stack(children: [
      Positioned.fill(child: ColoredBox(color: context.hr.bg)),
      Positioned.fill(child: RepaintBoundary(child: AnimatedBuilder(
        animation: _c,
        builder: (context, _) {
          final t = _c.value * 2 * math.pi;
          return Stack(children: [
            PositionedDirectional(
              top: -160,
              end: -120,
              child: Transform.translate(offset: Offset(math.sin(t) * 40, math.cos(t) * 30), child: blob(HrColors.emeraldGlow, 560)),
            ),
            PositionedDirectional(
              bottom: -200,
              start: 120,
              child: Transform.translate(offset: Offset(math.cos(t) * 50, math.sin(t) * 36), child: blob(HrColors.turquoiseLight, 620)),
            ),
          ]);
        },
      ))),
      Positioned.fill(child: RepaintBoundary(child: widget.child)),
    ]);
  }
}

/// Dialog transition: backdrop fades in with an 8 px blur, the dialog scales from 0.94 and rises 12 px (spring, 0.5 s).
Future<T?> showHrDialog<T>({required BuildContext context, required WidgetBuilder builder, bool barrierDismissible = true}) {
  final reduced = HrMotion.reduced(context);
  return showGeneralDialog<T>(
    context: context,
    barrierDismissible: barrierDismissible,
    barrierLabel: MaterialLocalizations.of(context).modalBarrierDismissLabel,
    barrierColor: Colors.transparent,
    transitionDuration: reduced ? Duration.zero : HrMotion.dialog,
    pageBuilder: (ctx, _, __) => builder(ctx),
    transitionBuilder: (ctx, anim, _, child) {
      final fade = CurvedAnimation(parent: anim, curve: HrMotion.enter, reverseCurve: Curves.easeIn);
      final pop = CurvedAnimation(parent: anim, curve: HrMotion.spring, reverseCurve: Curves.easeIn);
      return Stack(children: [
        // Fixed 8 px blur; only its opacity animates.
        Positioned.fill(
          child: IgnorePointer(
            child: FadeTransition(
              opacity: fade,
              child: BackdropFilter(
                filter: ImageFilter.blur(sigmaX: HrMotion.dialogBackdropBlur, sigmaY: HrMotion.dialogBackdropBlur),
                child: const ColoredBox(color: Color(0x520F172A)),
              ),
            ),
          ),
        ),
        FadeTransition(
          opacity: fade,
          child: AnimatedBuilder(
            animation: pop,
            child: child,
            builder: (_, c) => Transform.translate(
              offset: Offset(0, HrMotion.dialogShift * (1 - pop.value)),
              child: Transform.scale(scale: HrMotion.dialogScaleFrom + (1 - HrMotion.dialogScaleFrom) * pop.value, child: c),
            ),
          ),
        ),
      ]);
    },
  );
}

OverlayEntry? _toastEntry;

/// Toast that slides in from the screen's end side and leaves after ~2.6 s. Icon + text, never color alone.
void showHrToast(BuildContext context, String message, {IconData icon = Icons.check_circle_outline, Color? color}) {
  final overlay = Overlay.maybeOf(context, rootOverlay: true);
  if (overlay == null) return;
  _toastEntry?.remove();
  _toastEntry = null;
  final reduced = HrMotion.reduced(context);
  final rtl = Directionality.of(context) == TextDirection.rtl;
  late OverlayEntry entry;
  entry = OverlayEntry(
    builder: (ctx) => _Toast(
      message: message,
      icon: icon,
      color: color,
      reduced: reduced,
      rtl: rtl,
      onDone: () {
        if (_toastEntry == entry) {
          entry.remove();
          _toastEntry = null;
        }
      },
    ),
  );
  _toastEntry = entry;
  overlay.insert(entry);
  // ignore: deprecated_member_use
  SemanticsService.announce(message, rtl ? TextDirection.rtl : TextDirection.ltr);
}

class _Toast extends StatefulWidget {
  const _Toast({required this.message, required this.icon, required this.color, required this.reduced, required this.rtl, required this.onDone});
  final String message;
  final IconData icon;
  final Color? color;
  final bool reduced;
  final bool rtl;
  final VoidCallback onDone;

  @override
  State<_Toast> createState() => _ToastState();
}

class _ToastState extends State<_Toast> with SingleTickerProviderStateMixin {
  late final AnimationController _c = AnimationController(vsync: this, duration: const Duration(milliseconds: 380));

  @override
  void initState() {
    super.initState();
    if (widget.reduced) {
      _c.value = 1;
    } else {
      _c.forward();
    }
    Future.delayed(HrMotion.toast, () async {
      if (!mounted) return;
      if (!widget.reduced) await _c.reverse();
      widget.onDone();
    });
  }

  @override
  void dispose() {
    _c.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    final curve = CurvedAnimation(parent: _c, curve: HrMotion.spring, reverseCurve: Curves.easeIn);
    final width = MediaQuery.sizeOf(context).width;
    return PositionedDirectional(
      bottom: 24,
      end: 16,
      child: SafeArea(
        child: Directionality(
          textDirection: widget.rtl ? TextDirection.rtl : TextDirection.ltr,
          child: AnimatedBuilder(
            animation: curve,
            builder: (context, child) => Opacity(
              opacity: _c.value,
              // from the end side: the left edge in RTL, the right edge in LTR
              child: Transform.translate(offset: Offset((1 - curve.value) * 80 * (widget.rtl ? -1 : 1), 0), child: child),
            ),
            child: Material(
              color: Colors.transparent,
              child: Container(
                constraints: BoxConstraints(maxWidth: math.min(420, width - 32)),
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                decoration: BoxDecoration(
                  color: c.surface,
                  borderRadius: BorderRadius.circular(HrColors.radiusControl),
                  border: Border.all(color: (widget.color ?? c.accent).withValues(alpha: .6)),
                  boxShadow: c.shadow,
                ),
                child: Row(mainAxisSize: MainAxisSize.min, children: [
                  Icon(widget.icon, color: widget.color ?? c.accent),
                  const SizedBox(width: 10),
                  Flexible(child: Text(widget.message, style: TextStyle(color: c.textPrimary))),
                ]),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
