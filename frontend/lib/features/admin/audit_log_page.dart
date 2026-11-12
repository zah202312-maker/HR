import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';

String auditActionLabel(S s, AuditActionType a) => switch (a) {
      AuditActionType.create => s.t('إنشاء', 'Create'),
      AuditActionType.update => s.t('تعديل', 'Update'),
      AuditActionType.delete => s.t('حذف', 'Delete'),
      AuditActionType.read => s.t('قراءة', 'Read'),
      AuditActionType.export => s.t('تصدير', 'Export'),
      AuditActionType.login => s.t('تسجيل دخول', 'Sign-in'),
      AuditActionType.loginFailed => s.t('دخول فاشل', 'Failed sign-in'),
      AuditActionType.passwordReset => s.t('إعادة تعيين كلمة المرور', 'Password reset'),
      AuditActionType.roleChanged => s.t('تغيير دور', 'Role changed'),
      AuditActionType.permissionChanged => s.t('تغيير صلاحية', 'Permission changed'),
      AuditActionType.settingChanged => s.t('تغيير إعداد', 'Setting changed'),
      AuditActionType.complaintHandlerAssignment => s.t('تعيين معالج شكاوى', 'Complaint handler assignment'),
      _ => a.value,
    };

/// S-AUD-01: read-only audit log with filters (systemAdmin, executiveDirector).
class AuditLogPage extends ConsumerStatefulWidget {
  const AuditLogPage({super.key});

  @override
  ConsumerState<AuditLogPage> createState() => _AuditLogPageState();
}

class _AuditLogPageState extends ConsumerState<AuditLogPage> {
  DateTime? _from = DateTime.now().subtract(const Duration(days: 7));
  DateTime? _to;
  AuditActionType? _action;
  bool? _succeeded;
  int _version = 0;

  void _apply() => setState(() => _version++);

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).auditLog;
    return HrPage(
      title: s.t('سجل التدقيق', 'Audit log'),
      child: PagedList<AuditLogEntryDto>(
        key: ValueKey(_version),
        pageSize: 50,
        header: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Wrap(spacing: 12, runSpacing: 12, crossAxisAlignment: WrapCrossAlignment.center, children: [
            SizedBox(
              width: 170,
              child: PickerField(label: s.from, value: _from == null ? null : f.date(_from), icon: Icons.event, onClear: () => setState(() => _from = null), onTap: () async {
                final x = await pickDate(context, initial: _from);
                if (x != null) setState(() => _from = x);
              }),
            ),
            SizedBox(
              width: 170,
              child: PickerField(label: s.to, value: _to == null ? null : f.date(_to), icon: Icons.event, onClear: () => setState(() => _to = null), onTap: () async {
                final x = await pickDate(context, initial: _to);
                if (x != null) setState(() => _to = x);
              }),
            ),
            SizedBox(
              width: 220,
              child: DropdownButtonFormField<AuditActionType?>(
                initialValue: _action,
                isExpanded: true,
                decoration: InputDecoration(labelText: s.t('الإجراء', 'Action')),
                items: [
                  DropdownMenuItem(value: null, child: Text(s.all)),
                  for (final a in AuditActionType.values.where((a) => a != AuditActionType.$unknown))
                    DropdownMenuItem(value: a, child: Text(auditActionLabel(s, a))),
                ],
                onChanged: (v) => setState(() => _action = v),
              ),
            ),
            SizedBox(
              width: 160,
              child: DropdownButtonFormField<bool?>(
                initialValue: _succeeded,
                isExpanded: true,
                decoration: InputDecoration(labelText: s.t('النتيجة', 'Result')),
                items: [
                  DropdownMenuItem(value: null, child: Text(s.all)),
                  DropdownMenuItem(value: true, child: Text(s.t('نجح', 'Succeeded'))),
                  DropdownMenuItem(value: false, child: Text(s.t('فشل', 'Failed'))),
                ],
                onChanged: (v) => setState(() => _succeeded = v),
              ),
            ),
            FilledButton.icon(onPressed: _apply, icon: const Icon(Icons.search), label: Text(s.t('بحث', 'Search'))),
          ]),
        ),
        fetch: (skip, max) async {
          final r = await api.AuditLog_List(
            skipCount: skip,
            maxResultCount: max,
            from: _from,
            // inclusive end of the chosen day
            to: _to == null ? null : DateTime(_to!.year, _to!.month, _to!.day, 23, 59, 59),
            action: _action,
            succeeded: _succeeded,
          );
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, e) => Padding(
          padding: const EdgeInsets.only(bottom: 8),
          child: HrCard(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Row(children: [
                StatusPill(auditActionLabel(s, e.action), tone: e.succeeded ? Tone.info : Tone.danger),
                const SizedBox(width: 8),
                Expanded(child: Text(e.user?.displayName ?? s.t('النظام', 'System'), style: const TextStyle(fontWeight: FontWeight.w600))),
                Text(f.dateTime(e.occurredAt), style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
              ]),
              if (e.changedFields.isNotEmpty)
                Padding(
                  padding: const EdgeInsets.only(top: 6),
                  child: Wrap(spacing: 6, runSpacing: 6, children: [for (final c in e.changedFields) Chip(label: Text(c), visualDensity: VisualDensity.compact)]),
                ),
              if (e.ipAddress != null)
                Padding(padding: const EdgeInsets.only(top: 6), child: Text('IP ${e.ipAddress}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12))),
            ]),
          ),
        ),
      ),
    );
  }
}
