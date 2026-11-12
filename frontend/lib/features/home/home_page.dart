import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/hr_api.dart';
import '../../core/l10n/locale_controller.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/theme/motion.dart';
import '../../core/theme/tokens.dart';
import '../../core/util/format.dart';
import '../../core/widgets/motion_widgets.dart';
import '../notifications/unread_count.dart';
import 'widgets/charts.dart';
import 'widgets/hero.dart';
import 'widgets/kpi_card.dart';

final inboxSummaryProvider = FutureProvider.autoDispose<InboxSummaryDto>(
  (ref) => ref.watch(hrApiProvider).currentUser.Me_GetInboxSummary(),
);

/// Open complaints waiting for me as a handler, and how many of them are late. Counts only, never content.
/// `null` = not a complaint handler (the card is hidden).
final myOpenComplaintsProvider = FutureProvider.autoDispose<({int open, int late})?>((ref) async {
  final session = ref.watch(sessionProvider);
  if (!session.can(HrOps.Complaint_GetAssignedList)) return null;
  final api = ref.watch(hrApiProvider).complaints;
  final all = await api.Complaint_GetAssignedList(maxResultCount: 1);
  final closed = await api.Complaint_GetAssignedList(status: ComplaintStatus.closed, maxResultCount: 1);
  final late = await api.Complaint_GetAssignedList(isLate: true, maxResultCount: 1);
  return (open: all.totalCount - closed.totalCount, late: late.totalCount);
});

/// Today's attendance of the people I oversee: the whole company for the HR manager
/// (Report_GetHrAttendance), my team for a department manager (Attendance_GetTeam).
/// `null` = neither (the row is hidden). `present` includes the late ones.
final teamTodayProvider = FutureProvider.autoDispose<({int present, int late, int absent, double onLeave})?>((ref) async {
  final session = ref.watch(sessionProvider);
  final api = ref.watch(hrApiProvider);
  final now = DateTime.now();
  final today = DateTime(now.year, now.month, now.day);
  // Executive director: company-wide counts only (decision 2026-11-11).
  if (session.can(HrOps.Report_GetTodayAttendanceSummary)) {
    final r = await api.reports.Report_GetTodayAttendanceSummary();
    return (present: r.present, late: r.late, absent: r.absent, onLeave: r.onLeave);
  }
  if (session.can(HrOps.Report_GetHrAttendance)) {
    var present = 0, late = 0, absent = 0;
    var onLeave = 0.0;
    const page = 200;
    for (var skip = 0;; skip += page) {
      final r = await api.reports.Report_GetHrAttendance(from: today, to: today, skipCount: skip, maxResultCount: page);
      for (final row in r.items) {
        present += row.presentDays;
        late += row.lateCount;
        absent += row.absentDays;
        onLeave += row.leaveDays;
      }
      if (r.items.isEmpty || skip + page >= r.totalCount) break;
    }
    return (present: present, late: late, absent: absent, onLeave: onLeave);
  }
  if (session.can(HrOps.Attendance_GetTeam)) {
    final r = await api.attendance.Attendance_GetTeam(fromDate: today, toDate: today, maxResultCount: 1);
    return (present: r.summary.presentDays, late: r.summary.lateDays, absent: r.summary.absentDays, onLeave: r.summary.leaveDays);
  }
  return null;
});

final myBalanceProvider = FutureProvider.autoDispose<LeaveBalanceDto?>((ref) async {
  final session = ref.watch(sessionProvider);
  if (!session.can(HrOps.LeaveBalance_GetMy) || session.isOutsideSchedule) return null;
  return ref.watch(hrApiProvider).leaveBalance.LeaveBalance_GetMy();
});

/// This month's own attendance (days in date order + the period summary).
final myMonthAttendanceProvider = FutureProvider.autoDispose<AttendanceDayPagedResultDto?>((ref) async {
  final session = ref.watch(sessionProvider);
  if (!session.can(HrOps.Attendance_GetMy) || session.isOutsideSchedule) return null;
  final now = DateTime.now();
  return ref.watch(hrApiProvider).attendance.Attendance_GetMy(
        fromDate: DateTime(now.year, now.month, 1),
        toDate: now,
        maxResultCount: 62,
        sorting: 'date asc',
      );
});

