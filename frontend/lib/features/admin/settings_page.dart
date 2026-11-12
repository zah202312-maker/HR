import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';

final settingsSectionsProvider = FutureProvider.autoDispose<SystemSettingsSectionListDto>(
  (ref) => ref.watch(hrApiProvider).systemSettings.SystemSettings_ListSections(),
);

String sectionLabel(S s, SystemSettingsSectionCode c) => switch (c) {
      SystemSettingsSectionCode.reminders => s.t('التذكيرات والتصعيد', 'Reminders and escalation'),
      SystemSettingsSectionCode.contracts => s.t('تنبيهات العقود', 'Contract alerts'),
      SystemSettingsSectionCode.complaints => s.t('مهل الشكاوى', 'Complaint deadlines'),
      SystemSettingsSectionCode.attendance => s.t('إعدادات الحضور', 'Attendance settings'),
      SystemSettingsSectionCode.hourlyLeave => s.t('الإجازة الزمنية', 'Hourly leave'),
      SystemSettingsSectionCode.finance => s.t('المالية', 'Finance'),
      SystemSettingsSectionCode.rolesAndPermissions => s.t('الأدوار والصلاحيات', 'Roles and permissions'),
      SystemSettingsSectionCode.requestTypes => s.t('أنواع الطلبات', 'Request types'),
      SystemSettingsSectionCode.otherLeaveTypes => s.t('أنواع الإجازات الأخرى', 'Other leave types'),
      SystemSettingsSectionCode.attendanceDevices => s.t('أجهزة البصمة', 'Attendance devices'),
      SystemSettingsSectionCode.employeeLeaveBalances => s.t('أرصدة الإجازات', 'Leave balances'),
      SystemSettingsSectionCode.workShifts => s.t('الدوامات', 'Work shifts'),
      SystemSettingsSectionCode.holidays => s.t('العطل', 'Holidays'),
      SystemSettingsSectionCode.departmentsAndManagerLinks => s.t('الأقسام وربط المدراء', 'Departments and managers'),
      SystemSettingsSectionCode.annualLeave => s.t('الإجازة السنوية', 'Annual leave'),
      SystemSettingsSectionCode.hrEvaluationAlert => s.t('تنبيه معدل التقييم', 'Evaluation average alert'),
      _ => c.value,
    };

/// Sections managed on their own screens elsewhere in the app.
const _sectionRoutes = <SystemSettingsSectionCode, String>{
  SystemSettingsSectionCode.attendance: '/attendance-settings',
  SystemSettingsSectionCode.rolesAndPermissions: '/roles',
  SystemSettingsSectionCode.requestTypes: '/request-types',
  SystemSettingsSectionCode.otherLeaveTypes: '/leave-types',
  SystemSettingsSectionCode.attendanceDevices: '/devices',
  SystemSettingsSectionCode.employeeLeaveBalances: '/leave-balances',
  SystemSettingsSectionCode.workShifts: '/shifts',
  SystemSettingsSectionCode.holidays: '/holidays',
  SystemSettingsSectionCode.departmentsAndManagerLinks: '/departments',
};

