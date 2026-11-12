import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/person_avatar.dart';
import '../finance/finance_shared.dart' show money;
import 'contracts_page.dart' show contractStatusLabel, contractTone;

// The executive director's read-only pages (decision 2026-11-11): every employee with the base salary only, and every
// employee with the details of the current contract only. Editing stays with the accountant and the HR manager.

/// A search box over a paged list of employees.
class _SearchedList<T> extends StatefulWidget {
  const _SearchedList({required this.fetch, required this.itemBuilder, required this.emptyMessage});
  final Future<(List<T>, int)> Function(String? search, int skip, int max) fetch;
  final Widget Function(BuildContext context, T item) itemBuilder;
  final String emptyMessage;

  @override
  State<_SearchedList<T>> createState() => _SearchedListState<T>();
}

class _SearchedListState<T> extends State<_SearchedList<T>> {
  final _search = TextEditingController();
  String _query = '';

  @override
  void dispose() {
    _search.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      TextField(
        controller: _search,
        decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: s.t('ابحث بالاسم أو الرقم الوظيفي', 'Search by name or employee number')),
        onSubmitted: (v) => setState(() => _query = v.trim()),
      ),
      const SizedBox(height: 12),
      Expanded(
        child: PagedList<T>(
          key: ValueKey(_query),
          emptyMessage: widget.emptyMessage,
          fetch: (skip, max) => widget.fetch(_query.isEmpty ? null : _query, skip, max),
          itemBuilder: widget.itemBuilder,
        ),
      ),
    ]);
  }
}

/// «رواتب الموظفين»: name and department, with the base salary in force today beside it.
class EmployeeSalariesPage extends ConsumerWidget {
  const EmployeeSalariesPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).finance;
    return HrPage(
      title: s.t('رواتب الموظفين', 'Employee salaries'),
      subtitle: s.t('الراتب الأساسي الساري اليوم لكل موظف. للاطلاع فقط.', 'The base salary in force today for each employee. View only.'),
      child: _SearchedList<EmployeeSalaryOverviewDto>(
        emptyMessage: s.t('لا يوجد موظفون.', 'No employees.'),
        fetch: (search, skip, max) async {
          final r = await api.SalaryProfile_GetOverview(search: search, skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, x) => HrCard(
          child: Row(children: [
            PersonAvatar.employee(x.employee, radius: 20),
            const SizedBox(width: 12),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(x.employee.fullName, style: const TextStyle(fontWeight: FontWeight.w600)),
                if (x.employee.departmentName != null)
                  Text(x.employee.departmentName!, style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
              ]),
            ),
            x.baseSalary == null
                ? Text(s.t('لا توجد بيانات مالية', 'No salary data'), style: TextStyle(color: context.hr.textSecondary))
                : Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
                    Text(money(x.baseSalary, x.currency), style: const TextStyle(fontWeight: FontWeight.w700)),
                    if (x.effectiveFrom != null)
                      Text('${s.t('من', 'From')} ${f.date(x.effectiveFrom!)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                  ]),
          ]),
        ),
      ),
    );
  }
}

/// «عقود الموظفين»: name and department, with the current contract's type, dates and status beside it.
class EmployeeContractsPage extends ConsumerWidget {
  const EmployeeContractsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).contracts;
    return HrPage(
      title: s.t('عقود الموظفين', 'Employee contracts'),
      subtitle: s.t('العقد الحالي لكل موظف. للاطلاع فقط.', 'The current contract of each employee. View only.'),
      child: _SearchedList<EmployeeContractOverviewDto>(
        emptyMessage: s.t('لا يوجد موظفون.', 'No employees.'),
        fetch: (search, skip, max) async {
          final r = await api.Contract_GetOverview(search: search, skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, x) => HrCard(
          child: Row(children: [
            PersonAvatar.employee(x.employee, radius: 20),
            const SizedBox(width: 12),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(x.employee.fullName, style: const TextStyle(fontWeight: FontWeight.w600)),
                if (x.employee.departmentName != null)
                  Text(x.employee.departmentName!, style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                if (x.contractType != null) ...[
                  const SizedBox(height: 4),
                  Text('${x.contractType} · ${f.date(x.startDate!)} → ${f.date(x.endDate!)}'),
                ],
              ]),
            ),
            x.status == null
                ? Text(s.t('لا يوجد عقد', 'No contract'), style: TextStyle(color: context.hr.textSecondary))
                : StatusPill(contractStatusLabel(s, x.status!), tone: contractTone(x.status!)),
          ]),
        ),
      ),
    );
  }
}