/// Everything the dashboard shows, already reduced from contract DTOs. `null` = still loading,
/// `available == false` = the user has no access to that block (it is hidden).
class DashboardData {
  const DashboardData({
    required this.displayName,
    required this.hasTeam,
    this.pending,
    this.pendingTotal,
    this.leaveAvailable = false,
    this.leave,
    this.attendanceAvailable = false,
    this.attendance,
    this.unread,
    this.complaintsAvailable = false,
    this.complaints,
    this.teamAvailable = false,
    this.teamToday,
    this.canNewRequest = false,
    this.canNewLeave = false,
    this.executiveLayout = false,
    this.preview = false,
  });

  final String displayName;
  final bool hasTeam;
  final List<(InboxCategory category, int count, int delegated)>? pending;
  final int? pendingTotal;
  final bool leaveAvailable;
  final ({double remaining, double used, double held, double entitlement, bool isSet})? leave;
  final bool attendanceAvailable;
  final ({int present, int lateDays, int absent, double leaveDays, int incomplete, int lateMinutes, double workedHours, List<double> hoursByDay, List<double> lateByDay, List<DateTime> days})? attendance;
  final int? unread;
  final bool complaintsAvailable;
  final ({int open, int late})? complaints;
  final bool teamAvailable;
  final ({int present, int late, int absent, double onLeave})? teamToday;
  final bool canNewRequest;
  final bool canNewLeave;

  /// Executive director (decision 2026-11-11): only pending, complaints, present today and on leave today.
  final bool executiveLayout;
  final bool preview;
}

/// S-HOME-01: the main dashboard (Bento): hero with the organizational network, items awaiting
/// my action, KPIs with trend lines, and charts. Contract-backed numbers only.
class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(sessionProvider);
    final me = session.me;
    final inbox = ref.watch(inboxSummaryProvider);
    final balance = ref.watch(myBalanceProvider);
    final month = ref.watch(myMonthAttendanceProvider);
    final unread = ref.watch(unreadCountProvider).valueOrNull;
    final complaints = ref.watch(myOpenComplaintsProvider).valueOrNull;
    final teamToday = ref.watch(teamTodayProvider).valueOrNull;

    final b = balance.valueOrNull;
    final m = month.valueOrNull;
    final data = DashboardData(
      displayName: me?.displayName ?? '',
      hasTeam: (me?.directReportCount ?? 0) > 0,
      pending: inbox.valueOrNull?.items.map((i) => (i.category, i.count, i.delegatedCount)).toList(),
      pendingTotal: inbox.valueOrNull?.totalCount,
      leaveAvailable: session.can(HrOps.LeaveBalance_GetMy) && !session.isOutsideSchedule,
      leave: b == null
          ? null
          : (
              remaining: b.remainingDays,
              used: b.usedDays,
              held: b.heldDays,
              entitlement: b.currentYearEntitlementDays + b.carriedDays + b.adjustmentDays,
              isSet: b.isEntitlementSet,
            ),
      attendanceAvailable: session.can(HrOps.Attendance_GetMy) && !session.isOutsideSchedule,
      attendance: m == null
          ? null
          : (
              present: m.summary.presentDays,
              lateDays: m.summary.lateDays,
              absent: m.summary.absentDays,
              leaveDays: m.summary.leaveDays,
              incomplete: m.summary.incompleteDays,
              lateMinutes: m.summary.totalLateMinutes,
              workedHours: m.summary.workedMinutes / 60,
              hoursByDay: [for (final d in m.items) d.workedMinutes / 60],
              lateByDay: [for (final d in m.items) d.lateMinutes.toDouble()],
              days: [for (final d in m.items) d.date],
            ),
      unread: unread,
      complaintsAvailable: session.can(HrOps.Complaint_GetAssignedList),
      complaints: complaints,
      teamAvailable: session.can(HrOps.Report_GetTodayAttendanceSummary) || session.can(HrOps.Report_GetHrAttendance) || session.can(HrOps.Attendance_GetTeam),
      executiveLayout: session.isExecutive,
      teamToday: teamToday,
      canNewRequest: session.can(HrOps.Request_Submit) && !session.isSystemAdminOnly && !session.isExecutive,
      canNewLeave: session.can(HrOps.LeaveRequest_Create) && !session.isOutsideSchedule,
    );

    return RefreshIndicator(
      onRefresh: () async {
        ref.invalidate(inboxSummaryProvider);
        ref.invalidate(myBalanceProvider);
        ref.invalidate(myMonthAttendanceProvider);
        ref.invalidate(myOpenComplaintsProvider);
        ref.invalidate(teamTodayProvider);
      },
      child: HomeDashboard(data: data),
    );
  }
}

