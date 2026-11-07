import 'package:flutter/widgets.dart';

/// Motion tokens for the whole app (dashboard brief, sections 6–8). Durations, delays and curves
/// live here only; widgets read them, never hard-code their own.
abstract final class HrMotion {
  // Curves
  /// Spring: linear(0, .18 6%, .58 15%, .9 27%, 1.04 36%, 1.05 44%, 1 60%, .99 75%, 1)
  static const Curve spring = PiecewiseLinearCurve([
    (0.0, 0.0), (0.06, 0.18), (0.15, 0.58), (0.27, 0.9), (0.36, 1.04),
    (0.44, 1.05), (0.60, 1.0), (0.75, 0.99), (1.0, 1.0),
  ]);
  /// General entrance: cubic-bezier(.2,.8,.2,1)
  static const Curve enter = Cubic(.2, .8, .2, 1);
  /// Line drawing: cubic-bezier(.4,0,.2,1)
  static const Curve draw = Cubic(.4, 0, .2, 1);

  // Page and cards
  static const pageIn = Duration(milliseconds: 450);
  static const pageOut = Duration(milliseconds: 240);
  static const pageShift = 16.0;
  static const cardIn = Duration(milliseconds: 550);
  static const cardShift = 24.0;
  static const cardStagger = Duration(milliseconds: 80);

  // Numbers and charts
  static const countUp = Duration(milliseconds: 1300);
  static const chartDraw = Duration(milliseconds: 1400);

  // Navigation and controls
  static const navSlide = Duration(milliseconds: 500);
  static const navIndicator = Duration(milliseconds: 600);
  static const press = Duration(milliseconds: 180);
  static const magneticX = 10.0;
  static const magneticY = 8.0;
  static const hoverLift = 4.0;
  static const maxTiltDegrees = 7.0;

  // Dialogs and toasts
  static const dialog = Duration(milliseconds: 500);
  static const dialogScaleFrom = 0.94;
  static const dialogShift = 12.0;
  static const dialogBackdropBlur = 8.0;
  static const toast = Duration(milliseconds: 2600);

  // Loading and ambience
  static const shimmer = Duration(milliseconds: 1400);
  static const ambient = Duration(seconds: 20);
  static const themeChange = Duration(milliseconds: 300);

  // Organizational network (viewBox 440 × 280)
  static const netLogoIn = Duration(milliseconds: 900);
  static const netLogoScaleFrom = 0.7;
  static const netLogoTurnFrom = -12.0; // degrees
  static const netLinkDraw = Duration(milliseconds: 1100);
  static const netDeptLinkDelays = [800, 1000, 1200]; // ms
  static const netDeptNodeIn = Duration(milliseconds: 700);
  static const netDeptNodeScaleFrom = 0.4;
  static const netEmployeeLinkStart = 1500; // ms, 1.5 s → 1.9 s, 0.12 s between siblings
  static const netEmployeeLinkStep = 120;
  static const netEmployeeGroupStep = 40;
  static const netEmployeeLinkDraw = Duration(milliseconds: 550);
  static const netEmployeeNodeIn = Duration(milliseconds: 350);
  static const netTotal = Duration(milliseconds: 2500);
  static const netPulse = Duration(seconds: 5);
  static const netHaloScale = 1.12;
  static const netHaloMinOpacity = 0.4;
  static const netFloat = Duration(seconds: 7);
  static const netFloatDistance = 2.5;
  static const netPerspective = 1 / 900;

  /// prefers-reduced-motion (web) / system "remove animations".
  static bool reduced(BuildContext context) => MediaQuery.maybeDisableAnimationsOf(context) ?? false;
}

/// CSS-like `linear()` easing: straight segments between (t, value) points.
class PiecewiseLinearCurve extends Curve {
  const PiecewiseLinearCurve(this.points);
  final List<(double, double)> points;

  @override
  double transformInternal(double t) {
    for (var i = 1; i < points.length; i++) {
      final (t1, v1) = points[i];
      if (t <= t1) {
        final (t0, v0) = points[i - 1];
        final span = t1 - t0;
        return span <= 0 ? v1 : v0 + (v1 - v0) * (t - t0) / span;
      }
    }
    return points.last.$2;
  }
}

/// Segment of a master timeline: 0 before [startMs], 1 after [startMs]+[duration], curved between.
double timelineValue(double elapsedMs, int startMs, Duration duration, [Curve curve = Curves.linear]) {
  final d = duration.inMilliseconds;
  if (elapsedMs <= startMs) return 0;
  if (elapsedMs >= startMs + d) return 1;
  return curve.transform((elapsedMs - startMs) / d);
}
