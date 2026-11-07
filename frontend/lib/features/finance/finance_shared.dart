import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/widgets/common.dart';

final _money = NumberFormat('#,##0.##', 'en');

String money(num? v, [String? currency]) => v == null ? '—' : '${_money.format(v)}${currency == null ? '' : ' $currency'}';

/// Payroll period as the contract writes it: yyyy-MM.
String periodOf(DateTime d) => '${d.year}-${d.month.toString().padLeft(2, '0')}';

DateTime periodStart(String p) => DateTime(int.parse(p.substring(0, 4)), int.parse(p.substring(5, 7)));

String shiftPeriod(String p, int months) {
  final d = periodStart(p);
  return periodOf(DateTime(d.year, d.month + months));
}

final periodPattern = RegExp(r'^[0-9]{4}-(0[1-9]|1[0-2])$');

/// Month switcher for a yyyy-MM period.
class PeriodBar extends StatelessWidget {
  const PeriodBar({super.key, required this.period, required this.onChanged, this.allowAll = false});
  final String? period;
  final ValueChanged<String?> onChanged;
  final bool allowAll;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final p = period;
    return Row(mainAxisSize: MainAxisSize.min, children: [
      IconButton(onPressed: () => onChanged(shiftPeriod(p ?? periodOf(DateTime.now()), -1)), icon: const Icon(Icons.chevron_left)),
      Text(p ?? s.all, style: Theme.of(context).textTheme.titleMedium),
      IconButton(onPressed: () => onChanged(shiftPeriod(p ?? periodOf(DateTime.now()), 1)), icon: const Icon(Icons.chevron_right)),
      if (allowAll && p != null) IconButton(onPressed: () => onChanged(null), icon: const Icon(Icons.clear), tooltip: s.all),
    ]);
  }
}

String payrollStatusLabel(S s, PayrollRunStatus st) => switch (st) {
      PayrollRunStatus.draft => s.t('مسودة', 'Draft'),
      PayrollRunStatus.approved => s.t('معتمد', 'Approved'),
      PayrollRunStatus.closed => s.t('مغلق', 'Closed'),
      _ => '—',
    };

Tone payrollTone(PayrollRunStatus st) => switch (st) {
      PayrollRunStatus.draft => Tone.warning,
      PayrollRunStatus.approved => Tone.info,
      _ => Tone.success,
    };

String adjustmentTypeLabel(S s, AdjustmentType t) => t == AdjustmentType.addition ? s.t('إضافة', 'Addition') : s.t('استقطاع', 'Deduction');

String adjustmentSourceLabel(S s, AdjustmentSource x) => switch (x) {
      AdjustmentSource.manual => s.t('يدوي', 'Manual'),
      AdjustmentSource.attendanceSuggestion => s.t('من الحضور', 'From attendance'),
      AdjustmentSource.reversal => s.t('عكس', 'Reversal'),
      _ => '—',
    };

String adjustmentStatusLabel(S s, AdjustmentStatus x) => switch (x) {
      AdjustmentStatus.open => s.t('مفتوحة', 'Open'),
      AdjustmentStatus.includedInRun => s.t('في كشف', 'In payroll'),
      AdjustmentStatus.reversed => s.t('معكوسة', 'Reversed'),
      _ => '—',
    };

String suggestionBasisLabel(S s, AdjustmentSuggestionBasis b) => switch (b) {
      AdjustmentSuggestionBasis.lateness => s.t('تأخر', 'Lateness'),
      AdjustmentSuggestionBasis.absence => s.t('غياب', 'Absence'),
      AdjustmentSuggestionBasis.overtime => s.t('عمل إضافي', 'Overtime'),
      AdjustmentSuggestionBasis.attendanceCorrectionSettlement => s.t('تسوية تصحيح حضور', 'Correction settlement'),
      _ => '—',
    };

String quantityUnitLabel(S s, AdjustmentSuggestionQuantityUnit u) => switch (u) {
      AdjustmentSuggestionQuantityUnit.minutes => s.minutes,
      AdjustmentSuggestionQuantityUnit.hours => s.hours,
      AdjustmentSuggestionQuantityUnit.days => s.days,
      _ => '',
    };

String payrollLineLabel(S s, PayrollLineKind k) => switch (k) {
      PayrollLineKind.baseSalary => s.t('الراتب الأساسي', 'Base salary'),
      PayrollLineKind.allowance => s.t('مخصص', 'Allowance'),
      PayrollLineKind.fixedDeduction => s.t('استقطاع ثابت', 'Fixed deduction'),
      PayrollLineKind.addition => s.t('إضافة', 'Addition'),
      PayrollLineKind.deduction => s.t('استقطاع', 'Deduction'),
      _ => '—',
    };

/// Gross / additions / deductions / net per currency.
Widget totalsWrap(BuildContext context, List<PayrollTotalDto> totals) {
  final s = S.of(context);
  return Wrap(spacing: 12, runSpacing: 12, children: [
    for (final t in totals) ...[
      SizedBox(width: 200, child: KpiTile(label: '${s.t('الإجمالي', 'Gross')} ${t.currency}', value: money(t.gross), icon: Icons.payments_outlined)),
      SizedBox(width: 200, child: KpiTile(label: s.t('الإضافات', 'Additions'), value: money(t.additions), icon: Icons.add_circle_outline)),
      SizedBox(width: 200, child: KpiTile(label: s.t('الاستقطاعات', 'Deductions'), value: money(t.deductions), icon: Icons.remove_circle_outline)),
      SizedBox(width: 200, child: KpiTile(label: s.t('الصافي', 'Net'), value: money(t.net, t.currency), icon: Icons.account_balance_wallet_outlined)),
    ],
  ]);
}

/// Lines of one payroll item / payslip.
Widget payrollLines(BuildContext context, List<PayrollLineDto> lines, String currency) {
  final s = S.of(context);
  return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
    for (final l in lines)
      InfoRow('${payrollLineLabel(s, l.kind)}${l.label.isEmpty ? '' : ' · ${l.label}'}', money(l.amount, currency)),
  ]);
}
