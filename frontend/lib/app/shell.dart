import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../api/generated/hr_api.dart';
import '../core/l10n/locale_controller.dart';
import '../core/l10n/strings.dart';
import '../core/session/session_controller.dart';
import '../core/theme/motion.dart';
import '../core/theme/tokens.dart';
import '../core/widgets/motion_widgets.dart';
import '../core/widgets/hr_logo.dart';
import '../features/notifications/unread_count.dart';

class NavItem {
  const NavItem(this.path, this.icon, this.label, {this.operation, this.anyOf = const <String>[], this.admin = false});
  final String path;
  final IconData icon;
  final String Function(S s) label;

  /// Shown only when the user holds this operation's x-permission.
  final String? operation;

  /// Alternatively shown when any of these operations is granted.
  final List<String> anyOf;

  /// Listed under the administration section.
  final bool admin;

  bool visibleFor(Session session) {
    if (session.isSystemAdminOnly && isHiddenForSystemAdmin(path)) return false;
    if (operation == null && anyOf.isEmpty) return true;
    if (operation != null && session.can(operation!)) return true;
    return anyOf.any(session.can);
  }
}

/// Employee-facing pages that a system admin (without a business role) does not use.
const systemAdminHiddenPaths = <String>[
  '/leaves',
  '/attendance',
  '/memos',
  '/incidents',
  '/complaints',
  '/evaluations',
  '/custody',
  '/payslips',
  '/contracts',
  '/warnings',
  '/appreciation',
  '/holidays',
];

/// Exact path or a sub-route of it ('/attendance' must not match '/attendance-admin').
bool isHiddenForSystemAdmin(String location) =>
    systemAdminHiddenPaths.any((p) => location == p || location.startsWith('$p/'));