/// S-SET-01: system settings. Lists the sections with their owner role; the sections owned by
/// this screen (reminders, complaints, hourly leave, and release 3 contracts/finance) are edited inline.
class SystemSettingsPage extends ConsumerWidget {
  const SystemSettingsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final sections = ref.watch(settingsSectionsProvider);
    return HrPage(
      title: s.t('إعدادات النظام', 'System settings'),
      child: ListView(padding: const EdgeInsets.only(bottom: 24), children: [
        if (session.can(HrOps.SystemSettings_GetReminders)) ...[const _RemindersCard(), const SizedBox(height: 12)],
        if (session.can(HrOps.SystemSettings_GetComplaints)) ...[const _ComplaintsCard(), const SizedBox(height: 12)],
        if (session.can(HrOps.SystemSettings_GetAnnualLeave)) ...[const _AnnualLeaveCard(), const SizedBox(height: 12)],
        if (session.can(HrOps.SystemSettings_GetHrEvaluationAlert)) ...[const _HrEvaluationAlertCard(), const SizedBox(height: 12)],
        if (session.can(HrOps.SystemSettings_GetHourlyLeave)) ...[const _HourlyLeaveCard(), const SizedBox(height: 12)],
        if (session.can(HrOps.SystemSettings_GetContracts)) ...[const _ContractsCard(), const SizedBox(height: 12)],
        if (session.can(HrOps.SystemSettings_GetFinance)) ...[const _FinanceCard(), const SizedBox(height: 12)],
        if (session.can(HrOps.SystemSettings_ListSections)) ...[
          SectionTitle(s.t('كل الأقسام', 'All sections')),
          AsyncBody<SystemSettingsSectionListDto>(
            value: sections,
            onRetry: () => ref.invalidate(settingsSectionsProvider),
            builder: (d) => Column(children: [
              for (final x in d.items)
                Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: HrCard(
                    onTap: _sectionRoutes[x.code] == null ? null : () => context.go(_sectionRoutes[x.code]!),
                    child: Row(children: [
                      Expanded(child: Text(sectionLabel(s, x.code), style: const TextStyle(fontWeight: FontWeight.w600))),
                      Text(s.role(x.ownerRole), style: TextStyle(color: context.hr.textSecondary)),
                      const SizedBox(width: 12),
                      StatusPill(x.isEditableByCaller ? s.t('قابل للتعديل', 'Editable') : s.t('قراءة', 'Read only'),
                          tone: x.isEditableByCaller ? Tone.success : Tone.neutral),
                      if (_sectionRoutes[x.code] != null) const Icon(Icons.chevron_left),
                    ]),
                  ),
                ),
            ]),
          ),
        ],
      ]),
    );
  }
}

/// Shared shell for an inline settings section: loads, shows a form, saves with concurrencyStamp.
class _SettingsCard<T> extends ConsumerStatefulWidget {
  const _SettingsCard({
    required this.title,
    required this.load,
    required this.editOp,
    required this.fields,
    required this.save,
    required this.bind,
    this.lastModified,
  });
  final String title;
  final Future<T> Function(HrApi api) load;
  final String editOp;
  final void Function(T value) bind;
  final List<Widget> Function(BuildContext context, bool enabled) fields;
  final Future<T> Function(HrApi api, T current) save;
  final DateTime? Function(T value)? lastModified;

  @override
  ConsumerState<_SettingsCard<T>> createState() => _SettingsCardState<T>();
}

class _SettingsCardState<T> extends ConsumerState<_SettingsCard<T>> {
  final _form = GlobalKey<FormState>();
  late Future<T> _future = _fetch();
  T? _value;
  bool _busy = false;

  Future<T> _fetch() async {
    final v = await widget.load(ref.read(hrApiProvider));
    _value = v;
    widget.bind(v);
    return v;
  }

  Future<void> _save() async {
    if (!_form.currentState!.validate() || _value == null) return;
    setState(() => _busy = true);
    final s = S.of(context);
    await runAction(context, () async {
      final v = await widget.save(ref.read(hrApiProvider), _value as T);
      _value = v;
      widget.bind(v);
    }, done: s.saved);
    if (mounted) setState(() => _busy = false);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final canEdit = ref.watch(sessionProvider).can(widget.editOp);
    return HrCard(
      child: FutureBuilder<T>(
        future: _future,
        builder: (context, snap) {
          if (snap.hasError) {
            return ErrorState(error: snap.error!, onRetry: () => setState(() => _future = _fetch()));
          }
          if (!snap.hasData) return const Center(child: Padding(padding: EdgeInsets.all(16), child: CircularProgressIndicator()));
          final modified = _value == null ? null : widget.lastModified?.call(_value as T);
          return Form(
            key: _form,
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              SectionTitle(widget.title,
                  trailing: modified == null ? null : Text(Fmt(context).dateTime(modified), style: TextStyle(color: context.hr.textSecondary, fontSize: 12))),
              FieldGrid(children: widget.fields(context, canEdit && !_busy)),
              if (canEdit) ...[
                const SizedBox(height: 12),
                Align(
                  alignment: AlignmentDirectional.centerEnd,
                  child: FilledButton(onPressed: _busy ? null : _save, child: Text(s.save)),
                ),
              ],
            ]),
          );
        },
      ),
    );
  }
}

