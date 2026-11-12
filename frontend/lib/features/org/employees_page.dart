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
import '../../core/widgets/person_avatar.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import 'org_providers.dart';

/// S-EMP-01: Employee_List for HR, filters by name, department and status.
class EmployeesPage extends ConsumerStatefulWidget {
  const EmployeesPage({super.key});

  @override
  ConsumerState<EmployeesPage> createState() => _EmployeesPageState();
}

class _EmployeesPageState extends ConsumerState<EmployeesPage> {
  final _search = TextEditingController();
  String _query = '';
  DepartmentRefDto? _department;
  EmployeeStatus? _status;

  @override
  void dispose() {
    _search.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).employees;
    final roles = ref.watch(rolesProvider).valueOrNull ?? const <RoleDto>[];
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.t('الموظفون', 'Employees'),
      actions: [
        if (session.can(HrOps.Employee_Create))
          FilledButton.icon(
            onPressed: () => context.go('/employees/new'),
            icon: const Icon(Icons.person_add_alt),
            label: Text(s.t('إضافة موظف', 'Add employee')),
          ),
      ],
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Wrap(spacing: 12, runSpacing: 12, crossAxisAlignment: WrapCrossAlignment.center, children: [
          SizedBox(
            width: 280,
            child: TextField(
              controller: _search,
              decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: s.t('ابحث بالاسم', 'Search by name')),
              onSubmitted: (v) => setState(() => _query = v.trim()),
            ),
          ),
          SizedBox(
            width: 240,
            child: PickerField(
              label: s.department,
              value: _department?.name,
              onClear: () => setState(() => _department = null),
              onTap: () async {
                final d = await showSearchPicker<DepartmentRefDto>(context,
                    title: s.department, search: (q) => searchDepartments(ref, q), label: (d) => d.name);
                if (d != null) setState(() => _department = d);
              },
            ),
          ),
          DropdownButton<EmployeeStatus?>(
            value: _status,
            hint: Text(s.status),
            items: [
              DropdownMenuItem(value: null, child: Text(s.all)),
              for (final st in EmployeeStatus.values.where((v) => v != EmployeeStatus.$unknown))
                DropdownMenuItem(value: st, child: Text(employeeStatusLabel(s, st))),
            ],
            onChanged: (v) => setState(() => _status = v),
          ),
        ]),
        const SizedBox(height: 12),
        Expanded(
          child: PagedList<EmployeeListItemDto>(
            key: ValueKey('$_query|${_department?.id}|$_status'),
            fetch: (skip, max) async {
              final r = await api.Employee_List(
                search: _query.isEmpty ? null : _query,
                departmentId: _department?.id,
                status: _status,
                skipCount: skip,
                maxResultCount: max,
              );
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, e) => HrCard(
              onTap: () => context.go('/employees/${e.id}'),
              child: Row(children: [
                PersonAvatar(name: e.fullName, photoId: e.photoId, radius: 20),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(e.fullName, style: const TextStyle(fontWeight: FontWeight.w600)),
                    const SizedBox(height: 2),
                    Text('${e.jobTitle} · ${e.department.name}', style: TextStyle(color: context.hr.textSecondary)),
                    const SizedBox(height: 4),
                    Text(
                      [
                        '${s.hireDate}: ${f.date(e.hireDate)}',
                        if (e.manager != null) '${s.manager}: ${e.manager!.fullName}',
                        e.roleNames.map((r) => roleLabel(context, r, roles)).join('، '),
                      ].join(' · '),
                      style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary),
                    ),
                  ]),
                ),
                if (!e.hasAccount) ...[
                  StatusPill(s.t('بدون حساب', 'No account'), tone: Tone.warning),
                  const SizedBox(width: 8),
                ],
                StatusPill(employeeStatusLabel(s, e.status),
                    tone: e.status == EmployeeStatus.active ? Tone.success : (e.status == EmployeeStatus.ended ? Tone.neutral : Tone.warning)),
              ]),
            ),
          ),
        ),
      ]),
    );
  }
}