/// Reusable dashboard body (also rendered by the design preview with sample data).
class HomeDashboard extends StatelessWidget {
  const HomeDashboard({super.key, required this.data});
  final DashboardData data;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final f = Fmt(context);
    final c = context.hr;
    final now = DateTime.now();
    final first = data.displayName.trim().split(RegExp(r'\s+')).first;
    final hour = now.hour;
    final greet = hour < 12 ? s.t('صباح الخير', 'Good morning') : (hour < 18 ? s.t('مساء الخير', 'Good afternoon') : s.t('مساء الخير', 'Good evening'));
    final title = data.hasTeam
        ? s.t('$greet يا $first، فريقك في أفضل حالاته', '$greet, $first. Your team is in great shape')
        : s.t('$greet يا $first، يومك منظّم وجاهز', '$greet, $first. Your day is organized');
    final pendingTotal = data.pendingTotal;
    final subtitle = pendingTotal == null
        ? s.t('نجهّز ملخص يومك…', 'Preparing your day…')
        : pendingTotal == 0
            ? s.t('لا شيء بانتظارك الآن. هذا ملخص حضورك وإجازاتك لهذا الشهر.', 'Nothing is waiting for you. Here is your attendance and leave this month.')
            : s.t('لديك $pendingTotal عنصر بانتظار إجرائك، وهذا ملخص شهرك.', 'You have $pendingTotal items awaiting your action, and your month at a glance.');

    final actions = <Widget>[
      if ((pendingTotal ?? 0) > 0)
        Magnetic(
          child: FilledButton.icon(
            onPressed: () => context.go('/inbox'),
            icon: const Icon(Icons.pending_actions_outlined),
            label: Text(s.t('افتح ما بانتظاري', 'Open my queue')),
          ),
        )
      else if (data.canNewRequest)
        Magnetic(
          child: FilledButton.icon(onPressed: () => context.go('/requests/new'), icon: const Icon(Icons.add_task), label: Text(s.newRequest)),
        ),
      if (data.canNewLeave)
        PressScale(
          child: OutlinedButton.icon(onPressed: () => context.go('/leaves/new'), icon: const Icon(Icons.beach_access_outlined), label: Text(s.newLeave)),
        ),
    ];

    final hero = WelcomeHero(
      chip: '${s.appTagline} · ${f.date(now)}',
      title: title,
      subtitle: subtitle,
      actions: actions,
    );

    final a = data.attendance;
    final l = data.leave;
    // The pending total is not repeated as a KPI: the pending card already shows it, by category.
    final kpis = <Widget>[
      if (data.attendanceAvailable)
        KpiCard(
          icon: Icons.event_available_outlined,
          label: s.t('أيام الحضور هذا الشهر', 'Days present this month'),
          value: a?.present,
          unit: s.days,
          caption: a == null ? null : s.t('ساعات العمل ${f.number(a.workedHours)}', 'Worked ${f.number(a.workedHours)} h'),
          trend: a?.hoursByDay,
          onTap: () => context.go('/attendance'),
        ),
      // Decision 2026-11-11: no late-minutes box on the home page.
      if (data.leaveAvailable)
        KpiCard(
          icon: Icons.beach_access_outlined,
          label: s.remainingBalance,
          value: l == null ? null : (l.isSet ? l.remaining : 0),
          decimals: (l?.remaining ?? 0) % 1 == 0 ? 0 : 1,
          unit: s.days,
          caption: l == null ? null : (l.isSet ? s.t('المستخدم ${f.number(l.used)} · المحجوز ${f.number(l.held)}', 'Used ${f.number(l.used)} · held ${f.number(l.held)}') : s.entitlementNotSet),
          onTap: () => context.go('/leaves'),
        ),
    ];

    final unreadCard = KpiCard(
      icon: Icons.notifications_none,
      label: s.unread,
      value: data.unread,
      caption: s.t('إشعارات لم تُقرأ بعد', 'Notifications not read yet'),
      onTap: () => context.go('/notifications'),
    );

