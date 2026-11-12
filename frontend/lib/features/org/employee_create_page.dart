import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/pickers.dart';
import 'credentials_dialog.dart';
import 'org_providers.dart';

/// S-EMP-03: Employee_Create creates the file, account, shift, first manager link and roles
/// in one transaction. The manager must follow x-global-rules.manager-link (server checks).
class EmployeeCreatePage extends ConsumerStatefulWidget {
  const EmployeeCreatePage({super.key});

  @override
  ConsumerState<EmployeeCreatePage> createState() => _EmployeeCreatePageState();
}

class _EmployeeCreatePageState extends ConsumerState<EmployeeCreatePage> {
  final _form = GlobalKey<FormState>();
  final _nameAr = TextEditingController();
  final _nameEn = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _address = TextEditingController();
  final _jobTitle = TextEditingController();
  final _userName = TextEditingController();
  final _number = TextEditingController();

  /// The salary agreed at hiring, optional. It goes to the accountant's salary file and is not shown again.
  final _salary = TextEditingController();
  DepartmentRefDto? _department;
  EmployeeRefDto? _manager;
  String? _shiftId;
  DateTime? _hireDate;
  DateTime? _birthDate;
  AppGender? _gender;
  final Set<String> _roles = <String>{AppRole.employee.value};
  bool _busy = false;
  bool _submitted = false;

  @override
  void dispose() {
    for (final c in [_nameAr, _nameEn, _email, _phone, _address, _jobTitle, _userName, _number, _salary]) {
      c.dispose();
    }
    super.dispose();
  }

  bool get _isExecutive => _roles.contains(AppRole.executiveDirector.value);

  static final _notAssignableHere = <String>{
    AppRole.executiveDirector.value,
    AppRole.hrManager.value,
    AppRole.systemAdmin.value,
  };

  /// x-global-rules.manager-link: the executive and the system admin have no manager
  /// (an HR manager who is also system admin follows the HR manager row).
  bool get _noManager =>
      _isExecutive ||
      (_roles.contains(AppRole.systemAdmin.value) && !_roles.contains(AppRole.hrManager.value));

  /// The executive, the HR manager and the system admin have no shift (decisions of 2026-11-10), nor do the
  /// accountant and the store officer (decision 2026-11-11).
  bool get _noShift =>
      _isExecutive ||
      _roles.contains(AppRole.hrManager.value) ||
      _roles.contains(AppRole.systemAdmin.value) ||
      _roles.contains(AppRole.accountant.value) ||
      _roles.contains(AppRole.storeOfficer.value);

  String? _opt(TextEditingController c) => c.text.trim().isEmpty ? null : c.text.trim();

