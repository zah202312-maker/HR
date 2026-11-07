import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import 'auth_frame.dart';

/// S-AUTH-02 (forced) and S-ME-02 (voluntary): Account_ChangePassword.
class ChangePasswordPage extends ConsumerStatefulWidget {
  const ChangePasswordPage({super.key, required this.forced});
  final bool forced;

  @override
  ConsumerState<ChangePasswordPage> createState() => _ChangePasswordPageState();
}

class _ChangePasswordPageState extends ConsumerState<ChangePasswordPage> {
  final _form = GlobalKey<FormState>();
  final _current = TextEditingController();
  final _new = TextEditingController();
  final _confirm = TextEditingController();
  bool _busy = false;
  Object? _error;

  @override
  void dispose() {
    _current.dispose();
    _new.dispose();
    _confirm.dispose();
    super.dispose();
  }

  /// Abp.Identity.Password.* settings from application-configuration.
  String? _policy(String? v, S s) {
    if (v == null || v.isEmpty) return s.requiredField;
    final settings = ref.read(sessionProvider).config?.setting.values ?? const <String, String?>{};
    final minLen = int.tryParse(settings['Abp.Identity.Password.RequiredLength'] ?? '') ?? 1;
    bool on(String k) => (settings['Abp.Identity.Password.$k'] ?? '').toLowerCase() == 'true';
    final issues = <String>[];
    if (v.length < minLen) issues.add(s.isAr ? 'الطول $minLen على الأقل' : 'at least $minLen characters');
    if (on('RequireDigit') && !RegExp(r'\d').hasMatch(v)) issues.add(s.isAr ? 'رقم' : 'a digit');
    if (on('RequireLowercase') && !RegExp(r'[a-z]').hasMatch(v)) issues.add(s.isAr ? 'حرف صغير' : 'a lowercase letter');
    if (on('RequireUppercase') && !RegExp(r'[A-Z]').hasMatch(v)) issues.add(s.isAr ? 'حرف كبير' : 'an uppercase letter');
    if (on('RequireNonAlphanumeric') && !RegExp(r'[^A-Za-z0-9]').hasMatch(v)) issues.add(s.isAr ? 'رمز' : 'a symbol');
    return issues.isEmpty ? null : '${s.isAr ? 'يلزم' : 'Needs'}: ${issues.join('، ')}';
  }

  Future<void> _submit() async {
    if (!_form.currentState!.validate()) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final result = await ref.read(hrApiProvider).currentUser.Account_ChangePassword(
            body: ChangePasswordInputDto(currentPassword: _current.text, newPassword: _new.text),
          );
      if (!mounted) return;
      showDone(context, S.of(context).passwordChanged);
      await ref.read(sessionProvider.notifier).gateChanged(result.accessGate);
      if (!widget.forced && mounted) context.pop();
    } catch (e) {
      if (mounted) setState(() => _error = e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final form = Form(
      key: _form,
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        if (widget.forced) ...[
          Text(s.changePassword, style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 8),
          Text(s.changePasswordRequired, style: TextStyle(color: context.hr.textSecondary)),
          const SizedBox(height: 20),
        ],
        TextFormField(
          controller: _current,
          obscureText: true,
          autofillHints: const [AutofillHints.password],
          decoration: InputDecoration(labelText: s.currentPassword),
          validator: (v) => (v == null || v.isEmpty) ? s.requiredField : null,
        ),
        const SizedBox(height: 12),
        TextFormField(
          controller: _new,
          obscureText: true,
          maxLength: 128,
          autofillHints: const [AutofillHints.newPassword],
          decoration: InputDecoration(labelText: s.newPassword),
          validator: (v) => _policy(v, s),
        ),
        const SizedBox(height: 4),
        TextFormField(
          controller: _confirm,
          obscureText: true,
          decoration: InputDecoration(labelText: s.confirmPassword),
          validator: (v) => v != _new.text ? s.passwordsDoNotMatch : null,
        ),
        if (_error != null) ...[
          const SizedBox(height: 12),
          Text(s.error(_error!), style: const TextStyle(color: HrColors.danger)),
        ],
        const SizedBox(height: 20),
        FilledButton(onPressed: _busy ? null : _submit, child: Text(s.save)),
        if (widget.forced)
          TextButton(onPressed: () => ref.read(sessionProvider.notifier).signOut(), child: Text(s.signOut)),
      ]),
    );
    if (widget.forced) return AuthFrame(child: form);
    return HrPage(title: s.changePassword, child: SingleChildScrollView(child: ConstrainedBox(constraints: const BoxConstraints(maxWidth: 480), child: HrCard(child: form))));
  }
}
