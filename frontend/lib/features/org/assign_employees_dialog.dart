import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/pickers.dart';
import 'org_providers.dart';

/// Manager_AssignEmployees: links up to 200 employees to [manager] in one all-or-nothing call.
/// The manager role itself is granted on the account (roles), not here.
Future<bool> showAssignEmployeesDialog(BuildContext context, WidgetRef ref, EmployeeRefDto manager) async {
  final s = S.of(context);
  final picked = <EmployeeRefDto>[];
  final reason = TextEditingController();
  return showFormDialog(
    context,
    title: s.t('تعيين موظفين للمدير ${manager.fullName}', 'Assign employees to ${manager.fullName}'),
    submitLabel: s.t('تعيين', 'Assign'),
    fields: (setState) => [
      Wrap(spacing: 6, runSpacing: 6, children: [
        for (final e in picked)
          InputChip(label: Text(e.fullName), onDeleted: () => setState(() => picked.remove(e))),
        ActionChip(
          avatar: const Icon(Icons.person_add_alt, size: 18),
          label: Text(s.t('إضافة موظف', 'Add employee')),
          onPressed: picked.length >= 200
              ? null
              : () async {
                  final e = await showSearchPicker<EmployeeRefDto>(context,
                      title: s.employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
                  if (e != null && e.id != manager.id && !picked.any((x) => x.id == e.id)) setState(() => picked.add(e));
                },
        ),
      ]),
      TextFormField(controller: reason, maxLines: 2, maxLength: 2000, decoration: InputDecoration(labelText: s.reason)),
    ],
    onSubmit: () async {
      if (picked.isEmpty) {
        showError(context, s.t('اختر موظفاً واحداً على الأقل', 'Pick at least one employee'));
        return false;
      }
      return runAction(context, () async {
        final r = await ref.read(hrApiProvider).employees.Manager_AssignEmployees(
              id: manager.id,
              body: ManagerAssignEmployeesInputDto(
                employeeIds: [for (final e in picked) e.id],
                reason: reason.text.trim().isEmpty ? null : reason.text.trim(),
              ),
            );
        if (context.mounted) {
          showDone(
            context,
            s.t('تم تعيين ${r.assignedCount}، مرتبطون مسبقاً ${r.alreadyAssignedCount}، عناصر معلقة منقولة ${r.movedPendingItemCount}',
                'Assigned ${r.assignedCount}, already linked ${r.alreadyAssignedCount}, pending items moved ${r.movedPendingItemCount}'),
          );
        }
      });
    },
  );
}