Widget _intField(BuildContext context, TextEditingController c, String label, {required bool enabled, int min = 0, int max = 365, String? helper}) {
  final s = S.of(context);
  return TextFormField(
    controller: c,
    enabled: enabled,
    keyboardType: TextInputType.number,
    inputFormatters: [FilteringTextInputFormatter.digitsOnly],
    decoration: InputDecoration(labelText: label, helperText: helper),
    validator: (v) {
      final n = int.tryParse(v ?? '');
      if (n == null) return s.requiredField;
      if (n < min || n > max) return '$min – $max';
      return null;
    },
  );
}

Widget _decimalField(BuildContext context, TextEditingController c, String label, {required bool enabled, double min = 0, double max = 1000}) {
  final s = S.of(context);
  return TextFormField(
    controller: c,
    enabled: enabled,
    keyboardType: const TextInputType.numberWithOptions(decimal: true),
    inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9.]'))],
    decoration: InputDecoration(labelText: label),
    validator: (v) {
      final n = double.tryParse(v ?? '');
      if (n == null) return s.requiredField;
      if (n < min || n > max) return '$min – $max';
      return null;
    },
  );
}

String _txt(num? v) => v == null ? '' : (v % 1 == 0 ? v.toInt().toString() : v.toString());

class _RemindersCard extends StatefulWidget {
  const _RemindersCard();
  @override
  State<_RemindersCard> createState() => _RemindersCardState();
}

class _RemindersCardState extends State<_RemindersCard> {
  final _open = TextEditingController();
  final _submission = TextEditingController();
  final _interval = TextEditingController();
  final _escalation = TextEditingController();
  final _approval = TextEditingController();
  final _minWorked = TextEditingController();

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return _SettingsCard<SystemSettingsRemindersDto>(
      title: s.t('التذكيرات والتصعيد', 'Reminders and escalation'),
      editOp: HrOps.SystemSettings_UpdateReminders,
      load: (api) => api.systemSettings.SystemSettings_GetReminders(),
      lastModified: (v) => v.lastModificationTime,
      bind: (v) {
        _open.text = v.monthEndOpenTime;
        _submission.text = '${v.monthlySubmissionWorkingDays}';
        _interval.text = '${v.monthlyReminderIntervalWorkingDays}';
        _escalation.text = '${v.monthlyEscalationWorkingDay}';
        _approval.text = _txt(v.approvalReminderIntervalWorkingDays);
        _minWorked.text = _txt(v.evaluationMinWorkedDays);
      },
      fields: (context, enabled) => [
        TextFormField(
          controller: _open,
          enabled: enabled,
          decoration: InputDecoration(labelText: s.t('وقت فتح نهاية الشهر (HH:mm)', 'Month-end open time (HH:mm)')),
          validator: (v) => RegExp(r'^([01]\d|2[0-3]):[0-5]\d$').hasMatch(v ?? '') ? null : 'HH:mm',
        ),
        _intField(context, _submission, s.t('مهلة الرفع الشهري (أيام عمل)', 'Monthly submission (working days)'), enabled: enabled, min: 1, max: 31),
        _intField(context, _interval, s.t('فاصل التذكير الشهري (أيام عمل)', 'Monthly reminder interval (working days)'), enabled: enabled, min: 1, max: 31),
        _intField(context, _escalation, s.t('يوم التصعيد (يوم عمل)', 'Escalation working day'), enabled: enabled, min: 1, max: 31),
        _intField(context, _approval, s.t('فاصل تذكير الموافقات (أيام عمل)', 'Approval reminder interval (working days)'), enabled: enabled, min: 1, max: 31),
        _intField(context, _minWorked, s.t('أقل أيام عمل للتقييم', 'Minimum worked days for evaluation'), enabled: enabled, min: 0, max: 31),
      ],
      save: (api, cur) => api.systemSettings.SystemSettings_UpdateReminders(
        body: SystemSettingsRemindersUpdateInputDto(
          monthEndOpenTime: _open.text.trim(),
          monthlySubmissionWorkingDays: int.parse(_submission.text),
          monthlyReminderIntervalWorkingDays: int.parse(_interval.text),
          monthlyEscalationWorkingDay: int.parse(_escalation.text),
          approvalReminderIntervalWorkingDays: int.parse(_approval.text),
          evaluationMinWorkedDays: int.parse(_minWorked.text),
          concurrencyStamp: cur.concurrencyStamp,
        ),
      ),
    );
  }
}

