import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/pickers.dart';
import 'org_providers.dart';
import 'permissions_editor.dart';

final accountProvider = FutureProvider.autoDispose.family<UserAccountDto, String>(
  (ref, id) => ref.watch(hrApiProvider).userAccounts.UserAccount_Get(id: id),
);

final userPermissionsProvider = FutureProvider.autoDispose.family<PermissionListResultDto, String>(
  (ref, id) => ref.watch(hrApiProvider).rolesPermissions.UserAccount_GetPermissions(id: id),
);

/// S-ACC-02/04/05/06/07: account state, roles, password reset link, MFA reset, direct permissions.
class AccountDetailPage extends ConsumerWidget {
  const AccountDetailPage({super.key, required this.id});
  final String id;

  Future<void> _setRoles(BuildContext context, WidgetRef ref, UserAccountDto a) async {
    final s = S.of(context);
    final selected = a.roleNames.toSet();
    EmployeeRefDto? manager;
    final ok = await showFormDialog(
      context,
      title: s.t('تعديل الأدوار', 'Edit roles'),
      fields: (setState) => [
        Consumer(builder: (context, ref, _) {
          final list = ref.watch(rolesProvider).valueOrNull ?? const <RoleDto>[];
          return Wrap(spacing: 8, runSpacing: 8, children: [
            for (final r in list)
              FilterChip(
                label: Text(r.displayName),
                selected: selected.contains(r.name),
                onSelected: (v) => setState(() => v ? selected.add(r.name) : selected.remove(r.name)),
              ),
          ]);
        }),
        Text(
          s.t('إذا تغيّر من يصلح مديراً للموظف بسبب الدور الجديد، اختر المدير الجديد.',
              'If the new roles change who may manage this employee, pick the new manager.'),
          style: Theme.of(context).textTheme.labelSmall,
        ),
        PickerField(
          label: s.t('المدير الجديد (اختياري)', 'New manager (optional)'),
          value: manager?.fullName,
          onClear: () => setState(() => manager = null),
          onTap: () async {
            final m = await showSearchPicker<EmployeeRefDto>(context,
                title: s.manager, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
            if (m != null) setState(() => manager = m);
          },
        ),
      ],
      onSubmit: () async {
        if (selected.isEmpty) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).userAccounts.UserAccount_SetRoles(
                id: a.id,
                body: UserRolesUpdateDto(roleNames: selected.toList(), managerEmployeeId: manager?.id, concurrencyStamp: a.concurrencyStamp),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) ref.invalidate(accountProvider(id));
  }

  Future<void> _resetPassword(BuildContext context, WidgetRef ref, UserAccountDto a) async {
    final s = S.of(context);
    final reason = await askText(context, title: s.t('إرسال رابط إعادة تعيين كلمة السر', 'Send password reset link'), label: s.reason);
    if (reason == null || !context.mounted) return;
    await runAction(context, () async {
      final r = await ref.read(hrApiProvider).userAccounts.UserAccount_ResetPassword(id: a.id, body: ReasonInputDto(reason: reason));
      if (context.mounted) {
        showDone(context, s.t('أُرسل الرابط إلى بريد صاحب الحساب، وينتهي ${Fmt(context).dateTime(r.expiresAt)}',
            'The link was emailed to the account owner; it expires ${Fmt(context).dateTime(r.expiresAt)}'));
      }
    });
  }

  Future<void> _resetMfa(BuildContext context, WidgetRef ref, UserAccountDto a) async {
    final s = S.of(context);
    final reason = await askText(context, title: s.t('إعادة ضبط التحقق بخطوتين', 'Reset two-step verification'), label: s.reason);
    if (reason == null || !context.mounted) return;
    final ok = await runAction(
      context,
      () => ref.read(hrApiProvider).mfa.UserAccount_ResetMfa(id: a.id, body: ReasonInputDto(reason: reason)),
      done: s.t('تمت إعادة الضبط، وسيُطلب منه التفعيل من جديد', 'Reset; the user must enroll again'),
    );
    if (ok) ref.invalidate(accountProvider(id));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final roles = ref.watch(rolesProvider).valueOrNull ?? const <RoleDto>[];
    final value = ref.watch(accountProvider(id));
    final canPermissions = session.can(HrOps.UserAccount_GetPermissions);
    return HrPage(
      title: value.valueOrNull?.employee?.fullName ?? value.valueOrNull?.userName ?? s.account,
      child: AsyncBody<UserAccountDto>(
        value: value,
        onRetry: () => ref.invalidate(accountProvider(id)),
        builder: (a) {
          final info = HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.account),
              InfoRow(s.t('اسم المستخدم', 'User name'), a.userName),
              if (a.email != null) InfoRow(s.email, a.email!),
              if (a.employee != null) InfoRow(s.employee, [a.employee!.fullName, if (a.employee!.departmentName != null) a.employee!.departmentName!].join(' · ')),
              InfoRow(s.status, a.isActive ? s.t('فعال', 'Active') : s.t('موقوف (الموظف موقوف أو منتهي)', 'Inactive (employee suspended or ended)')),
              if (a.isLockedOut) InfoRow(s.t('القفل', 'Lockout'), s.t('مقفل مؤقتاً، ويُرفع بإعادة تعيين كلمة السر', 'Temporarily locked; a password reset clears it')),
              InfoRow(s.roles, a.roleNames.map((r) => roleLabel(context, r, roles)).join('، ')),
              InfoRow(s.mfa, a.mfaEnabled ? s.mfaEnabled : (a.mfaRequired ? s.t('مطلوب وغير مفعّل', 'Required, not enabled') : s.mfaDisabled)),
              InfoRow(s.t('أُنشئ', 'Created'), f.dateTime(a.creationTime)),
              const SizedBox(height: 12),
              Wrap(spacing: 8, runSpacing: 8, children: [
                if (session.can(HrOps.UserAccount_SetRoles))
                  FilledButton.icon(onPressed: () => _setRoles(context, ref, a), icon: const Icon(Icons.badge_outlined), label: Text(s.t('الأدوار', 'Roles'))),
                if (session.can(HrOps.UserAccount_ResetPassword))
                  OutlinedButton.icon(
                      onPressed: () => _resetPassword(context, ref, a), icon: const Icon(Icons.lock_reset), label: Text(s.t('رابط كلمة السر', 'Password link'))),
                if (session.can(HrOps.UserAccount_ResetMfa) && a.mfaEnabled)
                  OutlinedButton.icon(
                      onPressed: () => _resetMfa(context, ref, a),
                      icon: const Icon(Icons.phonelink_erase, color: HrColors.danger),
                      label: Text(s.t('إعادة ضبط التحقق', 'Reset MFA'))),
                if (a.employee != null && session.can(HrOps.Employee_Get))
                  TextButton.icon(
                      onPressed: () => context.go('/employees/${a.employee!.id}'),
                      icon: const Icon(Icons.badge),
                      label: Text(s.t('ملف الموظف', 'Employee file'))),
              ]),
            ]),
          );
          if (!canPermissions) return SingleChildScrollView(child: info);
          return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            info,
            const SizedBox(height: 12),
            SectionTitle(s.t('الصلاحيات المباشرة', 'Direct permissions')),
            Expanded(
              child: Consumer(builder: (context, ref, _) {
                return AsyncBody<PermissionListResultDto>(
                  value: ref.watch(userPermissionsProvider(id)),
                  onRetry: () => ref.invalidate(userPermissionsProvider(id)),
                  builder: (p) => PermissionsEditor(
                    data: p,
                    readOnly: !session.can(HrOps.UserAccount_UpdatePermissions),
                    onSave: (changes) async {
                      final ok = await runAction(
                        context,
                        () => ref.read(hrApiProvider).rolesPermissions.UserAccount_UpdatePermissions(
                              id: id, body: PermissionGrantUpdateDto(permissions: changes)),
                        done: s.saved,
                      );
                      if (ok) ref.invalidate(userPermissionsProvider(id));
                      return ok;
                    },
                  ),
                );
              }),
            ),
          ]);
        },
      ),
    );
  }
}
