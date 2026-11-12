import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/download.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';
import '../org/org_providers.dart' show searchEmployees;
import 'finance_shared.dart';
import 'petty_cash_page.dart';

/// S-FIN: finance (accountant). Payroll approvers (HR manager / executive director per settings)
/// see the payroll runs only. Every read is audited by the server.
class FinancePage extends ConsumerStatefulWidget {
  const FinancePage({super.key});

  @override
  ConsumerState<FinancePage> createState() => _FinancePageState();
}

class _FinancePageState extends ConsumerState<FinancePage> {
  int _version = 0;
  String _filter = '';
  bool? _hasProfile;
  String? _adjPeriod = periodOf(DateTime.now());
  AdjustmentStatus? _adjStatus;
  String? _sugPeriod;
  AdjustmentSuggestionStatus? _sugStatus = AdjustmentSuggestionStatus.pendingReview;
  ExpenseSource? _expSource;
  AccountantRequestStatus? _reqStatus = AccountantRequestStatus.pendingAccountant;

  void _bump() => setState(() => _version++);

  Future<EmployeeRefDto?> _pickEmployee() =>
      showSearchPicker<EmployeeRefDto>(context, title: S.of(context).employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');

  Future<void> _newAdjustment() async {
    final s = S.of(context);
    EmployeeRefDto? employee;
    var type = AdjustmentType.deduction;
    final period = TextEditingController(text: periodOf(DateTime.now()));
    final amount = TextEditingController();
    final reason = TextEditingController();
    final requestId = TextEditingController();
    PlatformFile? file;
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('إضافة أو استقطاع', 'Addition or deduction'),
      fields: (setState) => [
        PickerField(
          label: s.employee,
          value: employee?.fullName,
          onTap: () async {
            final x = await _pickEmployee();
            if (x != null) setState(() => employee = x);
          },
        ),
        SegmentedButton<AdjustmentType>(
          segments: [
            ButtonSegment(value: AdjustmentType.deduction, label: Text(adjustmentTypeLabel(s, AdjustmentType.deduction))),
            ButtonSegment(value: AdjustmentType.addition, label: Text(adjustmentTypeLabel(s, AdjustmentType.addition))),
          ],
          selected: {type},
          onSelectionChanged: (v) => setState(() => type = v.first),
        ),
        TextFormField(
          controller: period,
          decoration: InputDecoration(labelText: s.t('الفترة (yyyy-MM)', 'Period (yyyy-MM)')),
          validator: (v) => periodPattern.hasMatch(v ?? '') ? null : 'yyyy-MM',
        ),
        numberField(context, amount, s.t('المبلغ', 'Amount'), positive: true, helper: s.t('العملة من البيانات المالية السارية.', 'Currency comes from the active salary profile.')),
        TextFormField(controller: reason, maxLines: 2, maxLength: 2000, decoration: InputDecoration(labelText: s.reason), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: requestId, decoration: InputDecoration(labelText: s.t('معرّف الطلب المرتبط (اختياري)', 'Linked request id (optional)'))),
        OutlinedButton.icon(
          onPressed: () async {
            final picked = await pickFiles();
            if (picked.isNotEmpty) setState(() => file = picked.first);
          },
          icon: const Icon(Icons.attach_file),
          label: Text(file?.name ?? s.t('المرفق (إلزامي)', 'Attachment (required)')),
        ),
      ],
      onSubmit: () async {
        if (employee == null || file == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).finance.Adjustment_Create(
                idempotencyKey: key,
                body: AdjustmentCreateInputDto(
                  employeeId: employee!.id,
                  period: period.text.trim(),
                  type: type,
                  amount: parseNum(amount)!,
                  reason: reason.text.trim(),
                  attachment: multipartOf(file!),
                  sourceRequestId: optText(requestId),
                ),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) _bump();
  }

  Future<void> _reverseAdjustment(AdjustmentDto a) async {
    final s = S.of(context);
    final reason = TextEditingController();
    final period = TextEditingController(text: periodOf(DateTime.now()));
    final ok = await showFormDialog(
      context,
      title: s.t('عكس الحركة', 'Reverse adjustment'),
      fields: (setState) => [
        Text('${a.employee.fullName} · ${adjustmentTypeLabel(s, a.type)} ${money(a.amount, a.currency)} · ${a.period}'),
        TextFormField(
          controller: period,
          decoration: InputDecoration(labelText: s.t('فترة العكس (yyyy-MM)', 'Reversal period (yyyy-MM)')),
          validator: (v) => periodPattern.hasMatch(v ?? '') ? null : 'yyyy-MM',
        ),
        TextFormField(controller: reason, maxLines: 2, maxLength: 2000, decoration: InputDecoration(labelText: s.reason), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () => runAction(context,
          () => ref.read(hrApiProvider).finance.Adjustment_Reverse(id: a.id, body: AdjustmentReverseInputDto(reason: reason.text.trim(), period: period.text.trim())),
          done: s.done),
    );
    if (ok) _bump();
  }

  Future<void> _approveSuggestion(AdjustmentSuggestionDto x) async {
    final s = S.of(context);
    final amount = TextEditingController(text: x.suggestedAmount == null ? '' : qty(x.suggestedAmount));
    final reason = TextEditingController();
    final ok = await showFormDialog(
      context,
      title: s.t('اعتماد الاقتراح', 'Approve suggestion'),
      fields: (setState) => [
        Text('${x.employee.fullName} · ${suggestionBasisLabel(s, x.basis)} ${qty(x.quantity)} ${quantityUnitLabel(s, x.quantityUnit)}'),
        numberField(context, amount, s.t('المبلغ', 'Amount'), required: x.suggestedAmount == null, positive: true),
        TextFormField(controller: reason, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () => runAction(context,
          () => ref.read(hrApiProvider).finance.AdjustmentSuggestion_Approve(id: x.id, body: AdjustmentSuggestionApproveInputDto(amount: parseNum(amount), reason: optText(reason))),
          done: s.done),
    );
    if (ok) _bump();
  }

  Future<void> _rejectSuggestion(AdjustmentSuggestionDto x) async {
    final s = S.of(context);
    final reason = await askText(context, title: s.t('رفض الاقتراح', 'Reject suggestion'), label: s.reason);
    if (reason == null || !mounted) return;
    if (await runAction(context, () => ref.read(hrApiProvider).finance.AdjustmentSuggestion_Reject(id: x.id, body: ReasonInputDto(reason: reason)), done: s.done)) _bump();
  }

  Future<void> _newExpense() async {
    final s = S.of(context);
    final f = Fmt(context);
    final category = TextEditingController();
    final amount = TextEditingController();
    final currency = TextEditingController(text: 'IQD');
    final description = TextEditingController();
    final requestId = TextEditingController();
    var date = DateTime.now();
    PlatformFile? file;
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('مصروف عام', 'General expense'),
      fields: (setState) => [
        TextFormField(controller: category, maxLength: 100, decoration: InputDecoration(labelText: s.t('التصنيف', 'Category')), validator: (v) => requiredValidator(context, v)),
        Row(children: [
          Expanded(child: numberField(context, amount, s.t('المبلغ', 'Amount'), positive: true)),
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
        PickerField(label: s.date, value: f.date(date), icon: Icons.event, onTap: () async {
          final x = await pickDate(context, initial: date);
          if (x != null) setState(() => date = x);
        }),
        TextFormField(controller: description, maxLines: 2, maxLength: 2000, decoration: InputDecoration(labelText: s.description)),
        TextFormField(controller: requestId, decoration: InputDecoration(labelText: s.t('معرّف الطلب المرتبط (اختياري)', 'Linked request id (optional)'))),
        OutlinedButton.icon(
          onPressed: () async {
            final picked = await pickFiles();
            if (picked.isNotEmpty) setState(() => file = picked.first);
          },
          icon: const Icon(Icons.attach_file),
          label: Text(file?.name ?? s.t('المرفق (إلزامي)', 'Attachment (required)')),
        ),
      ],
      onSubmit: () async {
        if (file == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).finance.Expense_Create(
                idempotencyKey: key,
                body: ExpenseCreateInputDto(
                  category: category.text.trim(),
                  amount: parseNum(amount)!,
                  currency: currency.text.trim(),
                  date: date,
                  description: optText(description),
                  attachment: multipartOf(file!),
                  sourceRequestId: optText(requestId),
                ),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) _bump();
  }

  Future<void> _newRun() async {
    final s = S.of(context);
    final period = TextEditingController(text: periodOf(DateTime(DateTime.now().year, DateTime.now().month - 1)));
    final key = const Uuid().v4();
    PayrollRunDto? created;
    final ok = await showFormDialog(
      context,
      title: s.t('إعداد كشف رواتب', 'Prepare payroll'),
      submitLabel: s.t('إعداد', 'Prepare'),
      fields: (setState) => [
        TextFormField(
          controller: period,
          decoration: InputDecoration(labelText: s.t('الفترة (yyyy-MM)', 'Period (yyyy-MM)')),
          validator: (v) => periodPattern.hasMatch(v ?? '') ? null : 'yyyy-MM',
        ),
        Text(s.t('يُحسب من البيانات المالية السارية والحركات المفتوحة. الاقتراحات غير المراجعة لا تدخل.',
            'Calculated from active salary profiles and open adjustments. Unreviewed suggestions are not included.'), style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () => runAction(context, () async {
        created = await ref.read(hrApiProvider).finance.PayrollRun_Create(idempotencyKey: key, body: PayrollRunCreateInputDto(period: period.text.trim()));
      }),
    );
    if (ok && mounted && created != null) context.go('/finance/payroll/${created!.id}');
  }

  Future<void> _expenseExport(FinanceExportFormat format) => runAction(context, () async {
        final file = await ref.read(hrApiProvider).finance.Expense_Export(format: format, source: _expSource);
        await saveBinary(file, fallbackName: 'expenses.${format == FinanceExportFormat.pdf ? 'pdf' : 'xlsx'}');
      });

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).finance;

    final tabs = <(String, Widget)>[
      if (session.can(HrOps.PayrollRun_List))
        (
          s.t('كشوف الرواتب والتسليم', 'Payroll & delivery'),
          PagedList<PayrollRunSummaryDto>(
            key: ValueKey('runs|$_version'),
            header: session.can(HrOps.PayrollRun_Create)
                ? Align(
                    alignment: AlignmentDirectional.centerEnd,
                    child: TextButton.icon(onPressed: _newRun, icon: const Icon(Icons.add), label: Text(s.t('إعداد كشف', 'Prepare payroll'))),
                  )
                : null,
            fetch: (skip, max) async {
              final r = await api.PayrollRun_List(skipCount: skip, maxResultCount: max, sorting: 'period desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, r) => OpsTile(
              leading: Icons.receipt_long_outlined,
              title: r.period,
              subtitle: [
                '${r.employeeCount} ${s.employee}',
                for (final t in r.totals) '${s.t('الصافي', 'Net')} ${money(t.net, t.currency)}',
                r.preparedBy.displayName,
                if (r.approvalRequestedAt != null && r.status == PayrollRunStatus.draft) s.t('بانتظار الاعتماد', 'Awaiting approval'),
              ].join(' · '),
              trailing: Row(mainAxisSize: MainAxisSize.min, children: [
                if (r.isStale) ...[StatusPill(s.t('يحتاج إعادة حساب', 'Stale'), tone: Tone.danger), const SizedBox(width: 6)],
                StatusPill(payrollStatusLabel(s, r.status), tone: payrollTone(r.status)),
              ]),
              onTap: () => context.go('/finance/payroll/${r.id}'),
            ),
          )
        ),
      if (session.can(HrOps.AccountantRequest_List))
        (
          s.t('طلبات المحاسب', 'Requests'),
          PagedList<AccountantRequestSummaryDto>(
            key: ValueKey('req|$_version|$_reqStatus'),
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Align(
                alignment: AlignmentDirectional.centerStart,
                child: enumFilter<AccountantRequestStatus>(context,
                    label: s.status,
                    value: _reqStatus,
                    values: AccountantRequestStatus.values,
                    text: (v) => switch (v) {
                          AccountantRequestStatus.pendingAccountant => s.t('بانتظاري', 'Pending'),
                          AccountantRequestStatus.completed => s.t('مكتمل', 'Completed'),
                          _ => s.t('مرفوض', 'Rejected'),
                        },
                    onChanged: (v) => setState(() => _reqStatus = v)),
              ),
            ),
            fetch: (skip, max) async {
              final r = await api.AccountantRequest_List(skipCount: skip, maxResultCount: max, status: _reqStatus);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, r) => OpsTile(
              leading: Icons.request_page_outlined,
              avatar: PersonAvatar.employee(r.employee),
              title: '${r.employee.fullName} · ${r.requestTypeName}',
              subtitle: '${s.t('وصل', 'Arrived')} ${f.dateTime(r.arrivedAt)}${r.decidedAt == null ? '' : ' · ${f.dateTime(r.decidedAt)}'}',
              trailing: StatusPill(
                  r.status == AccountantRequestStatus.pendingAccountant
                      ? s.t('بانتظاري', 'Pending')
                      : (r.status == AccountantRequestStatus.completed ? s.t('مكتمل', 'Completed') : s.t('مرفوض', 'Rejected')),
                  tone: r.status == AccountantRequestStatus.pendingAccountant ? Tone.warning : Tone.neutral),
              onTap: () => _accountantRequest(r.id),
            ),
          )
        ),
      if (session.can(HrOps.SalaryProfile_List))
        (
          s.t('رواتب الموظفين', 'Employee salaries'),
          PagedList<SalaryProfileListItemDto>(
            key: ValueKey('sal|$_version|$_filter|$_hasProfile'),
            pageSize: 50,
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                SizedBox(
                  width: 240,
                  child: TextField(
                    decoration: InputDecoration(labelText: s.t('بحث', 'Search'), prefixIcon: const Icon(Icons.search), isDense: true),
                    onSubmitted: (v) => setState(() => _filter = v.trim()),
                  ),
                ),
                FilterChip(
                    label: Text(s.t('بلا بيانات مالية', 'Without profile')),
                    selected: _hasProfile == false,
                    onSelected: (v) => setState(() => _hasProfile = v ? false : null)),
              ]),
            ),
            fetch: (skip, max) async {
              final r = await api.SalaryProfile_List(skipCount: skip, maxResultCount: max, hasProfile: _hasProfile, filter: _filter.isEmpty ? null : _filter);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, p) => OpsTile(
              leading: Icons.badge_outlined,
              avatar: PersonAvatar.employee(p.employee),
              title: p.employee.fullName,
              subtitle: [if (p.employee.departmentName != null) p.employee.departmentName!, if (p.effectiveFrom != null) '${s.t('ساري من', 'From')} ${f.date(p.effectiveFrom)}'].join(' · '),
              trailing: p.hasProfile ? Text(money(p.baseSalary, p.currency)) : StatusPill(s.t('غير محددة', 'Not set'), tone: Tone.warning),
              onTap: () => context.go('/finance/salary/${p.employee.id}'),
            ),
          )
        ),
      if (session.can(HrOps.AdjustmentSuggestion_List))
        (
          s.t('اقتراحات الحضور', 'Attendance suggestions'),
          PagedList<AdjustmentSuggestionDto>(
            key: ValueKey('sug|$_version|$_sugPeriod|$_sugStatus'),
            pageSize: 50,
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                PeriodBar(period: _sugPeriod, allowAll: true, onChanged: (p) => setState(() => _sugPeriod = p)),
                enumFilter<AdjustmentSuggestionStatus>(context,
                    label: s.status,
                    value: _sugStatus,
                    values: AdjustmentSuggestionStatus.values,
                    text: (v) => switch (v) {
                          AdjustmentSuggestionStatus.pendingReview => s.t('بانتظار المراجعة', 'Pending review'),
                          AdjustmentSuggestionStatus.approved => s.t('معتمد', 'Approved'),
                          _ => s.t('مرفوض', 'Rejected'),
                        },
                    onChanged: (v) => setState(() => _sugStatus = v)),
              ]),
            ),
            fetch: (skip, max) async {
              final r = await api.AdjustmentSuggestion_List(skipCount: skip, maxResultCount: max, period: _sugPeriod, status: _sugStatus);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, x) => OpsTile(
              leading: Icons.lightbulb_outline,
              avatar: PersonAvatar.employee(x.employee),
              title: '${x.employee.fullName} · ${suggestionBasisLabel(s, x.basis)}',
              subtitle: [
                x.period,
                '${qty(x.quantity)} ${quantityUnitLabel(s, x.quantityUnit)}',
                adjustmentTypeLabel(s, x.type),
                if (x.suggestedAmount != null) money(x.suggestedAmount, x.currency),
                if (x.settlementForPeriod != null) '${s.t('تسوية', 'Settles')} ${x.settlementForPeriod}',
                if (x.reviewNote != null) x.reviewNote!,
              ].join(' · '),
              actions: [
                if (x.status == AdjustmentSuggestionStatus.pendingReview && session.can(HrOps.AdjustmentSuggestion_Approve)) ...[
                  TextButton(onPressed: () => _rejectSuggestion(x), child: Text(s.reject)),
                  FilledButton.tonal(onPressed: () => _approveSuggestion(x), child: Text(s.approve)),
                ],
              ],
            ),
          )
        ),
      if (session.can(HrOps.Adjustment_List))
        (
          s.t('الإضافات والاستقطاعات', 'Adjustments'),
          PagedList<AdjustmentDto>(
            key: ValueKey('adj|$_version|$_adjPeriod|$_adjStatus'),
            pageSize: 50,
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                PeriodBar(period: _adjPeriod, allowAll: true, onChanged: (p) => setState(() => _adjPeriod = p)),
                enumFilter<AdjustmentStatus>(context,
                    label: s.status, value: _adjStatus, values: AdjustmentStatus.values, text: (v) => adjustmentStatusLabel(s, v), onChanged: (v) => setState(() => _adjStatus = v)),
                if (session.can(HrOps.Adjustment_Create))
                  FilledButton.tonalIcon(onPressed: _newAdjustment, icon: const Icon(Icons.add), label: Text(s.t('حركة جديدة', 'New adjustment'))),
              ]),
            ),
            fetch: (skip, max) async {
              final r = await api.Adjustment_List(skipCount: skip, maxResultCount: max, period: _adjPeriod, status: _adjStatus);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, a) => OpsTile(
              leading: a.type == AdjustmentType.addition ? Icons.add_circle_outline : Icons.remove_circle_outline,
              avatar: PersonAvatar.employee(a.employee),
              title: '${a.employee.fullName} · ${adjustmentTypeLabel(s, a.type)} ${money(a.amount, a.currency)}',
              subtitle: [a.period, adjustmentSourceLabel(s, a.source), a.reason, a.createdBy.displayName].join(' · '),
              trailing: StatusPill(adjustmentStatusLabel(s, a.status), tone: a.status == AdjustmentStatus.reversed ? Tone.neutral : Tone.info),
              actions: [
                if (a.attachment != null)
                  TextButton.icon(
                    onPressed: () => runAction(context, () async => openDownload(await api.Adjustment_GetAttachmentDownloadUrl(id: a.id))),
                    icon: const Icon(Icons.download),
                    label: Text(a.attachment!.fileName),
                  ),
                if (a.status != AdjustmentStatus.reversed && a.source != AdjustmentSource.reversal && session.can(HrOps.Adjustment_Reverse))
                  TextButton(onPressed: () => _reverseAdjustment(a), child: Text(s.t('عكس', 'Reverse'))),
              ],
            ),
          )
        ),
      if (session.can(HrOps.Expense_List))
        (
          s.t('المصروفات', 'Expenses'),
          PagedList<ExpenseDto>(
            key: ValueKey('exp|$_version|$_expSource'),
            pageSize: 50,
            header: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                enumFilter<ExpenseSource>(context,
                    label: s.t('المصدر', 'Source'),
                    value: _expSource,
                    values: ExpenseSource.values,
                    text: (v) => v == ExpenseSource.general ? s.t('عام', 'General') : s.t('مشتريات المخزن', 'Store purchase'),
                    onChanged: (v) => setState(() => _expSource = v)),
                if (session.can(HrOps.Expense_Create))
                  FilledButton.tonalIcon(onPressed: _newExpense, icon: const Icon(Icons.add), label: Text(s.t('مصروف جديد', 'New expense'))),
                OutlinedButton(onPressed: () => _expenseExport(FinanceExportFormat.pdf), child: const Text('PDF')),
                OutlinedButton(onPressed: () => _expenseExport(FinanceExportFormat.excel), child: const Text('Excel')),
              ]),
            ),
            fetch: (skip, max) async {
              final r = await api.Expense_List(skipCount: skip, maxResultCount: max, source: _expSource, sorting: 'date desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, x) => OpsTile(
              leading: x.source == ExpenseSource.storePurchase ? Icons.store_outlined : Icons.receipt_outlined,
              title: '${x.category} · ${money(x.amount, x.currency)}',
              subtitle: [f.date(x.date), if (x.description != null) x.description!, if (x.createdBy != null) x.createdBy!.displayName].join(' · '),
              trailing: StatusPill(x.status == ExpenseStatus.reversed ? s.t('معكوس', 'Reversed') : s.t('فعّال', 'Active'),
                  tone: x.status == ExpenseStatus.reversed ? Tone.neutral : Tone.success),
              actions: [
                if (x.attachment != null)
                  TextButton.icon(
                    onPressed: () => runAction(context, () async => openDownload(await api.Expense_GetAttachmentDownloadUrl(id: x.id))),
                    icon: const Icon(Icons.download),
                    label: Text(x.attachment!.fileName),
                  ),
                if (x.status == ExpenseStatus.active && x.reversalOfExpenseId == null && session.can(HrOps.Expense_Reverse))
                  TextButton(
                    onPressed: () async {
                      final reason = await askText(context, title: s.t('عكس المصروف', 'Reverse expense'), label: s.reason);
                      if (reason == null || !context.mounted) return;
                      if (await runAction(context, () => api.Expense_Reverse(id: x.id, body: ReasonInputDto(reason: reason)), done: s.done)) _bump();
                    },
                    child: Text(s.t('عكس', 'Reverse')),
                  ),
              ],
            ),
          )
        ),
      if (session.can(HrOps.FinanceReport_GetMonthlyStatement)) (s.t('التقارير المالية', 'Finance reports'), const _FinanceReports()),
      if (session.can(HrOps.PettyCashEntry_GetMonth)) (s.t('النثريات', 'Petty cash'), const PettyCashTab()),
    ];
    // The accountant's order (decision 2026-11-11): salaries, payroll and delivery, adjustments, petty cash, then the rest.
    final order = [
      s.t('رواتب الموظفين', 'Employee salaries'),
      s.t('كشوف الرواتب والتسليم', 'Payroll & delivery'),
      s.t('الإضافات والاستقطاعات', 'Adjustments'),
      s.t('النثريات', 'Petty cash'),
      s.t('اقتراحات الحضور', 'Attendance suggestions'),
      s.t('المصروفات', 'Expenses'),
      s.t('طلبات المحاسب', 'Requests'),
      s.t('التقارير المالية', 'Finance reports'),
    ];
    int rank((String, Widget) tab) => order.contains(tab.$1) ? order.indexOf(tab.$1) : order.length;
    tabs.sort((a, b) => rank(a).compareTo(rank(b)));
    return HrPage(title: s.t('المالية', 'Finance'), child: tabbedBody(context, tabs));
  }

  Future<void> _accountantRequest(String id) async {
    final s = S.of(context);
    final f = Fmt(context);
    await showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(s.t('طلب المحاسب', 'Accountant request')),
        content: SizedBox(
          width: 560,
          child: FutureBuilder<AccountantRequestDto>(
            future: ref.read(hrApiProvider).finance.AccountantRequest_Get(id: id),
            builder: (context, snap) {
              if (snap.hasError) return ErrorState(error: snap.error!);
              if (!snap.hasData) return const LinearProgressIndicator();
              final r = snap.data!;
              return SingleChildScrollView(
                child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  InfoRow(s.employee, r.employee.fullName),
                  InfoRow(s.requestType, r.requestTypeName),
                  InfoRow(s.submittedBy, r.submittedBy.displayName),
                  if (r.description != null) InfoRow(s.description, r.description!),
                  InfoRow(s.t('الحركات المرتبطة', 'Linked adjustments'), '${r.linkedAdjustmentIds.length}'),
                  InfoRow(s.t('المصروفات المرتبطة', 'Linked expenses'), '${r.linkedExpenseIds.length}'),
                  const Divider(),
                  for (final t in r.timeline)
                    Padding(
                      padding: const EdgeInsets.only(bottom: 6),
                      child: Text('${f.dateTime(t.occurredAt)} · ${t.action}${t.actor == null ? '' : ' · ${t.actor!.displayName}'}${t.note == null ? '' : ' · ${t.note}'}',
                          style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                    ),
                  SelectableText('${s.t('معرّف الطلب', 'Request id')}: ${r.id}', style: Theme.of(context).textTheme.labelSmall),
                ]),
              );
            },
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.close)),
          FilledButton(
            onPressed: () {
              Navigator.pop(ctx);
              context.go('/requests/$id');
            },
            child: Text(s.t('فتح الطلب للموافقة أو الرفض', 'Open request to approve or reject')),
          ),
        ],
      ),
    );
  }
}