class _ComplaintsCard extends StatefulWidget {
  const _ComplaintsCard();
  @override
  State<_ComplaintsCard> createState() => _ComplaintsCardState();
}

class _ComplaintsCardState extends State<_ComplaintsCard> {
  final _deadline = TextEditingController();
  final _interval = TextEditingController();

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return _SettingsCard<SystemSettingsComplaintsDto>(
      title: s.t('مهل الشكاوى', 'Complaint deadlines'),
      editOp: HrOps.SystemSettings_UpdateComplaints,
      load: (api) => api.systemSettings.SystemSettings_GetComplaints(),
      lastModified: (v) => v.lastModificationTime,
      bind: (v) {
        _deadline.text = _txt(v.delayDeadlineWorkingDays);
        _interval.text = _txt(v.overdueReminderIntervalWorkingDays);
      },
      fields: (context, enabled) => [
        _intField(context, _deadline, s.t('مهلة المعالجة (أيام عمل)', 'Handling deadline (working days)'), enabled: enabled, min: 1, max: 365),
        _intField(context, _interval, s.t('فاصل تذكير التأخر (أيام عمل)', 'Overdue reminder interval (working days)'), enabled: enabled, min: 1, max: 365),
      ],
      save: (api, cur) => api.systemSettings.SystemSettings_UpdateComplaints(
        body: SystemSettingsComplaintsUpdateInputDto(
          delayDeadlineWorkingDays: int.parse(_deadline.text),
          overdueReminderIntervalWorkingDays: int.parse(_interval.text),
          concurrencyStamp: cur.concurrencyStamp,
        ),
      ),
    );
  }
}

/// The company's annual leave days per employee year (decision 2026-11-10). Every year that starts afterwards takes
/// the new number; no running year changes.
class _AnnualLeaveCard extends StatefulWidget {
  const _AnnualLeaveCard();
  @override
  State<_AnnualLeaveCard> createState() => _AnnualLeaveCardState();
}

