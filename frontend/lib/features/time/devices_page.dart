import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:uuid/uuid.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/paged_list.dart';
import '../../core/widgets/pickers.dart';
import 'time_shared.dart';

/// S-ATT-07: attendance devices. The key is shown once on create and on rotation.
class DevicesPage extends ConsumerStatefulWidget {
  const DevicesPage({super.key});

  @override
  ConsumerState<DevicesPage> createState() => _DevicesPageState();
}

class _DevicesPageState extends ConsumerState<DevicesPage> {
  int _version = 0;

  Future<void> _create() async {
    final s = S.of(context);
    final serial = TextEditingController();
    final name = TextEditingController();
    final key = const Uuid().v4();
    AttendanceDeviceKeyDto? created;
    final ok = await showFormDialog(
      context,
      title: s.t('تسجيل جهاز بصمة', 'Register device'),
      fields: (_) => [
        TextFormField(controller: serial, textDirection: TextDirection.ltr, decoration: InputDecoration(labelText: s.t('الرقم التسلسلي', 'Serial number')), validator: (v) => requiredValidator(context, v)),
        TextFormField(controller: name, decoration: InputDecoration(labelText: s.t('الاسم', 'Name')), validator: (v) => requiredValidator(context, v)),
      ],
      onSubmit: () => runAction(context, () async {
        created = await ref.read(hrApiProvider).attendanceDevice.AttendanceDevice_Create(
            idempotencyKey: key, body: AttendanceDeviceCreateDto(serialNumber: serial.text.trim(), name: name.text.trim()));
      }),
    );
    if (ok && created != null && mounted) {
      await showSecretOnce(context, title: s.t('مفتاح الجهاز', 'Device key'), secret: created!.deviceKey);
      setState(() => _version++);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final api = ref.watch(hrApiProvider).attendanceDevice;
    final session = ref.watch(sessionProvider);
    return HrPage(
      title: s.t('أجهزة البصمة', 'Attendance devices'),
      actions: [
        if (session.can(HrOps.AttendanceDevice_Create))
          FilledButton.icon(onPressed: _create, icon: const Icon(Icons.add), label: Text(s.t('جهاز جديد', 'New device'))),
      ],
      child: PagedList<AttendanceDeviceDto>(
        key: ValueKey(_version),
        fetch: (skip, max) async {
          final r = await api.AttendanceDevice_List(skipCount: skip, maxResultCount: max);
          return (r.items, r.totalCount);
        },
        itemBuilder: (context, d) => HrCard(
          onTap: () => context.go('/devices/${d.id}'),
          child: Row(children: [
            Icon(Icons.fingerprint, color: context.hr.accent, size: 32),
            const SizedBox(width: 12),
            Expanded(
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(d.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                Text('${d.serialNumber} · ${s.t('آخر اتصال', 'Last seen')}: ${f.dateTime(d.lastSeenAt)}', style: TextStyle(color: context.hr.textSecondary, fontSize: 12)),
              ]),
            ),
            StatusPill('${d.mappedUserCount} ${s.t('مربوط', 'mapped')}', tone: Tone.info),
            const SizedBox(width: 6),
            if (d.unmappedUserCount > 0) ...[StatusPill('${d.unmappedUserCount} ${s.t('غير مربوط', 'unmapped')}', tone: Tone.warning), const SizedBox(width: 6)],
            StatusPill(d.isActive ? s.t('مفعّل', 'Active') : s.t('معطّل', 'Inactive'), tone: d.isActive ? Tone.success : Tone.neutral),
          ]),
        ),
      ),
    );
  }
}

/// Device detail: edit/disable, rotate key, user map (DeviceUserMap_*), unmapped users.
class DeviceDetailPage extends ConsumerStatefulWidget {
  const DeviceDetailPage({super.key, required this.id});
  final String id;

  @override
  ConsumerState<DeviceDetailPage> createState() => _DeviceDetailPageState();
}

