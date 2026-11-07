import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../api/runtime/api_client.dart' show ApiException;
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/download.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';
import 'finance_shared.dart';

final payrollRunProvider = FutureProvider.autoDispose.family<PayrollRunDto, String>((ref, id) => ref.watch(hrApiProvider).finance.PayrollRun_Get(id: id));

/// One payroll run: totals, items, and the draft → approved → closed flow.
class PayrollRunPage extends ConsumerWidget {
  const PayrollRunPage({super.key, required this.id});
  final String id;

  Future<void> _act(BuildContext context, WidgetRef ref, PayrollRunDto r, String action) async {
    final s = S.of(context);
    final api = ref.read(hrApiProvider).finance;
    final input = PayrollRunActionInputDto(concurrencyStamp: r.concurrencyStamp);
    bool ok;
    switch (action) {
      case 'recalc':
        ok = await runAction(context, () => api.PayrollRun_Recalculate(id: r.id, body: input), done: s.done);
      case 'request':
        ok = await runAction(context, () => api.PayrollRun_RequestApproval(id: r.id, body: input), done: s.done);
      case 'approve':
        if (!await confirmAction(context, title: s.t('اعتماد الكشف', 'Approve payroll'), message: s.t('تثبت حركات الفترة ولا تقبل حركات جديدة.', 'The period adjustments are locked.'))) return;
        if (!context.mounted) return;
        ok = await runAction(context, () => api.PayrollRun_Approve(id: r.id, body: input), done: s.done);
      case 'return':
        final reason = await askText(context, title: s.t('إعادة للمحاسب', 'Return to accountant'), label: s.reason);
        if (reason == null || !context.mounted) return;
        ok = await runAction(context, () => api.PayrollRun_ReturnForChanges(id: r.id, body: ReasonInputDto(reason: reason)), done: s.done);
      case 'close':
        if (!await confirmAction(context,
            title: s.t('إغلاق الكشف نهائياً', 'Close payroll'), message: s.t('تصبح القسائم متاحة للموظفين، وأي أثر لاحق يكون تسوية.', 'Payslips become available; later effects become settlements.'))) {
          return;
        }
        if (!context.mounted) return;
        ok = await runAction(context, () => api.PayrollRun_Close(id: r.id, body: input), done: s.done);
      default:
        return;
    }
    if (ok) ref.invalidate(payrollRunProvider(id));
  }

