import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../api/generated/models.dart';
import '../core/session/session_controller.dart';
import '../features/admin/audit_log_page.dart';
import '../features/admin/reports_page.dart';
import '../features/admin/request_types_page.dart';
import '../features/admin/settings_page.dart';
import '../features/attendance/attendance_page.dart';
import '../features/eval/circulars_page.dart';
import '../features/eval/department_reports_page.dart';
import '../features/eval/review_reports_page.dart';
import '../features/eval/evaluation_admin_page.dart';
import '../features/eval/evaluations_page.dart';
import '../features/finance/finance_page.dart';
import '../features/finance/payroll_pages.dart';
import '../features/hrx/appreciation_page.dart';
import '../features/hrx/contracts_page.dart';
import '../features/hrx/warnings_page.dart';
import '../features/ops/custody_page.dart';
import '../features/ops/store_page.dart';
import '../features/attendance/correction_page.dart';
import '../features/auth/change_password_page.dart';
import '../features/auth/mfa_enroll_page.dart';
import '../features/auth/sign_in_page.dart';
import '../features/auth/splash_page.dart';
import '../features/home/home_page.dart';
import '../features/leaves/leaves_page.dart';
import '../features/leaves/new_leave_page.dart';
import '../features/notifications/notifications_page.dart';
import '../features/org/account_detail_page.dart';
import '../features/org/accounts_page.dart';
import '../features/org/delegations_page.dart';
import '../features/org/departments_page.dart';
import '../features/org/employee_create_page.dart';
import '../features/org/employee_file_page.dart';
import '../features/org/employees_page.dart';
import '../features/org/roles_page.dart';
import '../features/org/team_page.dart';
import '../features/time/attendance_admin_page.dart';
import '../features/workflow/complaint_handlers_page.dart';
import '../features/workflow/complaints_page.dart';
import '../features/workflow/incidents_page.dart';
import '../features/workflow/memos_page.dart';
import '../features/time/devices_page.dart';
import '../features/time/holidays_page.dart';
import '../features/time/import_page.dart';
import '../features/time/leave_admin_page.dart';
import '../features/time/shifts_page.dart';
import '../features/profile/profile_page.dart';
import '../features/requests/inbox_page.dart';
import '../features/requests/my_requests_page.dart';
import '../features/requests/new_request_page.dart';
import '../features/requests/request_detail_page.dart';
import 'shell.dart';

/// Bridges the Riverpod session to go_router's refreshListenable.
class _SessionListenable extends ChangeNotifier {
  _SessionListenable(Ref ref) {
    ref.listen<Session>(sessionProvider, (_, __) => notifyListeners());
  }
}

