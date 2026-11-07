import 'package:dio/dio.dart' show MultipartFile;
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../hrx/employee_records_tab.dart';
import 'assign_employees_dialog.dart';
import 'credentials_dialog.dart';
import 'org_providers.dart';

final employeeFileProvider = FutureProvider.autoDispose.family<EmployeeHrDto, String>(
  (ref, id) => ref.watch(hrApiProvider).employees.Employee_Get(id: id),
);

/// S-EMP-02: the full employee file for HR (Employee_Get), with tabs for data,
/// manager link, shift, documents, administrative task log and account.
class EmployeeFilePage extends ConsumerWidget {
  const EmployeeFilePage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final value = ref.watch(employeeFileProvider(id));
    return HrPage(
      title: value.valueOrNull?.fullName ?? s.t('ملف الموظف', 'Employee file'),
      subtitle: value.valueOrNull == null ? null : '${value.valueOrNull!.jobTitle} · ${value.valueOrNull!.department.name}',
      child: AsyncBody<EmployeeHrDto>(
        value: value,
        onRetry: () => ref.invalidate(employeeFileProvider(id)),
        builder: (e) => DefaultTabController(
          length: 7,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [
              Tab(text: s.t('البيانات', 'Data')),
              Tab(text: s.manager),
              Tab(text: s.shift),
              Tab(text: s.t('الوثائق', 'Documents')),
              Tab(text: s.t('سجل المهام', 'Task log')),
              Tab(text: s.account),
              Tab(text: s.t('السجل الوظيفي', 'Records')),
            ]),
            const SizedBox(height: 12),
            Expanded(
              child: TabBarView(children: [
                _DataTab(e: e),
                _ManagerTab(e: e),
                _ShiftTab(e: e),
                EmployeeDocumentsTab(employeeId: e.id, canManage: true),
                _TaskLogTab(e: e),
                _AccountTab(e: e),
                EmployeeRecordsTab(employeeId: e.id),
              ]),
            ),
          ]),
        ),
      ),
    );
  }
}

