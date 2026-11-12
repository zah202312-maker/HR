import 'package:file_picker/file_picker.dart' show PlatformFile;
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';
import '../org/org_providers.dart' show searchEmployees;

String contractStatusLabel(S s, ContractStatus st) => switch (st) {
      ContractStatus.active => s.t('ساري', 'Active'),
      ContractStatus.pendingRecommendation => s.t('بانتظار التوصية', 'Awaiting recommendation'),
      ContractStatus.pendingDecision => s.t('بانتظار القرار', 'Awaiting decision'),
      ContractStatus.renewed => s.t('جُدّد', 'Renewed'),
      ContractStatus.notRenewed => s.t('لم يُجدّد', 'Not renewed'),
      _ => '—',
    };

Tone contractTone(ContractStatus st) => switch (st) {
      ContractStatus.active || ContractStatus.renewed => Tone.success,
      ContractStatus.notRenewed => Tone.neutral,
      _ => Tone.warning,
    };

String renewalLabel(S s, ContractRenewalOption o) => o == ContractRenewalOption.renew ? s.t('تجديد', 'Renew') : s.t('عدم التجديد', 'Do not renew');

/// Creates a contract (also a renewal when [previous] is given).
Future<bool> showContractDialog(BuildContext context, WidgetRef ref, {EmployeeRefDto? employee, ContractDto? previous}) async {
  final s = S.of(context);
  final f = Fmt(context);
  var emp = employee ?? previous?.employee;
  final type = TextEditingController(text: previous?.contractType);
  DateTime? start = previous?.endDate.add(const Duration(days: 1));
  DateTime? end;
  final files = <PlatformFile>[];
  final key = const Uuid().v4();
  return showFormDialog(
    context,
    title: previous == null ? s.t('عقد جديد', 'New contract') : s.t('عقد تجديد', 'Renewal contract'),
    fields: (setState) => [
      PickerField(
        label: s.employee,
        value: emp?.fullName,
        onTap: previous != null
            ? () {}
            : () async {
                final x = await showSearchPicker<EmployeeRefDto>(context,
                    title: s.employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
                if (x != null) setState(() => emp = x);
              },
      ),
      TextFormField(controller: type, maxLength: 100, decoration: InputDecoration(labelText: s.t('نوع العقد', 'Contract type')), validator: (v) => requiredValidator(context, v)),
      PickerField(label: s.startDate, value: start == null ? null : f.date(start), icon: Icons.event, onTap: () async {
        final x = await pickDate(context, initial: start);
        if (x != null) setState(() => start = x);
      }),
      PickerField(label: s.endDate, value: end == null ? null : f.date(end), icon: Icons.event, onTap: () async {
        final x = await pickDate(context, initial: end ?? start);
        if (x != null) setState(() => end = x);
      }),
      // The paper contract and its documents, up to 10 files (decision 2026-11-10).
      Text(s.t('مرفقات العقد (اختياري)', 'Contract attachments (optional)'), style: Theme.of(context).textTheme.labelLarge),
      Wrap(spacing: 6, runSpacing: 6, children: [
        for (final x in files) InputChip(label: Text(x.name), onDeleted: () => setState(() => files.remove(x))),
        if (files.length < 10)
          ActionChip(
            avatar: const Icon(Icons.attach_file, size: 18),
            label: Text(s.t('إرفاق العقد الورقي', 'Attach paper contract')),
            onPressed: () async {
              final picked = await pickFiles(multiple: true);
              setState(() => files.addAll(picked.take(10 - files.length)));
            },
          ),
      ]),
    ],
    onSubmit: () async {
      if (emp == null || start == null || end == null) {
        showError(context, s.validation);
        return false;
      }
      return runAction(
        context,
        () => ref.read(hrApiProvider).contracts.Contract_Create(
              idempotencyKey: key,
              body: ContractCreateInputDto(
                employeeId: emp!.id,
                contractType: type.text.trim(),
                startDate: start!,
                endDate: end!,
                previousContractId: previous?.id,
                attachments: files.isEmpty ? null : [for (final x in files) multipartOf(x)],
              ),
            ),
        done: s.saved,
      );
    },
  );
}

/// S-CON: contracts — mine (employee), expiring and all (HR).
class ContractsPage extends ConsumerStatefulWidget {
  const ContractsPage({super.key});

  @override
  ConsumerState<ContractsPage> createState() => _ContractsPageState();
}

class _ContractsPageState extends ConsumerState<ContractsPage> {
  int _version = 0;
  ContractStatus? _status;

  Widget _tile(BuildContext context, ContractDto c) {
    final s = S.of(context);
    final f = Fmt(context);
    return OpsTile(
      leading: Icons.description_outlined,
      avatar: PersonAvatar.employee(c.employee),
      title: '${c.employee.fullName} · ${c.contractType}',
      subtitle: [
        '${f.date(c.startDate)} → ${f.date(c.endDate)}',
        if (!c.isExpired && c.status != ContractStatus.renewed && c.status != ContractStatus.notRenewed) '${s.t('متبقي', 'Left')} ${c.daysRemaining} ${s.days}',
        if (c.isExpired) s.t('منتهي', 'Expired'),
        if (c.isDecisionOverdue) s.t('القرار متأخر', 'Decision overdue'),
      ].join(' · '),
      trailing: StatusPill(contractStatusLabel(s, c.status), tone: c.isDecisionOverdue ? Tone.danger : contractTone(c.status)),
      onTap: () => context.go('/contracts/${c.id}'),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).contracts;
    final tabs = <(String, Widget)>[
      if (session.can(HrOps.Contract_GetExpiring))
        (
          s.t('قاربت على الانتهاء', 'Expiring'),
          PagedList<ContractDto>(
            key: ValueKey('exp|$_version'),
            fetch: (skip, max) async {
              final r = await api.Contract_GetExpiring(skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: _tile,
          )
        ),
      if (session.can(HrOps.Contract_List))
        (
          s.t('كل العقود', 'All contracts'),
          PagedList<ContractDto>(
            key: ValueKey('all|$_version|$_status'),
            pageSize: 50,
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Align(
                alignment: AlignmentDirectional.centerStart,
                child: enumFilter<ContractStatus>(context,
                    label: s.status, value: _status, values: ContractStatus.values, text: (v) => contractStatusLabel(s, v), onChanged: (v) => setState(() => _status = v)),
              ),
            ),
            fetch: (skip, max) async {
              final r = await api.Contract_List(skipCount: skip, maxResultCount: max, status: _status);
              return (r.items, r.totalCount);
            },
            itemBuilder: _tile,
          )
        ),
      if (session.can(HrOps.Contract_GetMyList))
        (
          s.t('عقودي', 'My contracts'),
          PagedList<ContractEmployeeDto>(
            key: const ValueKey('my'),
            fetch: (skip, max) async {
              final r = await api.Contract_GetMyList(skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => OpsTile(
              leading: Icons.description_outlined,
              title: c.contractType,
              subtitle: [
                '${f.date(c.startDate)} → ${f.date(c.endDate)}',
                if (c.employeeMessage != null) c.employeeMessage!,
                if (c.decidedAt != null) '${s.t('القرار', 'Decision')} ${f.date(c.decidedAt)}',
              ].join(' · '),
              trailing: StatusPill(
                switch (c.status) {
                  ContractEmployeeStatus.renewed => s.t('جُدّد', 'Renewed'),
                  ContractEmployeeStatus.notRenewed => s.t('لم يُجدّد', 'Not renewed'),
                  _ => c.isExpired ? s.t('منتهي', 'Expired') : s.t('ساري', 'Active'),
                },
                tone: c.status == ContractEmployeeStatus.notRenewed ? Tone.neutral : Tone.success,
              ),
              // A copy of my paper contract (decision 2026-11-10).
              actions: [
                for (final a in c.attachments)
                  ActionChip(
                    avatar: const Icon(Icons.download, size: 18),
                    label: Text(a.fileName),
                    onPressed: () => runAction(context, () async {
                      await openDownload(await ref.read(hrApiProvider).contracts.Contract_GetMyAttachmentDownloadUrl(id: c.id, attachmentId: a.id));
                    }),
                  ),
              ],
            ),
          )
        ),
    ];
    return HrPage(
      title: s.t('العقود', 'Contracts'),
      actions: [
        if (session.can(HrOps.Contract_Create))
          FilledButton.icon(
            onPressed: () async {
              if (await showContractDialog(context, ref)) setState(() => _version++);
            },
            icon: const Icon(Icons.add),
            label: Text(s.t('عقد جديد', 'New contract')),
          ),
      ],
      child: tabbedBody(context, tabs),
    );
  }
}

final contractProvider = FutureProvider.autoDispose.family<ContractDto, String>((ref, id) => ref.watch(hrApiProvider).contracts.Contract_Get(id: id));

/// Contract detail: the two documented steps (recommendation, decision) and renewal.
class ContractDetailPage extends ConsumerWidget {
  const ContractDetailPage({super.key, required this.id});
  final String id;

  Future<void> _recommend(BuildContext context, WidgetRef ref, ContractDto c) async {
    final s = S.of(context);
    final position = TextEditingController();
    final note = TextEditingController();
    var option = ContractRenewalOption.renew;
    final ok = await showFormDialog(
      context,
      title: s.t('التوصية', 'Recommendation'),
      fields: (setState) => [
        TextFormField(
          controller: position,
          maxLines: 3,
          maxLength: 2000,
          decoration: InputDecoration(labelText: s.t('موقف الموظف', 'Employee position')),
          validator: (v) => requiredValidator(context, v),
        ),
        SegmentedButton<ContractRenewalOption>(
          segments: [
            ButtonSegment(value: ContractRenewalOption.renew, label: Text(renewalLabel(s, ContractRenewalOption.renew))),
            ButtonSegment(value: ContractRenewalOption.doNotRenew, label: Text(renewalLabel(s, ContractRenewalOption.doNotRenew))),
          ],
          selected: {option},
          onSelectionChanged: (v) => setState(() => option = v.first),
        ),
        TextFormField(controller: note, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () => runAction(
          context,
          () => ref.read(hrApiProvider).contracts.Contract_Recommend(
              id: c.id, body: ContractRecommendInputDto(employeePosition: position.text.trim(), recommendation: option, note: optText(note))),
          done: s.saved),
    );
    if (ok) ref.invalidate(contractProvider(id));
  }

  Future<void> _decide(BuildContext context, WidgetRef ref, ContractDto c) async {
    final s = S.of(context);
    final note = TextEditingController();
    final message = TextEditingController();
    var option = c.recommendation?.recommendation ?? ContractRenewalOption.renew;
    final ok = await showFormDialog(
      context,
      title: s.t('القرار', 'Decision'),
      fields: (setState) => [
        SegmentedButton<ContractRenewalOption>(
          segments: [
            ButtonSegment(value: ContractRenewalOption.renew, label: Text(renewalLabel(s, ContractRenewalOption.renew))),
            ButtonSegment(value: ContractRenewalOption.doNotRenew, label: Text(renewalLabel(s, ContractRenewalOption.doNotRenew))),
          ],
          selected: {option},
          onSelectionChanged: (v) => setState(() => option = v.first),
        ),
        TextFormField(controller: note, maxLength: 2000, decoration: InputDecoration(labelText: s.t('ملاحظة داخلية', 'Internal note'))),
        TextFormField(
          controller: message,
          maxLines: 3,
          maxLength: 2000,
          decoration: InputDecoration(labelText: s.t('رسالة للموظف (تصله فوراً)', 'Message to the employee (sent immediately)')),
        ),
      ],
      onSubmit: () => runAction(
          context,
          () => ref.read(hrApiProvider).contracts.Contract_Decide(
              id: c.id, body: ContractDecideInputDto(decision: option, note: optText(note), employeeMessage: optText(message))),
          done: s.saved),
    );
    if (ok) ref.invalidate(contractProvider(id));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.t('العقد', 'Contract'),
      child: AsyncBody<ContractDto>(
        value: ref.watch(contractProvider(id)),
        onRetry: () => ref.invalidate(contractProvider(id)),
        builder: (c) => ListView(padding: const EdgeInsets.only(bottom: 24), children: [
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(c.employee.fullName, trailing: StatusPill(contractStatusLabel(s, c.status), tone: contractTone(c.status))),
              InfoRow(s.t('نوع العقد', 'Contract type'), c.contractType),
              InfoRow(s.startDate, f.date(c.startDate)),
              InfoRow(s.endDate, '${f.date(c.endDate)}${c.isExpired ? ' (${s.t('منتهي', 'expired')})' : ' · ${c.daysRemaining} ${s.days}'}'),
              if (c.decisionDueDate != null)
                InfoRow(s.t('موعد القرار', 'Decision due'), '${f.date(c.decisionDueDate)}${c.isDecisionOverdue ? ' · ${s.t('متأخر', 'overdue')}' : ''}'),
              if (c.employeeNotifiedAt != null) InfoRow(s.t('أُبلغ الموظف', 'Employee notified'), f.dateTime(c.employeeNotifiedAt)),
            ]),
          ),
          const SizedBox(height: 12),
          // The paper contract and its documents: download, and soft delete of a mistaken file (decision 2026-11-10).
          HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(s.t('مرفقات العقد', 'Contract attachments')),
              if (c.attachments.isEmpty)
                Text(s.t('لا توجد مرفقات.', 'No attachments.'), style: Theme.of(context).textTheme.bodySmall)
              else
                for (final a in c.attachments)
                  ListTile(
                    contentPadding: EdgeInsets.zero,
                    leading: const Icon(Icons.attach_file),
                    title: Text(a.fileName),
                    subtitle: Text(f.dateTime(a.creationTime)),
                    trailing: Wrap(spacing: 4, children: [
                      IconButton(
                        tooltip: s.t('تنزيل', 'Download'),
                        icon: const Icon(Icons.download),
                        onPressed: () => runAction(context, () async {
                          await openDownload(await ref.read(hrApiProvider).contracts.Contract_GetAttachmentDownloadUrl(id: c.id, attachmentId: a.id));
                        }),
                      ),
                      if (session.can(HrOps.Contract_DeleteAttachment))
                        IconButton(
                          tooltip: s.t('حذف', 'Delete'),
                          icon: const Icon(Icons.delete_outline),
                          onPressed: () async {
                            final ok = await confirmAction(context,
                                title: s.t('حذف المرفق', 'Delete attachment'),
                                message: s.t('يختفي «${a.fileName}» من العقد، ويبقى مسجلاً في سجل التدقيق.', '"${a.fileName}" will be removed from the contract and kept in the audit log.'),
                                destructive: true);
                            if (!ok || !context.mounted) return;
                            if (await runAction(context, () => ref.read(hrApiProvider).contracts.Contract_DeleteAttachment(id: c.id, attachmentId: a.id), done: s.done)) {
                              ref.invalidate(contractProvider(id));
                            }
                          },
                        ),
                    ]),
                  ),
            ]),
          ),
          if (c.recommendation != null) ...[
            const SizedBox(height: 12),
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(s.t('التوصية', 'Recommendation')),
                InfoRow(s.t('التوصية', 'Recommendation'), renewalLabel(s, c.recommendation!.recommendation)),
                InfoRow(s.t('موقف الموظف', 'Employee position'), c.recommendation!.employeePosition),
                if (c.recommendation!.note != null) InfoRow(s.note, c.recommendation!.note!),
                InfoRow(s.t('بواسطة', 'By'), '${c.recommendation!.recommendedBy.displayName} · ${f.dateTime(c.recommendation!.recommendedAt)}'),
              ]),
            ),
          ],
          if (c.decision != null) ...[
            const SizedBox(height: 12),
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(s.t('القرار', 'Decision')),
                InfoRow(s.t('القرار', 'Decision'), renewalLabel(s, c.decision!.decision)),
                if (c.decision!.note != null) InfoRow(s.note, c.decision!.note!),
                if (c.decision!.employeeMessage != null) InfoRow(s.t('رسالة الموظف', 'Employee message'), c.decision!.employeeMessage!),
                InfoRow(s.t('بواسطة', 'By'), '${c.decision!.decidedBy.displayName} · ${f.dateTime(c.decision!.decidedAt)}'),
              ]),
            ),
          ],
          const SizedBox(height: 12),
          Wrap(spacing: 8, runSpacing: 8, children: [
            if (c.status == ContractStatus.pendingRecommendation && session.can(HrOps.Contract_Recommend))
              FilledButton(onPressed: () => _recommend(context, ref, c), child: Text(s.t('رفع التوصية', 'Recommend'))),
            if (c.status == ContractStatus.pendingDecision && session.can(HrOps.Contract_Decide))
              FilledButton(onPressed: () => _decide(context, ref, c), child: Text(s.t('إصدار القرار', 'Decide'))),
            if (c.status == ContractStatus.renewed && session.can(HrOps.Contract_Create))
              OutlinedButton.icon(
                onPressed: () async {
                  if (await showContractDialog(context, ref, previous: c) && context.mounted) context.go('/contracts');
                },
                icon: const Icon(Icons.autorenew),
                label: Text(s.t('إنشاء عقد التجديد', 'Create renewal contract')),
              ),
          ]),
        ]),
      ),
    );
  }
}
