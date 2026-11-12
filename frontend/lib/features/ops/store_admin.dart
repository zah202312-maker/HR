import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/util/download.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import '../org/org_providers.dart' show searchDepartments;
import 'ops_shared.dart';
import 'store_shared.dart';

// ---------------------------------------------------------------- dashboard

class StoreDashboardTab extends ConsumerWidget {
  const StoreDashboardTab({super.key, required this.version, required this.onOpenTab});
  final int version;
  final void Function(String tab) onOpenTab;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    return FutureBuilder<StoreDashboardDto>(
      key: ValueKey(version),
      future: ref.read(hrApiProvider).store.StoreDashboard_Get(),
      builder: (context, snap) {
        if (snap.hasError) return ErrorState(error: snap.error!);
        if (!snap.hasData) return const Center(child: CircularProgressIndicator());
        final d = snap.data!;
        final tiles = [
          KpiTile(label: s.t('عدد المواد', 'Items'), value: qty(d.itemsCount), icon: Icons.inventory_2_outlined, onTap: () => onOpenTab('items')),
          KpiTile(label: s.t('عمليات الشراء', 'Purchases'), value: qty(d.purchasesCount), icon: Icons.receipt_long_outlined, onTap: () => onOpenTab('purchases')),
          KpiTile(label: s.t('عمليات الصرف', 'Issues'), value: qty(d.issueVouchersCount), icon: Icons.outbox_outlined, onTap: () => onOpenTab('vouchers')),
          KpiTile(
              label: s.t('منخفضة المخزون', 'Low stock'),
              value: qty(d.lowStockCount),
              caption: '${s.t('نافدة', 'Out of stock')} ${qty(d.outOfStockCount)}',
              icon: Icons.warning_amber_outlined,
              onTap: () => onOpenTab('items')),
          KpiTile(label: s.t('إجمالي قيمة المخزون', 'Stock value'), value: money(d.totalStockValue, d.currency), icon: Icons.payments_outlined),
        ];
        return LayoutBuilder(builder: (context, c) {
          final columns = c.maxWidth >= 1000 ? 5 : (c.maxWidth >= 600 ? 3 : 2);
          return GridView.count(
            crossAxisCount: columns,
            childAspectRatio: 1.7,
            mainAxisSpacing: 12,
            crossAxisSpacing: 12,
            shrinkWrap: true,
            children: tiles,
          );
        });
      },
    );
  }
}

// ---------------------------------------------------------------- teams

class TeamsTab extends ConsumerStatefulWidget {
  const TeamsTab({super.key});

  @override
  ConsumerState<TeamsTab> createState() => _TeamsTabState();
}

class _TeamsTabState extends ConsumerState<TeamsTab> {
  int _version = 0;

  Future<void> _edit([StoreTeamDto? team]) async {
    final s = S.of(context);
    final name = TextEditingController(text: team?.name);
    DepartmentRefDto? department = team?.department;
    var active = team?.isActive ?? true;
    final key = const Uuid().v4();
    final api = ref.read(hrApiProvider).store;
    final ok = await showFormDialog(
      context,
      title: team == null ? s.t('فريق جديد', 'New team') : s.t('تعديل الفريق', 'Edit team'),
      fields: (setState) => [
        TextFormField(controller: name, maxLength: 128, decoration: InputDecoration(labelText: s.t('اسم الفريق', 'Team name')), validator: (v) => requiredValidator(context, v)),
        PickerField(
          label: s.t('القسم (اختياري، للعهد المشتركة)', 'Department (optional, for shared custody)'),
          value: department?.name,
          onClear: () => setState(() => department = null),
          onTap: () async {
            final x = await showSearchPicker<DepartmentRefDto>(context, title: s.department, search: (q) => searchDepartments(ref, q), label: (d) => d.name);
            if (x != null) setState(() => department = x);
          },
        ),
        if (team != null) SwitchListTile(contentPadding: EdgeInsets.zero, value: active, title: Text(s.t('مفعّل', 'Active')), onChanged: (v) => setState(() => active = v)),
      ],
      onSubmit: () => runAction(context, () async {
        if (team == null) {
          await api.StoreTeam_Create(idempotencyKey: key, body: StoreTeamCreateInputDto(name: name.text.trim(), departmentId: department?.id));
        } else {
          await api.StoreTeam_Update(
              id: team.id, body: StoreTeamUpdateInputDto(name: name.text.trim(), departmentId: department?.id, isActive: active, concurrencyStamp: team.concurrencyStamp));
        }
      }, done: s.saved),
    );
    if (ok) setState(() => _version++);
  }