class _DataTab extends ConsumerWidget {
  const _DataTab({required this.e});
  final EmployeeHrDto e;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    return SingleChildScrollView(
      padding: const EdgeInsets.only(bottom: 24),
      child: HrCard(
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Row(children: [
            Expanded(child: SectionTitle(s.t('بيانات الملف', 'File data'))),
            StatusPill(employeeStatusLabel(s, e.status), tone: e.status == EmployeeStatus.active ? Tone.success : Tone.warning),
          ]),
          InfoRow(s.t('الاسم بالعربية', 'Name (Arabic)'), e.fullNameAr),
          if (e.fullNameEn != null) InfoRow(s.t('الاسم بالإنجليزية', 'Name (English)'), e.fullNameEn!),
          if (e.employeeNumber != null) InfoRow(s.employeeNumber, e.employeeNumber!),
          InfoRow(s.department, e.department.name),
          InfoRow(s.jobTitle, e.jobTitle),
          InfoRow(s.hireDate, f.date(e.hireDate)),
          if (e.email != null) InfoRow(s.email, e.email!),
          if (e.phoneNumber != null) InfoRow(s.phone, e.phoneNumber!),
          if (e.dateOfBirth != null) InfoRow(s.t('تاريخ الميلاد', 'Date of birth'), f.date(e.dateOfBirth)),
          if (e.address != null) InfoRow(s.t('العنوان', 'Address'), e.address!),
          InfoRow(s.t('عدد المرتبطين به', 'Direct reports'), '${e.directReportCount}'),
          const SizedBox(height: 12),
          Wrap(spacing: 8, runSpacing: 8, children: [
            if (session.can(HrOps.Employee_Update))
              FilledButton.icon(onPressed: () => _edit(context, ref), icon: const Icon(Icons.edit), label: Text(s.t('تعديل', 'Edit'))),
            if (session.can(HrOps.Employee_CorrectHireDate))
              OutlinedButton.icon(
                  onPressed: () => _correctHireDate(context, ref),
                  icon: const Icon(Icons.event_repeat),
                  label: Text(s.t('تصحيح تاريخ المباشرة', 'Correct hire date'))),
          ]),
        ]),
      ),
    );
  }

  Future<void> _edit(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    final f = Fmt(context);
    final nameAr = TextEditingController(text: e.fullNameAr);
    final nameEn = TextEditingController(text: e.fullNameEn ?? '');
    final email = TextEditingController(text: e.email ?? '');
    final phone = TextEditingController(text: e.phoneNumber ?? '');
    final address = TextEditingController(text: e.address ?? '');
    final job = TextEditingController(text: e.jobTitle);
    final number = TextEditingController(text: e.employeeNumber ?? '');
    var department = e.department;
    var status = e.status;
    var hireDate = e.hireDate;
    String? opt(TextEditingController c) => c.text.trim().isEmpty ? null : c.text.trim();
    final ok = await showFormDialog(
      context,
      title: s.t('تعديل بيانات الموظف', 'Edit employee'),
      fields: (setState) => [
        TextFormField(controller: nameAr, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Name (Arabic)')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: nameEn, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Name (English)'))),
        TextFormField(controller: email, decoration: InputDecoration(labelText: s.email), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: phone, decoration: InputDecoration(labelText: s.phone)),
        TextFormField(controller: number, decoration: InputDecoration(labelText: s.employeeNumber)),
        TextFormField(controller: job, decoration: InputDecoration(labelText: s.jobTitle), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: address, decoration: InputDecoration(labelText: s.t('العنوان', 'Address'))),
        PickerField(
          label: s.department,
          value: department.name,
          onTap: () async {
            final d = await showSearchPicker<DepartmentRefDto>(context,
                title: s.department, search: (q) => searchDepartments(ref, q), label: (d) => d.name);
            if (d != null) setState(() => department = d);
          },
        ),
        PickerField(
          label: s.hireDate,
          value: f.date(hireDate),
          icon: Icons.event,
          onTap: () async {
            final d = await pickDate(context, initial: hireDate);
            if (d != null) setState(() => hireDate = d);
          },
        ),
        DropdownButtonFormField<EmployeeStatus>(
          initialValue: status,
          decoration: InputDecoration(labelText: s.status),
          items: [
            for (final st in EmployeeStatus.values.where((v) => v != EmployeeStatus.$unknown))
              DropdownMenuItem(value: st, child: Text(employeeStatusLabel(s, st))),
          ],
          onChanged: (v) => setState(() => status = v ?? status),
        ),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).employees.Employee_Update(
              id: e.id,
              body: EmployeeUpdateDto(
                fullNameAr: nameAr.text.trim(),
                fullNameEn: opt(nameEn),
                email: email.text.trim(),
                phoneNumber: opt(phone),
                dateOfBirth: e.dateOfBirth,
                address: opt(address),
                departmentId: department.id,
                jobTitle: job.text.trim(),
                hireDate: hireDate,
                status: status,
                concurrencyStamp: e.concurrencyStamp,
                gender: e.gender,
                employeeNumber: opt(number),
              ),
            ),
        done: s.saved,
      ),
    );
    if (ok) ref.invalidate(employeeFileProvider(e.id));
  }

  Future<void> _correctHireDate(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    final f = Fmt(context);
    final reason = TextEditingController();
    DateTime? date;
    final ok = await showFormDialog(
      context,
      title: s.t('تصحيح تاريخ المباشرة', 'Correct hire date'),
      fields: (setState) => [
        PickerField(
          label: s.hireDate,
          value: date == null ? null : f.date(date),
          icon: Icons.event,
          onTap: () async {
            final d = await pickDate(context, initial: e.hireDate);
            if (d != null) setState(() => date = d);
          },
        ),
        TextFormField(controller: reason, maxLines: 3, decoration: InputDecoration(labelText: s.reason), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () async {
        if (date == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).employees.Employee_CorrectHireDate(
                id: e.id,
                body: EmployeeHireDateCorrectionDto(hireDate: date!, reason: reason.text.trim(), concurrencyStamp: e.concurrencyStamp),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) ref.invalidate(employeeFileProvider(e.id));
  }
}

class _ManagerTab extends ConsumerStatefulWidget {
  const _ManagerTab({required this.e});
  final EmployeeHrDto e;

  @override
  ConsumerState<_ManagerTab> createState() => _ManagerTabState();
}

class _ManagerTabState extends ConsumerState<_ManagerTab> {
  int _version = 0;

  Future<void> _change() async {
    final s = S.of(context);
    final reason = TextEditingController();
    EmployeeRefDto? manager;
    final ok = await showFormDialog(
      context,
      title: s.t('تغيير المدير', 'Change manager'),
      fields: (setState) => [
        PickerField(
          label: s.manager,
          value: manager?.fullName,
          onTap: () async {
            final m = await showSearchPicker<EmployeeRefDto>(context,
                title: s.manager, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
            if (m != null) setState(() => manager = m);
          },
        ),
        TextFormField(controller: reason, maxLines: 2, decoration: InputDecoration(labelText: s.note)),
      ],
      onSubmit: () async {
        if (manager == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(context, () async {
          final r = await ref.read(hrApiProvider).employees.Employee_ChangeManager(
                id: widget.e.id,
                body: ManagerChangeInputDto(
                  managerEmployeeId: manager!.id,
                  reason: reason.text.trim().isEmpty ? null : reason.text.trim(),
                  concurrencyStamp: widget.e.concurrencyStamp,
                ),
              );
          if (mounted && r.movedPendingItemCount > 0) {
            showDone(context, s.t('انتقل ${r.movedPendingItemCount} عنصر معلق إلى المدير الجديد', '${r.movedPendingItemCount} pending items moved to the new manager'));
          }
        }, done: s.saved);
      },
    );
    if (ok) {
      ref.invalidate(employeeFileProvider(widget.e.id));
      setState(() => _version++);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).employees;
    return PagedList<EmployeeManagerHistoryDto>(
      key: ValueKey(_version),
      header: HrCard(
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Row(children: [
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(s.t('المدير الحالي', 'Current manager'), style: TextStyle(color: context.hr.textSecondary)),
              const SizedBox(height: 4),
              Text(widget.e.manager?.fullName ?? s.t('بلا مدير', 'No manager'), style: Theme.of(context).textTheme.titleMedium),
            ]),
          ),
          if (session.can(HrOps.Employee_ChangeManager) && widget.e.status != EmployeeStatus.ended)
            FilledButton.icon(onPressed: _change, icon: const Icon(Icons.swap_horiz), label: Text(s.t('تغيير', 'Change'))),
        ]),
        if (widget.e.directReportCount > 0 || (session.can(HrOps.Manager_AssignEmployees) && widget.e.status == EmployeeStatus.active))
          Padding(
            padding: const EdgeInsets.only(top: 12),
            child: Row(children: [
              Expanded(child: Text(s.t('المرؤوسون المباشرون: ${widget.e.directReportCount}', 'Direct reports: ${widget.e.directReportCount}'))),
              if (session.can(HrOps.Manager_AssignEmployees) && widget.e.status == EmployeeStatus.active)
                OutlinedButton.icon(
                  onPressed: () async {
                    final ok = await showAssignEmployeesDialog(context, ref,
                        EmployeeRefDto(id: widget.e.id, fullName: widget.e.fullName, departmentName: widget.e.department.name));
                    if (ok) ref.invalidate(employeeFileProvider(widget.e.id));
                  },
                  icon: const Icon(Icons.group_add_outlined),
                  label: Text(s.t('تعيين موظفين له', 'Assign employees')),
                ),
            ]),
          ),
        ]),
      ),
      fetch: (skip, max) async {
        if (!session.can(HrOps.Employee_GetManagerHistory)) return (<EmployeeManagerHistoryDto>[], 0);
        final r = await api.Employee_GetManagerHistory(id: widget.e.id, skipCount: skip, maxResultCount: max);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, h) => HrCard(
        child: ListTile(
          contentPadding: EdgeInsets.zero,
          leading: Icon(h.toDate == null ? Icons.link : Icons.history, color: context.hr.accent),
          title: Text(h.manager.fullName),
          subtitle: Text([
            '${f.date(h.fromDate)} → ${h.toDate == null ? s.t('الآن', 'now') : f.date(h.toDate)}',
            if (h.changedBy != null) h.changedBy!.displayName,
            if (h.reason != null) h.reason!,
          ].join(' · ')),
        ),
      ),
    );
  }
}