    final cm = data.complaints;
    final complaintsCard = !data.complaintsAvailable
        ? null
        : KpiCard(
            icon: Icons.shield_outlined,
            label: s.t('الشكاوى الواصلة', 'Complaints assigned to me'),
            value: cm?.open,
            caption: cm == null
                ? null
                : cm.late > 0
                    ? s.t('منها ${cm.late} متأخرة', '${cm.late} of them late')
                    : s.t('مفتوحة بانتظار معالجتك', 'Open, awaiting your handling'),
            status: cm == null || cm.late == 0 ? null : (s.t('متأخرة', 'Late'), Icons.error_outline, HrColors.warning),
            onTap: () => context.go('/complaints'),
          );

    // Today's attendance of the people I oversee (HR manager: the company; department manager: the team).
    final t = data.teamToday;
    final teamCards = !data.teamAvailable
        ? <Widget>[]
        : <Widget>[
            KpiCard(
              icon: Icons.event_available_outlined,
              label: s.t('الحاضرون اليوم', 'Present today'),
              value: t?.present,
              caption: t == null ? null : s.t('منهم ${t.late} متأخر', '${t.late} of them late'),
              onTap: data.executiveLayout ? null : () => context.go('/attendance-admin'),
            ),
            KpiCard(
              icon: Icons.person_off_outlined,
              label: s.t('الغائبون اليوم', 'Absent today'),
              value: t?.absent,
              caption: s.t('بدون بصمة ولا إجازة', 'No punch and no leave'),
              status: t == null || t.absent == 0 ? null : (s.t('انتبه', 'Watch'), Icons.error_outline, HrColors.warning),
              onTap: data.executiveLayout ? null : () => context.go('/attendance-admin'),
            ),
            KpiCard(
              icon: Icons.beach_access_outlined,
              label: s.t('بإجازة اليوم', 'On leave today'),
              value: t?.onLeave,
              decimals: (t?.onLeave ?? 0) % 1 == 0 ? 0 : 1,
              caption: s.t('إجازات معتمدة لهذا اليوم', 'Approved leave for today'),
              onTap: data.executiveLayout ? null : () => context.go('/attendance-admin'),
            ),
          ];

    final lineChart = a == null || a.hoursByDay.length < 2
        ? null
        : ChartCard(
            title: s.t('ساعات العمل اليومية', 'Daily worked hours'),
            subtitle: s.t('هذا الشهر', 'This month'),
            semanticsSummary: s.t('ساعات العمل لكل يوم هذا الشهر، المجموع ${f.number(a.workedHours)}', 'Worked hours per day this month, total ${f.number(a.workedHours)}'),
            table: [for (var i = 0; i < a.days.length; i++) (f.date(a.days[i]), f.number(a.hoursByDay[i]))],
            chart: LineChart(values: a.hoursByDay, labels: [for (final d in a.days) '${d.day}']),
          );
    final barsChart = a == null
        ? null
        : ChartCard(
            title: s.t('حالة أيام الشهر', 'Days this month'),
            table: [
              (s.dayStatus(AttendanceDayStatus.present), '${a.present}'),
              (s.dayStatus(AttendanceDayStatus.late), '${a.lateDays}'),
              (s.dayStatus(AttendanceDayStatus.absent), '${a.absent}'),
              (s.dayStatus(AttendanceDayStatus.onLeave), f.number(a.leaveDays)),
              (s.dayStatus(AttendanceDayStatus.incomplete), '${a.incomplete}'),
            ],
            chart: BarsChart(bars: [
              (s.dayStatus(AttendanceDayStatus.present), a.present.toDouble(), HrColors.success, Icons.check_circle_outline),
              (s.dayStatus(AttendanceDayStatus.late), a.lateDays.toDouble(), HrColors.warning, Icons.schedule),
              (s.dayStatus(AttendanceDayStatus.absent), a.absent.toDouble(), HrColors.danger, Icons.cancel_outlined),
              (s.dayStatus(AttendanceDayStatus.onLeave), a.leaveDays, HrColors.info, Icons.beach_access_outlined),
              (s.dayStatus(AttendanceDayStatus.incomplete), a.incomplete.toDouble(), c.textSecondary, Icons.help_outline),
            ]),
          );
    final donut = l == null || !l.isSet
        ? null
        : ChartCard(
            title: s.t('رصيد الإجازة السنوية', 'Annual leave balance'),
            table: [(s.remaining, f.number(l.remaining)), (s.used, f.number(l.used)), (s.held, f.number(l.held))],
            chart: DonutChart(
              centerValue: f.number(l.remaining),
              centerLabel: s.t('يوم متبقٍ', 'days left'),
              segments: [
                (s.remaining, l.remaining, c.accent),
                (s.used, l.used, HrColors.info),
                (s.held, l.held, HrColors.warning),
              ],
            ),
          );

