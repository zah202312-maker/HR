import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../l10n/strings.dart';
import '../theme/tokens.dart';
import 'motion_widgets.dart';

/// Page frame: title row, optional actions, bounded content width (no horizontal overflow).
class HrPage extends StatelessWidget {
  const HrPage({super.key, required this.title, required this.child, this.actions = const [], this.subtitle});

  final String title;
  final String? subtitle;
  final List<Widget> actions;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final wide = MediaQuery.sizeOf(context).width >= 768;
    return Align(
      alignment: AlignmentDirectional.topStart,
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 1280),
        child: Padding(
          padding: EdgeInsets.fromLTRB(wide ? 24 : 16, wide ? 24 : 12, wide ? 24 : 16, 0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(title, style: Theme.of(context).textTheme.headlineSmall),
                        if (subtitle != null)
                          Padding(
                            padding: const EdgeInsets.only(top: 4),
                            child: Text(subtitle!, style: TextStyle(color: context.hr.textSecondary)),
                          ),
                      ],
                    ),
                  ),
                  ...actions,
                ],
              ),
              const SizedBox(height: 16),
              Expanded(child: child),
            ],
          ),
        ),
      ),
    );
  }
}

class HrCard extends StatelessWidget {
  const HrCard({super.key, required this.child, this.padding = const EdgeInsets.all(16), this.onTap});

  final Widget child;
  final EdgeInsetsGeometry padding;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    return Container(
      decoration: BoxDecoration(
        color: c.surface,
        borderRadius: BorderRadius.circular(HrColors.radiusCard),
        border: Border.all(color: c.border),
        boxShadow: c.shadow,
      ),
      child: Material(
        type: MaterialType.transparency,
        child: InkWell(
          borderRadius: BorderRadius.circular(HrColors.radiusCard),
          onTap: onTap,
          child: Padding(padding: padding, child: child),
        ),
      ),
    );
  }
}

enum Tone { success, warning, danger, info, neutral }

/// Status pill: always a text label, never color alone (Design System 3.2).
class StatusPill extends StatelessWidget {
  const StatusPill(this.label, {super.key, this.tone = Tone.neutral, this.icon});

  final String label;
  final Tone tone;
  final IconData? icon;

  @override
  Widget build(BuildContext context) {
    final color = switch (tone) {
      Tone.success => HrColors.success,
      Tone.warning => HrColors.warning,
      Tone.danger => HrColors.danger,
      Tone.info => HrColors.info,
      Tone.neutral => context.hr.textSecondary,
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: .12),
        borderRadius: BorderRadius.circular(HrColors.radiusBadge),
        border: Border.all(color: color.withValues(alpha: .35)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[Icon(icon, size: 14, color: color), const SizedBox(width: 4)],
          Text(label,
              style: Theme.of(context).textTheme.labelSmall?.copyWith(
                    color: Theme.of(context).brightness == Brightness.dark ? color : Color.lerp(color, Colors.black, .35),
                  )),
        ],
      ),
    );
  }
}

class SectionTitle extends StatelessWidget {
  const SectionTitle(this.text, {super.key, this.trailing});
  final String text;
  final Widget? trailing;

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.only(bottom: 12, top: 8),
        child: Row(children: [
          Expanded(child: Text(text, style: Theme.of(context).textTheme.titleMedium)),
          if (trailing != null) trailing!,
        ]),
      );
}

/// Key/value row for detail screens.
class InfoRow extends StatelessWidget {
  const InfoRow(this.label, this.value, {super.key});
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.symmetric(vertical: 6),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(width: 150, child: Text(label, style: TextStyle(color: context.hr.textSecondary))),
            Expanded(child: SelectableText(value)),
          ],
        ),
      );
}

class KpiTile extends StatelessWidget {
  const KpiTile({super.key, required this.label, required this.value, this.icon, this.caption, this.onTap});
  final String label;
  final String value;
  final IconData? icon;
  final String? caption;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    return HrCard(
      onTap: onTap,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(children: [
            if (icon != null)
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(color: c.glow, borderRadius: BorderRadius.circular(10)),
                child: Icon(icon, size: 20, color: c.accent),
              ),
            const SizedBox(width: 10),
            Expanded(child: Text(label, style: TextStyle(color: c.textSecondary, fontWeight: FontWeight.w600))),
          ]),
          const SizedBox(height: 12),
          Text(value,
              style: Theme.of(context)
                  .textTheme
                  .headlineSmall
                  ?.copyWith(fontFeatures: const [FontFeature.tabularFigures()])),
          if (caption != null) Text(caption!, style: Theme.of(context).textTheme.labelSmall?.copyWith(color: c.textSecondary)),
        ],
      ),
    );
  }
}

class EmptyState extends StatelessWidget {
  const EmptyState({super.key, this.message, this.icon = Icons.inbox_outlined});
  final String? message;
  final IconData icon;

  @override
  Widget build(BuildContext context) => Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            Icon(icon, size: 48, color: context.hr.textSecondary),
            const SizedBox(height: 12),
            Text(message ?? S.of(context).empty, textAlign: TextAlign.center, style: TextStyle(color: context.hr.textSecondary)),
          ]),
        ),
      );
}