final navItems = <NavItem>[
  NavItem('/', Icons.space_dashboard_outlined, (s) => s.home),
  NavItem('/inbox', Icons.pending_actions_outlined, (s) => s.inbox, operation: HrOps.Request_GetInbox),
  NavItem('/requests', Icons.assignment_outlined, (s) => s.myRequests, operation: HrOps.Request_GetMy),
  NavItem('/leaves', Icons.beach_access_outlined, (s) => s.leaves, operation: HrOps.LeaveBalance_GetMy),
  NavItem('/attendance', Icons.schedule_outlined, (s) => s.attendance, operation: HrOps.Attendance_GetMy),
  NavItem('/notifications', Icons.notifications_none, (s) => s.notifications, operation: HrOps.Notification_List),
  NavItem('/memos', Icons.description_outlined, (s) => s.t('المطالعات', 'Memos'), anyOf: [HrOps.Memo_GetMy, HrOps.Memo_GetInbox]),
  NavItem('/incidents', Icons.build_circle_outlined, (s) => s.t('بلاغات الأعطال', 'Fault incidents'), anyOf: [HrOps.Incident_GetMy, HrOps.Incident_GetInbox]),
  NavItem('/complaints', Icons.shield_outlined, (s) => s.t('الشكاوى', 'Complaints'), anyOf: [HrOps.Complaint_GetMyList, HrOps.Complaint_GetAssignedList]),
  NavItem('/circulars', Icons.campaign_outlined, (s) => s.t('التعاميم', 'Circulars'), operation: HrOps.Circular_GetReceivedList),
  NavItem('/evaluations', Icons.rate_review_outlined, (s) => s.t('التقييمات', 'Evaluations'),
      anyOf: [HrOps.Evaluation_GetMyList, HrOps.Evaluation_GetTeamList, HrOps.Evaluation_GetHrList]),
  NavItem('/custody', Icons.inventory_2_outlined, (s) => s.t('العهد', 'Custody'), anyOf: [HrOps.Custody_GetMy, HrOps.Custody_GetTeam, HrOps.Custody_GetList]),
  NavItem('/payslips', Icons.receipt_long_outlined, (s) => s.t('قسائم الراتب', 'Payslips'), operation: HrOps.Payslip_GetMyList),
  NavItem('/contracts', Icons.description_outlined, (s) => s.t('العقود', 'Contracts'), anyOf: [HrOps.Contract_GetMyList, HrOps.Contract_List]),
  NavItem('/warnings', Icons.report_outlined, (s) => s.t('الإنذارات', 'Warnings'),
      anyOf: [HrOps.Warning_GetMyList, HrOps.Warning_GetRecommendedByMe, HrOps.Warning_List, HrOps.Warning_GetObjections]),
  NavItem('/appreciation', Icons.workspace_premium_outlined, (s) => s.t('كتب الشكر', 'Appreciation letters'),
      anyOf: [HrOps.AppreciationLetter_GetMyList, HrOps.AppreciationLetter_List]),
  NavItem('/holidays', Icons.event_note_outlined, (s) => s.t('العطل', 'Holidays'), operation: HrOps.Holiday_List),
  NavItem('/delegations', Icons.swap_horizontal_circle_outlined, (s) => s.t('التفويض', 'Delegations'), operation: HrOps.Delegation_GetMy),
  NavItem('/profile', Icons.person_outline, (s) => s.profile),
  NavItem('/team', Icons.groups_2_outlined, (s) => s.t('فريقي', 'My team'), operation: HrOps.Manager_GetMyTeam, admin: true),
  NavItem('/team-calendar', Icons.calendar_month_outlined, (s) => s.t('تقويم إجازات الفريق', 'Team leave calendar'), operation: HrOps.LeaveCalendar_GetTeam, admin: true),
  NavItem('/attendance-admin', Icons.fact_check_outlined, (s) => s.t('حضور الموظفين', 'Employee attendance'), operation: HrOps.Attendance_GetByEmployee, admin: true),
  NavItem('/leave-balances', Icons.account_balance_wallet_outlined, (s) => s.t('أرصدة الإجازات', 'Leave balances'), operation: HrOps.LeaveBalance_SetEntitlement, admin: true),
  NavItem('/employees', Icons.badge_outlined, (s) => s.t('الموظفون', 'Employees'), operation: HrOps.Employee_List, admin: true),
  NavItem('/departments', Icons.apartment_outlined, (s) => s.t('الأقسام', 'Departments'), operation: HrOps.Department_List, admin: true),
  NavItem('/accounts', Icons.manage_accounts_outlined, (s) => s.t('الحسابات', 'Accounts'), operation: HrOps.UserAccount_List, admin: true),
  NavItem('/shifts', Icons.schedule_outlined, (s) => s.t('قوالب الدوام', 'Shift templates'), operation: HrOps.WorkShift_List, admin: true),
  NavItem('/devices', Icons.fingerprint, (s) => s.t('أجهزة البصمة', 'Attendance devices'), operation: HrOps.AttendanceDevice_List, admin: true),
  NavItem('/attendance-import', Icons.upload_file_outlined, (s) => s.t('استيراد الحضور', 'Attendance import'), operation: HrOps.AttendanceImport_List, admin: true),
  NavItem('/attendance-settings', Icons.tune, (s) => s.t('إعدادات الحضور', 'Attendance settings'), operation: HrOps.AttendanceSettings_Update, admin: true),
  NavItem('/leave-types', Icons.category_outlined, (s) => s.t('أنواع الإجازات', 'Leave types'), operation: HrOps.OtherLeaveType_List, admin: true),
  NavItem('/complaint-handlers', Icons.gavel_outlined, (s) => s.t('معالجو الشكاوى', 'Complaint handlers'), operation: HrOps.ComplaintHandler_GetList, admin: true),
  NavItem('/roles', Icons.admin_panel_settings_outlined, (s) => s.t('الأدوار والصلاحيات', 'Roles and permissions'), operation: HrOps.Role_List, admin: true),
  NavItem('/store', Icons.warehouse_outlined, (s) => s.t('المخزن', 'Store'), anyOf: [HrOps.StockIssue_GetList, HrOps.Item_GetList], admin: true),
  NavItem('/finance', Icons.account_balance_outlined, (s) => s.t('المالية', 'Finance'), anyOf: [HrOps.PayrollRun_List, HrOps.SalaryProfile_List], admin: true),
  NavItem('/department-reports', Icons.summarize_outlined, (s) => s.t('التقارير الشهرية', 'Monthly reports'), operation: HrOps.DepartmentReport_GetList, admin: true),
  NavItem('/review-reports', Icons.insights_outlined, (s) => s.t('تقارير المتابعة', 'Follow-up reports'), operation: HrOps.ReviewReport_GetList, admin: true),
  NavItem('/evaluation-admin', Icons.checklist, (s) => s.t('معايير التقييم', 'Evaluation criteria'), operation: HrOps.EvaluationCriterion_GetList, admin: true),
  NavItem('/request-types', Icons.list_alt_outlined, (s) => s.t('أنواع الطلبات', 'Request types'), operation: HrOps.RequestType_GetAdminList, admin: true),
  NavItem('/reports', Icons.bar_chart_outlined, (s) => s.t('التقارير', 'Reports'),
      anyOf: [HrOps.Report_GetTeamAttendance, HrOps.Report_GetReviewDashboard, HrOps.Report_GetHrAttendance], admin: true),
  NavItem('/system-settings', Icons.settings_suggest_outlined, (s) => s.t('إعدادات النظام', 'System settings'), operation: HrOps.SystemSettings_ListSections, admin: true),
  NavItem('/audit-log', Icons.history_edu_outlined, (s) => s.t('سجل التدقيق', 'Audit log'), operation: HrOps.AuditLog_List, admin: true),
];

