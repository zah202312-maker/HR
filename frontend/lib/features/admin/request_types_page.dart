import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';

String destinationLabel(S s, RequestDestination d) => switch (d) {
      RequestDestination.none => s.t('بلا وجهة (موافقات فقط)', 'None (approvals only)'),
      RequestDestination.store => s.t('المخزن', 'Store'),
      RequestDestination.accountant => s.t('المحاسب', 'Accountant'),
      _ => d.value,
    };

/// S-SET request types (systemAdmin): list, create, edit. System types keep their destination.
class RequestTypesPage extends ConsumerStatefulWidget {
  const RequestTypesPage({super.key});

  @override
  ConsumerState<RequestTypesPage> createState() => _RequestTypesPageState();
}

class _RequestTypesPageState extends ConsumerState<RequestTypesPage> {
  int _version = 0;
  bool? _active;

  Future<void> _edit([RequestTypeAdminDto? t]) async {
    final s = S.of(context);
    final code = TextEditingController(text: t?.code);
    final nameAr = TextEditingController(text: t?.nameAr);
    final nameEn = TextEditingController(text: t?.nameEn);
    var destination = t?.destination ?? RequestDestination.none;
    var active = t?.isActive ?? true;
    final api = ref.read(hrApiProvider).requests;
    final ok = await showFormDialog(
      context,
      title: t == null ? s.t('نوع طلب جديد', 'New request type') : s.t('تعديل نوع الطلب', 'Edit request type'),
      fields: (setState) => [
        TextFormField(
          controller: code,
          enabled: t == null,
          decoration: InputDecoration(labelText: s.t('الرمز (camelCase)', 'Code (camelCase)')),
          validator: (v) => RegExp(r'^[a-z][A-Za-z0-9]{1,63}$').hasMatch(v ?? '') ? null : s.t('حروف إنجليزية بصيغة camelCase', 'camelCase English letters'),
        ),
        TextFormField(controller: nameAr, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Arabic name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: nameEn, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'English name')), validator: (v) => requiredValidator(context, v)),
        DropdownButtonFormField<RequestDestination>(
          initialValue: destination,
          decoration: InputDecoration(
            labelText: s.t('الوجهة بعد الموافقة', 'Destination after approval'),
            helperText: t?.isSystem == true ? s.t('نوع نظامي: الوجهة مقفلة', 'System type: destination locked') : null,
          ),
          items: [
            for (final d in RequestDestination.values.where((d) => d != RequestDestination.$unknown))
              DropdownMenuItem(value: d, child: Text(destinationLabel(s, d))),
          ],
          onChanged: t?.isSystem == true ? null : (v) => setState(() => destination = v ?? destination),
        ),
        SwitchListTile(
          contentPadding: EdgeInsets.zero,
          value: active,
          title: Text(s.t('مفعّل', 'Active')),
          onChanged: (v) => setState(() => active = v),
        ),
      ],
      onSubmit: () => runAction(context, () async {
        if (t == null) {
          await api.RequestType_Create(
            body: RequestTypeCreateInputDto(
                code: code.text.trim(), nameAr: nameAr.text.trim(), nameEn: nameEn.text.trim(), destination: destination, isActive: active),
          );
        } else {
          await api.RequestType_Update(
            id: t.id,
            body: RequestTypeUpdateInputDto(
                nameAr: nameAr.text.trim(), nameEn: nameEn.text.trim(), destination: destination, isActive: active, concurrencyStamp: t.concurrencyStamp),
          );
        }
      }, done: s.saved),
    );
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final canManage = session.can(HrOps.RequestType_Create);
    final api = ref.watch(hrApiProvider).requests;
    final ar = Localizations.localeOf(context).languageCode == 'ar';
    return HrPage(
      title: s.t('أنواع الطلبات', 'Request types'),
      actions: [
        if (canManage) FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.add), label: Text(s.t('نوع جديد', 'New type'))),
      ],
      child: PagedList<RequestTypeAdminDto>(
        key: ValueKey('$_version|$_active'),
        pageSize: 50,
        header: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: SegmentedButton<bool?>(
            segments: [
              ButtonSegment(value: null, label: Text(s.all)),
              ButtonSegment(value: true, label: Text(s.t('المفعّلة', 'Active'))),
              ButtonSegment(value: false, label: Text(s.t('الموقوفة', 'Inactive'))),
            ],
            selected: {_active},
            onSelectionChanged: (v) => setState(() => _active = v.first),
          ),
        ),
        fetch: (skip, max) async {
          final r = await api.RequestType_GetAdminList(skipCount: skip, maxResultCount: max, isActive: _active);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, t) => Padding(
          padding: const EdgeInsets.only(bottom: 8),
          child: HrCard(
            onTap: session.can(HrOps.RequestType_Update) ? () => _edit(t) : null,
            child: Row(children: [
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(ar ? t.nameAr : t.nameEn, style: const TextStyle(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 2),
                  Text('${t.code} · ${destinationLabel(s, t.destination)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                ]),
              ),
              if (t.isSystem) ...[StatusPill(s.t('نظامي', 'System'), tone: Tone.info), const SizedBox(width: 6)],
              StatusPill(t.isActive ? s.t('مفعّل', 'Active') : s.t('موقوف', 'Inactive'), tone: t.isActive ? Tone.success : Tone.neutral),
            ]),
          ),
        ),
      ),
    );
  }
}