/// Full error card with retry (S-STATE-02).
class ErrorState extends StatelessWidget {
  const ErrorState({super.key, required this.error, this.onRetry});
  final Object error;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          const Icon(Icons.error_outline, size: 48, color: HrColors.danger),
          const SizedBox(height: 12),
          Text(s.error(error), textAlign: TextAlign.center),
          if (onRetry != null) ...[
            const SizedBox(height: 16),
            OutlinedButton.icon(onPressed: onRetry, icon: const Icon(Icons.refresh), label: Text(s.retry)),
          ],
        ]),
      ),
    );
  }
}

/// Renders an AsyncValue with loading, error+retry and data states.
class AsyncBody<T> extends StatelessWidget {
  const AsyncBody({super.key, required this.value, required this.builder, this.onRetry});
  final AsyncValue<T> value;
  final Widget Function(T data) builder;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) => value.when(
        data: builder,
        loading: () => const Center(child: Padding(padding: EdgeInsets.all(24), child: CircularProgressIndicator())),
        error: (e, _) => ErrorState(error: e, onRetry: onRetry),
      );
}

void showError(BuildContext context, Object error) {
  showHrToast(context, error is String ? error : S.of(context).error(error), icon: Icons.error_outline, color: HrColors.danger);
}

void showDone(BuildContext context, String message) {
  showHrToast(context, message);
}

/// Dialog asking for a text value (reason / note). Returns null when cancelled.
Future<String?> askText(BuildContext context,
    {required String title, required String label, bool mandatory = true, int maxLength = 2000}) {
  final controller = TextEditingController();
  final formKey = GlobalKey<FormState>();
  final s = S.of(context);
  return showHrDialog<String>(
    context: context,
    builder: (ctx) => AlertDialog(
      title: Text(title),
      content: SizedBox(
        width: 420,
        child: Form(
          key: formKey,
          child: TextFormField(
            controller: controller,
            autofocus: true,
            maxLines: 4,
            maxLength: maxLength,
            decoration: InputDecoration(labelText: label),
            validator: (v) => mandatory && (v == null || v.trim().isEmpty) ? s.requiredField : null,
          ),
        ),
      ),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.cancel)),
        FilledButton(
          onPressed: () {
            if (formKey.currentState!.validate()) Navigator.pop(ctx, controller.text.trim());
          },
          child: Text(s.confirm),
        ),
      ],
    ),
  );
}

/// Runs a contract call, shows the mapped error on failure and an optional done message.
/// Returns true on success.
Future<bool> runAction(BuildContext context, Future<void> Function() action, {String? done}) async {
  try {
    await action();
    if (context.mounted && done != null) showDone(context, done);
    return true;
  } catch (e) {
    if (context.mounted) showError(context, e);
    return false;
  }
}

/// Shows a modal form dialog; [onSubmit] returns true to close it.
Future<bool> showFormDialog(
  BuildContext context, {
  required String title,
  required List<Widget> Function(StateSetter setState) fields,
  required Future<bool> Function() onSubmit,
  String? submitLabel,
  double width = 520,
}) async {
  final formKey = GlobalKey<FormState>();
  final s = S.of(context);
  var busy = false;
  final result = await showHrDialog<bool>(
    context: context,
    barrierDismissible: false,
    builder: (ctx) => StatefulBuilder(
      builder: (ctx, setState) => AlertDialog(
        title: Text(title),
        content: SizedBox(
          width: width,
          child: Form(
            key: formKey,
            child: SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  for (final f in fields(setState)) Padding(padding: const EdgeInsets.only(bottom: 12), child: f),
                ],
              ),
            ),
          ),
        ),
        actions: [
          TextButton(onPressed: busy ? null : () => Navigator.pop(ctx, false), child: Text(s.cancel)),
          FilledButton(
            onPressed: busy
                ? null
                : () async {
                    if (!formKey.currentState!.validate()) return;
                    setState(() => busy = true);
                    final ok = await onSubmit();
                    if (!ctx.mounted) return;
                    setState(() => busy = false);
                    if (ok) Navigator.pop(ctx, true);
                  },
            child: Text(submitLabel ?? s.save),
          ),
        ],
      ),
    ),
  );
  return result ?? false;
}

String? requiredValidator(BuildContext context, String? v) =>
    (v == null || v.trim().isEmpty) ? S.of(context).requiredField : null;

/// Two-column responsive grid for detail/form fields.
class FieldGrid extends StatelessWidget {
  const FieldGrid({super.key, required this.children, this.minColumnWidth = 280});
  final List<Widget> children;
  final double minColumnWidth;

  @override
  Widget build(BuildContext context) => LayoutBuilder(builder: (context, c) {
        final cols = (c.maxWidth / minColumnWidth).floor().clamp(1, 2);
        final w = (c.maxWidth - 12 * (cols - 1)) / cols;
        return Wrap(spacing: 12, runSpacing: 12, children: [for (final x in children) SizedBox(width: w, child: x)]);
      });
}
