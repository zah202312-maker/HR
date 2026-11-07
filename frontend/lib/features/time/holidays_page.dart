import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import 'time_shared.dart';

final weeklyOffProvider = FutureProvider.autoDispose<WeeklyOffSettingDto>(
  (ref) => ref.watch(hrApiProvider).holiday.WeeklyOffSetting_Get(),
);

/// S-HOL-01: official holidays (one record per day) and the company weekly-off days.
class HolidaysPage extends ConsumerStatefulWidget {
  const HolidaysPage({super.key});

  @override
  ConsumerState<HolidaysPage> createState() => _HolidaysPageState();
}

class _HolidaysPageState extends ConsumerState<HolidaysPage> {
  int _year = DateTime.now().year;
  int _version = 0;

  Future<void> _edit([HolidayDto? h]) async {
    final s = S.of(context);
    final f = Fmt(context);
    final ar = TextEditingController(text: h?.nameAr ?? '');
    final en = TextEditingController(text: h?.nameEn ?? '');
    DateTime? date = h?.date;
    final api = ref.read(hrApiProvider).holiday;
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: h == null ? s.t('عطلة جديدة', 'New holiday') : s.t('تعديل العطلة', 'Edit holiday'),
      fields: (setState) => [
        PickerField(
          label: s.date,
          value: date == null ? null : f.date(date),
          icon: Icons.event,
          onTap: () async {
            final d = await pickDate(context, initial: date ?? DateTime(_year, DateTime.now().month, DateTime.now().day));
            if (d != null) setState(() => date = d);
          },
        ),
        TextFormField(controller: ar, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Name (Arabic)')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'Name (English)')), validator: (v) => requiredValidator(context, v)),
        Text(s.t('العطلة ليوم واحد؛ العطلة الممتدة تُدخل بسجل لكل يوم.', 'One day per record; enter a record for each day of a longer holiday.'),
            style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () async {
        if (date == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => h == null
              ? api.Holiday_Create(idempotencyKey: key, body: HolidayWriteDto(date: date!, nameAr: ar.text.trim(), nameEn: en.text.trim()))
              : api.Holiday_Update(
                  id: h.id, body: HolidayUpdateDto(date: date!, nameAr: ar.text.trim(), nameEn: en.text.trim(), concurrencyStamp: h.concurrencyStamp)),
          done: s.saved,
        );
      },
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _delete(HolidayDto h) async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('حذف العطلة', 'Delete holiday'), message: h.name, destructive: true)) return;
    if (!mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).holiday.Holiday_Delete(id: h.id), done: s.t('تم الحذف', 'Deleted'));
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final canManage = ref.watch(sessionProvider).can(HrOps.Holiday_Create);
    final api = ref.watch(hrApiProvider).holiday;
    return HrPage(
      title: s.t('العطل والعطلة الأسبوعية', 'Holidays and weekly off'),
      actions: [
        if (canManage) FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('عطلة جديدة', 'New holiday'))),
      ],
      child: PagedList<HolidayDto>(
        key: ValueKey('$_year|$_version'),
        pageSize: 100,
        header: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          _WeeklyOffCard(canManage: ref.watch(sessionProvider).can(HrOps.WeeklyOffSetting_Update)),
          const SizedBox(height: 12),
          Row(children: [
            Expanded(child: SectionTitle(s.t('العطل الرسمية', 'Official holidays'))),
            IconButton(onPressed: () => setState(() => _year--), icon: const Icon(Icons.chevron_left)),
            Text('$_year', style: Theme.of(context).textTheme.titleMedium),
            IconButton(onPressed: () => setState(() => _year++), icon: const Icon(Icons.chevron_right)),
          ]),
        ]),
        fetch: (skip, max) async {
          final r = await api.Holiday_List(
              fromDate: DateTime(_year, 1, 1), toDate: DateTime(_year, 12, 31), sorting: 'date', skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, h) => HrCard(
          child: Row(children: [
            Icon(Icons.celebration_outlined, color: context.hr.accent),
            const SizedBox(width: 12),
            SizedBox(width: 160, child: Text('${f.weekday(h.date)} ${f.date(h.date)}')),
            Expanded(child: Text(h.name, style: const TextStyle(fontWeight: FontWeight.w600))),
            if (canManage) ...[
              IconButton(onPressed: () => _edit(h), icon: const Icon(Icons.edit_outlined)),
              IconButton(onPressed: () => _delete(h), icon: const Icon(Icons.delete_outline, color: HrColors.danger)),
            ],
          ]),
        ),
      ),
    );
  }
}

class _WeeklyOffCard extends ConsumerStatefulWidget {
  const _WeeklyOffCard({required this.canManage});
  final bool canManage;

  @override
  ConsumerState<_WeeklyOffCard> createState() => _WeeklyOffCardState();
}

class _WeeklyOffCardState extends ConsumerState<_WeeklyOffCard> {
  Set<WorkShiftDayOfWeek>? _days;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return AsyncBody<WeeklyOffSettingDto>(
      value: ref.watch(weeklyOffProvider),
      onRetry: () => ref.invalidate(weeklyOffProvider),
      builder: (w) {
        final days = _days ?? w.days.toSet();
        final changed = _days != null && !(_days!.length == w.days.length && _days!.containsAll(w.days));
        return HrCard(
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            SectionTitle(s.t('العطلة الأسبوعية في تقويم الشركة', 'Company weekly off')),
            Text(
              s.t('تحدد المواعيد الإدارية مثل آخر يوم دوام ومهل التقييم، ولا تغير قوالب الدوام.',
                  'Drives administrative deadlines; it does not change shift templates.'),
              style: Theme.of(context).textTheme.labelSmall,
            ),
            const SizedBox(height: 8),
            Wrap(spacing: 8, runSpacing: 8, children: [
              for (final d in workWeek)
                FilterChip(
                  label: Text(dayOfWeekLabel(s, d)),
                  selected: days.contains(d),
                  onSelected: widget.canManage ? (v) => setState(() => _days = {...days}..removeWhere((x) => !v && x == d)..addAll(v ? [d] : [])) : null,
                ),
            ]),
            if (changed) ...[
              const SizedBox(height: 8),
              Align(
                alignment: AlignmentDirectional.centerStart,
                child: FilledButton(
                  onPressed: () async {
                    final ok = await runAction(
                      context,
                      () => ref.read(hrApiProvider).holiday.WeeklyOffSetting_Update(
                          body: WeeklyOffSettingUpdateDto(days: days.toList(), concurrencyStamp: w.concurrencyStamp)),
                      done: s.saved,
                    );
                    if (ok) {
                      setState(() => _days = null);
                      ref.invalidate(weeklyOffProvider);
                    }
                  },
                  child: Text(s.save),
                ),
              ),
            ],
          ]),
        );
      },
    );
  }
}