class _DeviceDetailPageState extends ConsumerState<DeviceDetailPage> {
  int _version = 0;
  AttendanceDeviceDto? _device;

  @override
  void initState() {
    super.initState();
    _loadDevice();
  }

  /// The contract has no AttendanceDevice_Get; the device comes from the list.
  Future<void> _loadDevice() async {
    try {
      final r = await ref.read(hrApiProvider).attendanceDevice.AttendanceDevice_List(maxResultCount: 200);
      final match = r.items.where((d) => d.id == widget.id);
      if (mounted) setState(() => _device = match.isEmpty ? null : match.first);
    } catch (e) {
      if (mounted) showError(context, e);
    }
  }

  Future<void> _edit(AttendanceDeviceDto d) async {
    final s = S.of(context);
    final name = TextEditingController(text: d.name);
    var active = d.isActive;
    final ok = await showFormDialog(
      context,
      title: s.t('تعديل الجهاز', 'Edit device'),
      fields: (setState) => [
        TextFormField(controller: name, decoration: InputDecoration(labelText: s.t('الاسم', 'Name')), validator: (v) => requiredValidator(context, v)),
        SwitchListTile(contentPadding: EdgeInsets.zero, title: Text(s.t('مفعّل', 'Active')), value: active, onChanged: (v) => setState(() => active = v)),
      ],
      onSubmit: () => runAction(
        context,
        () => ref.read(hrApiProvider).attendanceDevice.AttendanceDevice_Update(
            id: d.id, body: AttendanceDeviceUpdateDto(name: name.text.trim(), isActive: active, concurrencyStamp: d.concurrencyStamp)),
        done: s.saved,
      ),
    );
    if (ok) _loadDevice();
  }

  Future<void> _rotate(AttendanceDeviceDto d) async {
    final s = S.of(context);
    if (!await confirmAction(context,
        title: s.t('إصدار مفتاح جديد', 'Rotate key'),
        message: s.t('يبطل المفتاح الحالي فوراً، ويتوقف الجهاز حتى تضعي المفتاح الجديد فيه.', 'The current key stops working immediately until the new key is set on the device.'),
        destructive: true)) {
      return;
    }
    if (!mounted) return;
    AttendanceDeviceKeyDto? k;
    final ok = await runAction(context, () async => k = await ref.read(hrApiProvider).attendanceDevice.AttendanceDevice_RotateKey(id: d.id));
    if (ok && k != null && mounted) await showSecretOnce(context, title: s.t('المفتاح الجديد', 'New key'), secret: k!.deviceKey);
  }

  Future<void> _map({String? deviceUserId}) async {
    final s = S.of(context);
    final userId = TextEditingController(text: deviceUserId ?? '');
    EmployeeRefDto? employee;
    final ok = await showFormDialog(
      context,
      title: s.t('ربط مستخدم الجهاز بموظف', 'Map device user to employee'),
      fields: (setState) => [
        TextFormField(
          controller: userId,
          maxLength: 64,
          textDirection: TextDirection.ltr,
          decoration: InputDecoration(labelText: s.t('رقم المستخدم في الجهاز', 'Device user id')),
          validator: (v) => requiredValidator(context, v),
        ),
        PickerField(
          label: s.employee,
          value: employee?.fullName,
          onTap: () async {
            final e = await pickEmployee(context, ref);
            if (e != null) setState(() => employee = e);
          },
        ),
      ],
      onSubmit: () async {
        if (employee == null) {
          showError(context, s.validation);
          return false;
        }
        return runAction(
          context,
          () => ref.read(hrApiProvider).attendanceDevice.DeviceUserMap_Create(
              id: widget.id, body: DeviceUserMapCreateDto(deviceUserId: userId.text.trim(), employeeId: employee!.id)),
          done: s.t('تم الربط، ويعاد حساب أيام الموظف في الخلفية', 'Mapped; the employee days are recalculated in the background'),
        );
      },
    );
    if (ok) {
      setState(() => _version++);
      _loadDevice();
    }
  }