    return LayoutBuilder(builder: (context, box) {
      final w = box.maxWidth;
      final medium = w >= 700;
      const gap = 16.0;
      var i = 0;
      Widget enter(Widget child) => FadeSlideIn(delay: HrMotion.cardStagger * (i++), child: child);

      Widget pair(Widget? a, Widget? b, int fa, int fb) {
        if (a == null && b == null) return const SizedBox.shrink();
        if (a == null || b == null || !medium) {
          return Column(children: [
            if (a != null) Padding(padding: const EdgeInsets.only(bottom: gap), child: enter(a)),
            if (b != null) Padding(padding: const EdgeInsets.only(bottom: gap), child: enter(b)),
          ]);
        }
        return Padding(
          padding: const EdgeInsets.only(bottom: gap),
          child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Expanded(flex: fa, child: enter(a)),
            const SizedBox(width: gap),
            Expanded(flex: fb, child: enter(b)),
          ]),
        );
      }

      // Every box has the same width and height (decision 2026-11-11), on the web and on a phone. From 700 px the
      // boxes form one grid of 3 or 2 columns, whichever leaves fewer empty places (4 boxes: 2 x 2). On a phone they
      // stack, still at the same height. The pending card scrolls its own list when it has more lines.
      const cardHeight = 220.0;
      Widget equalRows(List<Widget> items) {
        if (items.isEmpty) return const SizedBox.shrink();
        if (!medium) {
          return Column(children: [
            for (final it in items) Padding(padding: const EdgeInsets.only(bottom: gap), child: SizedBox(height: cardHeight, child: enter(it))),
          ]);
        }
        int empty(int c) => (c - items.length % c) % c;
        final columns = items.length <= 3 ? items.length.clamp(2, 3) : (empty(2) < empty(3) ? 2 : 3);
        final rows = <Widget>[];
        for (var r = 0; r < items.length; r += columns) {
          rows.add(Padding(
            padding: const EdgeInsets.only(bottom: gap),
            child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
              for (var k = 0; k < columns; k++) ...[
                if (k > 0) const SizedBox(width: gap),
                Expanded(child: r + k < items.length ? SizedBox(height: cardHeight, child: enter(items[r + k])) : const SizedBox()),
              ],
            ]),
          ));
        }
        return Column(children: rows);
      }

      // One grid in this order: awaiting my action, unread, complaints assigned to me, today's attendance of the
      // people I oversee, then my own attendance and leave.
      final boxes = data.executiveLayout
          // Executive director (decision 2026-11-11): awaiting my action, complaints that reach me, present today and
          // on leave today. Unread stays on the bell.
          ? <Widget>[
              _PendingCard(items: data.pending, fill: true),
              if (complaintsCard != null) complaintsCard,
              if (teamCards.length == 3) ...[teamCards[0], teamCards[2]],
            ]
          : <Widget>[
              _PendingCard(items: data.pending, fill: true),
              unreadCard,
              if (complaintsCard != null) complaintsCard,
              ...teamCards,
              ...kpis,
            ];

      final children = <Widget>[
        if (data.preview) _PreviewBanner(),
        Padding(padding: const EdgeInsets.only(bottom: gap), child: enter(hero)),
        equalRows(boxes),
        pair(lineChart, donut, 7, 5),
        if (barsChart != null) Padding(padding: const EdgeInsets.only(bottom: gap), child: enter(barsChart)),
      ];

      return ListView(
        padding: EdgeInsets.fromLTRB(w >= 768 ? 24 : 16, 8, w >= 768 ? 24 : 16, 32),
        children: [
          Align(
            alignment: AlignmentDirectional.topStart,
            child: ConstrainedBox(constraints: const BoxConstraints(maxWidth: 1440), child: Column(children: children)),
          ),
        ],
      );
    });
  }
}