class _ShiftTab extends ConsumerStatefulWidget {
  const _ShiftTab({required this.e});
  final EmployeeHrDto e;

  @override
  ConsumerState<_ShiftTab> createState() => _ShiftTabState();
}

class _ShiftTabState extends ConsumerState<_ShiftTab> {
  int _version = 0;

  Future<void> _assign() async {
    final s = S.of(context);
    final f = Fmt(context);
    String? shiftId;
    DateTime? start;
    final ok = await showFormDialog(
      context,
      title: s.t('تعيين وردية', 'Assign shift'),
      fields: (setState) => [
        Consumer(builder: (context, ref, _) {
          final shifts = ref.watch(activeShiftsProvider);
          return shifts.when(
            loading: () => const LinearProgressIndicator(),
            error: (e, _) => Text(s.error(e)),
            data: (list) => DropdownButtonFormField<String>(
              initialValue: shiftId,
              isExpanded: true,
              decoration: InputDecoration(labelText: s.shift),
              items: [for (final w in list) DropdownMenuItem(value: w.id, child: Text(w.name))],
              onChanged: (v) => setState(() => shiftId = v),
              validator: (v) => v == null ? s.requiredField : null,
            ),
          );
        }),
        PickerField(
          label: s.startDate,
          value: start == null ? null : f.date(start),
          icon: Icons.event,
          onTap: () async {
            final d = await pickDate(context, initial: start);
            if (d != null) setState(() => start = d);
          },
        ),
      ],
      onSubmit: () async {
        if (start == null || shiftId == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).workShift.EmployeeShift_Assign(
                id: widget.e.id,
                body: EmployeeShiftAssignDto(shiftId: shiftId!, startDate: start!),
              ),
          done: s.saved,
        );
      },
    );
    if (ok) {
      ref.invalidate(employeeFileProvider(widget.e.id));
      setState(() => _version++);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).workShift;
    return PagedList<EmployeeShiftDto>(
      key: ValueKey(_version),
      header: HrCard(
        child: Row(children: [
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(s.t('الوردية الحالية', 'Current shift'), style: TextStyle(color: context.hr.textSecondary)),
              const SizedBox(height: 4),
              Text(
                widget.e.currentShift == null
                    ? s.t('لا توجد', 'None')
                    : '${widget.e.currentShift!.name} · ${s.from} ${f.date(widget.e.currentShift!.effectiveFrom)}',
                style: Theme.of(context).textTheme.titleMedium,
              ),
            ]),
          ),
          if (session.can(HrOps.EmployeeShift_Assign) && widget.e.status != EmployeeStatus.ended)
            FilledButton.icon(onPressed: _assign, icon: const Icon(Icons.schedule), label: Text(s.t('تعيين', 'Assign'))),
        ]),
      ),
      fetch: (skip, max) async {
        if (!session.can(HrOps.EmployeeShift_GetHistory)) return (<EmployeeShiftDto>[], 0);
        final r = await api.EmployeeShift_GetHistory(id: widget.e.id, skipCount: skip, maxResultCount: max);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, h) => HrCard(
        child: ListTile(
          contentPadding: EdgeInsets.zero,
          leading: Icon(h.endDate == null ? Icons.schedule : Icons.history, color: context.hr.accent),
          title: Text(h.shift.name),
          subtitle: Text('${f.date(h.startDate)} → ${h.endDate == null ? s.t('الآن', 'now') : f.date(h.endDate)}'),
        ),
      ),
    );
  }
}