  Future<void> _submit() async {
    setState(() => _submitted = true);
    final valid = _form.currentState!.validate();
    final s = S.of(context);
    if (!valid || _department == null || _hireDate == null || _roles.isEmpty) return;
    if (!_noShift && _shiftId == null) return;
    if (!_noManager && _manager == null) return;
    setState(() => _busy = true);
    try {
      final created = await ref.read(hrApiProvider).employees.Employee_Create(
            body: EmployeeCreateDto(
              fullNameAr: _nameAr.text.trim(),
              fullNameEn: _opt(_nameEn),
              email: _email.text.trim(),
              phoneNumber: _opt(_phone),
              dateOfBirth: _birthDate,
              address: _opt(_address),
              departmentId: _department!.id,
              jobTitle: _jobTitle.text.trim(),
              hireDate: _hireDate!,
              workShiftId: _noShift ? null : _shiftId,
              managerEmployeeId: _noManager ? null : _manager?.id,
              userName: _userName.text.trim(),
              roleNames: _roles.toList(),
              gender: _gender,
              employeeNumber: _opt(_number),
              baseSalary: _opt(_salary) == null ? null : double.parse(_salary.text.trim()),
            ),
          );
      if (!mounted) return;
      await showCredentialsDialog(context, created.credentials);
      if (!mounted) return;
      showDone(context, s.t('تمت إضافة الموظف', 'Employee added'));
      context.go('/employees/${created.employee.id}');
    } catch (e) {
      if (mounted) showError(context, e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final roles = ref.watch(rolesProvider);
    final shifts = ref.watch(activeShiftsProvider);
    String? missing(Object? v) => _submitted && v == null ? s.requiredField : null;

    return HrPage(
      title: s.t('إضافة موظف', 'Add employee'),
      child: SingleChildScrollView(
        padding: const EdgeInsets.only(bottom: 24),
        child: Form(
          key: _form,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(s.t('البيانات الشخصية', 'Personal data')),
                FieldGrid(children: [
                  TextFormField(
                      controller: _nameAr,
                      maxLength: 200,
                      decoration: InputDecoration(labelText: s.t('الاسم الكامل بالعربية', 'Full name (Arabic)')),
                      validator: (v) => requiredValidator(context, v)),
                  TextFormField(
                      controller: _nameEn,
                      maxLength: 200,
                      decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Full name (English)'))),
                  TextFormField(
                      controller: _email,
                      keyboardType: TextInputType.emailAddress,
                      decoration: InputDecoration(labelText: s.email),
                      validator: (v) {
                        if (v == null || v.trim().isEmpty) return s.requiredField;
                        return RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$').hasMatch(v.trim()) ? null : s.t('بريد غير صالح', 'Invalid email');
                      }),
                  TextFormField(controller: _phone, maxLength: 32, decoration: InputDecoration(labelText: s.phone)),
                  TextFormField(controller: _number, maxLength: 32, decoration: InputDecoration(labelText: s.employeeNumber)),
                  DropdownButtonFormField<AppGender>(
                    initialValue: _gender,
                    decoration: InputDecoration(labelText: s.t('الجنس', 'Gender')),
                    items: [
                      DropdownMenuItem(value: AppGender.male, child: Text(s.t('ذكر', 'Male'))),
                      DropdownMenuItem(value: AppGender.female, child: Text(s.t('أنثى', 'Female'))),
                    ],
                    onChanged: (v) => setState(() => _gender = v),
                  ),
                  PickerField(
                    label: s.t('تاريخ الميلاد', 'Date of birth'),
                    value: _birthDate == null ? null : f.date(_birthDate),
                    icon: Icons.event,
                    onTap: () async {
                      final d = await pickDate(context, initial: _birthDate ?? DateTime(1990), last: DateTime.now());
                      if (d != null) setState(() => _birthDate = d);
                    },
                  ),
                  TextFormField(controller: _address, maxLength: 500, decoration: InputDecoration(labelText: s.t('العنوان', 'Address'))),
                ]),
              ]),
            ),
            const SizedBox(height: 12),
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(s.t('العمل', 'Employment')),
                FieldGrid(children: [
                  PickerField(
                    label: s.department,
                    value: _department?.name,
                    errorText: missing(_department),
                    onTap: () async {
                      final d = await showSearchPicker<DepartmentRefDto>(context,
                          title: s.department, search: (q) => searchDepartments(ref, q), label: (d) => d.name);
                      if (d != null) setState(() => _department = d);
                    },
                  ),
                  TextFormField(
                      controller: _jobTitle,
                      maxLength: 128,
                      decoration: InputDecoration(labelText: s.jobTitle),
                      validator: (v) => requiredValidator(context, v)),
                  TextFormField(
                    controller: _salary,
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'^\d*\.?\d{0,2}'))],
                    decoration: InputDecoration(
                      labelText: s.t('الراتب المتفق عليه (اختياري)', 'Agreed salary (optional)'),
                      suffixText: 'IQD',
                      helperText: s.t('يصل للمحاسب كأول راتب من تاريخ المباشرة، ولا يظهر هنا بعد الحفظ.',
                          'Goes to the accountant as the first salary from the hire date, and is not shown here after saving.'),
                      helperMaxLines: 2,
                    ),
                    validator: (v) {
                      final t = (v ?? '').trim();
                      if (t.isEmpty) return null;
                      final n = double.tryParse(t);
                      return n == null || n <= 0 ? s.t('اكتب مبلغاً أكبر من صفر', 'Enter an amount above zero') : null;
                    },
                  ),
                  PickerField(
                    label: s.hireDate,
                    value: _hireDate == null ? null : f.date(_hireDate),
                    icon: Icons.event,
                    errorText: missing(_hireDate),
                    onTap: () async {
                      final d = await pickDate(context, initial: _hireDate);
                      if (d != null) setState(() => _hireDate = d);
                    },
                  ),
                  if (!_noShift)
                    shifts.when(
                      loading: () => const LinearProgressIndicator(),
                      error: (e, _) => Text(s.error(e), style: const TextStyle(color: HrColors.danger)),
                      data: (list) => DropdownButtonFormField<String>(
                        initialValue: _shiftId,
                        isExpanded: true,
                        decoration: InputDecoration(labelText: s.shift, errorText: missing(_shiftId)),
                        items: [for (final w in list) DropdownMenuItem(value: w.id, child: Text(w.name))],
                        onChanged: (v) => setState(() => _shiftId = v),
                      ),
                    ),
                  if (!_noManager)
                    PickerField(
                      label: s.manager,
                      value: _manager?.fullName,
                      errorText: missing(_manager),
                      onTap: () async {
                        final m = await showSearchPicker<EmployeeRefDto>(context,
                            title: s.manager,
                            search: (q) => searchEmployees(ref, q),
                            label: (e) => e.fullName,
                            subtitle: (e) => e.departmentName ?? '');
                        if (m != null) setState(() => _manager = m);
                      },
                    ),
                ]),
                const SizedBox(height: 8),
                Text(
                  s.t('المدير حسب قواعد الربط: مدير القسم للموظف، والموارد البشرية لمدير القسم، والمدير التنفيذي للموارد البشرية وللمحاسب ولمسؤول المخزن، والمدير التنفيذي أو مدير الموارد البشرية للمراجع. المدير التنفيذي ومسؤول النظام بلا مدير ولا فترة عمل، ومدير الموارد البشرية والمحاسب ومسؤول المخزن بلا فترة عمل.',
                      'Manager per the link rules: department manager for employees, HR manager for department managers, executive director for HR, the accountant and the store officer, executive director or HR manager for the reviewer. The executive director and the system admin have no manager or work period; the HR manager, the accountant and the store officer have no work period.'),
                  style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary),
                ),
              ]),
            ),
            const SizedBox(height: 12),
            HrCard(
              child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                SectionTitle(s.t('الحساب والأدوار', 'Account and roles')),
                SizedBox(
                  width: 360,
                  child: TextFormField(
                    controller: _userName,
                    maxLength: 64,
                    textDirection: TextDirection.ltr,
                    decoration: InputDecoration(labelText: s.t('اسم المستخدم', 'User name')),
                    validator: (v) => (v == null || v.trim().length < 3) ? s.t('3 أحرف على الأقل', 'At least 3 characters') : null,
                  ),
                ),
                const SizedBox(height: 8),
                Text(s.roles, style: TextStyle(color: context.hr.textSecondary)),
                const SizedBox(height: 8),
                roles.when(
                  loading: () => const LinearProgressIndicator(),
                  error: (e, _) => Text(s.error(e), style: const TextStyle(color: HrColors.danger)),
                  data: (list) => Wrap(spacing: 8, runSpacing: 8, children: [
                    // The executive, HR manager and system admin roles are not given from this page (decision 2026-11-10).
                    for (final r in list.where((r) => !_notAssignableHere.contains(r.name)))
                      FilterChip(
                        label: Text(r.displayName),
                        selected: _roles.contains(r.name),
                        onSelected: (v) => setState(() => v ? _roles.add(r.name) : _roles.remove(r.name)),
                      ),
                  ]),
                ),
                if (_submitted && _roles.isEmpty)
                  Padding(
                    padding: const EdgeInsets.only(top: 6),
                    child: Text(s.t('اختر دوراً واحداً على الأقل (يضيف الخادم دور الموظف دائماً)', 'Pick at least one role (the server always adds employee)'),
                        style: const TextStyle(color: HrColors.danger)),
                  ),
              ]),
            ),
            const SizedBox(height: 16),
            Align(
              alignment: AlignmentDirectional.centerStart,
              child: FilledButton.icon(
                onPressed: _busy ? null : _submit,
                icon: const Icon(Icons.check),
                label: Text(s.t('إنشاء الموظف والحساب', 'Create employee and account')),
              ),
            ),
          ]),
        ),
      ),
    );
  }
}
