import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../ops/ops_shared.dart';

/// S-EVA: evaluation criteria and the verbal grade scale (systemAdmin). Changes apply from the next cycle.
class EvaluationAdminPage extends ConsumerStatefulWidget {
  const EvaluationAdminPage({super.key});

  @override
  ConsumerState<EvaluationAdminPage> createState() => _EvaluationAdminPageState();
}

class _EvaluationAdminPageState extends ConsumerState<EvaluationAdminPage> {
  int _version = 0;

  /// Shared editor for a criterion or a grade (grade has a code on create).
  Future<void> _edit({EvaluationCriterionDto? criterion, EvaluationGradeDto? grade, required bool isGrade}) async {
    final s = S.of(context);
    final code = TextEditingController(text: grade?.code);
    final ar = TextEditingController(text: criterion?.nameAr ?? grade?.nameAr);
    final en = TextEditingController(text: criterion?.nameEn ?? grade?.nameEn);
    final order = TextEditingController(text: '${criterion?.sortOrder ?? grade?.sortOrder ?? 0}');
    var active = criterion?.isActive ?? grade?.isActive ?? true;
    final isNew = criterion == null && grade == null;
    final api = ref.read(hrApiProvider).evaluationAdmin;
    final ok = await showFormDialog(
      context,
      title: isGrade ? s.t('درجة لفظية', 'Grade') : s.t('معيار تقييم', 'Criterion'),
      fields: (setState) => [
        if (isGrade)
          TextFormField(
            controller: code,
            enabled: isNew,
            decoration: InputDecoration(labelText: s.t('الرمز (camelCase)', 'Code (camelCase)')),
            validator: (v) => RegExp(r'^[a-z][A-Za-z0-9]*$').hasMatch(v ?? '') ? null : 'camelCase',
          ),
        TextFormField(controller: ar, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Arabic name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: en, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'English name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(
          controller: order,
          keyboardType: TextInputType.number,
          inputFormatters: [FilteringTextInputFormatter.digitsOnly],
          decoration: InputDecoration(labelText: s.t('الترتيب', 'Sort order')),
          validator: (v) => int.tryParse(v ?? '') == null ? s.requiredField : null,
        ),
        SwitchListTile(contentPadding: EdgeInsets.zero, value: active, title: Text(s.t('فعّال', 'Active')), onChanged: (v) => setState(() => active = v)),
        Text(s.t('يسري التغيير من الدورة التالية.', 'Applies from the next cycle.'), style: Theme.of(context).textTheme.labelSmall),
      ],
      onSubmit: () => runAction(context, () async {
        final sort = int.parse(order.text);
        if (isGrade) {
          if (grade == null) {
            await api.EvaluationGrade_Create(
                body: EvaluationGradeCreateDto(code: code.text.trim(), nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: sort, isActive: active));
          } else {
            await api.EvaluationGrade_Update(
                id: grade.id,
                body: EvaluationGradeUpdateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: sort, isActive: active, concurrencyStamp: grade.concurrencyStamp));
          }
        } else {
          if (criterion == null) {
            await api.EvaluationCriterion_Create(
                body: EvaluationCriterionCreateDto(nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: sort, isActive: active));
          } else {
            await api.EvaluationCriterion_Update(
                id: criterion.id,
                body: EvaluationCriterionUpdateDto(
                    nameAr: ar.text.trim(), nameEn: en.text.trim(), sortOrder: sort, isActive: active, concurrencyStamp: criterion.concurrencyStamp));
          }
        }
      }, done: s.saved),
    );
    if (ok) setState(() => _version++);
  }

  Widget _activePill(S s, bool active) =>
      StatusPill(active ? s.t('فعّال', 'Active') : s.t('موقوف', 'Inactive'), tone: active ? Tone.success : Tone.neutral);

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).evaluationAdmin;
    final tabs = <(String, Widget)>[
      if (session.can(HrOps.EvaluationCriterion_GetList))
        (
          s.t('المعايير', 'Criteria'),
          PagedList<EvaluationCriterionDto>(
            key: ValueKey('c|$_version'),
            pageSize: 100,
            header: Align(
              alignment: AlignmentDirectional.centerEnd,
              child: TextButton.icon(onPressed: () => _edit(isGrade: false), icon: const Icon(Icons.add), label: Text(s.t('معيار جديد', 'New criterion'))),
            ),
            fetch: (skip, max) async {
              final r = await api.EvaluationCriterion_GetList(skipCount: skip, maxResultCount: max, sorting: 'sortOrder');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, c) => OpsTile(
              leading: Icons.checklist,
              title: c.name,
              subtitle: '${c.nameAr} · ${c.nameEn} · #${c.sortOrder}',
              trailing: _activePill(s, c.isActive),
              onTap: () => _edit(criterion: c, isGrade: false),
            ),
          )
        ),
      if (session.can(HrOps.EvaluationGrade_GetList))
        (
          s.t('السلم اللفظي', 'Grade scale'),
          PagedList<EvaluationGradeDto>(
            key: ValueKey('g|$_version'),
            pageSize: 100,
            header: Align(
              alignment: AlignmentDirectional.centerEnd,
              child: TextButton.icon(onPressed: () => _edit(isGrade: true), icon: const Icon(Icons.add), label: Text(s.t('درجة جديدة', 'New grade'))),
            ),
            fetch: (skip, max) async {
              final r = await api.EvaluationGrade_GetList(skipCount: skip, maxResultCount: max, sorting: 'sortOrder');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, g) => OpsTile(
              leading: Icons.star_half,
              title: g.name,
              subtitle: '${g.code} · ${g.nameAr} · ${g.nameEn} · #${g.sortOrder}',
              trailing: _activePill(s, g.isActive),
              onTap: () => _edit(grade: g, isGrade: true),
            ),
          )
        ),
    ];
    return HrPage(title: s.t('معايير التقييم', 'Evaluation criteria'), child: tabbedBody(context, tabs));
  }
}
