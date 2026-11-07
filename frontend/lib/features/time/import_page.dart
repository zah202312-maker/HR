import 'package:dio/dio.dart' show MultipartFile;
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/paged_list.dart';

String importStatusLabel(S s, AttendanceImportStatus st) => switch (st) {
      AttendanceImportStatus.queued => s.t('بالانتظار', 'Queued'),
      AttendanceImportStatus.processing => s.t('قيد المعالجة', 'Processing'),
      AttendanceImportStatus.completed => s.t('مكتمل', 'Completed'),
      AttendanceImportStatus.completedWithErrors => s.t('مكتمل مع أخطاء', 'Completed with errors'),
      AttendanceImportStatus.failed => s.t('فشل', 'Failed'),
      AttendanceImportStatus.$unknown => '—',
    };

/// S-ATT-06: AttendanceImport_Create (202, processed in the background), list and row errors.
class AttendanceImportPage extends ConsumerStatefulWidget {
  const AttendanceImportPage({super.key});

  @override
  ConsumerState<AttendanceImportPage> createState() => _AttendanceImportPageState();
}

class _AttendanceImportPageState extends ConsumerState<AttendanceImportPage> {
  int _version = 0;

  Future<void> _upload() async {
    final s = S.of(context);
    String? deviceId;
    PlatformFile? file;
    final devices = await ref.read(hrApiProvider).attendanceDevice.AttendanceDevice_List(isActive: true, maxResultCount: 200);
    if (!mounted) return;
    final ok = await showFormDialog(
      context,
      title: s.t('استيراد ملف حضور', 'Import attendance file'),
      fields: (setState) => [
        DropdownButtonFormField<String>(
          initialValue: deviceId,
          isExpanded: true,
          decoration: InputDecoration(labelText: s.t('الجهاز الذي جاءت منه البيانات', 'Source device')),
          items: [for (final d in devices.items) DropdownMenuItem(value: d.id, child: Text('${d.name} (${d.serialNumber})'))],
          onChanged: (v) => setState(() => deviceId = v),
          validator: (v) => v == null ? s.requiredField : null,
        ),
        OutlinedButton.icon(
          onPressed: () async {
            final r = await FilePicker.platform.pickFiles(withData: true, type: FileType.custom, allowedExtensions: const ['xlsx', 'csv']);
            if (r != null && r.files.single.bytes != null) setState(() => file = r.files.single);
          },
          icon: const Icon(Icons.upload_file),
          label: Text(file?.name ?? s.t('اختر ملف xlsx أو csv', 'Choose an xlsx or csv file')),
        ),
        Text(s.t('العمودان الإلزاميان في الصف الأول: deviceUserId وpunchDateTime.', 'Required columns in the first row: deviceUserId and punchDateTime.'),
            style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () async {
        final f = file;
        if (f == null || deviceId == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).attendanceImport.AttendanceImport_Create(
              body: AttendanceImportUploadDto(file: MultipartFile.fromBytes(f.bytes!, filename: f.name), deviceId: deviceId!)),
          done: s.t('استُلم الملف وتُعالج صفوفه في الخلفية', 'Received; rows are processed in the background'),
        );
      },
      submitLabel: s.t('رفع', 'Upload'),
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _details(AttendanceImportDto i) async {
    final s = S.of(context);
    AttendanceImportDto detail = i;
    try {
      detail = await ref.read(hrApiProvider).attendanceImport.AttendanceImport_Get(id: i.id);
    } catch (e) {
      if (mounted) showError(context, e);
      return;
    }
    if (!mounted) return;
    await showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(detail.fileName),
        content: SizedBox(
          width: 560,
          height: 420,
          child: ListView(children: [
            InfoRow(s.status, importStatusLabel(s, detail.status)),
            InfoRow(s.t('الصفوف', 'Rows'), '${detail.totalRows}'),
            InfoRow(s.t('مقبولة', 'Accepted'), '${detail.acceptedCount}'),
            InfoRow(s.t('مكررة', 'Duplicates'), '${detail.duplicateCount}'),
            InfoRow(s.t('بدون ربط', 'Unmapped'), '${detail.unmappedCount}'),
            InfoRow(s.t('أخطاء', 'Errors'), '${detail.errorCount}'),
            const Divider(),
            for (final e in detail.errors)
              ListTile(
                dense: true,
                leading: Text('#${e.rowNumber}'),
                title: Text(e.message),
                subtitle: e.columnName == null ? null : Text(e.columnName!),
              ),
          ]),
        ),
        actions: [TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.close))],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).attendanceImport;
    return HrPage(
      title: s.t('استيراد الحضور', 'Attendance import'),
      actions: [
        IconButton(tooltip: s.retry, onPressed: () => setState(() => _version++), icon: const Icon(Icons.refresh)),
        FilledButton.icon(onPressed: _upload, icon: const Icon(Icons.upload_file), label: Text(s.t('استيراد', 'Import'))),
      ],
      child: PagedList<AttendanceImportDto>(
        key: ValueKey(_version),
        fetch: (skip, max) async {
          final r = await api.AttendanceImport_List(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, i) => HrCard(
          onTap: () => _details(i),
          child: Row(children: [
            Icon(Icons.table_view, color: context.hr.accent),
            const SizedBox(width: 12),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(i.fileName, style: const TextStyle(fontWeight: FontWeight.w600)),
                Text('${i.uploadedBy.displayName} · ${f.dateTime(i.creationTime)} · ${i.acceptedCount}/${i.totalRows}',
                    style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
              ]),
            ),
            StatusPill(
              importStatusLabel(s, i.status),
              tone: switch (i.status) {
                AttendanceImportStatus.completed => Tone.success,
                AttendanceImportStatus.completedWithErrors => Tone.warning,
                AttendanceImportStatus.failed => Tone.danger,
                _ => Tone.info,
              },
            ),
          ]),
        ),
      ),
    );
  }
}