  Future<void> _export(BuildContext context, WidgetRef ref, PayrollRunDto r, FinanceExportFormat format) => runAction(context, () async {
        final file = await ref.read(hrApiProvider).finance.PayrollRun_Export(id: r.id, format: format);
        await saveBinary(file, fallbackName: 'payroll-${r.period}.${format == FinanceExportFormat.pdf ? 'pdf' : 'xlsx'}');
      });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.t('كشف الرواتب', 'Payroll run'),
      child: AsyncBody<PayrollRunDto>(
        value: ref.watch(payrollRunProvider(id)),
        onRetry: () => ref.invalidate(payrollRunProvider(id)),
        builder: (r) {
          final draft = r.status == PayrollRunStatus.draft;
          final requested = r.approvalRequestedAt != null;
          return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
            HrCard(
              child: Wrap(spacing: 16, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                Text(r.period, style: Theme.of(context).textTheme.titleLarge),
                StatusPill(payrollStatusLabel(s, r.status), tone: payrollTone(r.status)),
                if (r.isStale) StatusPill(s.t('تغيرت البيانات: أعد الحساب', 'Data changed: recalculate'), tone: Tone.danger),
                if (draft && requested) StatusPill(s.t('بانتظار الاعتماد', 'Awaiting approval'), tone: Tone.warning),
                Text('${s.t('أعده', 'Prepared by')} ${r.preparedBy.displayName}'),
                Text('${s.t('آخر حساب', 'Calculated')} ${f.dateTime(r.lastCalculatedAt)}'),
                if (r.approvedBy != null) Text('${s.t('اعتمده', 'Approved by')} ${r.approvedBy!.displayName} · ${f.dateTime(r.approvedAt)}'),
                if (r.closedAt != null) Text('${s.t('أُغلق', 'Closed')} ${f.dateTime(r.closedAt)}'),
              ]),
            ),
            const SizedBox(height: 12),
            Wrap(spacing: 8, runSpacing: 8, children: [
              if (draft && session.can(HrOps.PayrollRun_Recalculate))
                OutlinedButton.icon(onPressed: () => _act(context, ref, r, 'recalc'), icon: const Icon(Icons.refresh), label: Text(s.t('إعادة الحساب', 'Recalculate'))),
              if (draft && !requested && !r.isStale && session.can(HrOps.PayrollRun_RequestApproval))
                FilledButton.icon(onPressed: () => _act(context, ref, r, 'request'), icon: const Icon(Icons.send_outlined), label: Text(s.t('طلب الاعتماد', 'Request approval'))),
              if (draft && requested && session.can(HrOps.PayrollRun_Approve))
                FilledButton.icon(onPressed: () => _act(context, ref, r, 'approve'), icon: const Icon(Icons.verified_outlined), label: Text(s.t('اعتماد', 'Approve'))),
              if (draft && requested && session.can(HrOps.PayrollRun_ReturnForChanges))
                OutlinedButton.icon(onPressed: () => _act(context, ref, r, 'return'), icon: const Icon(Icons.undo), label: Text(s.t('إعادة للمحاسب', 'Return'))),
              if (r.status == PayrollRunStatus.approved && session.can(HrOps.PayrollRun_Close))
                FilledButton.icon(onPressed: () => _act(context, ref, r, 'close'), icon: const Icon(Icons.lock_outline), label: Text(s.t('إغلاق نهائي', 'Close'))),
              if (session.can(HrOps.PayrollRun_Export)) ...[
                OutlinedButton(onPressed: () => _export(context, ref, r, FinanceExportFormat.pdf), child: const Text('PDF')),
                OutlinedButton(onPressed: () => _export(context, ref, r, FinanceExportFormat.excel), child: const Text('Excel')),
              ],
            ]),
            const SizedBox(height: 12),
            totalsWrap(context, r.totals),
            if (r.employeesWithoutProfile.isNotEmpty || r.pendingSuggestionCount > 0) ...[
              const SizedBox(height: 12),
              HrCard(
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  if (r.pendingSuggestionCount > 0)
                    Text(s.t('${r.pendingSuggestionCount} اقتراح حضور لم يُراجع ولا يدخل الكشف.', '${r.pendingSuggestionCount} attendance suggestions not reviewed and not included.')),
                  if (r.employeesWithoutProfile.isNotEmpty) ...[
                    Text(s.t('موظفون بلا بيانات مالية (خارج الكشف):', 'Employees without a salary profile (excluded):')),
                    Wrap(spacing: 6, runSpacing: 6, children: [
                      for (final e in r.employeesWithoutProfile)
                        ActionChip(
                          label: Text(e.fullName),
                          onPressed: session.can(HrOps.SalaryProfile_Get) ? () => context.go('/finance/salary/${e.id}') : null,
                        ),
                    ]),
                  ],
                ]),
              ),
            ],
            const SizedBox(height: 12),
            for (final i in r.items)
              Padding(
                padding: const EdgeInsets.only(bottom: 8),
                child: HrCard(
                  child: ExpansionTile(
                    tilePadding: EdgeInsets.zero,
                    title: Text(i.employee.fullName),
                    subtitle: Text(i.employee.departmentName ?? ''),
                    trailing: Text(money(i.net, i.currency), style: const TextStyle(fontWeight: FontWeight.w700)),
                    children: [
                      payrollLines(context, i.lines, i.currency),
                      const Divider(),
                      InfoRow(s.t('الإجمالي', 'Gross'), money(i.gross, i.currency)),
                      InfoRow(s.t('الإضافات', 'Additions'), money(i.additions, i.currency)),
                      InfoRow(s.t('الاستقطاعات', 'Deductions'), money(i.deductions, i.currency)),
                    ],
                  ),
                ),
              ),
          ]);
        },
      ),
    );
  }
}

/// Salary profile of one employee: current version, history, and a new effective-dated version.
class SalaryProfilePage extends ConsumerStatefulWidget {
  const SalaryProfilePage({super.key, required this.employeeId});
  final String employeeId;

  @override
  ConsumerState<SalaryProfilePage> createState() => _SalaryProfilePageState();
}

class _SalaryProfilePageState extends ConsumerState<SalaryProfilePage> {
  int _version = 0;

