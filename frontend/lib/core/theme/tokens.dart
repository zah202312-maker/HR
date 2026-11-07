import 'package:flutter/material.dart';

/// HR Design System V3, section 3 (frontend/HR-Design-System.md).
@immutable
class HrColors extends ThemeExtension<HrColors> {
  const HrColors({
    required this.bg,
    required this.surface,
    required this.textPrimary,
    required this.textSecondary,
    required this.accent,
    required this.border,
    required this.glow,
    required this.shadow,
  });

  final Color bg;
  final Color surface;
  final Color textPrimary;
  final Color textSecondary;
  final Color accent;
  final Color border;
  final Color glow;
  final List<BoxShadow> shadow;

  static const success = Color(0xFF10B981);
  static const warning = Color(0xFFF59E0B);
  static const danger = Color(0xFFEF4444);
  static const info = Color(0xFF3B82F6);
  // Decorative only, never a substitute for the tokens above.
  static const emeraldGlow = Color(0x3300C49F); // rgba(0,196,159,.20)
  static const turquoiseLight = Color(0x1F14B8A6); // rgba(20,184,166,.12)
  static const cyanEdge = Color(0x2922D3EE); // rgba(34,211,238,.16)
  static const turquoise = Color(0xFF14B8A6);

  static const radiusCard = 16.0;
  static const radiusControl = 10.0;
  static const radiusBadge = 20.0;

  static const light = HrColors(
    bg: Color(0xFFF8FAFC),
    surface: Color(0xFFFFFFFF),
    textPrimary: Color(0xFF0F172A),
    textSecondary: Color(0xFF475569),
    accent: Color(0xFF00A887),
    border: Color(0xFFE2E8F0),
    glow: Color(0x1F00A887),
    shadow: [BoxShadow(color: Color(0x0D0F172A), blurRadius: 16, offset: Offset(0, 4))],
  );

  static const dark = HrColors(
    bg: Color(0xFF0F172A),
    surface: Color(0xFF1E293B),
    textPrimary: Color(0xFFF8FAFC),
    textSecondary: Color(0xFF94A3B8),
    accent: Color(0xFF00C49F),
    border: Color(0xFF334155),
    glow: Color(0x3300C49F),
    shadow: [BoxShadow(color: Color(0x59000000), blurRadius: 20, offset: Offset(0, 4))],
  );

  @override
  HrColors copyWith() => this;

  @override
  HrColors lerp(ThemeExtension<HrColors>? other, double t) {
    if (other is! HrColors) return this;
    return HrColors(
      bg: Color.lerp(bg, other.bg, t)!,
      surface: Color.lerp(surface, other.surface, t)!,
      textPrimary: Color.lerp(textPrimary, other.textPrimary, t)!,
      textSecondary: Color.lerp(textSecondary, other.textSecondary, t)!,
      accent: Color.lerp(accent, other.accent, t)!,
      border: Color.lerp(border, other.border, t)!,
      glow: Color.lerp(glow, other.glow, t)!,
      shadow: t < .5 ? shadow : other.shadow,
    );
  }
}

extension HrThemeX on BuildContext {
  HrColors get hr => Theme.of(this).extension<HrColors>()!;
}
