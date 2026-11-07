import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

/// Supported cultures are ar and en, default ar (x-server-requirements.language).
const supportedLocales = <Locale>[Locale('ar'), Locale('en')];

class LocaleController extends Notifier<Locale> {
  @override
  Locale build() => const Locale('ar');

  void set(String languageCode) {
    state = Locale(languageCode == 'en' ? 'en' : 'ar');
  }
}

final localeProvider = NotifierProvider<LocaleController, Locale>(LocaleController.new);

class ThemeModeController extends Notifier<ThemeMode> {
  @override
  ThemeMode build() => ThemeMode.system;
  void set(ThemeMode mode) => state = mode;
}

final themeModeProvider = NotifierProvider<ThemeModeController, ThemeMode>(ThemeModeController.new);
