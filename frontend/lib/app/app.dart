import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/l10n/locale_controller.dart';
import '../core/theme/motion.dart';
import '../core/theme/theme.dart';
import 'router.dart';

class HrApp extends ConsumerWidget {
  const HrApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final locale = ref.watch(localeProvider);
    return MaterialApp.router(
      title: 'HR',
      debugShowCheckedModeBanner: false,
      routerConfig: ref.watch(routerProvider),
      locale: locale,
      supportedLocales: supportedLocales,
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      themeMode: ref.watch(themeModeProvider),
      // theme and language switch with a soft color transition; layout direction does not jump
      themeAnimationDuration: HrMotion.themeChange,
      themeAnimationCurve: HrMotion.enter,
      theme: buildTheme(Brightness.light, locale),
      darkTheme: buildTheme(Brightness.dark, locale),
    );
  }
}