class _AnnualLeaveCardState extends State<_AnnualLeaveCard> {
  final _days = TextEditingController();

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return _SettingsCard<SystemSettingsAnnualLeaveDto>(
      title: s.t('الإجازة السنوية', 'Annual leave'),
      editOp: HrOps.SystemSettings_UpdateAnnualLeave,
      load: (api) => api.systemSettings.SystemSettings_GetAnnualLeave(),
      lastModified: (v) => v.lastModificationTime,
      bind: (v) => _days.text = _txt(v.defaultEntitlementDays),
      fields: (context, enabled) => [
        _decimalField(context, _days, s.t('أيام الإجازة السنوية لكل موظف', 'Annual leave days per employee'), enabled: enabled, min: 0, max: 365),
        Text(
          s.t('تُفتح سنة كل موظف جديد بهذا العدد من تاريخ مباشرته. عند تغييره يأخذ كل موظف العدد الجديد من بداية سنته القادمة، ولا تتغير السنة الحالية. لتصحيح رصيد شخص واحد استخدم «تعديل الرصيد».',
              "Each new employee's year opens with this number from the hire date. A change reaches everyone from their next year; the current year stays. Use \"Adjust\" for one person."),
          style: Theme.of(context).textTheme.labelSmall,
        ),
      ],
      save: (api, cur) => api.systemSettings.SystemSettings_UpdateAnnualLeave(
        body: SystemSettingsAnnualLeaveUpdateInputDto(defaultEntitlementDays: double.parse(_days.text), concurrencyStamp: cur.concurrencyStamp),
      ),
    );
  }
}

/// The low average alert of HR evaluations (decision 2026-11-11): each cycle, an employee with enough evaluations whose
/// average on a heading is the threshold or lower alerts the HR manager. Applies from the next cycle.
class _HrEvaluationAlertCard extends StatefulWidget {
  const _HrEvaluationAlertCard();
  @override
  State<_HrEvaluationAlertCard> createState() => _HrEvaluationAlertCardState();
}

class _HrEvaluationAlertCardState extends State<_HrEvaluationAlertCard> {
  final _threshold = TextEditingController();
  final _weeks = TextEditingController();
  final _min = TextEditingController();

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return _SettingsCard<SystemSettingsHrEvaluationAlertDto>(
      title: s.t('تنبيه معدل التقييم', 'Evaluation average alert'),
      editOp: HrOps.SystemSettings_UpdateHrEvaluationAlert,
      load: (api) => api.systemSettings.SystemSettings_GetHrEvaluationAlert(),
      lastModified: (v) => v.lastModificationTime,
      bind: (v) {
        _threshold.text = _txt(v.threshold);
        _weeks.text = _txt(v.windowWeeks);
        _min.text = _txt(v.minEvaluations);
      },
      fields: (context, enabled) => [
        _decimalField(context, _threshold, s.t('التنبيه إذا كان المعدل هذا الرقم أو أقل', 'Alert when the average is this or lower'), enabled: enabled, min: 1, max: 10),
        _intField(context, _weeks, s.t('طول الدورة بالأسابيع', 'Cycle length in weeks'), enabled: enabled, min: 1, max: 12),
        _intField(context, _min, s.t('أقل عدد تقييمات في الدورة', 'Minimum evaluations in the cycle'), enabled: enabled, min: 1, max: 20),
        Text(
          s.t('في نهاية كل دورة يُجمع كل عنوان لكل موظف ويُقسم على عدد تقييماته. إذا كان المعدل الحد أو أقل يصلك تنبيه. التغيير يبدأ من الدورة القادمة.',
              "At the end of each cycle, each heading's scores per employee are summed and divided by their count. An average at or below the level alerts you. A change applies from the next cycle."),
          style: Theme.of(context).textTheme.labelSmall,
        ),
      ],
      save: (api, cur) => api.systemSettings.SystemSettings_UpdateHrEvaluationAlert(
        body: SystemSettingsHrEvaluationAlertUpdateInputDto(
          threshold: double.parse(_threshold.text),
          windowWeeks: int.parse(_weeks.text),
          minEvaluations: int.parse(_min.text),
          concurrencyStamp: cur.concurrencyStamp,
        ),
      ),
    );
  }
}

class _HourlyLeaveCard extends StatefulWidget {
  const _HourlyLeaveCard();
  @override
  State<_HourlyLeaveCard> createState() => _HourlyLeaveCardState();
}