  Future<SalaryProfileDto?> _current() async {
    try {
      return await ref.read(hrApiProvider).finance.SalaryProfile_Get(employeeId: widget.employeeId);
    } catch (e) {
      if (e is ApiException && e.statusCode == 404) return null; // no profile yet: PUT with concurrencyStamp = null
      rethrow;
    }
  }

  Future<void> _edit(SalaryProfileDto? cur) async {
    final s = S.of(context);
    final f = Fmt(context);
    final base = TextEditingController(text: cur == null ? '' : qty(cur.baseSalary));
    final currency = TextEditingController(text: cur?.currency ?? 'IQD');
    var effective = DateTime(DateTime.now().year, DateTime.now().month + 1);
    final comps = <(TextEditingController, SalaryComponentType, TextEditingController)>[
      for (final c in cur?.components ?? const <SalaryComponentDto>[]) (TextEditingController(text: c.name), c.type, TextEditingController(text: qty(c.amount))),
    ];
    final ok = await showFormDialog(
      context,
      width: 640,
      title: s.t('نسخة جديدة من البيانات المالية', 'New salary profile version'),
      fields: (setState) => [
        Row(children: [
          Expanded(child: numberField(context, base, s.t('الراتب الأساسي', 'Base salary'), min: 0)),
          const SizedBox(width: 12),
          SizedBox(
            width: 110,
            child: TextFormField(
              controller: currency,
              maxLength: 3,
              decoration: InputDecoration(labelText: s.t('العملة', 'Currency')),
              validator: (v) => RegExp(r'^[A-Z]{3}$').hasMatch(v ?? '') ? null : 'ISO',
            ),
          ),
        ]),
        PickerField(label: s.t('تاريخ السريان', 'Effective from'), value: f.date(effective), icon: Icons.event, onTap: () async {
          final x = await pickDate(context, initial: effective);
          if (x != null) setState(() => effective = x);
        }),
        SectionTitle(s.t('المخصصات والاستقطاعات الثابتة', 'Allowances and fixed deductions'),
            trailing: IconButton(
              onPressed: () => setState(() => comps.add((TextEditingController(), SalaryComponentType.allowance, TextEditingController()))),
              icon: const Icon(Icons.add),
            )),
        for (var i = 0; i < comps.length; i++)
          Row(children: [
            Expanded(
              flex: 3,
              child: TextFormField(controller: comps[i].$1, decoration: InputDecoration(labelText: s.t('الاسم', 'Name')), validator: (v) => requiredValidator(context, v)),
            ),
            const SizedBox(width: 8),
            Expanded(
              flex: 2,
              child: DropdownButtonFormField<SalaryComponentType>(
                initialValue: comps[i].$2,
                isExpanded: true,
                items: [
                  DropdownMenuItem(value: SalaryComponentType.allowance, child: Text(s.t('مخصص', 'Allowance'))),
                  DropdownMenuItem(value: SalaryComponentType.fixedDeduction, child: Text(s.t('استقطاع ثابت', 'Fixed deduction'))),
                ],
                onChanged: (v) => setState(() => comps[i] = (comps[i].$1, v ?? comps[i].$2, comps[i].$3)),
              ),
            ),
            const SizedBox(width: 8),
            Expanded(flex: 2, child: numberField(context, comps[i].$3, s.t('المبلغ', 'Amount'), min: 0)),
            IconButton(onPressed: () => setState(() => comps.removeAt(i)), icon: const Icon(Icons.delete_outline)),
          ]),
        Text(s.t('لا تُعدل النسخ السابقة، والكشوف المسودة المتأثرة تحتاج إعادة حساب.', 'Earlier versions are kept; affected draft payrolls need recalculation.'),
            style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).finance.SalaryProfile_Update(
              employeeId: widget.employeeId,
              body: SalaryProfileUpdateInputDto(
                baseSalary: parseNum(base)!,
                currency: currency.text.trim(),
                effectiveFrom: effective,
                components: [for (final c in comps) SalaryComponentInputDto(name: c.$1.text.trim(), type: c.$2, amount: parseNum(c.$3)!)],
                concurrencyStamp: cur?.concurrencyStamp,
              ),
            ),
        done: s.saved,
      ),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).finance;
    return HrPage(
      title: s.t('البيانات المالية', 'Salary profile'),
      child: FutureBuilder<SalaryProfileDto?>(
        key: ValueKey(_version),
        future: _current(),
        builder: (context, snap) {
          if (snap.hasError) return ErrorState(error: snap.error!, onRetry: () => setState(() => _version++));
          if (snap.connectionState != ConnectionState.done) return const Center(child: CircularProgressIndicator());
          final cur = snap.data;
          return PagedList<SalaryProfileDto>(
            header: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              HrCard(
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  SectionTitle(cur?.employee.fullName ?? s.t('لا توجد بيانات مالية بعد', 'No salary profile yet'),
                      trailing: session.can(HrOps.SalaryProfile_Update)
                          ? FilledButton.icon(onPressed: () => _edit(cur), icon: const Icon(Icons.edit_outlined), label: Text(s.t('نسخة جديدة', 'New version')))
                          : null),
                  if (cur != null) ...[
                    InfoRow(s.t('الراتب الأساسي', 'Base salary'), money(cur.baseSalary, cur.currency)),
                    InfoRow(s.t('ساري من', 'Effective from'), f.date(cur.effectiveFrom)),
                    for (final c in cur.components)
                      InfoRow('${c.type == SalaryComponentType.allowance ? s.t('مخصص', 'Allowance') : s.t('استقطاع ثابت', 'Fixed deduction')} · ${c.name}', money(c.amount, cur.currency)),
                  ],
                ]),
              ),
              const SizedBox(height: 12),
              SectionTitle(s.t('النسخ السابقة', 'History')),
            ]),
            fetch: (skip, max) async {
              if (cur == null) return (<SalaryProfileDto>[], 0);
              final r = await api.SalaryProfile_GetHistory(employeeId: widget.employeeId, skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, p) => OpsTile(
              leading: Icons.history,
              title: '${f.date(p.effectiveFrom)} · ${money(p.baseSalary, p.currency)}',
              subtitle: p.components.map((c) => '${c.name} ${money(c.amount)}').join('، '),
            ),
          );
        },
      ),
    );
  }
}

