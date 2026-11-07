import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../api/runtime/json.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';

/// S-ATT-05: AttendanceCorrection_Create. Decided later through /api/app/request/{id}.
class CorrectionPage extends ConsumerStatefulWidget {
  const CorrectionPage({super.key, this.date});
  final String? date;

  @override
  ConsumerState<CorrectionPage> createState() => _CorrectionPageState();
}

class _CorrectionPageState extends ConsumerState<CorrectionPage> {
  final _form = GlobalKey<FormState>();
  final _reason = TextEditingController();
  DateTime? _date;
  TimeOfDay? _time;
  AttendancePunchType _type = AttendancePunchType.checkIn;
  bool _busy = false;
  final String _idempotencyKey = const Uuid().v4();

  @override
  void initState() {
    super.initState();
    final d = widget.date;
    if (d != null) {
      try {
        _date = hrDateFromJson(d);
      } catch (_) {}
    }
  }

  @override
  void dispose() {
    _reason.dispose();
    super.dispose();
  }

  String _hhmm(TimeOfDay t) => '${t.hour.toString().padLeft(2, '0')}:${t.minute.toString().padLeft(2, '0')}';

  Future<void> _submit() async {
    final s = S.of(context);
    if (!_form.currentState!.validate()) return;
    if (_date == null || _time == null) {
      showError(context, s.validation);
      return;
    }
    setState(() => _busy = true);
    try {
      final r = await ref.read(hrApiProvider).attendanceCorrection.AttendanceCorrection_Create(
            idempotencyKey: _idempotencyKey,
            body: AttendanceCorrectionCreateDto(
              date: _date!,
              punchType: _type,
              correctTime: _hhmm(_time!),
              reason: _reason.text.trim(),
            ),
          );
      if (!mounted) return;
      showDone(context, s.submitted);
      context.go('/requests/${r.requestId}');
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
    return HrPage(
      title: s.correction,
      child: SingleChildScrollView(
        padding: const EdgeInsets.only(bottom: 24),
        child: Align(
          alignment: AlignmentDirectional.topStart,
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 560),
            child: HrCard(
              child: Form(
                key: _form,
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  InkWell(
                    borderRadius: BorderRadius.circular(HrColors.radiusControl),
                    onTap: () async {
                      final now = DateTime.now();
                      final d = await showDatePicker(
                        context: context,
                        initialDate: _date ?? now,
                        firstDate: DateTime(now.year - 1),
                        lastDate: now, // the date cannot be in the future
                      );
                      if (d != null) setState(() => _date = d);
                    },
                    child: InputDecorator(
                      decoration: InputDecoration(labelText: s.date, suffixIcon: const Icon(Icons.event)),
                      child: Text(_date == null ? '—' : f.date(_date)),
                    ),
                  ),
                  const SizedBox(height: 12),
                  SegmentedButton<AttendancePunchType>(
                    segments: [
                      ButtonSegment(value: AttendancePunchType.checkIn, label: Text(s.checkIn)),
                      ButtonSegment(value: AttendancePunchType.checkOut, label: Text(s.checkOut)),
                    ],
                    selected: {_type},
                    onSelectionChanged: (v) => setState(() => _type = v.first),
                  ),
                  const SizedBox(height: 12),
                  InkWell(
                    borderRadius: BorderRadius.circular(HrColors.radiusControl),
                    onTap: () async {
                      final t = await showTimePicker(
                        context: context,
                        initialTime: _time ?? const TimeOfDay(hour: 8, minute: 0),
                        builder: (context, child) => MediaQuery(
                          data: MediaQuery.of(context).copyWith(alwaysUse24HourFormat: true),
                          child: child!,
                        ),
                      );
                      if (t != null) setState(() => _time = t);
                    },
                    child: InputDecorator(
                      decoration: InputDecoration(labelText: s.correctTime, suffixIcon: const Icon(Icons.schedule)),
                      child: Text(_time == null ? '—' : _hhmm(_time!)),
                    ),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _reason,
                    maxLines: 3,
                    maxLength: 2000,
                    decoration: InputDecoration(labelText: s.reason, alignLabelWithHint: true),
                    validator: (v) => (v == null || v.trim().isEmpty) ? s.requiredField : null,
                  ),
                  const SizedBox(height: 16),
                  FilledButton(onPressed: _busy ? null : _submit, child: Text(s.submit)),
                ]),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