class _HourlyLeaveCardState extends State<_HourlyLeaveCard> {
  final _monthly = TextEditingController();
  final _perDay = TextEditingController();

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return _SettingsCard<SystemSettingsHourlyLeaveDto>(
      title: s.t('الإجازة الزمنية', 'Hourly leave'),
      editOp: HrOps.SystemSettings_UpdateHourlyLeave,
      load: (api) => api.systemSettings.SystemSettings_GetHourlyLeave(),
      lastModified: (v) => v.lastModificationTime,
      bind: (v) {
        _monthly.text = _txt(v.monthlyAllowedHours);
        _perDay.text = _txt(v.hoursPerDay);
      },
      fields: (context, enabled) => [
        _decimalField(context, _monthly, s.t('الساعات المسموحة شهرياً', 'Monthly allowed hours'), enabled: enabled, min: 0, max: 744),
        _decimalField(context, _perDay, s.t('ساعات اليوم الواحد', 'Hours per day'), enabled: enabled, min: 0.01, max: 24),
      ],
      save: (api, cur) => api.systemSettings.SystemSettings_UpdateHourlyLeave(
        body: SystemSettingsHourlyLeaveUpdateInputDto(
          monthlyAllowedHours: double.parse(_monthly.text),
          hoursPerDay: double.parse(_perDay.text),
          concurrencyStamp: cur.concurrencyStamp,
        ),
      ),
    );
  }
}

class _ContractsCard extends StatefulWidget {
  const _ContractsCard();
  @override
  State<_ContractsCard> createState() => _ContractsCardState();
}

class _ContractsCardState extends State<_ContractsCard> {
  final _days = TextEditingController();

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return _SettingsCard<SystemSettingsContractsDto>(
      title: s.t('تنبيهات العقود', 'Contract alerts'),
      editOp: HrOps.SystemSettings_UpdateContracts,
      load: (api) => api.systemSettings.SystemSettings_GetContracts(),
      lastModified: (v) => v.lastModificationTime,
      bind: (v) => _days.text = '${v.alertDays}',
      fields: (context, enabled) => [
        _intField(context, _days, s.t('التنبيه قبل انتهاء العقد (أيام)', 'Alert before contract end (days)'), enabled: enabled, min: 1, max: 365),
      ],
      save: (api, cur) => api.systemSettings.SystemSettings_UpdateContracts(
        body: SystemSettingsContractsUpdateInputDto(alertDays: int.parse(_days.text), concurrencyStamp: cur.concurrencyStamp),
      ),
    );
  }
}

class _FinanceCard extends StatefulWidget {
  const _FinanceCard();
  @override
  State<_FinanceCard> createState() => _FinanceCardState();
}

class _FinanceCardState extends State<_FinanceCard> {
  PayrollApproverRole? _role;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return _SettingsCard<SystemSettingsFinanceDto>(
      title: s.t('المالية', 'Finance'),
      editOp: HrOps.SystemSettings_UpdateFinance,
      load: (api) => api.systemSettings.SystemSettings_GetFinance(),
      lastModified: (v) => v.lastModificationTime,
      bind: (v) => _role = v.payrollApproverRole,
      fields: (context, enabled) => [
        StatefulBuilder(
          builder: (context, setLocal) => DropdownButtonFormField<PayrollApproverRole>(
            initialValue: _role,
            decoration: InputDecoration(labelText: s.t('معتمد الرواتب', 'Payroll approver')),
            items: [
              DropdownMenuItem(value: PayrollApproverRole.hrManager, child: Text(s.role(AppRole.hrManager))),
              DropdownMenuItem(value: PayrollApproverRole.executiveDirector, child: Text(s.role(AppRole.executiveDirector))),
            ],
            onChanged: enabled ? (v) => setLocal(() => _role = v) : null,
            validator: (v) => v == null ? s.requiredField : null,
          ),
        ),
      ],
      save: (api, cur) => api.systemSettings.SystemSettings_UpdateFinance(
        body: SystemSettingsFinanceUpdateInputDto(payrollApproverRole: _role!, concurrencyStamp: cur.concurrencyStamp),
      ),
    );
  }
}
