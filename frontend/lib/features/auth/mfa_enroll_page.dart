import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import 'auth_frame.dart';

/// S-AUTH-03 / S-AUTH-04: TOTP enrollment in three steps, then one-time recovery codes.
/// Non-forced mode (from the profile) also shows the status and regenerates codes.
class MfaEnrollPage extends ConsumerStatefulWidget {
  const MfaEnrollPage({super.key, required this.forced});
  final bool forced;

  @override
  ConsumerState<MfaEnrollPage> createState() => _MfaEnrollPageState();
}

class _MfaEnrollPageState extends ConsumerState<MfaEnrollPage> {
  MfaStatusDto? _status;
  MfaEnrollmentDto? _enrollment;
  MfaRecoveryCodesDto? _codes;
  final _code = TextEditingController();
  bool _busy = false;
  Object? _error;

  MfaApi get _api => ref.read(hrApiProvider).mfa;

  @override
  void initState() {
    super.initState();
    _loadStatus();
  }

  @override
  void dispose() {
    _code.dispose();
    super.dispose();
  }

  Future<void> _run(Future<void> Function() action) async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await action();
    } catch (e) {
      if (mounted) setState(() => _error = e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _loadStatus() => _run(() async {
        final st = await _api.Mfa_GetStatus();
        if (mounted) setState(() => _status = st);
      });

  Future<void> _start() => _run(() async {
        final e = await _api.Mfa_StartEnrollment();
        if (mounted) setState(() => _enrollment = e);
      });

  bool _validCode() => RegExp(r'^[0-9]{6}$').hasMatch(_code.text.trim());

  Future<void> _confirm() => _run(() async {
        final codes = await _api.Mfa_ConfirmEnrollment(body: MfaCodeInputDto(code: _code.text.trim()));
        if (mounted) setState(() => _codes = codes);
      });

  Future<void> _regenerate() => _run(() async {
        final codes = await _api.Mfa_RegenerateRecoveryCodes(body: MfaCodeInputDto(code: _code.text.trim()));
        if (mounted) setState(() => _codes = codes);
      });

  Future<void> _finish() async {
    final gate = _codes?.accessGate ?? AccessGate.none;
    await ref.read(sessionProvider.notifier).gateChanged(gate);
    if (!widget.forced && mounted) {
      setState(() {
        _codes = null;
        _enrollment = null;
        _code.clear();
      });
      _loadStatus();
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final content = Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      Text(s.mfa, style: Theme.of(context).textTheme.titleMedium),
      const SizedBox(height: 8),
      if (widget.forced) Text(s.mfaRequired, style: TextStyle(color: context.hr.textSecondary)),
      const SizedBox(height: 16),
      ..._body(context, s),
      if (_error != null) ...[
        const SizedBox(height: 12),
        Text(s.error(_error!), style: const TextStyle(color: HrColors.danger)),
      ],
      if (widget.forced) ...[
        const SizedBox(height: 8),
        TextButton(onPressed: () => ref.read(sessionProvider.notifier).signOut(), child: Text(s.signOut)),
      ],
    ]);
    if (widget.forced) return AuthFrame(child: content);
    return HrPage(
      title: s.mfa,
      child: SingleChildScrollView(
        child: Align(
          alignment: AlignmentDirectional.topStart,
          child: ConstrainedBox(constraints: const BoxConstraints(maxWidth: 520), child: HrCard(child: content)),
        ),
      ),
    );
  }

  List<Widget> _body(BuildContext context, S s) {
    // Step 4: recovery codes, shown once.
    final codes = _codes;
    if (codes != null) {
      return [
        Text(s.recoveryCodes, style: const TextStyle(fontWeight: FontWeight.w700)),
        const SizedBox(height: 6),
        Text(s.recoveryCodesHint, style: const TextStyle(color: HrColors.warning)),
        const SizedBox(height: 12),
        Directionality(
          textDirection: TextDirection.ltr,
          child: Wrap(spacing: 8, runSpacing: 8, children: [
            for (final c in codes.recoveryCodes)
              Chip(label: Text(c, style: const TextStyle(fontFamily: 'monospace'))),
          ]),
        ),
        const SizedBox(height: 8),
        OutlinedButton.icon(
          onPressed: () => Clipboard.setData(ClipboardData(text: codes.recoveryCodes.join('\n'))),
          icon: const Icon(Icons.copy),
          label: Text(s.isAr ? 'نسخ' : 'Copy'),
        ),
        const SizedBox(height: 12),
        FilledButton(onPressed: _finish, child: Text(s.continueLabel)),
      ];
    }
    final status = _status;
    if (status == null) {
      return [if (_busy) const Center(child: CircularProgressIndicator())];
    }
    // Already enabled (profile): status and regenerate codes.
    if (status.isEnabled && _enrollment == null) {
      return [
        InfoRow(s.status, s.mfaEnabled),
        InfoRow(s.isAr ? 'منذ' : 'Since', Fmt(context).dateTime(status.enabledAt)),
        InfoRow(s.recoveryCodes, '${status.recoveryCodesRemaining}'),
        const SizedBox(height: 12),
        _codeField(s),
        const SizedBox(height: 12),
        OutlinedButton(onPressed: _busy ? null : () => _validCode() ? _regenerate() : null, child: Text(s.regenerateCodes)),
      ];
    }
    final enrollment = _enrollment;
    if (enrollment == null) {
      return [
        InfoRow(s.status, s.mfaDisabled),
        const SizedBox(height: 12),
        FilledButton(onPressed: _busy ? null : _start, child: Text(s.mfaStart)),
      ];
    }
    return [
      Text('1. ${s.mfaStep1}'),
      const SizedBox(height: 12),
      Center(
        child: Container(
          color: Colors.white,
          padding: const EdgeInsets.all(8),
          child: QrImageView(data: enrollment.authenticatorUri, size: 200),
        ),
      ),
      const SizedBox(height: 8),
      Directionality(
        textDirection: TextDirection.ltr,
        child: SelectableText(enrollment.sharedKey, textAlign: TextAlign.center, style: const TextStyle(fontFamily: 'monospace')),
      ),
      const SizedBox(height: 16),
      Text('2. ${s.mfaStep2}'),
      const SizedBox(height: 8),
      _codeField(s),
      const SizedBox(height: 16),
      FilledButton(onPressed: _busy ? null : () => _validCode() ? _confirm() : null, child: Text(s.confirm)),
    ];
  }

  Widget _codeField(S s) => TextField(
        controller: _code,
        keyboardType: TextInputType.number,
        maxLength: 6,
        textAlign: TextAlign.center,
        inputFormatters: [FilteringTextInputFormatter.digitsOnly],
        decoration: InputDecoration(labelText: s.mfaCode),
        onChanged: (_) => setState(() {}),
      );
}