/// Monthly statement, adjustments and store expenses reports with export.
class _FinanceReports extends ConsumerStatefulWidget {
  const _FinanceReports();

  @override
  ConsumerState<_FinanceReports> createState() => _FinanceReportsState();
}

class _FinanceReportsState extends ConsumerState<_FinanceReports> {
  FinanceReportCode _code = FinanceReportCode.monthlyStatement;
  String _period = periodOf(DateTime(DateTime.now().year, DateTime.now().month - 1));

  DateTime get _from => periodStart(_period);
  DateTime get _to => DateTime(_from.year, _from.month + 1, 0);

  Future<void> _export(FinanceExportFormat format) => runAction(context, () async {
        final api = ref.read(hrApiProvider).finance;
        final file = await api.FinanceReport_Export(
          code: _code,
          format: format,
          period: _code == FinanceReportCode.monthlyStatement ? _period : null,
          periodFrom: _code == FinanceReportCode.adjustments ? _period : null,
          periodTo: _code == FinanceReportCode.adjustments ? _period : null,
          dateFrom: _code == FinanceReportCode.storeExpenses ? _from : null,
          dateTo: _code == FinanceReportCode.storeExpenses ? _to : null,
        );
        await saveBinary(file, fallbackName: '${_code.value}-$_period.${format == FinanceExportFormat.pdf ? 'pdf' : 'xlsx'}');
      });

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).finance;
    Widget body;
    switch (_code) {
      case FinanceReportCode.monthlyStatement:
        body = FutureBuilder<FinanceMonthlyStatementReportDto>(
          key: ValueKey('m|$_period'),
          future: api.FinanceReport_GetMonthlyStatement(period: _period),
          builder: (context, snap) {
            if (snap.hasError) return ErrorState(error: snap.error!);
            if (!snap.hasData) return const Center(child: CircularProgressIndicator());
            final d = snap.data!;
            return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
              Row(children: [StatusPill(payrollStatusLabel(s, d.runStatus), tone: payrollTone(d.runStatus))]),
              const SizedBox(height: 12),
              totalsWrap(context, d.totals),
              const SizedBox(height: 12),
              for (final i in d.items)
                OpsTile(avatar: PersonAvatar.employee(i.employee), title: i.employee.fullName, subtitle: i.employee.departmentName, trailing: Text(money(i.net, i.currency), style: const TextStyle(fontWeight: FontWeight.w700))),
            ]);
          },
        );
      case FinanceReportCode.adjustments:
        body = FutureBuilder<FinanceAdjustmentReportPagedResultDto>(
          key: ValueKey('a|$_period'),
          future: api.FinanceReport_GetAdjustments(periodFrom: _period, periodTo: _period, maxResultCount: 200),
          builder: (context, snap) {
            if (snap.hasError) return ErrorState(error: snap.error!);
            if (!snap.hasData) return const Center(child: CircularProgressIndicator());
            final d = snap.data!;
            return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
              Wrap(spacing: 12, runSpacing: 12, children: [
                for (final t in d.totals) ...[
                  SizedBox(width: 220, child: KpiTile(label: '${s.t('الإضافات', 'Additions')} ${t.currency}', value: money(t.additions))),
                  SizedBox(width: 220, child: KpiTile(label: '${s.t('الاستقطاعات', 'Deductions')} ${t.currency}', value: money(t.deductions))),
                ],
              ]),
              const SizedBox(height: 12),
              for (final a in d.items)
                OpsTile(
                  avatar: PersonAvatar.employee(a.employee),
                  title: '${a.employee.fullName} · ${adjustmentTypeLabel(s, a.type)}',
                  subtitle: '${adjustmentSourceLabel(s, a.source)} · ${a.reason}',
                  trailing: Text(money(a.amount, a.currency)),
                ),
              if (d.totalCount > d.items.length) Text('${d.items.length} / ${d.totalCount}', style: Theme.of(context).textTheme.labelSmall),
            ]);
          },
        );
      default:
        body = FutureBuilder<FinanceStoreExpenseReportPagedResultDto>(
          key: ValueKey('e|$_period'),
          future: api.FinanceReport_GetStoreExpenses(dateFrom: _from, dateTo: _to, maxResultCount: 200),
          builder: (context, snap) {
            if (snap.hasError) return ErrorState(error: snap.error!);
            if (!snap.hasData) return const Center(child: CircularProgressIndicator());
            final d = snap.data!;
            return ListView(padding: const EdgeInsets.only(bottom: 24), children: [
              Wrap(spacing: 12, runSpacing: 12, children: [
                for (final t in d.totals) SizedBox(width: 220, child: KpiTile(label: t.currency, value: money(t.amount), icon: Icons.store_outlined)),
              ]),
              const SizedBox(height: 12),
              for (final x in d.items) OpsTile(title: x.category, subtitle: f.date(x.date), trailing: Text(money(x.amount, x.currency))),
            ]);
          },
        );
    }
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
        SegmentedButton<FinanceReportCode>(
          segments: [
            ButtonSegment(value: FinanceReportCode.monthlyStatement, label: Text(s.t('الكشف الشهري', 'Monthly statement'))),
            ButtonSegment(value: FinanceReportCode.adjustments, label: Text(s.t('الحركات', 'Adjustments'))),
            ButtonSegment(value: FinanceReportCode.storeExpenses, label: Text(s.t('مشتريات المخزن', 'Store purchases'))),
          ],
          selected: {_code},
          onSelectionChanged: (v) => setState(() => _code = v.first),
        ),
        PeriodBar(period: _period, onChanged: (p) => setState(() => _period = p ?? _period)),
        OutlinedButton(onPressed: () => _export(FinanceExportFormat.pdf), child: const Text('PDF')),
        OutlinedButton(onPressed: () => _export(FinanceExportFormat.excel), child: const Text('Excel')),
      ]),
      const SizedBox(height: 12),
      Expanded(child: body),
    ]);
  }
}
