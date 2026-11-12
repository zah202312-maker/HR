import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../api/runtime/api_client.dart';
import '../l10n/locale_controller.dart';
import '../network/api_providers.dart';
import '../network/session_signals.dart';

enum SessionStatus { loading, signedOut, signedIn }

class Session {
  const Session({required this.status, this.config, this.me, this.gate = AccessGate.none, this.error});

  final SessionStatus status;
  final ApplicationConfigurationDto? config;
  final MyAccountDto? me;
  final AccessGate gate;
  final Object? error;

  static const loading = Session(status: SessionStatus.loading);
  static const signedOut = Session(status: SessionStatus.signedOut);

  Map<String, bool> get grantedPolicies => config?.auth.grantedPolicies ?? const <String, bool>{};
  List<String> get roles => me?.roleNames ?? const <String>[];
  bool hasRole(AppRole role) => roles.contains(role.value);

  /// System admin with no business role (employee aside): employee-facing pages are hidden.
  bool get isSystemAdminOnly =>
      hasRole(AppRole.systemAdmin) &&
      roles.every((r) => r == AppRole.systemAdmin.value || r == AppRole.employee.value);

  /// HR manager who is not also system admin: the roles and permissions page is hidden (decision 2026-11-10).
  /// The server permission stays, because the role pickers of the employee and account forms read Role_List.
  bool get isHrWithoutSystemAdmin => hasRole(AppRole.hrManager) && !hasRole(AppRole.systemAdmin);

  /// Accountant or store officer with no other business role (employee aside): circulars, evaluations, warnings,
  /// appreciation letters, notices and holidays are hidden (decision 2026-11-11). AccountProvisioner.IsBackOfficeOnly.
  /// Executive director: the employee-facing pages «طلباتي», leaves, attendance, warnings, holidays, leave balances
  /// and «تقييماتي» are hidden (decision 2026-11-11). Details opened from the inbox or a notification stay reachable.
  bool get isExecutive => hasRole(AppRole.executiveDirector);

  bool get isBackOfficeOnly =>
      (hasRole(AppRole.accountant) || hasRole(AppRole.storeOfficer)) &&
      roles.every((r) => r == AppRole.accountant.value || r == AppRole.storeOfficer.value || r == AppRole.employee.value);

  /// No shift, and outside attendance and leave: executive, HR manager and system admin (decisions of 2026-11-10),
  /// accountant and store officer (decision 2026-11-11). ManagerLinkService.IsOutsideSchedule on the server.
  bool get isOutsideSchedule =>
      hasRole(AppRole.executiveDirector) ||
      hasRole(AppRole.hrManager) ||
      hasRole(AppRole.systemAdmin) ||
      hasRole(AppRole.accountant) ||
      hasRole(AppRole.storeOfficer);

  /// UI gate for an operation: its x-permission must be granted (null = any signed-in user).
  /// The server still checks every call.
  bool can(String operationId) {
    final meta = hrOperations[operationId];
    if (meta == null || status != SessionStatus.signedIn) return false;
    final permission = meta.permission;
    return permission == null || grantedPolicies[permission] == true;
  }

  Session copyWith({AccessGate? gate, MyAccountDto? me, ApplicationConfigurationDto? config}) => Session(
        status: status,
        config: config ?? this.config,
        me: me ?? this.me,
        gate: gate ?? this.gate,
      );
}

class SessionController extends Notifier<Session> {
  StreamSubscription<SessionSignal>? _signals;

  @override
  Session build() {
    _signals = ref.read(sessionSignalsProvider).stream.listen(_onSignal);
    ref.onDispose(() => _signals?.cancel());
    Future.microtask(bootstrap);
    return Session.loading;
  }

  HrApi get _api => ref.read(hrApiProvider);

  Future<void> bootstrap() async {
    final auth = ref.read(authServiceProvider);
    if (!await auth.hasSession()) {
      state = Session.signedOut;
      return;
    }
    await reload();
  }

  /// application-configuration then /me: the reference for the access gate.
  Future<void> reload() async {
    try {
      final config = await _api.auth.ApplicationConfiguration_Get(includeLocalizationResources: false);
      if (!config.currentUser.isAuthenticated) {
        state = Session.signedOut;
        return;
      }
      final me = await _api.currentUser.Me_Get();
      ref.read(localeProvider.notifier).set(me.preferredLanguage.value);
      state = Session(status: SessionStatus.signedIn, config: config, me: me, gate: me.accessGate);
    } on ApiException catch (e) {
      state = e.isUnauthorized ? Session.signedOut : Session(status: SessionStatus.signedOut, error: e);
    }
  }

  Future<void> signIn() async {
    final language = ref.read(localeProvider).languageCode;
    final mode = ref.read(themeModeProvider);
    final dark = mode == ThemeMode.dark ||
        (mode == ThemeMode.system && WidgetsBinding.instance.platformDispatcher.platformBrightness == Brightness.dark);
    await ref.read(authServiceProvider).signIn(language: language, theme: dark ? 'dark' : 'light');
    state = Session.loading;
    await reload();
  }

  Future<void> signOut() async {
    await ref.read(authServiceProvider).signOut();
    state = Session.signedOut;
  }

  /// After Account_ChangePassword or Mfa_ConfirmEnrollment, which return the next gate.
  Future<void> gateChanged(AccessGate gate) async {
    state = state.copyWith(gate: gate);
    await reload();
  }

  void _onSignal(SessionSignal signal) {
    switch (signal) {
      case SessionSignal.signedOut:
        state = Session.signedOut;
      case SessionSignal.passwordChangeRequired:
        state = state.copyWith(gate: AccessGate.passwordChangeRequired);
      case SessionSignal.mfaEnrollmentRequired:
        state = state.copyWith(gate: AccessGate.mfaEnrollmentRequired);
    }
  }
}

final sessionProvider = NotifierProvider<SessionController, Session>(SessionController.new);
