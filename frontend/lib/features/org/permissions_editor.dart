import 'package:flutter/material.dart';

import '../../api/generated/models.dart';
import '../../core/l10n/strings.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';

/// Edits ordinary permissions (PermissionListResultDto). Only the direct grant can change;
/// permissions inherited from roles show as checked and locked. Only changes are sent.
class PermissionsEditor extends StatefulWidget {
  const PermissionsEditor({super.key, required this.data, required this.onSave, required this.readOnly});
  final PermissionListResultDto data;
  final Future<bool> Function(List<PermissionGrantChangeDto> changes) onSave;
  final bool readOnly;

  @override
  State<PermissionsEditor> createState() => _PermissionsEditorState();
}

class _PermissionsEditorState extends State<PermissionsEditor> {
  final Map<String, bool> _changes = <String, bool>{};
  bool _busy = false;
  String _filter = '';

  bool _direct(PermissionGrantInfoDto p) => _changes[p.name] ?? p.isDirectlyGranted;

  void _toggle(PermissionGrantInfoDto p, bool v) {
    setState(() {
      if (v == p.isDirectlyGranted) {
        _changes.remove(p.name);
      } else {
        _changes[p.name] = v;
      }
    });
  }

  Future<void> _save() async {
    setState(() => _busy = true);
    final ok = await widget.onSave([for (final e in _changes.entries) PermissionGrantChangeDto(name: e.key, isGranted: e.value)]);
    if (!mounted) return;
    setState(() {
      _busy = false;
      if (ok) _changes.clear();
    });
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final q = _filter.toLowerCase();
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      Row(children: [
        Expanded(
          child: TextField(
            decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: s.t('ابحث عن صلاحية', 'Search permissions')),
            onChanged: (v) => setState(() => _filter = v),
          ),
        ),
        const SizedBox(width: 12),
        if (!widget.readOnly)
          FilledButton.icon(
            onPressed: _busy || _changes.isEmpty ? null : _save,
            icon: const Icon(Icons.save),
            label: Text('${s.save}${_changes.isEmpty ? '' : ' (${_changes.length})'}'),
          ),
      ]),
      const SizedBox(height: 8),
      Text(
        s.t('صلاحيات الشكاوى لا تظهر ولا تُمنح من هنا، والصلاحيات المالية لا تُمنح لمدير القسم ولا لمسؤول المخزن.',
            'Complaint permissions never appear or get granted here; financial permissions cannot go to department managers or store officers.'),
        style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary),
      ),
      const SizedBox(height: 8),
      Expanded(
        child: ListView(children: [
          for (final g in widget.data.groups)
            if (g.permissions.any((p) => q.isEmpty || p.displayName.toLowerCase().contains(q) || p.name.toLowerCase().contains(q)))
              Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: HrCard(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  child: ExpansionTile(
                    initiallyExpanded: q.isNotEmpty,
                    title: Text(g.displayName, style: const TextStyle(fontWeight: FontWeight.w600)),
                    children: [
                      for (final p in g.permissions)
                        if (q.isEmpty || p.displayName.toLowerCase().contains(q) || p.name.toLowerCase().contains(q))
                          CheckboxListTile(
                            dense: true,
                            contentPadding: EdgeInsetsDirectional.only(start: p.parentName == null ? 8 : 32),
                            value: _direct(p) || p.grantedByRoleNames.isNotEmpty,
                            onChanged: widget.readOnly || (p.grantedByRoleNames.isNotEmpty && !p.isDirectlyGranted)
                                ? null
                                : (v) => _toggle(p, v ?? false),
                            title: Text(p.displayName),
                            subtitle: p.grantedByRoleNames.isEmpty
                                ? null
                                : Text('${s.t('من الدور', 'From role')}: ${p.grantedByRoleNames.join('، ')}',
                                    style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                            secondary: _changes.containsKey(p.name) ? const Icon(Icons.edit, size: 16, color: HrColors.warning) : null,
                          ),
                    ],
                  ),
                ),
              ),
        ]),
      ),
    ]);
  }
}
