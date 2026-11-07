import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';

final requestTypesProvider = FutureProvider.autoDispose<List<RequestTypeDto>>((ref) async {
  final r = await ref.watch(hrApiProvider).requests.RequestType_GetList(maxResultCount: 200);
  return r.items;
});

final itemOptionsProvider = FutureProvider.autoDispose<List<ItemRefDto>>((ref) async {
  final r = await ref.watch(hrApiProvider).store.Item_Lookup(maxResultCount: 200);
  return r.items;
});

/// Request_Submit. The server builds the approval route; the client never picks it.
/// Leave and punch correction types use their own endpoints (usesDedicatedEndpoint).
class NewRequestPage extends ConsumerStatefulWidget {
  const NewRequestPage({super.key});

  @override
  ConsumerState<NewRequestPage> createState() => _NewRequestPageState();
}

class _NewRequestPageState extends ConsumerState<NewRequestPage> {
  final _form = GlobalKey<FormState>();
  final _description = TextEditingController();
  final _quantity = TextEditingController();
  RequestTypeDto? _type;
  ItemRefDto? _item;
  bool _busy = false;
  // One key per form instance: a double tap or retry does not create two requests.
  final String _idempotencyKey = const Uuid().v4();

  @override
  void dispose() {
    _description.dispose();
    _quantity.dispose();
    super.dispose();
  }

  void _onType(RequestTypeDto? t) {
    if (t == null) return;
    if (t.usesDedicatedEndpoint) {
      if (t.code == 'leave') context.go('/leaves/new');
      if (t.code == 'attendanceCorrection') context.go('/attendance/correction');
      return;
    }
    setState(() {
      _type = t;
      _item = null;
    });
  }

  Future<void> _submit() async {
    final type = _type;
    if (type == null || !_form.currentState!.validate()) return;
    setState(() => _busy = true);
    try {
      final r = await ref.read(hrApiProvider).requests.Request_Submit(
            idempotencyKey: _idempotencyKey,
            body: RequestSubmitInputDto(
              requestTypeId: type.id,
              description: _description.text.trim(),
              itemId: type.requiresItem ? _item?.id : null,
              quantity: type.requiresItem ? double.tryParse(_quantity.text.trim()) : null,
            ),
          );
      if (!mounted) return;
      showDone(context, S.of(context).submitted);
      context.go('/requests/${r.id}');
    } catch (e) {
      if (mounted) showError(context, e);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final types = ref.watch(requestTypesProvider);
    final canItems = ref.watch(sessionProvider).can(HrOps.Item_Lookup);
    return HrPage(
      title: s.newRequest,
      child: AsyncBody<List<RequestTypeDto>>(
        value: types,
        onRetry: () => ref.invalidate(requestTypesProvider),
        builder: (list) => SingleChildScrollView(
          padding: const EdgeInsets.only(bottom: 24),
          child: Align(
            alignment: AlignmentDirectional.topStart,
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 640),
              child: HrCard(
                child: Form(
                  key: _form,
                  child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                    DropdownButtonFormField<RequestTypeDto>(
                      initialValue: _type,
                      isExpanded: true,
                      decoration: InputDecoration(labelText: s.requestType),
                      items: [
                        for (final t in list)
                          DropdownMenuItem(
                            value: t,
                            child: Text(t.usesDedicatedEndpoint ? '${t.name} ↗' : t.name, overflow: TextOverflow.ellipsis),
                          ),
                      ],
                      onChanged: _onType,
                      validator: (v) => v == null ? s.requiredField : null,
                    ),
                    if (_type?.requiresItem ?? false) ...[
                      const SizedBox(height: 12),
                      if (canItems)
                        Consumer(builder: (context, ref, _) {
                          final items = ref.watch(itemOptionsProvider);
                          return items.when(
                            loading: () => const LinearProgressIndicator(),
                            error: (e, _) => Text(s.error(e), style: const TextStyle(color: HrColors.danger)),
                            data: (options) => DropdownButtonFormField<ItemRefDto>(
                              initialValue: _item,
                              isExpanded: true,
                              decoration: InputDecoration(labelText: s.item),
                              items: [for (final i in options) DropdownMenuItem(value: i, child: Text(i.name))],
                              onChanged: (v) => setState(() => _item = v),
                              validator: (v) => v == null ? s.requiredField : null,
                            ),
                          );
                        })
                      else
                        Text(s.forbidden, style: const TextStyle(color: HrColors.danger)),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _quantity,
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        decoration: InputDecoration(labelText: s.quantity),
                        validator: (v) {
                          final q = double.tryParse((v ?? '').trim());
                          return q == null || q <= 0 ? s.requiredField : null;
                        },
                      ),
                    ],
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _description,
                      maxLines: 5,
                      maxLength: 4000,
                      decoration: InputDecoration(labelText: s.description, alignLabelWithHint: true),
                      validator: (v) => (v == null || v.trim().isEmpty) ? s.requiredField : null,
                    ),
                    const SizedBox(height: 12),
                    Text(s.dedicatedFlow, style: Theme.of(context).textTheme.labelSmall?.copyWith(color: context.hr.textSecondary)),
                    const SizedBox(height: 16),
                    FilledButton(onPressed: _busy ? null : _submit, child: Text(s.submit)),
                  ]),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
