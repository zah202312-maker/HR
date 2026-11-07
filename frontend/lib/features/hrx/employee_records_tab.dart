import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../ops/custody_page.dart' show custodyStatusLabel, custodyTone;
import '../ops/ops_shared.dart';
import 'appreciation_page.dart';
import 'contracts_page.dart' show contractStatusLabel, contractTone;
import 'warnings_page.dart' show warningLevelLabel, warningStatusLabel, warningTone;

/// Employee file record: contracts, warnings file, appreciation letters, custody — each by permission.
class EmployeeRecordsTab extends ConsumerWidget {
  const EmployeeRecordsTab({super.key, required this.employeeId});
  final String employeeId;

  Widget _section<T>(BuildContext context, String title, Future<List<T>> future, Widget Function(T) item) => Padding(
        padding: const EdgeInsets.only(bottom: 16),
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          SectionTitle(title),
          FutureBuilder<List<T>>(
            future: future,
            builder: (context, snap) {
              if (snap.hasError) return ErrorState(error: snap.error!);
              if (!snap.hasData) return const LinearProgressIndicator();
              if (snap.data!.isEmpty) return Text('—', style: Theme.of(context).textTheme.bodySmall);
              return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [for (final x in snap.data!) item(x)]);
            },
          ),
        ]),
      );

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider);
    final sections = <Widget>[
      if (session.can(HrOps.Contract_List))
        _section<ContractDto>(
          context,
          s.t('العقود', 'Contracts'),
          api.contracts.Contract_List(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
          (c) => OpsTile(
            leading: Icons.description_outlined,
            title: c.contractType,
            subtitle: '${f.date(c.startDate)} → ${f.date(c.endDate)}',
            trailing: StatusPill(contractStatusLabel(s, c.status), tone: contractTone(c.status)),
            onTap: () => context.go('/contracts/${c.id}'),
          ),
        ),
      if (session.can(HrOps.Warning_GetEmployeeFile))
        _section<WarningFileEntryDto>(
          context,
          s.t('سجل الإنذارات', 'Warnings record'),
          api.warnings.Warning_GetEmployeeFile(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
          (w) => OpsTile(
            leading: Icons.report_outlined,
            title: '${warningLevelLabel(s, w.level)} · ${f.date(w.incidentDate)}',
            subtitle: [w.decisionNote, if (w.employeeResponse != null) '${s.t('الرد', 'Response')}: ${w.employeeResponse!.content}'].join(' · '),
            trailing: StatusPill(warningStatusLabel(s, w.status), tone: warningTone(w.status)),
          ),
        ),
      if (session.can(HrOps.AppreciationLetter_GetEmployeeFile))
        _section<AppreciationLetterDto>(
          context,
          s.t('كتب الشكر', 'Appreciation letters'),
          api.appreciationLetters.AppreciationLetter_GetEmployeeFile(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
          (l) => AppreciationTile(letter: l),
        ),
      if (session.can(HrOps.Custody_GetList))
        _section<CustodyDto>(
          context,
          s.t('العهد', 'Custody'),
          api.custody.Custody_GetList(employeeId: employeeId, maxResultCount: 50).then((r) => r.items),
          (c) => OpsTile(
            leading: Icons.inventory_2_outlined,
            title: '${qty(c.quantity)} × ${c.item.name}',
            subtitle: f.date(c.assignedAt),
            trailing: StatusPill(custodyStatusLabel(s, c.status), tone: custodyTone(c.status)),
          ),
        ),
    ];
    if (sections.isEmpty) return EmptyState(message: s.forbidden);
    return ListView(padding: const EdgeInsets.only(bottom: 24), children: sections);
  }
}