  Future<void> _history(StoreTeamDto team) async {
    final s = S.of(context);
    final f = Fmt(context);
    await showHrDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('${s.t('ما استلمه', 'Received by')} ${team.name}'),
        content: SizedBox(
          width: 720,
          height: 480,
          child: PagedList<StockMovementDto>(
            fetch: (skip, max) async {
              final r = await ref.read(hrApiProvider).store.Stock_GetMovements(teamId: team.id, skipCount: skip, maxResultCount: max, sorting: 'creationTime desc');
              return (r.items, r.totalCount);
            },
            itemBuilder: (context, m) => OpsTile(
              leading: m.quantity < 0 ? Icons.north_east : Icons.south_west,
              title: '${m.item.name} · ${qty(-m.quantity)}',
              subtitle: [f.dateTime(m.creationTime), if (m.documentNumber != null) m.documentNumber!, if (m.employee != null) m.employee!.fullName].join(' · '),
            ),
          ),
        ),
        actions: [TextButton(onPressed: () => Navigator.pop(ctx), child: Text(s.t('إغلاق', 'Close')))],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    return PagedList<StoreTeamDto>(
      key: ValueKey('teams|$_version'),
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Align(
          alignment: AlignmentDirectional.centerStart,
          child: session.can(HrOps.StoreTeam_Create)
              ? FilledButton.icon(onPressed: () => _edit(), icon: const Icon(Icons.group_add_outlined), label: Text(s.t('فريق جديد', 'New team')))
              : const SizedBox.shrink(),
        ),
      ),
      fetch: (skip, max) async {
        final r = await ref.read(hrApiProvider).store.StoreTeam_GetList(skipCount: skip, maxResultCount: max);
        return (r.items, r.totalCount);
      },
      itemBuilder: (context, t) => OpsTile(
        leading: Icons.groups_outlined,
        title: t.name,
        subtitle: t.department?.name ?? s.t('بلا قسم', 'No department'),
        trailing: t.isActive ? null : StatusPill(s.t('موقوف', 'Inactive'), tone: Tone.neutral),
        onTap: session.can(HrOps.StoreTeam_Update) ? () => _edit(t) : null,
        actions: [TextButton.icon(onPressed: () => _history(t), icon: const Icon(Icons.history), label: Text(s.t('سجل المستلم', 'Received')))],
      ),
    );
  }
}

// ---------------------------------------------------------------- categories and units

class CatalogTab extends ConsumerStatefulWidget {
  const CatalogTab({super.key});

  @override
  ConsumerState<CatalogTab> createState() => _CatalogTabState();
}

class _CatalogTabState extends ConsumerState<CatalogTab> {
  int _version = 0;