class _PreviewBanner extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
        decoration: BoxDecoration(
          color: HrColors.info.withValues(alpha: .12),
          borderRadius: BorderRadius.circular(HrColors.radiusControl),
        ),
        child: Row(children: [
          const Icon(Icons.visibility_outlined, size: 18, color: HrColors.info),
          const SizedBox(width: 8),
          Expanded(child: Text(s.t('معاينة التصميم ببيانات تجريبية (وضع التطوير فقط).', 'Design preview with sample data (development only).'))),
        ]),
      ),
    );
  }
}

/// «Awaiting my action»: one line per category, with count, delegated part and destination.
class _PendingCard extends StatelessWidget {
  const _PendingCard({required this.items, this.fill = false});
  final List<(InboxCategory, int, int)>? items;

  /// Inside a fixed-height row: the lines scroll within the card instead of growing it.
  final bool fill;

  static String? _route(InboxCategory c) => switch (c) {
        InboxCategory.request || InboxCategory.stockIssue => '/inbox',
        InboxCategory.memo => '/memos',
        InboxCategory.incident => '/incidents',
        InboxCategory.evaluation || InboxCategory.evaluationObjection || InboxCategory.evaluationAcknowledgement => '/evaluations',
        InboxCategory.departmentReport => '/department-reports',
        InboxCategory.circularAcknowledgement => '/circulars',
        InboxCategory.custodyAcknowledgement => '/custody',
        InboxCategory.warningResponse || InboxCategory.warningDecision => '/warnings',
        InboxCategory.contractDecision => '/contracts',
        InboxCategory.appreciationLetterAcknowledgement => '/appreciation',
        InboxCategory.noticeAcknowledgement => '/notices',
        _ => null,
      };

  static IconData _icon(InboxCategory c) => switch (c) {
        InboxCategory.request => Icons.assignment_outlined,
        InboxCategory.memo => Icons.description_outlined,
        InboxCategory.incident => Icons.build_circle_outlined,
        InboxCategory.stockIssue => Icons.outbox_outlined,
        InboxCategory.evaluation || InboxCategory.evaluationObjection || InboxCategory.evaluationAcknowledgement => Icons.rate_review_outlined,
        InboxCategory.departmentReport => Icons.summarize_outlined,
        InboxCategory.circularAcknowledgement => Icons.campaign_outlined,
        InboxCategory.custodyAcknowledgement => Icons.inventory_2_outlined,
        InboxCategory.warningResponse || InboxCategory.warningDecision => Icons.report_outlined,
        InboxCategory.contractDecision => Icons.description_outlined,
        InboxCategory.appreciationLetterAcknowledgement => Icons.workspace_premium_outlined,
        InboxCategory.noticeAcknowledgement => Icons.mark_email_unread_outlined,
        _ => Icons.inbox_outlined,
      };

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final c = context.hr;
    final list = items;
    final lines = <Widget>[
      if (list == null)
        for (var i = 0; i < 3; i++)
          const Padding(padding: EdgeInsets.symmetric(vertical: 10), child: Skeleton(height: 24))
      else if (list.isEmpty)
        Padding(
          padding: const EdgeInsets.symmetric(vertical: 20),
          child: Row(children: [
            const Icon(Icons.check_circle_outline, color: HrColors.success),
            const SizedBox(width: 10),
            Expanded(child: Text(s.nothingPending, style: TextStyle(color: c.textSecondary))),
          ]),
        )
      else
        for (final (cat, count, delegated) in list) _line(context, cat, count, delegated),
    ];
    return Container(
      padding: const EdgeInsets.fromLTRB(20, 18, 20, 12),
      decoration: BoxDecoration(
        color: c.surface,
        borderRadius: BorderRadius.circular(HrColors.radiusCard),
        border: Border.all(color: c.border),
        boxShadow: c.shadow,
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Row(children: [
          const Icon(Icons.notifications_active_outlined, color: HrColors.warning, size: 20),
          const SizedBox(width: 8),
          Expanded(child: Semantics(header: true, child: Text(s.pendingActions, style: Theme.of(context).textTheme.titleMedium))),
          TextButton(onPressed: () => context.go('/inbox'), child: Text(s.t('عرض الكل', 'View all'))),
        ]),
        const SizedBox(height: 8),
        if (fill) Expanded(child: ListView(padding: EdgeInsets.zero, children: lines)) else ...lines,
      ]),
    );
  }

  Widget _line(BuildContext context, InboxCategory cat, int count, int delegated) {
    final s = S.of(context);
    final c = context.hr;
    return Material(
      type: MaterialType.transparency,
      child: InkWell(
        borderRadius: BorderRadius.circular(HrColors.radiusControl),
        onTap: _route(cat) == null ? null : () => context.go(_route(cat)!),
        child: ConstrainedBox(
          constraints: const BoxConstraints(minHeight: 48),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 8),
            child: Row(children: [
              Icon(_icon(cat), color: c.accent, size: 20),
              const SizedBox(width: 12),
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(s.inboxCategory(cat), style: const TextStyle(fontWeight: FontWeight.w600)),
                  if (delegated > 0)
                    Text('$delegated ${s.viaDelegation}', style: TextStyle(color: c.textSecondary, fontSize: 12)),
                ]),
              ),
              Container(
                constraints: const BoxConstraints(minWidth: 32),
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(color: c.glow, borderRadius: BorderRadius.circular(HrColors.radiusBadge)),
                child: Text('$count',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: c.textPrimary, fontWeight: FontWeight.w700, fontFeatures: const [FontFeature.tabularFigures()])),
              ),
              if (_route(cat) != null) Icon(Directionality.of(context) == TextDirection.rtl ? Icons.chevron_left : Icons.chevron_right, color: c.textSecondary),
            ]),
          ),
        ),
      ),
    );
  }
}

