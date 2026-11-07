import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import 'org_providers.dart';

/// S-ACC-01: UserAccount_List. No form anywhere carries a password.
class AccountsPage extends ConsumerStatefulWidget {
  const AccountsPage({super.key});

  @override
  ConsumerState<AccountsPage> createState() => _AccountsPageState();
}

class _AccountsPageState extends ConsumerState<AccountsPage> {
  String _query = '';
  String? _role;
  bool? _active;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final api = ref.watch(hrApiProvider).userAccounts;
    final roles = ref.watch(rolesProvider).valueOrNull ?? const <RoleDto>[];
    return HrPage(
      title: s.t('الحسابات', 'Accounts'),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Wrap(spacing: 12, runSpacing: 12, crossAxisAlignment: WrapCrossAlignment.center, children: [
          SizedBox(
            width: 280,
            child: TextField(
              decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: s.t('اسم المستخدم أو الموظف', 'User or employee name')),
              onSubmitted: (v) => setState(() => _query = v.trim()),
            ),
          ),
          DropdownButton<String?>(
            value: _role,
            hint: Text(s.roles),
            items: [
              DropdownMenuItem(value: null, child: Text(s.all)),
              for (final r in roles) DropdownMenuItem(value: r.name, child: Text(r.displayName)),
            ],
            onChanged: (v) => setState(() => _role = v),
          ),
          DropdownButton<bool?>(
            value: _active,
            hint: Text(s.status),
            items: [
              DropdownMenuItem(value: null, child: Text(s.all)),
              DropdownMenuItem(value: true, child: Text(s.t('فعال', 'Active'))),
              DropdownMenuItem(value: false, child: Text(s.t('موقوف', 'Inactive'))),
            ],
            onChanged: (v) => setState(() => _active = v),
          ),
        ]),
        const SizedBox(height: 12),
        Expanded(
          child: PagedList<UserAccountDto>(
            key: ValueKey('$_query|$_role|$_active'),
            fetch: (skip, max) async {
              final r = await api.UserAccount_List(
                  search: _query.isEmpty ? null : _query, roleName: _role, isActive: _active, skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, a) => HrCard(
              onTap: () => context.go('/accounts/${a.id}'),
              child: Row(children: [
                Icon(Icons.account_circle_outlined, color: context.hr.accent, size: 32),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(a.employee?.fullName ?? a.userName, style: const TextStyle(fontWeight: FontWeight.w600)),
                    Directionality(textDirection: TextDirection.ltr, child: Text(a.userName, style: TextStyle(color: context.hr.textSecondary))),
                    Text(a.roleNames.map((r) => roleLabel(context, r, roles)).join('، '),
                        style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary)),
                  ]),
                ),
                if (a.isLockedOut) ...[StatusPill(s.t('مقفل مؤقتاً', 'Locked out'), tone: Tone.danger), const SizedBox(width: 6)],
                if (a.mfaRequired && !a.mfaEnabled) ...[StatusPill(s.t('بانتظار التحقق بخطوتين', 'MFA pending'), tone: Tone.warning), const SizedBox(width: 6)],
                StatusPill(a.isActive ? s.t('فعال', 'Active') : s.t('موقوف', 'Inactive'), tone: a.isActive ? Tone.success : Tone.neutral),
              ]),
            ),
          ),
        ),
      ]),
    );
  }
}