/// Desktop ≥1280: full sidebar. Tablet 768–1279: collapsed rail (symbol only). Mobile <768: drawer.
/// Ambient light behind everything, a floating translucent top bar, and page transitions.
class AppShell extends ConsumerWidget {
  const AppShell({super.key, required this.child, required this.location});
  final Widget child;
  final String location;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(sessionProvider);
    final items = navItems.where((i) => i.visibleFor(session)).toList();
    final width = MediaQuery.sizeOf(context).width;
    final selected = _selectedIndex(items);
    final page = RepaintBoundary(child: _PageTransition(location: location, child: child));

    if (width < 768) {
      return Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          backgroundColor: context.hr.bg.withValues(alpha: .85),
          title: const HrLogo(size: 26, clearSpace: false),
          actions: const [_TopActions()],
        ),
        drawer: Drawer(child: SafeArea(child: _SideNav(items: items, selected: selected, extended: true, floating: false))),
        body: AmbientBackground(child: SafeArea(child: page)),
      );
    }
    return Scaffold(
      body: AmbientBackground(
        child: SafeArea(
          child: Row(
            children: [
              RepaintBoundary(child: _SideNav(items: items, selected: selected, extended: width >= 1280)),
              Expanded(
                child: Column(children: [
                  const _TopBar(),
                  Expanded(child: page),
                ]),
              ),
            ],
          ),
        ),
      ),
    );
  }

  int _selectedIndex(List<NavItem> items) {
    var best = -1;
    var bestLen = -1;
    for (var i = 0; i < items.length; i++) {
      final p = items[i].path;
      final match = p == '/' ? location == '/' : location.startsWith(p);
      if (match && p.length > bestLen) {
        best = i;
        bestLen = p.length;
      }
    }
    return best;
  }
}

/// Page enter: opacity + 16 px rise (450 ms), replayed on every location change.
/// The shell's child is go_router's nested Navigator (it owns a GlobalKey), so it must never be
/// duplicated or re-keyed: an AnimatedSwitcher here kept two copies alive during the switch,
/// which broke the page area (blank and unresponsive). This animates the same child in place.
class _PageTransition extends StatefulWidget {
  const _PageTransition({required this.location, required this.child});
  final String location;
  final Widget child;

  @override
  State<_PageTransition> createState() => _PageTransitionState();
}

class _PageTransitionState extends State<_PageTransition> with SingleTickerProviderStateMixin {
  late final AnimationController _c = AnimationController(vsync: this, duration: HrMotion.pageIn, value: 1);

  @override
  void didUpdateWidget(covariant _PageTransition old) {
    super.didUpdateWidget(old);
    if (old.location != widget.location && !HrMotion.reduced(context)) {
      _c.forward(from: 0);
    }
  }

  @override
  void dispose() {
    _c.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _c,
      child: widget.child,
      builder: (context, child) {
        final t = HrMotion.enter.transform(_c.value);
        return Opacity(
          opacity: t.clamp(0.0, 1.0),
          child: Transform.translate(offset: Offset(0, HrMotion.pageShift * (1 - t)), child: child),
        );
      },
    );
  }
}

