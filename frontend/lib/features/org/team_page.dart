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
import '../hrx/employee_records_tab.dart';
import 'employee_file_page.dart' show EmployeeDocumentsTab, TaskLogTile;
import 'org_providers.dart';

/// S-EMP-07: Manager_GetMyTeam, plus the manager's own administrative task log (S-ME-04).
class TeamPage extends ConsumerStatefulWidget {
  const TeamPage({super.key});

  @override
  ConsumerState<TeamPage> createState() => _TeamPageState();
}

class _TeamPageState extends ConsumerState<TeamPage> {
  String _query = '';

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider);
    final session = ref.watch(sessionProvider);
    final team = Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      SizedBox(
        width: 320,
        child: TextField(
          decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: s.t('ابحث بالاسم', 'Search by name')),
          onSubmitted: (v) => setState(() => _query = v.trim()),
        ),
      ),
      const SizedBox(height: 12),
      Expanded(
        child: PagedList<EmployeeTeamMemberDto>(
          key: ValueKey(_query),
          emptyMessage: s.t('لا يوجد موظفون مرتبطون بك', 'No employees are linked to you'),
          fetch: (skip, max) async {
            final r = await api.employees.Manager_GetMyTeam(search: _query.isEmpty ? null : _query, skipCount: skip, maxResultCount: max);
            return (r.items, r.totalCount);
          },
          itemBuilder: (context, m) => HrCard(
            onTap: () => context.go('/team/${m.id}'),
            child: Row(children: [
              PersonAvatar(name: m.fullName, photoId: m.photoId, radius: 20),
              const SizedBox(width: 12),
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(m.fullName, style: const TextStyle(fontWeight: FontWeight.w600)),
                  Text('${m.jobTitle} · ${m.department.name}', style: TextStyle(color: context.hr.textSecondary)),
                  if (m.currentShift != null) Text('${s.shift}: ${m.currentShift!.name}', style: Theme.of(context).textTheme.labelSmall),
                ]),
              ),
              StatusPill(employeeStatusLabel(s, m.status), tone: m.status == EmployeeStatus.active ? Tone.success : Tone.warning),
            ]),
          ),
        ),
      ),
    ]);
    if (!session.can(HrOps.Me_GetAdministrativeTaskLog)) {
      return HrPage(title: s.t('فريقي', 'My team'), child: team);
    }
    return HrPage(
      title: s.t('فريقي', 'My team'),
      child: DefaultTabController(
        length: 2,
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [
            Tab(text: s.t('الموظفون', 'Employees')),
            Tab(text: s.t('سجل مهامي الإدارية', 'My task log')),
          ]),
          const SizedBox(height: 12),
          Expanded(
            child: TabBarView(children: [
              team,
              PagedList<AdministrativeTaskLogEntryDto>(
                emptyMessage: s.t('لا توجد مهام مسجلة', 'No tasks recorded'),
                fetch: (skip, max) async {
                  final r = await api.currentUser.Me_GetAdministrativeTaskLog(skipCount: skip, maxResultCount: max);
                  return (r.items, r.totalCount);
                },
                itemBuilder: (context, t) => TaskLogTile(entry: t, when: f.dateTime(t.occurredAt)),
              ),
            ]),
          ),
        ]),
      ),
    );
  }
}

final teamMemberProvider = FutureProvider.autoDispose.family<EmployeeTeamMemberDto, String>(
  (ref, id) => ref.watch(hrApiProvider).employees.Manager_GetTeamMember(id: id),
);

final teamMemberBalanceProvider = FutureProvider.autoDispose.family<LeaveBalanceDto?, String>((ref, id) async {
  if (!ref.watch(sessionProvider).can(HrOps.LeaveBalance_GetByEmployee)) return null;
  return ref.watch(hrApiProvider).leaveBalance.LeaveBalance_GetByEmployee(id: id);
});

/// S-EMP-08: the file as the direct manager sees it (no financial data, complaints or contract).
class TeamMemberPage extends ConsumerWidget {
  const TeamMemberPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final value = ref.watch(teamMemberProvider(id));
    return HrPage(
      title: value.valueOrNull?.fullName ?? s.t('موظف', 'Employee'),
      child: AsyncBody<EmployeeTeamMemberDto>(
        value: value,
        onRetry: () => ref.invalidate(teamMemberProvider(id)),
        builder: (m) {
          final records = employeeRecordTabs(s, ref.watch(sessionProvider), m.id, withComplaints: false);
          return DefaultTabController(
          length: 2 + records.length,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [
              Tab(text: s.t('البيانات', 'Data')),
              Tab(text: s.t('الوثائق', 'Documents')),
              for (final r in records) Tab(text: r.label),
            ]),
            const SizedBox(height: 12),
            Expanded(
              child: TabBarView(children: [
                ListView(padding: const EdgeInsets.only(bottom: 24), children: [
                  HrCard(
                    child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                      InfoRow(s.department, m.department.name),
                      InfoRow(s.jobTitle, m.jobTitle),
                      InfoRow(s.hireDate, f.date(m.hireDate)),
                      InfoRow(s.status, employeeStatusLabel(s, m.status)),
                      if (m.currentShift != null) InfoRow(s.shift, m.currentShift!.name),
                      if (m.email != null) InfoRow(s.email, m.email!),
                      if (m.phoneNumber != null) InfoRow(s.phone, m.phoneNumber!),
                    ]),
                  ),
                  const SizedBox(height: 12),
                  AsyncBody<LeaveBalanceDto?>(
                    value: ref.watch(teamMemberBalanceProvider(id)),
                    onRetry: () => ref.invalidate(teamMemberBalanceProvider(id)),
                    builder: (b) => b == null
                        ? const SizedBox.shrink()
                        : HrCard(
                            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                              SectionTitle(s.t('رصيد الإجازات', 'Leave balance')),
                              if (!b.isEntitlementSet)
                                Text(s.entitlementNotSet)
                              else ...[
                                InfoRow(s.remaining, '${f.number(b.remainingDays)} ${s.days}'),
                                InfoRow(s.used, f.number(b.usedDays)),
                                InfoRow(s.held, f.number(b.heldDays)),
                              ],
                            ]),
                          ),
                  ),
                ]),
                EmployeeDocumentsTab(employeeId: m.id, canManage: false),
                for (final r in records) r.body,
              ]),
            ),
          ]),
        );
        },
      ),
    );
  }
}
