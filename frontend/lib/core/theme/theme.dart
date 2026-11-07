import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'tokens.dart';

/// Arabic: Cairo, English: Inter (Design System section 4). Final Arabic face (Cairo or
/// Tajawal) is an open owner decision (UNRESOLVED_ISSUES U-02).
ThemeData buildTheme(Brightness brightness, Locale locale) {
  final c = brightness == Brightness.dark ? HrColors.dark : HrColors.light;
  final base = ThemeData(brightness: brightness, useMaterial3: true);
  final textBase = locale.languageCode == 'ar'
      ? GoogleFonts.cairoTextTheme(base.textTheme)
      : GoogleFonts.interTextTheme(base.textTheme);
  final text = textBase
      .copyWith(
        headlineSmall: textBase.headlineSmall?.copyWith(fontSize: 28, height: 36 / 28, fontWeight: FontWeight.w700),
        titleMedium: textBase.titleMedium?.copyWith(fontSize: 18, height: 24 / 18, fontWeight: FontWeight.w600),
        bodyMedium: textBase.bodyMedium?.copyWith(fontSize: 14, height: 20 / 14),
        labelSmall: textBase.labelSmall?.copyWith(fontSize: 12, height: 16 / 12, fontWeight: FontWeight.w500),
      )
      .apply(bodyColor: c.textPrimary, displayColor: c.textPrimary);

  // Light teal with white text fails contrast (Design System 3.2): use dark text on it.
  const onAccent = Color(0xFF0F172A);
  final scheme = ColorScheme.fromSeed(
    seedColor: c.accent,
    brightness: brightness,
  ).copyWith(
    primary: c.accent,
    onPrimary: onAccent,
    surface: c.surface,
    onSurface: c.textPrimary,
    outline: c.border,
    error: HrColors.danger,
  );

  final controlShape = RoundedRectangleBorder(borderRadius: BorderRadius.circular(HrColors.radiusControl));
  return base.copyWith(
    colorScheme: scheme,
    scaffoldBackgroundColor: c.bg,
    textTheme: text,
    extensions: <ThemeExtension<dynamic>>[c],
    dividerColor: c.border,
    appBarTheme: AppBarTheme(
      backgroundColor: c.bg,
      foregroundColor: c.textPrimary,
      elevation: 0,
      scrolledUnderElevation: 0,
      titleTextStyle: text.titleMedium,
    ),
    cardTheme: CardThemeData(
      color: c.surface,
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(HrColors.radiusCard),
        side: BorderSide(color: c.border),
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: c.accent,
        foregroundColor: onAccent,
        minimumSize: const Size(44, 44),
        shape: controlShape,
        textStyle: text.bodyMedium?.copyWith(fontWeight: FontWeight.w600),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: c.textPrimary,
        minimumSize: const Size(44, 44),
        side: BorderSide(color: c.border),
        shape: controlShape,
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(foregroundColor: c.accent, minimumSize: const Size(44, 44), shape: controlShape),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: c.surface,
      contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(HrColors.radiusControl),
        borderSide: BorderSide(color: c.border),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(HrColors.radiusControl),
        borderSide: BorderSide(color: c.border),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(HrColors.radiusControl),
        borderSide: BorderSide(color: c.accent, width: 2),
      ),
      labelStyle: TextStyle(color: c.textSecondary),
    ),
    chipTheme: base.chipTheme.copyWith(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(HrColors.radiusBadge)),
      side: BorderSide(color: c.border),
    ),
    navigationRailTheme: NavigationRailThemeData(
      backgroundColor: c.surface,
      indicatorColor: c.glow,
      selectedIconTheme: IconThemeData(color: c.accent),
      unselectedIconTheme: IconThemeData(color: c.textSecondary),
    ),
    navigationBarTheme: NavigationBarThemeData(
      backgroundColor: c.surface,
      indicatorColor: c.glow,
    ),
    dialogTheme: DialogThemeData(
      backgroundColor: c.surface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(HrColors.radiusCard)),
    ),
    snackBarTheme: const SnackBarThemeData(behavior: SnackBarBehavior.floating),
  );
}