/// Employee payslips from closed payroll runs only.
class PayslipsPage extends ConsumerWidget {
  const PayslipsPage({super.key});

  Future<void> _open(BuildContext context, WidgetRef ref, PayslipSummaryDto p) async {
    final s = S.of(context);
    final api = ref.read(hrApiProvider).finance;
    await showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('${s.t('قسيمة', 'Payslip')} ${p.period}'),
        content: SizedBox(
          width: 480,
          child: FutureBuilder<PayslipDto>(
            future: api.Payslip_GetMy(id: p.id),
            builder: (context, snap) {
              if (snap.hasError) return ErrorState(error: snap.error!);
              if (!snap.hasData) return const LinearProgressIndicator();
              final d = snap.data!;
              return SingleChildScrollView(
                child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  payrollLines(context, d.lines, d.currency),
                  const Divider(),
                  InfoRow(s.t('الإجمالي', 'Gross'), money(d.gross, d.currency)),
                  InfoRow(s.t('الإضافات', 'Additions'), money(d.additions, d.currency)),
                  InfoRow(s.t('الاستقطاعات', 'Deductions'), money(d.deductions, d.currency)),
                  InfoRow(s.t('الصافي', 'Net'), money(d.net, d.currency)),
                ]),
              );
            },
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.close)),
          FilledButton.icon(
            onPressed: () => runAction(ctx, () async {
              final file = await api.Payslip_DownloadMyPdf(id: p.id);
              await saveBinary(file, fallbackName: 'payslip-${p.period}.pdf');
            }),
            icon: const Icon(Icons.picture_as_pdf_outlined),
            label: const Text('PDF'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).finance;
    return HrPage(
      title: s.t('قسائم الراتب', 'Payslips'),
      child: PagedList<PayslipSummaryDto>(
        emptyMessage: s.t('لا توجد قسائم بعد.', 'No payslips yet.'),
        fetch: (skip, max) async {
          final r = await api.Payslip_GetMyList(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, p) => OpsTile(
          leading: Icons.receipt_long_outlined,
          title: p.period,
          subtitle: '${s.t('أُغلق', 'Closed')} ${f.date(p.closedAt)}',
          trailing: Text(money(p.net, p.currency), style: const TextStyle(fontWeight: FontWeight.w700)),
          onTap: () => _open(context, ref, p),
        ),
      ),
    );
  }
}