/// Floating, translucent top bar with notifications, language and theme.
class _TopBar extends StatelessWidget {
  const _TopBar();

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    return Padding(
      padding: const EdgeInsetsDirectional.fromSTEB(12, 12, 16, 4),
      // Translucent without a live backdrop blur: on some GPUs (Edge without hardware
      // acceleration) a backdrop filter kept the page under it from painting until a resize.
      child: Container(
        height: 60,
        padding: const EdgeInsets.symmetric(horizontal: 8),
        decoration: BoxDecoration(
          color: c.surface.withValues(alpha: .82),
          borderRadius: BorderRadius.circular(HrColors.radiusCard),
          border: Border.all(color: c.border.withValues(alpha: .7)),
        ),
        child: const Row(children: [Spacer(), _TopActions()]),
      ),
    );
  }
}

class _SideNav extends ConsumerStatefulWidget {
  const _SideNav({required this.items, required this.selected, required this.extended, this.floating = true});
  final List<NavItem> items;
  final int selected;
  final bool extended;
  final bool floating;

  @override
  ConsumerState<_SideNav> createState() => _SideNavState();
}

class _SideNavState extends ConsumerState<_SideNav> {
  final _stackKey = GlobalKey();
  final _keys = <int, GlobalKey>{};
  Rect? _indicator;

  GlobalKey _key(int i) => _keys.putIfAbsent(i, GlobalKey.new);

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _measure());
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    WidgetsBinding.instance.addPostFrameCallback((_) => _measure());
  }

  @override
  void didUpdateWidget(covariant _SideNav old) {
    super.didUpdateWidget(old);
    WidgetsBinding.instance.addPostFrameCallback((_) => _measure());
  }

  /// Places the shared active indicator on the selected tile (it then slides between tiles).
  void _measure() {
    if (!mounted) return;
    final stack = _stackKey.currentContext?.findRenderObject() as RenderBox?;
    final tile = widget.selected < 0 ? null : _keys[widget.selected]?.currentContext?.findRenderObject() as RenderBox?;
    Rect? r;
    if (stack != null && tile != null && tile.attached && stack.attached) {
      final o = tile.localToGlobal(Offset.zero, ancestor: stack);
      r = o & tile.size;
    }
    if (r != _indicator) setState(() => _indicator = r);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final c = context.hr;
    final items = widget.items;
    final extended = widget.extended;
    final reduced = HrMotion.reduced(context);
    final ind = _indicator;
    final rtl = Directionality.of(context) == TextDirection.rtl;

    final list = Stack(key: _stackKey, children: [
      if (ind != null)
        AnimatedPositioned(
          duration: reduced ? Duration.zero : HrMotion.navIndicator,
          curve: HrMotion.spring,
          left: ind.left,
          top: ind.top,
          width: ind.width,
          height: ind.height - 4,
          child: TweenAnimationBuilder<double>(
            tween: Tween(begin: reduced ? 1 : 0, end: 1),
            duration: HrMotion.navIndicator,
            curve: HrMotion.spring,
            builder: (context, t, _) => Transform(
              alignment: rtl ? Alignment.centerRight : Alignment.centerLeft,
              transform: Matrix4.diagonal3Values(t, 1, 1),
              child: Stack(children: [
                Positioned.fill(
                  child: DecoratedBox(decoration: BoxDecoration(color: c.glow, borderRadius: BorderRadius.circular(12))),
                ),
                PositionedDirectional(
                  start: 0,
                  top: 10,
                  bottom: 10,
                  width: 3,
                  child: DecoratedBox(decoration: BoxDecoration(color: c.accent, borderRadius: BorderRadius.circular(2))),
                ),
              ]),
            ),
          ),
        ),
      Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        for (var i = 0; i < items.length; i++) ...[
          if (items[i].admin && (i == 0 || !items[i - 1].admin))
            extended
                ? Padding(
                    padding: const EdgeInsetsDirectional.fromSTEB(14, 14, 14, 6),
                    child: Text(s.t('الإدارة', 'Administration'), style: Theme.of(context).textTheme.labelSmall?.copyWith(color: c.textSecondary)),
                  )
                : const Divider(height: 20),
          // measured outside the slide-in transform so the indicator lands on the final position
          KeyedSubtree(
            key: _key(i),
            child: FadeSlideIn(
              duration: HrMotion.navSlide,
              delay: Duration(milliseconds: 25 * (i < 16 ? i : 16)),
              offset: const Offset(24, 0),
              child: _NavTile(item: items[i], selected: i == widget.selected, extended: extended, label: items[i].label(s)),
            ),
          ),
        ],
      ]),
    ]);

    return Container(
      width: extended ? 264 : 84,
      margin: widget.floating ? const EdgeInsets.all(12) : EdgeInsets.zero,
      padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 10),
      decoration: widget.floating
          ? BoxDecoration(
              color: c.surface.withValues(alpha: .86),
              borderRadius: BorderRadius.circular(HrColors.radiusCard),
              border: Border.all(color: c.border),
              boxShadow: c.shadow,
            )
          : null,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Center(child: HrLogo(size: extended ? 34 : 30, showWordmark: extended, animateIn: true)),
          const SizedBox(height: 4),
          Expanded(
            child: NotificationListener<ScrollMetricsNotification>(
              onNotification: (_) {
                WidgetsBinding.instance.addPostFrameCallback((_) => _measure());
                return false;
              },
              child: SingleChildScrollView(child: list),
            ),
          ),
          _NavTile(
            item: NavItem('', Icons.logout, (s) => s.signOut),
            selected: false,
            extended: extended,
            label: s.signOut,
            onTap: () => ref.read(sessionProvider.notifier).signOut(),
          ),
        ],
      ),
    );
  }
}

