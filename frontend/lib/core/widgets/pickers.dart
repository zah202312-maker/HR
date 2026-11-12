import 'dart:async';

import 'package:flutter/material.dart';

import '../../api/generated/hr_api.dart';
import '../l10n/strings.dart';
import '../theme/tokens.dart';
import 'common.dart';
import 'motion_widgets.dart';
import 'person_avatar.dart';

/// Searchable picker dialog over a contract lookup operation (server-side search).
Future<T?> showSearchPicker<T>(
  BuildContext context, {
  required String title,
  required Future<List<T>> Function(String query) search,
  required String Function(T item) label,
  String Function(T item)? subtitle,
}) {
  return showHrDialog<T>(
    context: context,
    builder: (_) => _SearchPickerDialog<T>(title: title, search: search, label: label, subtitle: subtitle),
  );
}

class _SearchPickerDialog<T> extends StatefulWidget {
  const _SearchPickerDialog({required this.title, required this.search, required this.label, this.subtitle});
  final String title;
  final Future<List<T>> Function(String query) search;
  final String Function(T item) label;
  final String Function(T item)? subtitle;

  @override
  State<_SearchPickerDialog<T>> createState() => _SearchPickerDialogState<T>();
}

class _SearchPickerDialogState<T> extends State<_SearchPickerDialog<T>> {
  final _query = TextEditingController();
  Timer? _debounce;
  List<T> _items = <T>[];
  bool _loading = false;
  Object? _error;

  @override
  void initState() {
    super.initState();
    _run('');
  }

  @override
  void dispose() {
    _debounce?.cancel();
    _query.dispose();
    super.dispose();
  }

  Future<void> _run(String q) async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final items = await widget.search(q.trim());
      if (mounted) setState(() => _items = items);
    } catch (e) {
      if (mounted) setState(() => _error = e);
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return AlertDialog(
      title: Text(widget.title),
      content: SizedBox(
        width: 460,
        height: 420,
        child: Column(children: [
          TextField(
            controller: _query,
            autofocus: true,
            decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: s.t('بحث...', 'Search...')),
            onChanged: (v) {
              _debounce?.cancel();
              _debounce = Timer(const Duration(milliseconds: 350), () => _run(v));
            },
          ),
          const SizedBox(height: 8),
          if (_loading) const LinearProgressIndicator(),
          Expanded(
            child: _error != null
                ? Center(child: Text(s.error(_error!), style: const TextStyle(color: HrColors.danger)))
                : _items.isEmpty && !_loading
                    ? const EmptyState()
                    : ListView.builder(
                        itemCount: _items.length,
                        itemBuilder: (context, i) {
                          final item = _items[i];
                          return ListTile(
                            // A person shows with their photo next to the name (decision 2026-11-11).
                            leading: switch (item) {
                              EmployeeRefDto e => PersonAvatar.employee(e),
                              UserRefDto u => PersonAvatar.user(u),
                              _ => null,
                            },
                            title: Text(widget.label(item)),
                            subtitle: widget.subtitle == null ? null : Text(widget.subtitle!(item)),
                            onTap: () => Navigator.pop(context, item),
                          );
                        },
                      ),
          ),
        ]),
      ),
      actions: [TextButton(onPressed: () => Navigator.pop(context), child: Text(s.cancel))],
    );
  }
}

/// Read-only field that opens a picker when tapped.
class PickerField extends StatelessWidget {
  const PickerField({super.key, required this.label, required this.value, required this.onTap, this.onClear, this.errorText, this.icon = Icons.search});
  final String label;
  final String? value;
  final VoidCallback onTap;
  final VoidCallback? onClear;
  final String? errorText;
  final IconData icon;

  @override
  Widget build(BuildContext context) => InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(HrColors.radiusControl),
        child: InputDecorator(
          decoration: InputDecoration(
            labelText: label,
            errorText: errorText,
            suffixIcon: value != null && onClear != null
                ? IconButton(icon: const Icon(Icons.clear), onPressed: onClear)
                : Icon(icon),
          ),
          child: Text(value ?? '—'),
        ),
      );
}

Future<DateTime?> pickDate(BuildContext context, {DateTime? initial, DateTime? first, DateTime? last}) {
  final now = DateTime.now();
  return showDatePicker(
    context: context,
    initialDate: initial ?? now,
    firstDate: first ?? DateTime(1940),
    lastDate: last ?? DateTime(now.year + 5),
  );
}

Future<DateTime?> pickDateTime(BuildContext context, {DateTime? initial}) async {
  final d = await pickDate(context, initial: initial, first: DateTime.now().subtract(const Duration(days: 1)));
  if (d == null || !context.mounted) return null;
  final t = await showTimePicker(
    context: context,
    initialTime: TimeOfDay.fromDateTime(initial ?? DateTime.now()),
    builder: (context, child) => MediaQuery(
      data: MediaQuery.of(context).copyWith(alwaysUse24HourFormat: true),
      child: child!,
    ),
  );
  if (t == null) return null;
  return DateTime(d.year, d.month, d.day, t.hour, t.minute);
}

/// Yes/No confirmation. Returns true when confirmed.
Future<bool> confirmAction(BuildContext context, {required String title, required String message, bool destructive = false}) async {
  final s = S.of(context);
  final ok = await showHrDialog<bool>(
    context: context,
    builder: (ctx) => AlertDialog(
      title: Text(title),
      content: Text(message),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(s.cancel)),
        FilledButton(
          style: destructive ? FilledButton.styleFrom(backgroundColor: HrColors.danger, foregroundColor: Colors.white) : null,
          onPressed: () => Navigator.pop(ctx, true),
          child: Text(s.confirm),
        ),
      ],
    ),
  );
  return ok ?? false;
}
