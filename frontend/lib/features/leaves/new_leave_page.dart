import 'package:dio/dio.dart' show MultipartFile, DioMediaType;
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../home/home_page.dart' show myBalanceProvider;

final otherLeaveTypesProvider = FutureProvider.autoDispose<List<OtherLeaveTypeOptionDto>>((ref) async {
  final r = await ref.watch(hrApiProvider).otherLeaveType.OtherLeaveType_ListAvailable();
  return r.items;
});

/// S-LV-02: hourly, annual or other; server preview (LeaveRequest_Preview) before
/// LeaveRequest_Create. Attachments go first through LeaveRequest_UploadAttachment.
class NewLeavePage extends ConsumerStatefulWidget {
  const NewLeavePage({super.key});

  @override
  ConsumerState<NewLeavePage> createState() => _NewLeavePageState();
}

class _PickedFile {
  _PickedFile(this.name, this.bytes);
  final String name;
  final List<int> bytes;
}

class _NewLeavePageState extends ConsumerState<NewLeavePage> {
  final _form = GlobalKey<FormState>();
  final _notes = TextEditingController();
  LeaveKind _kind = LeaveKind.annual;
  DateTime? _date;
  DateTime? _start;
  DateTime? _end;
  TimeOfDay? _fromTime;
  TimeOfDay? _toTime;
  OtherLeaveTypeOptionDto? _other;
  final List<_PickedFile> _files = [];
  LeaveRequestPreviewDto? _preview;
  bool _busy = false;
  Object? _error;
  final String _idempotencyKey = const Uuid().v4();

  @override
  void dispose() {
    _notes.dispose();
    super.dispose();
  }

  String _hhmm(TimeOfDay t) => '${t.hour.toString().padLeft(2, '0')}:${t.minute.toString().padLeft(2, '0')}';

  bool get _hourly => _kind == LeaveKind.hourly;

  void _changed() => setState(() => _preview = null);

  LeaveRequestPreviewInputDto _previewInput() => LeaveRequestPreviewInputDto(
        kind: _kind,
        date: _hourly ? _date : null,
        fromTime: _hourly && _fromTime != null ? _hhmm(_fromTime!) : null,
        toTime: _hourly && _toTime != null ? _hhmm(_toTime!) : null,
        startDate: _hourly ? null : _start,
        endDate: _hourly ? null : _end,
        otherLeaveTypeId: _kind == LeaveKind.other ? _other?.id : null,
      );

  bool _validate() {
    final ok = _form.currentState!.validate();
    final datesOk = _hourly ? (_date != null && _fromTime != null && _toTime != null) : (_start != null && _end != null);
    final otherOk = _kind != LeaveKind.other || _other != null;
    if (!datesOk || !otherOk) setState(() => _error = S.of(context).validation);
    return ok && datesOk && otherOk;
  }