final attendanceSettingsProvider = FutureProvider.autoDispose<AttendanceSettingsDto>(
  (ref) => ref.watch(hrApiProvider).attendanceSettings.AttendanceSettings_Get(),
);

/// S-ATT-08: capture method and company time zone (system admin).
class AttendanceSettingsPage extends ConsumerWidget {
  const AttendanceSettingsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    return HrPage(
      title: s.t('إعدادات الحضور', 'Attendance settings'),
      child: AsyncBody<AttendanceSettingsDto>(
        value: ref.watch(attendanceSettingsProvider),
        onRetry: () => ref.invalidate(attendanceSettingsProvider),
        builder: (a) => _SettingsForm(settings: a),
      ),
    );
  }
}

class _SettingsForm extends ConsumerStatefulWidget {
  const _SettingsForm({required this.settings});
  final AttendanceSettingsDto settings;

  @override
  ConsumerState<_SettingsForm> createState() => _SettingsFormState();
}

class _SettingsFormState extends ConsumerState<_SettingsForm> {
  late AttendanceCaptureMethod _method = widget.settings.method;
  late final _tz = TextEditingController(text: widget.settings.timeZone);

  @override
  void dispose() {
    _tz.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final gps = widget.settings.gpsAvailable;
    return Align(
      alignment: AlignmentDirectional.topStart,
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 560),
        child: HrCard(
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            Text(s.t('طريقة الحضور', 'Capture method'), style: TextStyle(color: context.hr.textSecondary)),
            const SizedBox(height: 8),
            SegmentedButton<AttendanceCaptureMethod>(
              segments: [
                ButtonSegment(value: AttendanceCaptureMethod.device, label: Text(s.t('بصمة', 'Device'))),
                ButtonSegment(value: AttendanceCaptureMethod.gps, label: Text('GPS'), enabled: gps),
                ButtonSegment(value: AttendanceCaptureMethod.both, label: Text(s.t('الاثنان', 'Both')), enabled: gps),
              ],
              selected: {_method},
              onSelectionChanged: (v) => setState(() => _method = v.first),
            ),
            if (!gps)
              Padding(
                padding: const EdgeInsets.only(top: 6),
                child: Text(s.t('خيار GPS غير متاح حتى إطلاق الميزة.', 'GPS is unavailable until the feature launches.'), style: Theme.of(context).textTheme.labelSmall),
              ),
            const SizedBox(height: 16),
            TextField(
              controller: _tz,
              textDirection: TextDirection.ltr,
              decoration: InputDecoration(labelText: s.t('توقيت الشركة (IANA)', 'Company time zone (IANA)'), hintText: 'Asia/Baghdad'),
            ),
            const SizedBox(height: 16),
            Align(
              alignment: AlignmentDirectional.centerStart,
              child: FilledButton(
                onPressed: () async {
                  final ok = await runAction(
                    context,
                    () => ref.read(hrApiProvider).attendanceSettings.AttendanceSettings_Update(
                        body: AttendanceSettingsUpdateDto(method: _method, timeZone: _tz.text.trim(), concurrencyStamp: widget.settings.concurrencyStamp)),
                    done: s.saved,
                  );
                  if (ok) ref.invalidate(attendanceSettingsProvider);
                },
                child: Text(s.save),
              ),
            ),
          ]),
        ),
      ),
    );
  }
}