final routerProvider = Provider<GoRouter>((ref) {
  final refresh = _SessionListenable(ref);
  return GoRouter(
    initialLocation: '/',
    refreshListenable: refresh,
    redirect: (context, state) {
      final session = ref.read(sessionProvider);
      final loc = state.matchedLocation;
      // Development-only design preview (sample data, no sign-in).
      if (kDebugMode && loc.startsWith('/preview')) return null;
      String? target;
      switch (session.status) {
        case SessionStatus.loading:
          target = '/splash';
        case SessionStatus.signedOut:
          target = '/sign-in';
        case SessionStatus.signedIn:
          // x-global-rules.access-gates: the mandatory screen comes first.
          if (session.gate == AccessGate.passwordChangeRequired) {
            target = '/gate/password';
          } else if (session.gate == AccessGate.mfaEnrollmentRequired) {
            target = '/gate/mfa';
          } else if (loc == '/splash' || loc == '/sign-in' || loc.startsWith('/gate')) {
            return '/';
          } else if (session.isSystemAdminOnly && isHiddenForSystemAdmin(loc)) {
            return '/';
          } else {
            return null;
          }
      }
      return loc == target ? null : target;
    },
    routes: [
      GoRoute(path: '/splash', builder: (_, __) => const SplashPage()),
      GoRoute(path: '/sign-in', builder: (_, __) => const SignInPage()),
      GoRoute(path: '/gate/password', builder: (_, __) => const ChangePasswordPage(forced: true)),
      GoRoute(path: '/gate/mfa', builder: (_, __) => const MfaEnrollPage(forced: true)),
      ShellRoute(
        builder: (context, state, child) => AppShell(location: state.matchedLocation, child: child),
        routes: [
          GoRoute(path: '/', builder: (_, __) => const HomePage()),
          if (kDebugMode)
            GoRoute(
              path: '/preview',
              builder: (_, st) => HomePreviewPage(theme: st.uri.queryParameters['theme'], lang: st.uri.queryParameters['lang']),
              routes: [
                // second page inside the shell, to check page transitions without signing in
                GoRoute(path: 'next', builder: (_, st) => const HomePreviewPage()),
              ],
            ),
          GoRoute(path: '/inbox', builder: (_, __) => const InboxPage()),
          GoRoute(path: '/requests', builder: (_, __) => const MyRequestsPage(), routes: [
            GoRoute(path: 'new', builder: (_, __) => const NewRequestPage()),
            GoRoute(path: ':id', builder: (_, st) => RequestDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/leaves', builder: (_, __) => const LeavesPage(), routes: [
            GoRoute(path: 'new', builder: (_, __) => const NewLeavePage()),
          ]),
          GoRoute(path: '/attendance', builder: (_, __) => const AttendancePage(), routes: [
            GoRoute(path: 'correction', builder: (_, st) => CorrectionPage(date: st.uri.queryParameters['date'])),
            GoRoute(path: 'day/:id', builder: (_, st) => AttendanceDayDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/notifications', builder: (_, __) => const NotificationsPage()),
          GoRoute(path: '/employees', builder: (_, __) => const EmployeesPage(), routes: [
            GoRoute(path: 'new', builder: (_, __) => const EmployeeCreatePage()),
            GoRoute(path: ':id', builder: (_, st) => EmployeeFilePage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/team', builder: (_, __) => const TeamPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => TeamMemberPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/departments', builder: (_, __) => const DepartmentsPage()),
          GoRoute(path: '/accounts', builder: (_, __) => const AccountsPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => AccountDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/roles', builder: (_, __) => const RolesPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => RoleDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/delegations', builder: (_, __) => const DelegationsPage()),
          GoRoute(path: '/shifts', builder: (_, __) => const ShiftsPage(), routes: [
            GoRoute(path: 'new', builder: (_, __) => const ShiftEditorPage()),
            GoRoute(path: ':id', builder: (_, st) => ShiftEditorPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/holidays', builder: (_, __) => const HolidaysPage()),
          GoRoute(path: '/attendance-admin', builder: (_, __) => const AttendanceAdminPage()),
          GoRoute(path: '/devices', builder: (_, __) => const DevicesPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => DeviceDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/attendance-import', builder: (_, __) => const AttendanceImportPage()),
          GoRoute(path: '/attendance-settings', builder: (_, __) => const AttendanceSettingsPage()),
          GoRoute(path: '/leave-balances', builder: (_, __) => const LeaveAdminPage()),
          GoRoute(path: '/leave-types', builder: (_, __) => const OtherLeaveTypesPage()),
          GoRoute(path: '/memos', builder: (_, __) => const MemosPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => MemoDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/incidents', builder: (_, __) => const IncidentsPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => IncidentDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/complaints', builder: (_, __) => const ComplaintsPage(), routes: [
            GoRoute(path: 'new', builder: (_, __) => const NewComplaintPage()),
            GoRoute(path: 'my/:id', builder: (_, st) => MyComplaintPage(id: st.pathParameters['id']!)),
            GoRoute(path: 'open/:id', builder: (_, st) => ComplaintOpenPage(id: st.pathParameters['id']!)),
            GoRoute(path: ':id', builder: (_, st) => HandlerComplaintPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/complaint-handlers', builder: (_, __) => const ComplaintHandlersPage()),
          GoRoute(path: '/reports', builder: (_, __) => const ReportsPage(), routes: [
            GoRoute(path: 'review', builder: (_, __) => const ReviewDashboardPage()),
            GoRoute(path: ':code', builder: (_, st) => ReportViewPage(key: ValueKey(st.pathParameters['code']), code: st.pathParameters['code']!)),
          ]),
          GoRoute(path: '/system-settings', builder: (_, __) => const SystemSettingsPage()),
          GoRoute(path: '/request-types', builder: (_, __) => const RequestTypesPage()),
          GoRoute(path: '/audit-log', builder: (_, __) => const AuditLogPage()),
          // release 2
          GoRoute(path: '/store', builder: (_, __) => const StorePage()),
          GoRoute(path: '/custody', builder: (_, __) => const CustodyPage()),
          GoRoute(path: '/evaluations', builder: (_, __) => const EvaluationsPage(), routes: [
            GoRoute(path: 'open/:id', builder: (_, st) => EvaluationOpenPage(key: ValueKey(st.pathParameters['id']), id: st.pathParameters['id']!)),
            GoRoute(path: 'my/:id', builder: (_, st) => MyEvaluationPage(id: st.pathParameters['id']!)),
            GoRoute(path: ':id', builder: (_, st) => EvaluationFormPage(key: ValueKey(st.pathParameters['id']), id: st.pathParameters['id']!), routes: [
              GoRoute(path: 'hr', builder: (_, st) => EvaluationHrPage(id: st.pathParameters['id']!)),
            ]),
          ]),
          GoRoute(path: '/evaluation-admin', builder: (_, __) => const EvaluationAdminPage()),
          GoRoute(path: '/review-reports', builder: (_, __) => const ReviewReportsPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => ReviewReportPage(key: ValueKey(st.pathParameters['id']), id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/department-reports', builder: (_, __) => const DepartmentReportsPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => DepartmentReportPage(key: ValueKey(st.pathParameters['id']), id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/circulars', builder: (_, __) => const CircularsPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => CircularDetailPage(id: st.pathParameters['id']!)),
          ]),
          // release 3
          GoRoute(path: '/contracts', builder: (_, __) => const ContractsPage(), routes: [
            GoRoute(path: ':id', builder: (_, st) => ContractDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/warnings', builder: (_, __) => const WarningsPage(), routes: [
            GoRoute(path: 'my/:id', builder: (_, st) => MyWarningPage(id: st.pathParameters['id']!)),
            GoRoute(path: ':id', builder: (_, st) => WarningDetailPage(id: st.pathParameters['id']!)),
          ]),
          GoRoute(path: '/appreciation', builder: (_, __) => const AppreciationLettersPage()),
          GoRoute(path: '/finance', builder: (_, __) => const FinancePage(), routes: [
            GoRoute(path: 'payroll/:id', builder: (_, st) => PayrollRunPage(id: st.pathParameters['id']!)),
            GoRoute(path: 'salary/:employeeId', builder: (_, st) => SalaryProfilePage(key: ValueKey(st.pathParameters['employeeId']), employeeId: st.pathParameters['employeeId']!)),
          ]),
          GoRoute(path: '/payslips', builder: (_, __) => const PayslipsPage()),
          GoRoute(path: '/team-calendar', builder: (_, __) => const TeamLeaveCalendarPage()),
          GoRoute(path: '/profile', builder: (_, __) => const ProfilePage(), routes: [
            GoRoute(path: 'password', builder: (_, __) => const ChangePasswordPage(forced: false)),
            GoRoute(path: 'mfa', builder: (_, __) => const MfaEnrollPage(forced: false)),
          ]),
        ],
      ),
    ],
  );
});