/// Development-only design preview: the dashboard with sample data, reachable without signing in
/// (`/preview?theme=dark&lang=en`), so the design can be reviewed and captured. Not built into release.
class HomePreviewPage extends ConsumerStatefulWidget {
  const HomePreviewPage({super.key, this.theme, this.lang});
  final String? theme;
  final String? lang;

  @override
  ConsumerState<HomePreviewPage> createState() => _HomePreviewPageState();
}

class _HomePreviewPageState extends ConsumerState<HomePreviewPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      if (widget.theme == 'dark') ref.read(themeModeProvider.notifier).set(ThemeMode.dark);
      if (widget.theme == 'light') ref.read(themeModeProvider.notifier).set(ThemeMode.light);
      if (widget.lang == 'en' || widget.lang == 'ar') ref.read(localeProvider.notifier).set(widget.lang!);
    });
  }

  @override
  Widget build(BuildContext context) {
    if (!kDebugMode) return const SizedBox.shrink();
    final s = S.of(context);
    final days = [for (var i = 1; i <= 20; i++) DateTime(2026, 11, i)];
    const hours = [8.0, 7.5, 8.2, 0, 0, 8.0, 7.8, 8.5, 6.9, 8.1, 0, 0, 8.3, 7.9, 8.0, 8.4, 7.2, 0, 0, 8.1];
    const late = [0.0, 12, 0, 0, 0, 5, 0, 0, 22, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0];
    final data = DashboardData(
      displayName: s.t('سارة أحمد', 'Sarah Ahmed'),
      hasTeam: true,
      pending: const [(InboxCategory.request, 4, 1), (InboxCategory.memo, 2, 0), (InboxCategory.evaluation, 3, 0), (InboxCategory.circularAcknowledgement, 1, 0)],
      pendingTotal: 10,
      leaveAvailable: true,
      leave: (remaining: 18.5, used: 9.0, held: 2.5, entitlement: 30.0, isSet: true),
      attendanceAvailable: true,
      attendance: (
        present: 13,
        lateDays: 4,
        absent: 1,
        leaveDays: 1.0,
        incomplete: 1,
        lateMinutes: 42,
        workedHours: hours.fold<double>(0, (a, b) => a + b.toDouble()),
        hoursByDay: hours.map((e) => e.toDouble()).toList(),
        lateByDay: late.map((e) => e.toDouble()).toList(),
        days: days,
      ),
      unread: 6,
      complaintsAvailable: true,
      complaints: (open: 3, late: 1),
      teamAvailable: true,
      teamToday: (present: 42, late: 5, absent: 2, onLeave: 3.0),
      canNewRequest: true,
      canNewLeave: true,
      preview: true,
    );
    return HomeDashboard(data: data);
  }
}