  Future<void> _edit({required bool unit, String? id, String? ar, String? en, bool active = true, String? stamp}) async {
    final s = S.of(context);
    final arC = TextEditingController(text: ar);
    final enC = TextEditingController(text: en);
    var isActive = active;
    final key = const Uuid().v4();
    final api = ref.read(hrApiProvider).store;
    final ok = await showFormDialog(
      context,
      title: unit ? s.t('وحدة قياس', 'Unit of measure') : s.t('تصنيف', 'Category'),
      fields: (setState) => [
        TextFormField(controller: arC, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالعربية', 'Arabic name')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: enC, maxLength: 128, decoration: InputDecoration(labelText: s.t('الاسم بالإنجليزية', 'English name')), validator: (v) => requiredValidator(context, v)),
        if (id != null) SwitchListTile(contentPadding: EdgeInsets.zero, value: isActive, title: Text(s.t('مفعّل', 'Active')), onChanged: (v) => setState(() => isActive = v)),
      ],
      onSubmit: () => runAction(context, () async {
        final a = arC.text.trim(), e = enC.text.trim();
        if (unit) {
          id == null
              ? await api.ItemUnit_Create(idempotencyKey: key, body: ItemUnitCreateInputDto(nameAr: a, nameEn: e))
              : await api.ItemUnit_Update(id: id, body: ItemUnitUpdateInputDto(nameAr: a, nameEn: e, isActive: isActive, concurrencyStamp: stamp!));
        } else {
          id == null
              ? await api.ItemCategory_Create(idempotencyKey: key, body: ItemCategoryCreateInputDto(nameAr: a, nameEn: e))
              : await api.ItemCategory_Update(id: id, body: ItemCategoryUpdateInputDto(nameAr: a, nameEn: e, isActive: isActive, concurrencyStamp: stamp!));
        }
      }, done: s.saved),
    );
    if (ok) setState(() => _version++);
  }

  Widget _list<T>({required bool unit, required Future<(List<T>, int)> Function(int, int) fetch, required (String, String, String, bool, String) Function(T) row}) {
    final s = S.of(context);
    final canManage = ref.watch(sessionProvider).can(HrOps.ItemCategory_Create);
    return PagedList<T>(
      key: ValueKey('catalog|$unit|$_version'),
      header: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Align(
          alignment: AlignmentDirectional.centerStart,
          child: canManage
              ? FilledButton.tonalIcon(onPressed: () => _edit(unit: unit), icon: const Icon(Icons.add), label: Text(unit ? s.t('وحدة جديدة', 'New unit') : s.t('تصنيف جديد', 'New category')))
              : const SizedBox.shrink(),
        ),
      ),
      fetch: fetch,
      itemBuilder: (context, x) {
        final (id, ar, en, active, stamp) = row(x);
        return OpsTile(
          leading: unit ? Icons.straighten : Icons.category_outlined,
          title: catalogName(context, ar, en),
          subtitle: catalogName(context, en, ar),
          trailing: active ? null : StatusPill(s.t('موقوف', 'Inactive'), tone: Tone.neutral),
          onTap: canManage ? () => _edit(unit: unit, id: id, ar: ar, en: en, active: active, stamp: stamp) : null,
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final api = ref.watch(hrApiProvider).store;
    return tabbedBody(context, [
      (
        s.t('التصنيفات', 'Categories'),
        _list<ItemCategoryDto>(
          unit: false,
          fetch: (skip, max) async {
            final r = await api.ItemCategory_GetList(skipCount: skip, maxResultCount: max);
            return (r.items, r.totalCount);
          },
          row: (x) => (x.id, x.nameAr, x.nameEn, x.isActive, x.concurrencyStamp),
        )
      ),
      (
        s.t('وحدات القياس', 'Units'),
        _list<ItemUnitDto>(
          unit: true,
          fetch: (skip, max) async {
            final r = await api.ItemUnit_GetList(skipCount: skip, maxResultCount: max);
            return (r.items, r.totalCount);
          },
          row: (x) => (x.id, x.nameAr, x.nameEn, x.isActive, x.concurrencyStamp),
        )
      ),
    ]);
  }
}

// ---------------------------------------------------------------- reports

class StoreReportsTab extends ConsumerStatefulWidget {
  const StoreReportsTab({super.key});

  @override
  ConsumerState<StoreReportsTab> createState() => _StoreReportsTabState();
}

class _StoreReportsTabState extends ConsumerState<StoreReportsTab> {
  StoreReportCode _code = StoreReportCode.currentStock;
  DateTime _from = DateTime(DateTime.now().year, DateTime.now().month, 1);
  DateTime _to = DateTime.now();
  ItemDto? _item;
  StoreTeamDto? _team;
  int _version = 0;

  String _title(S s, StoreReportCode c) => switch (c) {
        StoreReportCode.purchases => s.t('المشتريات خلال فترة', 'Purchases in a period'),
        StoreReportCode.currentStock => s.t('المواد الموجودة حاليًا', 'Current stock'),
        StoreReportCode.lowStock => s.t('المواد منخفضة المخزون', 'Low stock'),
        StoreReportCode.outOfStock => s.t('المواد النافدة', 'Out of stock'),
        StoreReportCode.issuesByTeam => s.t('المصروف لكل فريق', 'Issued per team'),
        StoreReportCode.stockValue => s.t('إجمالي قيمة المخزون', 'Stock value'),
        StoreReportCode.itemMovement => s.t('حركة مادة خلال فترة', 'Item movement'),
        _ => '—',
      };

  bool get _needsPeriod => _code == StoreReportCode.purchases || _code == StoreReportCode.issuesByTeam || _code == StoreReportCode.itemMovement;

  Future<void> _export(ReportExportFormat format) async {
    final s = S.of(context);
    if (_code == StoreReportCode.itemMovement && _item == null) {
      showError(context, s.validation);
      return;
    }
    await runAction(context, () async {
      final file = await ref.read(hrApiProvider).store.StoreReport_Export(
            code: _code,
            format: format,
            from: _needsPeriod ? _from : null,
            to: _needsPeriod ? _to : null,
            itemId: _item?.id,
            teamId: _code == StoreReportCode.issuesByTeam ? _team?.id : null,
          );
      await saveBinary(file, fallbackName: '${_code.value}.${format == ReportExportFormat.pdf ? 'pdf' : 'xlsx'}');
    });
  }

  Future<Widget> _body() async {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.read(hrApiProvider).store;
    switch (_code) {
      case StoreReportCode.purchases:
        final r = await api.StoreReport_GetPurchases(from: _from, to: _to, itemId: _item?.id, maxResultCount: 200);
        return StoreTable(
          columns: [s.t('رقم الوصل', 'Receipt'), s.t('المورد', 'Supplier'), s.t('التاريخ', 'Date'), s.t('الرمز', 'SKU'), s.item, s.quantity, s.t('سعر الوحدة', 'Unit price'), s.t('المجموع', 'Total')],
          rows: [
            for (final x in r.items)
              [Text(x.receiptNumber), Text(x.supplierName), Text(f.date(x.purchaseDate)), Text(x.sku), Text(x.item.name), Text(qty(x.quantity)), Text(money(x.unitPrice, r.currency)), Text(money(x.lineTotal, r.currency))],
          ],
          footer: [Text('${r.purchasesCount} ${s.t('وصل', 'receipts')}'), const Text(''), const Text(''), const Text(''), const Text(''), Text(qty(r.totalQuantity)), const Text(''), Text(money(r.totalAmount, r.currency))],
        );
      case StoreReportCode.issuesByTeam:
        final r = await api.StoreReport_GetIssuesByTeam(from: _from, to: _to, teamId: _team?.id, itemId: _item?.id, maxResultCount: 200);
        return StoreTable(
          columns: [s.t('الفريق', 'Team'), s.t('الرمز', 'SKU'), s.item, s.t('الوحدة', 'Unit'), s.quantity, s.t('القيمة', 'Value')],
          rows: [for (final x in r.items) [Text(x.team.name), Text(x.sku), Text(x.item.name), Text(x.unit?.name ?? '—'), Text(qty(x.quantity)), Text(money(x.value, r.currency))]],
          footer: [Text(s.t('الإجمالي', 'Total')), const Text(''), const Text(''), const Text(''), Text(qty(r.totalQuantity)), Text(money(r.totalValue, r.currency))],
        );
      case StoreReportCode.itemMovement:
        if (_item == null) return EmptyState(message: s.t('اختر مادة.', 'Choose an item.'));
        final r = await api.StoreReport_GetItemMovement(itemId: _item!.id, from: _from, to: _to, maxResultCount: 200);
        return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Wrap(spacing: 24, children: [
            InfoRow(s.t('رصيد البداية', 'Opening'), qty(r.openingBalance)),
            InfoRow(s.t('الداخل', 'In'), qty(r.totalIn)),
            InfoRow(s.t('الخارج', 'Out'), qty(r.totalOut)),
            InfoRow(s.t('رصيد النهاية', 'Closing'), qty(r.closingBalance)),
          ]),
          StoreTable(
            columns: [s.t('التاريخ', 'Date'), s.t('النوع', 'Type'), s.t('داخل', 'In'), s.t('خارج', 'Out'), s.t('الرصيد', 'Balance'), s.t('الوصل أو السند', 'Document'), s.t('الفريق', 'Team'), s.t('المنفذ', 'By')],
            rows: [
              for (final m in r.items)
                [
                  Text(f.dateTime(m.creationTime)),
                  Text(storeMovementTypeLabel(s, m.type)),
                  Text(m.quantity > 0 ? qty(m.quantity) : ''),
                  Text(m.quantity < 0 ? qty(-m.quantity) : ''),
                  Text(qty(m.balanceAfter)),
                  Text(m.documentNumber ?? '—'),
                  Text(m.team?.name ?? '—'),
                  Text(m.performedBy.displayName),
                ],
            ],
          ),
        ]);
      default:
        final status = switch (_code) { StoreReportCode.lowStock => StockStatus.lowStock, StoreReportCode.outOfStock => StockStatus.outOfStock, _ => null };
        final r = await api.StoreReport_GetStock(stockStatus: status, maxResultCount: 200);
        return StoreTable(
          columns: [s.t('الرمز', 'SKU'), s.item, s.t('التصنيف', 'Category'), s.t('الوحدة', 'Unit'), s.t('الرصيد', 'On hand'), s.t('الحد الأدنى', 'Min'), s.t('متوسط السعر', 'Avg cost'), s.t('القيمة', 'Value'), s.status],
          rows: [
            for (final x in r.items)
              [
                Text(x.sku),
                Text(x.item.name),
                Text(x.category?.name ?? '—'),
                Text(x.unit?.name ?? '—'),
                Text(qty(x.quantityOnHand)),
                Text(qty(x.minQuantity)),
                Text(money(x.averageUnitCost, r.currency)),
                Text(money(x.stockValue, r.currency)),
                StatusPill(stockStatusLabel(s, x.stockStatus), tone: stockStatusTone(x.stockStatus)),
              ],
          ],
          footer: [Text('${r.itemsCount} ${s.t('مادة', 'items')}'), const Text(''), const Text(''), const Text(''), const Text(''), const Text(''), const Text(''), Text(money(r.totalStockValue, r.currency)), const Text('')],
        );
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final canExport = ref.watch(sessionProvider).can(HrOps.StoreReport_Export);
    return ListView(children: [
      Wrap(spacing: 12, runSpacing: 8, crossAxisAlignment: WrapCrossAlignment.center, children: [
        SizedBox(
          width: 260,
          child: DropdownButtonFormField<StoreReportCode>(
            initialValue: _code,
            isExpanded: true,
            decoration: InputDecoration(labelText: s.t('التقرير', 'Report'), isDense: true),
            items: [for (final c in StoreReportCode.values.where((x) => x.value.isNotEmpty)) DropdownMenuItem(value: c, child: Text(_title(s, c)))],
            onChanged: (v) => setState(() => _code = v ?? _code),
          ),
        ),
        if (_needsPeriod) ...[
          SizedBox(
            width: 160,
            child: PickerField(label: s.t('من', 'From'), value: dateText(_from), icon: Icons.event, onTap: () async {
              final d = await pickDate(context, initial: _from);
              if (d != null) setState(() => _from = d);
            }),
          ),
          SizedBox(
            width: 160,
            child: PickerField(label: s.t('إلى', 'To'), value: dateText(_to), icon: Icons.event, onTap: () async {
              final d = await pickDate(context, initial: _to);
              if (d != null) setState(() => _to = d);
            }),
          ),
        ],
        if (_code == StoreReportCode.purchases || _code == StoreReportCode.issuesByTeam || _code == StoreReportCode.itemMovement)
          SizedBox(
            width: 240,
            child: PickerField(
              label: s.item,
              value: _item == null ? null : '${_item!.sku} · ${itemName(context, _item!)}',
              onClear: () => setState(() => _item = null),
              onTap: () async {
                final x = await showSearchPicker<ItemDto>(context, title: s.item, search: (q) => searchStoreItems(ref, q), label: (i) => '${i.sku} · ${itemName(context, i)}');
                if (x != null) setState(() => _item = x);
              },
            ),
          ),
        if (_code == StoreReportCode.issuesByTeam)
          SizedBox(
            width: 220,
            child: PickerField(
              label: s.t('الفريق', 'Team'),
              value: _team?.name,
              onClear: () => setState(() => _team = null),
              onTap: () async {
                final x = await showSearchPicker<StoreTeamDto>(context, title: s.t('الفريق', 'Team'), search: (q) => searchTeams(ref, q), label: (t) => t.name);
                if (x != null) setState(() => _team = x);
              },
            ),
          ),
        FilledButton.tonalIcon(onPressed: () => setState(() => _version++), icon: const Icon(Icons.refresh), label: Text(s.t('عرض', 'Show'))),
        if (canExport) ...[
          OutlinedButton.icon(onPressed: () => _export(ReportExportFormat.excel), icon: const Icon(Icons.grid_on), label: const Text('Excel')),
          OutlinedButton.icon(onPressed: () => _export(ReportExportFormat.pdf), icon: const Icon(Icons.picture_as_pdf_outlined), label: const Text('PDF')),
        ],
      ]),
      const SizedBox(height: 16),
      FutureBuilder<Widget>(
        key: ValueKey('$_code|$_from|$_to|${_item?.id}|${_team?.id}|$_version'),
        future: _body(),
        builder: (context, snap) => snap.hasError
            ? ErrorState(error: snap.error!)
            : snap.hasData
                ? snap.data!
                : const Padding(padding: EdgeInsets.all(32), child: Center(child: CircularProgressIndicator())),
      ),
    ]);
  }
}

String storeMovementTypeLabel(S s, StockMovementType t) => switch (t) {
      StockMovementType.receive => s.t('إدخال', 'Receive'),
      StockMovementType.purchase => s.t('شراء', 'Purchase'),
      StockMovementType.issue => s.t('صرف', 'Issue'),
      StockMovementType.return_ => s.t('إرجاع', 'Return'),
      StockMovementType.adjustment => s.t('تسوية', 'Adjustment'),
      _ => '—',
    };
