import 'package:dio/dio.dart' show DioMediaType, MultipartFile;
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';

/// Tabs for the visible parts of a screen. One tab shows without a tab bar; none shows "forbidden".
Widget tabbedBody(BuildContext context, List<(String, Widget)> tabs) {
  if (tabs.isEmpty) return EmptyState(message: S.of(context).forbidden);
  if (tabs.length == 1) return tabs.first.$2;
  return DefaultTabController(
    length: tabs.length,
    child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [for (final t in tabs) Tab(text: t.$1)]),
      const SizedBox(height: 12),
      Expanded(child: TabBarView(children: [for (final t in tabs) t.$2])),
    ]),
  );
}

/// Quantity / amount text without trailing zeros.
String qty(num? v) => v == null ? '—' : (v % 1 == 0 ? v.toInt().toString() : v.toStringAsFixed(2));

/// Decimal input with optional sign and bounds.
Widget numberField(BuildContext context, TextEditingController c, String label,
    {bool required = true, bool signed = false, double? min, double? max, bool positive = false, String? helper, bool enabled = true}) {
  final s = S.of(context);
  return TextFormField(
    controller: c,
    enabled: enabled,
    keyboardType: TextInputType.numberWithOptions(decimal: true, signed: signed),
    inputFormatters: [FilteringTextInputFormatter.allow(RegExp(signed ? r'[0-9.\-]' : r'[0-9.]'))],
    decoration: InputDecoration(labelText: label, helperText: helper),
    validator: (v) {
      if ((v ?? '').trim().isEmpty) return required ? s.requiredField : null;
      final n = double.tryParse(v!.trim());
      if (n == null) return s.validation;
      if (positive && n <= 0) return '> 0';
      if (min != null && n < min) return '≥ ${qty(min)}';
      if (max != null && n > max) return '≤ ${qty(max)}';
      return null;
    },
  );
}

double? parseNum(TextEditingController c) => double.tryParse(c.text.trim());
String? optText(TextEditingController c) => c.text.trim().isEmpty ? null : c.text.trim();

Future<List<ItemRefDto>> searchItems(WidgetRef ref, String q, {ItemKind? kind}) async =>
    (await ref.read(hrApiProvider).store.Item_Lookup(filter: q.isEmpty ? null : q, kind: kind, maxResultCount: 50)).items;

/// Small two-line list tile used by the release 2/3 lists.
class OpsTile extends StatelessWidget {
  const OpsTile({super.key, required this.title, this.subtitle, this.trailing, this.leading, this.onTap, this.actions = const []});
  final String title;
  final String? subtitle;
  final Widget? trailing;
  final IconData? leading;
  final VoidCallback? onTap;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.only(bottom: 8),
        child: HrCard(
          onTap: onTap,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            Row(children: [
              if (leading != null) ...[Icon(leading, color: context.hr.accent), const SizedBox(width: 12)],
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(title, style: const TextStyle(fontWeight: FontWeight.w600)),
                  if (subtitle != null && subtitle!.isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(top: 2),
                      child: Text(subtitle!, style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
                    ),
                ]),
              ),
              if (trailing != null) trailing!,
            ]),
            if (actions.isNotEmpty)
              Padding(
                padding: const EdgeInsets.only(top: 8),
                child: Wrap(spacing: 8, runSpacing: 8, alignment: WrapAlignment.end, children: actions),
              ),
          ]),
        ),
      );
}

/// Enum dropdown filter with an "all" entry.
Widget enumFilter<T extends Enum>(BuildContext context,
    {required String label, required T? value, required List<T> values, required String Function(T) text, required ValueChanged<T?> onChanged, double width = 200}) {
  final s = S.of(context);
  return SizedBox(
    width: width,
    child: DropdownButtonFormField<T?>(
      initialValue: value,
      isExpanded: true,
      decoration: InputDecoration(labelText: label, isDense: true),
      items: [
        DropdownMenuItem<T?>(value: null, child: Text(s.all)),
        for (final v in values.where((v) => v.name != r'$unknown')) DropdownMenuItem<T?>(value: v, child: Text(text(v))),
      ],
      onChanged: onChanged,
    ),
  );
}

/// Multipart part from a picked file, with a content type from its extension.
MultipartFile multipartOf(PlatformFile f) {
  final n = f.name.toLowerCase();
  final type = switch (n.split('.').last) {
    'pdf' => DioMediaType('application', 'pdf'),
    'png' => DioMediaType('image', 'png'),
    'jpg' || 'jpeg' => DioMediaType('image', 'jpeg'),
    'doc' => DioMediaType('application', 'msword'),
    'docx' => DioMediaType('application', 'vnd.openxmlformats-officedocument.wordprocessingml.document'),
    'xlsx' => DioMediaType('application', 'vnd.openxmlformats-officedocument.spreadsheetml.sheet'),
    _ => DioMediaType('application', 'octet-stream'),
  };
  return MultipartFile.fromBytes(f.bytes!, filename: f.name, contentType: type);
}

/// Picks one or more files with their bytes (web and mobile).
Future<List<PlatformFile>> pickFiles({bool multiple = false}) async {
  final r = await FilePicker.platform.pickFiles(withData: true, allowMultiple: multiple);
  return r == null ? const [] : r.files.where((f) => f.bytes != null).toList();
}

/// Opens a pre-signed download URL (sent without the Authorization header).
Future<void> openDownload(FileDownloadUrlDto link) => launchUrl(Uri.parse(link.url), mode: LaunchMode.externalApplication);
