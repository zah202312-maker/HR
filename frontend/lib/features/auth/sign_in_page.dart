import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/l10n/strings.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import 'auth_frame.dart';

/// S-AUTH-01: Authorization Code + PKCE on the server's bilingual login page.
class SignInPage extends ConsumerStatefulWidget {
  const SignInPage({super.key});

  @override
  ConsumerState<SignInPage> createState() => _SignInPageState();
}

class _SignInPageState extends ConsumerState<SignInPage> {
  bool _busy = false;
  String? _error;

  Future<void> _signIn() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await ref.read(sessionProvider.notifier).signIn();
    } catch (e) {
      if (mounted) setState(() => _error = S.of(context).signInFailed);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final sessionError = ref.watch(sessionProvider).error;
    return AuthFrame(
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Text(s.appTagline, textAlign: TextAlign.center, style: Theme.of(context).textTheme.titleMedium),
        const SizedBox(height: 8),
        Text(s.signInHint, textAlign: TextAlign.center, style: TextStyle(color: context.hr.textSecondary)),
        const SizedBox(height: 24),
        FilledButton.icon(
          onPressed: _busy ? null : _signIn,
          icon: _busy
              ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
              : const Icon(Icons.login),
          label: Text(s.signIn),
        ),
        if (_error != null || sessionError != null) ...[
          const SizedBox(height: 16),
          Text(_error ?? s.error(sessionError!), textAlign: TextAlign.center, style: const TextStyle(color: HrColors.danger)),
        ],
      ]),
    );
  }
}
