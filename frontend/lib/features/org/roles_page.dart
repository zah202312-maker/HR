import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/pickers.dart';
import 'org_providers.dart';
import 'permissions_editor.dart';

/// S-ROLE-01: Role_List, Role_Create, Role_Delete (custom roles only).
class RolesPage extends ConsumerWidget {
  const RolesPage({super.key});

  Future<void> _create(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    final name = TextEditingController();
    final ar = TextEditingController();
    final en = TextEditingController();
    final ok = await showFormDialog(
      context,
      title: s.t('دور جديد', 'New role'),
      fields: (_) => [
        TextFormField(
          controller: name,
          textDirection: TextDirection.ltr,
          decoration: InputDecoration(labelText: s.t('المعرّف (camelCase)', 'Identifier (camelCase)'), hintText: 'payrollViewer'),
          validator: (v) => RegExp(r'^[a-z][a-zA-Z0-9]{1,63}$').hasMatch(v ?? '') ? null : s.t('حرف صغير أولاً ثم حروف وأرقام', 'Lowercase first, then letters and digits'),
        ),
        TextFormField(controller: ar, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Name (Arabic)')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Name (English)')), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).rolesPermissions.Role_Create(
            body: RoleCreateDto(name: name.text.trim(), nameAr: ar.text.trim(), nameEn: en.text.trim())),
        done: s.saved,
      ),
    );
    if (ok) ref.invalidate(rolesProvider);
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.t('الأدوار والصلاحيات', 'Roles and permissions'),
      actions: [
        if (session.can(HrOps.Role_Create))
          FilledButton.icon(onPressed: () => _create(context, ref), icon: const Icon(Icons.add), label: Text(s.t('دور جديد', 'New role'))),
      ],
      child: AsyncBody<List<RoleDto>>(
        value: ref.watch(rolesProvider),
        onRetry: () => ref.invalidate(rolesProvider),
        builder: (roles) => ListView.separated(
          padding: const EdgeInsets.only(bottom: 24),
          itemCount: roles.length,
          separatorBuilder: (_, __) => const SizedBox(height: 10),
          itemBuilder: (context, i) {
            final r = roles[i];
            return HrCard(
              onTap: session.can(HrOps.Role_Get) ? () => context.go('/roles/${r.id}') : null,
              child: Row(children: [
                Icon(r.isStatic ? Icons.verified_user_outlined : Icons.tune, color: context.hr.accent),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(r.displayName, style: const TextStyle(fontWeight: FontWeight.w600)),
                    Directionality(textDirection: TextDirection.ltr, child: Text(r.name, style: TextStyle(color: context.hr.textSecondary))),
                  ]),
                ),
                StatusPill('${r.userCount} ${s.t('مستخدم', 'users')}', tone: Tone.info),
                const SizedBox(width: 6),
                StatusPill(r.isStatic ? s.t('ثابت', 'Built-in') : s.t('مخصص', 'Custom'), tone: r.isStatic ? Tone.neutral : Tone.success),
              ]),
            );
          },
        ),
      ),
    );
  }
}

final roleProvider = FutureProvider.autoDispose.family<RoleDto, String>(
  (ref, id) => ref.watch(hrApiProvider).rolesPermissions.Role_Get(id: id),
);

final rolePermissionsProvider = FutureProvider.autoDispose.family<PermissionListResultDto, String>(
  (ref, id) => ref.watch(hrApiProvider).rolesPermissions.Role_GetPermissions(id: id),
);

/// S-ROLE-02: Role_Get, Role_Update (display names), Role_Delete, role permissions.
class RoleDetailPage extends ConsumerWidget {
  const RoleDetailPage({super.key, required this.id});
  final String id;

  Future<void> _rename(BuildContext context, WidgetRef ref, RoleDto r) async {
    final s = S.of(context);
    final ar = TextEditingController(text: r.nameAr);
    final en = TextEditingController(text: r.nameEn);
    final ok = await showFormDialog(
      context,
      title: s.t('تعديل الاسم', 'Rename'),
      fields: (_) => [
        TextFormField(controller: ar, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Name (Arabic)')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Name (English)')), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).rolesPermissions.Role_Update(
            id: r.id, body: RoleUpdateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), concurrencyStamp: r.concurrencyStamp)),
        done: s.saved,
      ),
    );
    if (ok) {
      ref.invalidate(roleProvider(id));
      ref.invalidate(rolesProvider);
    }
  }

  Future<void> _delete(BuildContext context, WidgetRef ref, RoleDto r) async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('حذف الدور', 'Delete role'), message: r.displayName, destructive: true)) return;
    if (!context.mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).rolesPermissions.Role_Delete(id: r.id), done: s.t('تم الحذف', 'Deleted'));
    if (ok && context.mounted) {
      ref.invalidate(rolesProvider);
      context.go('/roles');
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final value = ref.watch(roleProvider(id));
    return HrPage(
      title: value.valueOrNull?.displayName ?? s.roles,
      child: AsyncBody<RoleDto>(
        value: value,
        onRetry: () => ref.invalidate(roleProvider(id)),
        builder: (r) => Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          HrCard(
            child: Row(children: [
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text('${r.nameAr} · ${r.nameEn}', style: const TextStyle(fontWeight: FontWeight.w600)),
                  Text('${r.name} · ${r.userCount} ${s.t('مستخدم', 'users')}', style: TextStyle(color: context.hr.textSecondary)),
                ]),
              ),
              if (session.can(HrOps.Role_Update))
                IconButton(tooltip: s.t('تعديل', 'Edit'), onPressed: () => _rename(context, ref, r), icon: const Icon(Icons.edit_outlined)),
              if (session.can(HrOps.Role_Delete) && !r.isStatic)
                IconButton(
                    tooltip: s.t('حذف', 'Delete'),
                    onPressed: r.userCount > 0 ? null : () => _delete(context, ref, r),
                    icon: const Icon(Icons.delete_outline, color: HrColors.danger)),
            ]),
          ),
          const SizedBox(height: 12),
          if (session.can(HrOps.Role_GetPermissions))
            Expanded(
              child: AsyncBody<PermissionListResultDto>(
                value: ref.watch(rolePermissionsProvider(id)),
                onRetry: () => ref.invalidate(rolePermissionsProvider(id)),
                builder: (p) => PermissionsEditor(
                  data: p,
                  readOnly: !session.can(HrOps.Role_UpdatePermissions),
                  onSave: (changes) async {
                    final ok = await runAction(
                      context,
                      () => ref.read(hrApiProvider).rolesPermissions.Role_UpdatePermissions(id: id, body: PermissionGrantUpdateDto(permissions: changes)),
                      done: s.saved,
                    );
                    if (ok) ref.invalidate(rolePermissionsProvider(id));
                    return ok;
                  },
                ),
              ),
            ),
        ]),
      ),
    );
  }
}
