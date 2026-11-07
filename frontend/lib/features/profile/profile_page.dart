import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/locale_controller.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';

final employeeProfileProvider = FutureProvider.autoDispose<EmployeeSelfDto?>((ref) async {
  if (!ref.watch(sessionProvider).can(HrOps.Me_GetEmployeeProfile)) return null;
  return ref.watch(hrApiProvider).currentUser.Me_GetEmployeeProfile();
});

/// S-ME-01..03: read-only employee file, security and language/appearance.
class ProfilePage extends ConsumerStatefulWidget {
  const ProfilePage({super.key});

  @override
  ConsumerState<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends ConsumerState<ProfilePage> {
  bool _savingLanguage = false;

  /// Me_Update stores the language for notifications and email, then the client
  /// reloads application-configuration (Me_Update description).
  Future<void> _setLanguage(AppLanguage lang) async {
    setState(() => _savingLanguage = true);
    try {
      await ref.read(hrApiProvider).currentUser.Me_Update(body: MyAccountUpdateDto(preferredLanguage: lang));
      ref.read(localeProvider.notifier).set(lang.value);
      await ref.read(sessionProvider.notifier).reload();
      if (mounted) showDone(context, S.of(context).saved);
    } catch (e) {
      if (mounted) showError(context, e);
    } finally {
      if (mounted) setState(() => _savingLanguage = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final me = session.me;
    final profile = ref.watch(employeeProfileProvider);
    final mode = ref.watch(themeModeProvider);
    final wide = MediaQuery.sizeOf(context).width >= 1024;

    final employeeCard = AsyncBody<EmployeeSelfDto?>(
      value: profile,
      onRetry: () => ref.invalidate(employeeProfileProvider),
      builder: (p) {
        if (p == null) return const SizedBox.shrink();
        return HrCard(
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            SectionTitle(p.fullName),
            if (p.employeeNumber != null) InfoRow(s.employeeNumber, p.employeeNumber!),
            InfoRow(s.department, p.department.name),
            InfoRow(s.jobTitle, p.jobTitle),
            InfoRow(s.hireDate, f.date(p.hireDate)),
            if (p.manager != null) InfoRow(s.manager, p.manager!.fullName),
            if (p.currentShift != null) InfoRow(s.shift, p.currentShift!.name),
            if (p.email != null) InfoRow(s.email, p.email!),
            if (p.phoneNumber != null) InfoRow(s.phone, p.phoneNumber!),
            const SizedBox(height: 8),
            Text(s.readOnlyProfile, style: Theme.of(context).textTheme.labelSmall),
          ]),
        );
      },
    );

    final accountCard = HrCard(
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        SectionTitle(s.account),
        if (me != null) ...[
          InfoRow(s.isAr ? 'اسم المستخدم' : 'User name', me.userName),
          if (me.email != null) InfoRow(s.email, me.email!),
          InfoRow(
              s.roles,
              me.roleNames
                  .map((r) => s.role(AppRole.values.firstWhere((v) => v.value == r, orElse: () => AppRole.$unknown)))
                  .join('، ')),
        ],
        const Divider(height: 24),
        SectionTitle(s.security),
        ListTile(
          contentPadding: EdgeInsets.zero,
          leading: const Icon(Icons.password),
          title: Text(s.changePassword),
          trailing: const Icon(Icons.chevron_right),
          onTap: () => context.go('/profile/password'),
        ),
        if (me != null && (me.mfaRequired || me.mfaEnabled))
          ListTile(
            contentPadding: EdgeInsets.zero,
            leading: const Icon(Icons.verified_user_outlined),
            title: Text(s.mfa),
            subtitle: Text(me.mfaEnabled ? s.mfaEnabled : s.mfaDisabled),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.go('/profile/mfa'),
          ),
        const Divider(height: 24),
        SectionTitle(s.notificationLanguage),
        SegmentedButton<AppLanguage>(
          segments: const [
            ButtonSegment(value: AppLanguage.ar, label: Text('العربية')),
            ButtonSegment(value: AppLanguage.en, label: Text('English')),
          ],
          selected: {me?.preferredLanguage ?? AppLanguage.ar},
          onSelectionChanged: _savingLanguage ? null : (v) => _setLanguage(v.first),
        ),
        const SizedBox(height: 16),
        SectionTitle(s.theme),
        SegmentedButton<ThemeMode>(
          segments: [
            ButtonSegment(value: ThemeMode.system, label: Text(s.themeSystem)),
            ButtonSegment(value: ThemeMode.light, label: Text(s.themeLight)),
            ButtonSegment(value: ThemeMode.dark, label: Text(s.themeDark)),
          ],
          selected: {mode},
          onSelectionChanged: (v) => ref.read(themeModeProvider.notifier).set(v.first),
        ),
        const SizedBox(height: 16),
        OutlinedButton.icon(
          onPressed: () => ref.read(sessionProvider.notifier).signOut(),
          icon: const Icon(Icons.logout),
          label: Text(s.signOut),
        ),
      ]),
    );

    return HrPage(
      title: s.profile,
      child: SingleChildScrollView(
        padding: const EdgeInsets.only(bottom: 24),
        child: wide
            ? Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Expanded(child: employeeCard),
                const SizedBox(width: 12),
                Expanded(child: accountCard),
              ])
            : Column(children: [employeeCard, const SizedBox(height: 12), accountCard]),
      ),
    );
  }
}