/// Documents of one employee (EmployeeDocument_*). HR uploads and deletes; the server
/// filters what other audiences see (managers: normal sensitivity only).
class EmployeeDocumentsTab extends ConsumerStatefulWidget {
  const EmployeeDocumentsTab({super.key, required this.employeeId, required this.canManage});
  final String employeeId;
  final bool canManage;

  @override
  ConsumerState<EmployeeDocumentsTab> createState() => _EmployeeDocumentsTabState();
}

class _EmployeeDocumentsTabState extends ConsumerState<EmployeeDocumentsTab> {
  int _version = 0;

  String _typeLabel(S s, EmployeeDocumentType t) => switch (t) {
        EmployeeDocumentType.identity => s.t('هوية', 'Identity'),
        EmployeeDocumentType.passport => s.t('جواز', 'Passport'),
        EmployeeDocumentType.contract => s.t('عقد', 'Contract'),
        EmployeeDocumentType.certificate => s.t('شهادة', 'Certificate'),
        EmployeeDocumentType.medical => s.t('طبية', 'Medical'),
        EmployeeDocumentType.other => s.t('أخرى', 'Other'),
        EmployeeDocumentType.$unknown => '—',
      };

  Future<void> _upload() async {
    final s = S.of(context);
    final title = TextEditingController();
    var type = EmployeeDocumentType.identity;
    var sensitivity = DocumentSensitivity.normal;
    PlatformFile? file;
    final ok = await showFormDialog(
      context,
      title: s.t('رفع وثيقة', 'Upload document'),
      fields: (setState) => [
        TextFormField(controller: title, maxLength: 200, decoration: InputDecoration(labelText: s.t('العنوان', 'Title')), validator: (v) => requiredValidator(context, v)),
        DropdownButtonFormField<EmployeeDocumentType>(
          initialValue: type,
          decoration: InputDecoration(labelText: s.t('النوع', 'Type')),
          items: [
            for (final t in EmployeeDocumentType.values.where((v) => v != EmployeeDocumentType.$unknown))
              DropdownMenuItem(value: t, child: Text(_typeLabel(s, t))),
          ],
          onChanged: (v) => setState(() => type = v ?? type),
        ),
        SegmentedButton<DocumentSensitivity>(
          segments: [
            ButtonSegment(value: DocumentSensitivity.normal, label: Text(s.t('عادية', 'Normal'))),
            ButtonSegment(value: DocumentSensitivity.sensitive, label: Text(s.t('حساسة', 'Sensitive'))),
          ],
          selected: {sensitivity},
          onSelectionChanged: (v) => setState(() => sensitivity = v.first),
        ),
        OutlinedButton.icon(
          onPressed: () async {
            final r = await FilePicker.platform.pickFiles(withData: true);
            if (r != null && r.files.single.bytes != null) setState(() => file = r.files.single);
          },
          icon: const Icon(Icons.attach_file),
          label: Text(file?.name ?? s.addAttachment),
        ),
      ],
      onSubmit: () async {
        final picked = file;
        if (picked == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).employeeDocuments.EmployeeDocument_Upload(
                id: widget.employeeId,
                body: EmployeeDocumentUploadDto(
                  file: MultipartFile.fromBytes(picked.bytes!, filename: picked.name),
                  type: type,
                  title: title.text.trim(),
                  sensitivity: sensitivity,
                ),
              ),
          done: s.t('تم رفع الوثيقة، وتُفحص قبل إتاحتها', 'Uploaded; it is scanned before it becomes available'),
        );
      },
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _download(EmployeeDocumentDto d) async {
    await runAction(context, () async {
      final link = await ref.read(hrApiProvider).employeeDocuments.EmployeeDocument_GetDownloadUrl(id: d.id);
      // Pre-signed URL: opened without the Authorization header.
      await launchUrl(Uri.parse(link.url), mode: LaunchMode.externalApplication);
    });
  }

  Future<void> _delete(EmployeeDocumentDto d) async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('حذف الوثيقة', 'Delete document'), message: d.title, destructive: true)) return;
    if (!mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).employeeDocuments.EmployeeDocument_Delete(id: d.id), done: s.t('تم الحذف', 'Deleted'));
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).employeeDocuments;
    final canUpload = widget.canManage && session.can(HrOps.EmployeeDocument_Upload);
    final canDelete = widget.canManage && session.can(HrOps.EmployeeDocument_Delete);
    return PagedList<EmployeeDocumentDto>(
      key: ValueKey(_version),
      header: canUpload
          ? Align(
              alignment: AlignmentDirectional.centerStart,
              child: FilledButton.icon(onPressed: _upload, icon: const Icon(Icons.upload_file), label: Text(s.t('رفع وثيقة', 'Upload document'))),
            )
          : null,
      fetch: (skip, max) async {
        final r = await api.EmployeeDocument_List(id: widget.employeeId, skipCount: skip, maxResultCount: max);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, d) => HrCard(
        child: Row(children: [
          Icon(Icons.description_outlined, color: context.hr.accent),
          const SizedBox(width: 12),
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(d.title, style: const TextStyle(fontWeight: FontWeight.w600)),
              Text('${_typeLabel(s, d.type)} · ${d.fileName} · ${f.date(d.creationTime)}',
                  style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary)),
            ]),
          ),
          if (d.sensitivity == DocumentSensitivity.sensitive) ...[
            StatusPill(s.t('حساسة', 'Sensitive'), tone: Tone.warning),
            const SizedBox(width: 6),
          ],
          StatusPill(
            switch (d.scanStatus) {
              DocumentScanStatus.clean => s.t('سليمة', 'Clean'),
              DocumentScanStatus.infected => s.t('مصابة', 'Infected'),
              _ => s.t('قيد الفحص', 'Scanning'),
            },
            tone: d.scanStatus == DocumentScanStatus.clean
                ? Tone.success
                : (d.scanStatus == DocumentScanStatus.infected ? Tone.danger : Tone.info),
          ),
          if (d.scanStatus == DocumentScanStatus.clean)
            IconButton(tooltip: s.t('تنزيل', 'Download'), onPressed: () => _download(d), icon: const Icon(Icons.download)),
          if (canDelete)
            IconButton(tooltip: s.t('حذف', 'Delete'), onPressed: () => _delete(d), icon: const Icon(Icons.delete_outline, color: HrColors.danger)),
        ]),
      ),
    );
  }
}

