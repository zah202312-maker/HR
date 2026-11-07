import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';

/// Role_List: the eight built-in roles and any custom role (HR and system admin).
final rolesProvider = FutureProvider.autoDispose<List<RoleDto>>((ref) async {
  if (!ref.watch(sessionProvider).can(HrOps.Role_List)) return const <RoleDto>[];
  final r = await ref.watch(hrApiProvider).rolesPermissions.Role_List(maxResultCount: 200);
  return r.items;
});

/// WorkShift_List (active only) for the create-employee wizard and shift assignment.
final activeShiftsProvider = FutureProvider.autoDispose<List<WorkShiftDto>>((ref) async {
  final r = await ref.watch(hrApiProvider).workShift.WorkShift_List(isActive: true, maxResultCount: 200);
  return r.items;
});

/// Role label: server display name when known, otherwise the built-in label.
String roleLabel(BuildContext context, String name, List<RoleDto> roles) {
  for (final r in roles) {
    if (r.name == name) return r.displayName;
  }
  final builtIn = AppRole.fromJson(name);
  return builtIn == AppRole.$unknown ? name : S.of(context).role(builtIn);
}

String employeeStatusLabel(S s, EmployeeStatus st) => switch (st) {
      EmployeeStatus.active => s.t('على رأس العمل', 'Active'),
      EmployeeStatus.suspended => s.t('موقوف', 'Suspended'),
      EmployeeStatus.ended => s.t('منتهي الخدمة', 'Ended'),
      EmployeeStatus.$unknown => '—',
    };

/// Search helpers over lookup operations, for pickers.
Future<List<EmployeeRefDto>> searchEmployees(WidgetRef ref, String q) async =>
    (await ref.read(hrApiProvider).employees.Employee_Lookup(search: q.isEmpty ? null : q, maxResultCount: 50)).items;

Future<List<DepartmentRefDto>> searchDepartments(WidgetRef ref, String q) async =>
    (await ref.read(hrApiProvider).departments.Department_Lookup(search: q.isEmpty ? null : q, maxResultCount: 50)).items;

Future<List<UserRefDto>> searchUsers(WidgetRef ref, String q) async =>
    (await ref.read(hrApiProvider).userAccounts.UserAccount_Lookup(search: q.isEmpty ? null : q, maxResultCount: 50)).items;
