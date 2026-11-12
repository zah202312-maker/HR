import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../ops/ops_shared.dart';
import '../org/org_providers.dart' show searchEmployees;

/// One appreciation letter tile: download, and acknowledge for its recipient.
class AppreciationTile extends ConsumerWidget {
  const AppreciationTile({super.key, required this.letter, this.mine = false, this.onChanged});
  final AppreciationLetterDto letter;
  final bool mine;
  final VoidCallback? onChanged;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final f = Fmt(context);
    final l = letter;
    final api = ref.read(hrApiProvider).appreciationLetters;
    return OpsTile(
      leading: Icons.workspace_premium_outlined,
      avatar: mine ? null : PersonAvatar.employee(l.employee),
      title: mine ? l.title : '${l.employee.fullName} · ${l.title}',
      subtitle: [l.issuedBy.displayName, f.date(l.issuedAt), if (l.note != null) l.note!].join(' · '),
      trailing: StatusPill(l.isAcknowledged ? s.t('مستلم', 'Acknowledged') : s.t('بانتظار الاستلام', 'Not acknowledged'),
          tone: l.isAcknowledged ? Tone.success : Tone.warning),
      actions: [
        TextButton.icon(
          onPressed: () => runAction(context, () async => openDownload(await api.AppreciationLetter_GetDownloadUrl(id: l.id))),
          icon: const Icon(Icons.download),
          label: Text(l.file.fileName),
        ),
        if (mine && !l.isAcknowledged && ref.watch(sessionProvider).can(HrOps.AppreciationLetter_Acknowledge))
          FilledButton.tonal(
            onPressed: () async {
              if (await runAction(context, () => api.AppreciationLetter_Acknowledge(id: l.id), done: s.done)) onChanged?.call();
            },
            child: Text(s.t('تأكيد الاستلام', 'Acknowledge')),
          ),
      ],
    );
  }
}

/// S-APL: appreciation letters — mine, all (HR / executive director) and issuing (executive director).
class AppreciationLettersPage extends ConsumerStatefulWidget {
  const AppreciationLettersPage({super.key});

  @override
  ConsumerState<AppreciationLettersPage> createState() => _AppreciationLettersPageState();
}

class _AppreciationLettersPageState extends ConsumerState<AppreciationLettersPage> {
  int _version = 0;

  Future<void> _issue() async {
    final s = S.of(context);
    EmployeeRefDto? employee;
    final title = TextEditingController();
    final note = TextEditingController();
    PlatformFile? file;
    final key = const Uuid().v4();
    final ok = await showFormDialog(
      context,
      title: s.t('إصدار كتاب شكر', 'Issue appreciation letter'),
      submitLabel: s.t('إصدار', 'Issue'),
      fields: (setState) => [
        PickerField(
          label: s.employee,
          value: employee?.fullName,
          onTap: () async {
            final x = await showSearchPicker<EmployeeRefDto>(context,
                title: s.employee, search: (q) => searchEmployees(ref, q), label: (e) => e.fullName, subtitle: (e) => e.departmentName ?? '');
            if (x != null) setState(() => employee = x);
          },
        ),
        TextFormField(controller: title, maxLength: 200, decoration: InputDecoration(labelText: s.t('العنوان', 'Title')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: note, maxLines: 3, maxLength: 2000, decoration: InputDecoration(labelText: s.note)),
        OutlinedButton.icon(
          onPressed: () async {
            final picked = await pickFiles();
            if (picked.isNotEmpty) setState(() => file = picked.first);
          },
          icon: const Icon(Icons.attach_file),
          label: Text(file?.name ?? s.t('ملف الكتاب', 'Letter file')),
        ),
      ],
      onSubmit: () async {
        if (employee == null || file == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).appreciationLetters.AppreciationLetter_Create(
                idempotencyKey: key,
                body: AppreciationLetterCreateInputDto(employeeId: employee!.id, title: title.text.trim(), note: optText(note), file: multipartOf(file!)),
              ),
          done: s.t('صدر الكتاب وأُشعر الموظف', 'Issued; the employee was notified'),
        );
      },
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).appreciationLetters;
    final tabs = <(String, Widget)>[
      if (session.can(HrOps.AppreciationLetter_GetMyList))
        (
          s.t('كتبي', 'Mine'),
          PagedList<AppreciationLetterDto>(
            key: ValueKey('my|$_version'),
            fetch: (skip, max) async {
              final r = await api.AppreciationLetter_GetMyList(skipCount: skip, maxResultCount: max);
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, l) => AppreciationTile(letter: l, mine: true, onChanged: () => setState(() => _version++)),
          )
        ),
      if (session.can(HrOps.AppreciationLetter_List))
        (
          s.t('كل الكتب', 'All letters'),
          PagedList<AppreciationLetterDto>(
            key: ValueKey('all|$_version'),
            pageSize: 50,
            fetch: (skip, max) async {
              final r = await api.AppreciationLetter_List(skipCount: skip, maxResultCount: max, sorting: 'issuedAt desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, l) => AppreciationTile(letter: l),
          )
        ),
    ];
    return HrPage(
      title: s.t('كتب الشكر', 'Appreciation letters'),
      actions: [
        if (session.can(HrOps.AppreciationLetter_Create))
          FilledButton.icon(onPressed: _issue, icon: const Icon(Icons.add), label: Text(s.t('كتاب شكر', 'New letter'))),
      ],
      child: tabbedBody(context, tabs),
    );
  }
}