class _TaskLogTab extends ConsumerWidget {
  const _TaskLogTab({required this.e});
  final EmployeeHrDto e;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).employees;
    final can = ref.watch(sessionProvider).can(HrOps.Employee_GetAdministrativeTaskLog);
    if (!can) return EmptyState(message: s.forbidden);
    return PagedList<AdministrativeTaskLogEntryDto>(
      emptyMessage: s.t('لا توجد مهام إدارية مسجلة', 'No administrative tasks recorded'),
      fetch: (skip, max) async {
        final r = await api.Employee_GetAdministrativeTaskLog(id: e.id, skipCount: skip, maxResultCount: max);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, t) => TaskLogTile(entry: t, when: f.dateTime(t.occurredAt)),
    );
  }
}

class TaskLogTile extends StatelessWidget {
  const TaskLogTile({super.key, required this.entry, required this.when});
  final AdministrativeTaskLogEntryDto entry;
  final String when;

  @override
  Widget build(BuildContext context) => HrCard(
        child: ListTile(
          contentPadding: EdgeInsets.zero,
          leading: Icon(Icons.task_alt, color: context.hr.accent),
          title: Text(entry.title),
          subtitle: Text(when),
        ),
      );
}

class _AccountTab extends ConsumerWidget {
  const _AccountTab({required this.e});
  final EmployeeHrDto e;