  Future<void> _unmap(DeviceUserMapDto m) async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('حذف الربط', 'Remove mapping'), message: '${m.deviceUserId} → ${m.employee.fullName}', destructive: true)) return;
    if (!mounted) return;
    final ok = await runAction(context, () => ref.read(hrApiProvider).attendanceDevice.DeviceUserMap_Delete(id: widget.id, mapId: m.id), done: s.t('تم الحذف', 'Deleted'));
    if (ok) setState(() => _version++);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final session = ref.watch(sessionProvider);
    final api = ref.watch(hrApiProvider).attendanceDevice;
    final d = _device;
    final canManage = session.can(HrOps.AttendanceDevice_Update);
    return HrPage(
      title: d?.name ?? s.t('جهاز', 'Device'),
      subtitle: d?.serialNumber,
      actions: [
        if (d != null && canManage) IconButton(tooltip: s.t('تعديل', 'Edit'), onPressed: () => _edit(d), icon: const Icon(Icons.edit_outlined)),
        if (d != null && session.can(HrOps.AttendanceDevice_RotateKey))
          IconButton(tooltip: s.t('مفتاح جديد', 'Rotate key'), onPressed: () => _rotate(d), icon: const Icon(Icons.key)),
      ],
      child: DefaultTabController(
        length: 2,
        child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          TabBar(isScrollable: true, tabAlignment: TabAlignment.start, tabs: [
            Tab(text: s.t('المستخدمون المربوطون', 'Mapped users')),
            Tab(text: s.t('غير المربوطين', 'Unmapped users')),
          ]),
          const SizedBox(height: 12),
          Expanded(
            child: TabBarView(children: [
              PagedList<DeviceUserMapDto>(
                key: ValueKey('map|$_version'),
                header: Align(
                  alignment: AlignmentDirectional.centerStart,
                  child: FilledButton.icon(onPressed: () => _map(), icon: const Icon(Icons.link), label: Text(s.t('ربط', 'Map'))),
                ),
                fetch: (skip, max) async {
                  final r = await api.DeviceUserMap_List(id: widget.id, skipCount: skip, maxResultCount: max);
                  return (r.items, r.totalCount);
                },
                itemBuilder: (context, m) => HrCard(
                  child: Row(children: [
                    SizedBox(width: 120, child: Directionality(textDirection: TextDirection.ltr, child: Text(m.deviceUserId, style: const TextStyle(fontFamily: 'monospace')))),
                    Expanded(child: Text([m.employee.fullName, if (m.employee.departmentName != null) m.employee.departmentName!].join(' · '))),
                    IconButton(onPressed: () => _unmap(m), icon: const Icon(Icons.link_off, color: HrColors.danger)),
                  ]),
                ),
              ),
              PagedList<UnmappedDeviceUserDto>(
                key: ValueKey('unmapped|$_version'),
                emptyMessage: s.t('كل مستخدمي الجهاز مربوطون', 'All device users are mapped'),
                fetch: (skip, max) async {
                  final r = await api.DeviceUserMap_ListUnmapped(id: widget.id, skipCount: skip, maxResultCount: max);
                  return (r.items, r.totalCount);
                },
                itemBuilder: (context, u) => HrCard(
                  child: Row(children: [
                    SizedBox(width: 120, child: Directionality(textDirection: TextDirection.ltr, child: Text(u.deviceUserId, style: const TextStyle(fontFamily: 'monospace')))),
                    Expanded(child: Text('${u.punchCount} ${s.t('عملية', 'punches')} · ${f.dateTime(u.firstPunchTime)} → ${f.dateTime(u.lastPunchTime)}')),
                    FilledButton.tonal(onPressed: () => _map(deviceUserId: u.deviceUserId), child: Text(s.t('ربط', 'Map'))),
                  ]),
                ),
              ),
            ]),
          ),
        ]),
      ),
    );
  }
}
