import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';

/// S-DEP-01: Department_List with create, edit (concurrencyStamp) and soft delete.
class DepartmentsPage extends ConsumerStatefulWidget {
  const DepartmentsPage({super.key});

  @override
  ConsumerState<DepartmentsPage> createState() => _DepartmentsPageState();
}

class _DepartmentsPageState extends ConsumerState<DepartmentsPage> {
  int _version = 0;
  String _query = '';

  Future<void> _edit([DepartmentDto? d]) async {
    final s = S.of(context);
    final ar = TextEditingController(text: d?.nameAr ?? '');
    final en = TextEditingController(text: d?.nameEn ?? '');
    final api = ref.read(hrApiProvider).departments;
    final ok = await showFormDialog(
      context,
      title: d == null ? s.t('قسم جديد', 'New department') : s.t('تعديل القسم', 'Edit department'),
      fields: (_) => [
        TextFormField(controller: ar, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Name (Arabic)')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Name (English)')), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () => runAction(
        context,
        () => d == null
            ? api.Department_Create(body: DepartmentCreateDto(nameAr: ar.text.trim(), nameEn: en.text.trim()))
            : api.Department_Update(
                id: d.id, body: DepartmentUpdateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), concurrencyStamp: d.concurrencyStamp)),
        done: s.saved,
      ),
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _delete(DepartmentDto d) async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('حذف القسم', 'Delete department'), message: d.name, destructive: true)) return;
    if (!mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).departments.Department_Delete(id: d.id), done: s.t('تم الحذف', 'Deleted'));
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final canManage = session.can(HrOps.Department_Create);
    final api = ref.watch(hrApiProvider).departments;
    return HrPage(
      title: s.t('الأقسام', 'Departments'),
      actions: [
        if (canManage) FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('قسم جديد', 'New department'))),
      ],
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        SizedBox(
          width: 320,
          child: TextField(
            decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: s.t('ابحث', 'Search')),
            onSubmitted: (v) => setState(() => _query = v.trim()),
          ),
        ),
        const SizedBox(height: 12),
        Expanded(
          child: PagedList<DepartmentDto>(
            key: ValueKey('$_version|$_query'),
            fetch: (skip, max) async {
              final r = await api.Department_List(search: _query.isEmpty ? null : _query, skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, d) => HrCard(
              child: Row(children: [
                Icon(Icons.apartment, color: context.hr.accent),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(d.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                    Text(s.isAr ? d.nameEn : d.nameAr, style: TextStyle(color: context.hr.textSecondary)),
                  ]),
                ),
                StatusPill('${d.employeeCount} ${s.t('موظف', 'employees')}', tone: Tone.info),
                if (canManage) ...[
                  IconButton(tooltip: s.t('تعديل', 'Edit'), onPressed: () => _edit(d), icon: const Icon(Icons.edit_outlined)),
                  IconButton(
                      tooltip: s.t('حذف', 'Delete'),
                      onPressed: d.employeeCount > 0 ? null : () => _delete(d),
                      icon: const Icon(Icons.delete_outline, color: HrColors.danger)),
                ],
              ]),
            ),
          ),
        ),
      ]),
    );
  }
}