class _NavTile extends StatelessWidget {
  const _NavTile({required this.item, required this.selected, required this.extended, required this.label, this.onTap});
  final NavItem item;
  final bool selected;
  final bool extended;
  final String label;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    final color = selected ? c.accent : c.textSecondary;
    final tile = Material(
      type: MaterialType.transparency,
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: onTap ??
            () {
              Scaffold.maybeOf(context)?.closeDrawer();
              context.go(item.path);
            },
        child: Container(
          constraints: const BoxConstraints(minHeight: 44),
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 11),
          child: Row(
            mainAxisAlignment: extended ? MainAxisAlignment.start : MainAxisAlignment.center,
            children: [
              Icon(item.icon, color: color, size: 22),
              if (extended) ...[
                const SizedBox(width: 14),
                Expanded(
                  child: Text(label,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(color: selected ? c.textPrimary : c.textSecondary, fontWeight: selected ? FontWeight.w700 : FontWeight.w500)),
                ),
              ],
            ],
          ),
        ),
      ),
    );
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Semantics(
        selected: selected,
        button: true,
        child: extended ? tile : Tooltip(message: label, child: tile),
      ),
    );
  }
}

class _TopActions extends ConsumerWidget {
  const _TopActions();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final s = S.of(context);
    final session = ref.watch(sessionProvider);
    final unread = ref.watch(unreadCountProvider).valueOrNull ?? 0;
    final isAr = ref.watch(localeProvider).languageCode == 'ar';
    final dark = Theme.of(context).brightness == Brightness.dark;
    return Row(mainAxisSize: MainAxisSize.min, children: [
      if (session.can(HrOps.Notification_List))
        IconButton(
          tooltip: s.notifications,
          onPressed: () => context.go('/notifications'),
          icon: Badge(isLabelVisible: unread > 0, label: Text('$unread'), child: const Icon(Icons.notifications_none)),
        ),
      IconButton(
        tooltip: s.language,
        onPressed: () => ref.read(localeProvider.notifier).set(isAr ? 'en' : 'ar'),
        icon: Text(isAr ? 'EN' : 'ع', style: const TextStyle(fontWeight: FontWeight.w700)),
      ),
      IconButton(
        tooltip: dark ? s.themeLight : s.themeDark,
        onPressed: () => ref.read(themeModeProvider.notifier).set(dark ? ThemeMode.light : ThemeMode.dark),
        icon: AnimatedSwitcher(
          duration: HrMotion.press,
          child: Icon(dark ? Icons.light_mode_outlined : Icons.dark_mode_outlined, key: ValueKey(dark)),
        ),
      ),
      if (session.me != null)
        Padding(
          padding: const EdgeInsetsDirectional.only(start: 4, end: 4),
          child: Tooltip(
            message: s.profile,
            child: InkWell(
              borderRadius: BorderRadius.circular(22),
              onTap: () => context.go('/profile'),
              child: SizedBox(
                width: 44,
                height: 44,
                child: Center(
                  child: CircleAvatar(
                    radius: 18,
                    backgroundColor: context.hr.glow,
                    child: Text(
                      session.me!.displayName.isNotEmpty ? session.me!.displayName.characters.first : '?',
                      style: TextStyle(color: context.hr.textPrimary, fontWeight: FontWeight.w700),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
    ]);
  }
}
