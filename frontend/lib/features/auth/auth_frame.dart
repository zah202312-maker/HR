import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/l10n/locale_controller.dart';
import '../../core/l10n/strings.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/hr_logo.dart';

/// Centered card used by sign-in and the mandatory gate screens,
/// with the language and light/dark switches in the top corner.
class AuthFrame extends ConsumerWidget {
  const AuthFrame({super.key, required this.child, this.actions = const []});
  final Widget child;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final c = context.hr;
    final s = S.of(context);
    final isAr = ref.watch(localeProvider).languageCode == 'ar';
    final dark = Theme.of(context).brightness == Brightness.dark;
    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: RadialGradient(
            center: const AlignmentDirectional(.8, -1.1).resolve(Directionality.of(context)),
            radius: 1.4,
            colors: [c.glow, c.bg],
          ),
        ),
        child: SafeArea(
          child: Stack(children: [
            PositionedDirectional(
              top: 8,
              end: 8,
              child: Row(mainAxisSize: MainAxisSize.min, children: [
                ...actions,
                TextButton(
                  onPressed: () => ref.read(localeProvider.notifier).set(isAr ? 'en' : 'ar'),
                  child: Text(isAr ? 'English' : 'العربية'),
                ),
                IconButton(
                  tooltip: dark ? s.themeLight : s.themeDark,
                  onPressed: () => ref.read(themeModeProvider.notifier).set(dark ? ThemeMode.light : ThemeMode.dark),
                  icon: Icon(dark ? Icons.light_mode_outlined : Icons.dark_mode_outlined, color: c.accent),
                ),
              ]),
            ),
            Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(16),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 440),
                  child: HrCard(
                    padding: const EdgeInsets.all(28),
                    child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                      const Center(child: HrLogo(size: 48, animateIn: true)),
                      const SizedBox(height: 24),
                      child,
                    ]),
                  ),
                ),
              ),
            ),
          ]),
        ),
      ),
    );
  }
}