  Future<void> _create(BuildContext context, WidgetRef ref) async {
    final s = S.of(context);
    final userName = TextEditingController();
    final roles = <String>{AppRole.employee.value};
    UserAccountCreatedDto? created;
    final ok = await showFormDialog(
      context,
      title: s.t('إنشاء حساب', 'Create account'),
      fields: (setState) => [
        TextFormField(
          controller: userName,
          textDirection: TextDirection.ltr,
          decoration: InputDecoration(labelText: s.t('اسم المستخدم', 'User name')),
          validator: (v) => (v == null || v.trim().length < 3) ? s.t('3 أحرف على الأقل', 'At least 3 characters') : null,
        ),
        Consumer(builder: (context, ref, _) {
          final list = ref.watch(rolesProvider).valueOrNull ?? const <RoleDto>[];
          return Wrap(spacing: 8, runSpacing: 8, children: [
            for (final r in list)
              FilterChip(
                label: Text(r.displayName),
                selected: roles.contains(r.name),
                onSelected: (v) => setState(() => v ? roles.add(r.name) : roles.remove(r.name)),
              ),
          ]);
        }),
      ],
      onSubmit: () => runAction(context, () async {
        created = await ref.read(hrApiProvider).userAccounts.UserAccount_Create(
              body: UserAccountCreateDto(employeeId: e.id, userName: userName.text.trim(), roleNames: roles.toList()),
            );
      }),
    );
    if (ok && created != null && context.mounted) {
      await showCredentialsDialog(context, created!.credentials);
      ref.invalidate(employeeFileProvider(e.id));
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final roles = ref.watch(rolesProvider).valueOrNull ?? const <RoleDto>[];
    final a = e.account;
    return SingleChildScrollView(
      child: HrCard(
        child: a == null
            ? Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                Text(s.t('لا يوجد حساب لهذا الموظف.', 'This employee has no account.')),
                const SizedBox(height: 12),
                if (session.can(HrOps.UserAccount_Create) && e.status != EmployeeStatus.ended)
                  Align(
                    alignment: AlignmentDirectional.centerStart,
                    child: FilledButton.icon(
                        onPressed: () => _create(context, ref), icon: const Icon(Icons.person_add), label: Text(s.t('إنشاء حساب', 'Create account'))),
                  ),
              ])
            : Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                InfoRow(s.t('اسم المستخدم', 'User name'), a.userName),
                InfoRow(s.status, a.isActive ? s.t('فعال', 'Active') : s.t('موقوف', 'Inactive')),
                InfoRow(s.roles, a.roleNames.map((r) => roleLabel(context, r, roles)).join('، ')),
                InfoRow(s.mfa, a.mfaEnabled ? s.mfaEnabled : (a.mfaRequired ? s.t('مطلوب وغير مفعّل', 'Required, not enabled') : s.mfaDisabled)),
                if (a.mustChangePassword) InfoRow(s.changePassword, s.t('مطلوب عند الدخول القادم', 'Required at next sign-in')),
                const SizedBox(height: 12),
                if (session.can(HrOps.UserAccount_Get))
                  Align(
                    alignment: AlignmentDirectional.centerStart,
                    child: OutlinedButton.icon(
                      onPressed: () => context.go('/accounts/${a.userId}'),
                      icon: const Icon(Icons.manage_accounts),
                      label: Text(s.t('إدارة الحساب', 'Manage account')),
                    ),
                  ),
              ]),
      ),
    );
  }
}