  Future<void> _runPreview() async {
    if (!_validate()) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final p = await ref.read(hrApiProvider).leaveRequest.LeaveRequest_Preview(body: _previewInput());
      if (mounted) setState(() => _preview = p);
    } catch (e) {
      if (mounted) setState(() => _error = e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _submit() async {
    if (!_validate()) return;
    final s = S.of(context);
    if ((_other?.requiresAttachment ?? false) && _files.isEmpty) {
      setState(() => _error = s.attachmentRequired);
      return;
    }
    setState(() {
      _busy = true;
      _error = null;
    });
    final api = ref.read(hrApiProvider).leaveRequest;
    try {
      final ids = <String>[];
      for (final f in _files) {
        final uploaded = await api.LeaveRequest_UploadAttachment(
          body: LeaveAttachmentUploadDto(
            file: MultipartFile.fromBytes(f.bytes, filename: f.name, contentType: _mediaType(f.name)),
          ),
        );
        ids.add(uploaded.id);
      }
      final p = _previewInput();
      final created = await api.LeaveRequest_Create(
        idempotencyKey: _idempotencyKey,
        body: LeaveRequestCreateDto(
          kind: p.kind,
          date: p.date,
          fromTime: p.fromTime,
          toTime: p.toTime,
          startDate: p.startDate,
          endDate: p.endDate,
          otherLeaveTypeId: p.otherLeaveTypeId,
          attachmentIds: ids.isEmpty ? null : ids,
          notes: _notes.text.trim().isEmpty ? null : _notes.text.trim(),
        ),
      );
      if (!mounted) return;
      ref.invalidate(myBalanceProvider);
      showDone(context, s.submitted);
      context.go('/requests/${created.requestId}');
    } catch (e) {
      if (mounted) setState(() => _error = e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  DioMediaType _mediaType(String name) {
    final n = name.toLowerCase();
    if (n.endsWith('.pdf')) return DioMediaType('application', 'pdf');
    if (n.endsWith('.png')) return DioMediaType('image', 'png');
    return DioMediaType('image', 'jpeg');
  }

  Future<void> _pickFile() async {
    final result = await FilePicker.platform.pickFiles(
      withData: true,
      type: FileType.custom,
      allowedExtensions: const ['pdf', 'jpg', 'jpeg', 'png'],
    );
    final file = result?.files.single;
    if (file == null || file.bytes == null) return;
    setState(() => _files.add(_PickedFile(file.name, file.bytes!)));
  }

  Future<void> _pickDate(void Function(DateTime) set, DateTime? current) async {
    final now = DateTime.now();
    final d = await showDatePicker(
      context: context,
      initialDate: current ?? now,
      firstDate: DateTime(now.year - 1),
      lastDate: DateTime(now.year + 2),
    );
    if (d != null) {
      set(d);
      _changed();
    }
  }

  Future<void> _pickTime(void Function(TimeOfDay) set, TimeOfDay? current) async {
    final t = await showTimePicker(
      context: context,
      initialTime: current ?? const TimeOfDay(hour: 9, minute: 0),
      builder: (context, child) => MediaQuery(
        data: MediaQuery.of(context).copyWith(alwaysUse24HourFormat: true),
        child: child!,
      ),
    );
    if (t != null) {
      set(t);
      _changed();
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    Widget pickerField(String label, String? value, VoidCallback onTap, IconData icon) => InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(HrColors.radiusControl),
          child: InputDecorator(
            decoration: InputDecoration(labelText: label, suffixIcon: Icon(icon)),
            child: Text(value ?? '—'),
          ),
        );

    return HrPage(
      title: s.newLeave,
      child: SingleChildScrollView(
        padding: const EdgeInsets.only(bottom: 24),
        child: Align(
          alignment: AlignmentDirectional.topStart,
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 720),
            child: HrCard(
              child: Form(
                key: _form,
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  Text(s.leaveKind, style: TextStyle(color: context.hr.textSecondary)),
                  const SizedBox(height: 8),
                  SegmentedButton<LeaveKind>(
                    segments: [
                      for (final k in const [LeaveKind.annual, LeaveKind.hourly, LeaveKind.other])
                        ButtonSegment(value: k, label: Text(s.leaveKindLabel(k))),
                    ],
                    selected: {_kind},
                    onSelectionChanged: (v) {
                      setState(() => _kind = v.first);
                      _changed();
                    },
                  ),
                  const SizedBox(height: 16),
                  if (_kind == LeaveKind.other) ...[
                    Consumer(builder: (context, ref, _) {
                      return ref.watch(otherLeaveTypesProvider).when(
                            loading: () => const LinearProgressIndicator(),
                            error: (e, _) => Text(s.error(e), style: const TextStyle(color: HrColors.danger)),
                            data: (types) => DropdownButtonFormField<OtherLeaveTypeOptionDto>(
                              initialValue: _other,
                              isExpanded: true,
                              decoration: InputDecoration(labelText: s.otherLeaveType),
                              items: [for (final t in types) DropdownMenuItem(value: t, child: Text(t.name))],
                              onChanged: (v) {
                                setState(() => _other = v);
                                _changed();
                              },
                              validator: (v) => v == null ? s.requiredField : null,
                            ),
                          );
                    }),
                    const SizedBox(height: 12),
                  ],
                  if (_hourly) ...[
                    pickerField(s.date, _date == null ? null : f.date(_date), () => _pickDate((d) => _date = d, _date),
                        Icons.event),
                    const SizedBox(height: 12),
                    Row(children: [
                      Expanded(
                          child: pickerField(s.fromTime, _fromTime == null ? null : _hhmm(_fromTime!),
                              () => _pickTime((t) => _fromTime = t, _fromTime), Icons.schedule)),
                      const SizedBox(width: 12),
                      Expanded(
                          child: pickerField(s.toTime, _toTime == null ? null : _hhmm(_toTime!),
                              () => _pickTime((t) => _toTime = t, _toTime), Icons.schedule)),
                    ]),
                  ] else
                    Row(children: [
                      Expanded(
                          child: pickerField(s.startDate, _start == null ? null : f.date(_start),
                              () => _pickDate((d) => _start = d, _start), Icons.event)),
                      const SizedBox(width: 12),
                      Expanded(
                          child: pickerField(s.endDate, _end == null ? null : f.date(_end),
                              () => _pickDate((d) => _end = d, _end ?? _start), Icons.event)),
                    ]),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _notes,
                    maxLines: 3,
                    decoration: InputDecoration(labelText: s.notes, alignLabelWithHint: true),
                  ),
                  if (_kind == LeaveKind.other) ...[
                    const SizedBox(height: 12),
                    Wrap(spacing: 8, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
                      for (final file in _files)
                        InputChip(label: Text(file.name), onDeleted: () => setState(() => _files.remove(file))),
                      OutlinedButton.icon(onPressed: _pickFile, icon: const Icon(Icons.attach_file), label: Text(s.addAttachment)),
                    ]),
                    if (_other?.requiresAttachment ?? false)
                      Padding(
                        padding: const EdgeInsets.only(top: 6),
                        child: Text(s.attachmentRequired, style: const TextStyle(color: HrColors.warning)),
                      ),
                  ],
                  if (_preview != null) ...[const SizedBox(height: 16), _PreviewCard(preview: _preview!)],
                  if (_error != null) ...[
                    const SizedBox(height: 12),
                    Text(_error is String ? _error as String : s.error(_error!), style: const TextStyle(color: HrColors.danger)),
                  ],
                  const SizedBox(height: 16),
                  Wrap(spacing: 8, runSpacing: 8, children: [
                    OutlinedButton.icon(onPressed: _busy ? null : _runPreview, icon: const Icon(Icons.calculate_outlined), label: Text(s.preview)),
                    FilledButton(
                      onPressed: _busy || (_preview != null && !_preview!.canSubmit) ? null : _submit,
                      child: Text(s.submit),
                    ),
                  ]),
                ]),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _PreviewCard extends StatelessWidget {
  const _PreviewCard({required this.preview});
  final LeaveRequestPreviewDto preview;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final p = preview;
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: context.hr.glow,
        borderRadius: BorderRadius.circular(HrColors.radiusControl),
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        if (p.kind == LeaveKind.hourly) InfoRow(s.requestedHours, f.number(p.requestedHours)) else InfoRow(s.workingDays, f.number(p.workingDays)),
        InfoRow(s.annualDays, f.number(p.annualDays)),
        InfoRow(s.remainingAfter, f.number(p.remainingDaysAfter)),
        if (!p.isBalanceSufficient) InfoRow(s.shortage, f.number(p.shortageDays)),
        for (final issue in p.issues)
          Padding(
            padding: const EdgeInsets.only(top: 6),
            child: Row(children: [
              const Icon(Icons.warning_amber, size: 18, color: HrColors.warning),
              const SizedBox(width: 6),
              Expanded(child: Text(issue.message)),
            ]),
          ),
      ]),
    );
  }
}
