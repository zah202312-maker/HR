import 'package:flutter/widgets.dart';

import '../../api/generated/models.dart';
import '../../api/runtime/api_client.dart';
import 'error_messages.dart';

/// UI texts in Arabic (default) and English. Server texts (statusLabel, detailsSummary,
/// notification title/body) are already localized by Accept-Language and shown as is.
class S {
  const S(this.isAr);
  final bool isAr;

  static S of(BuildContext context) => S(Localizations.localeOf(context).languageCode != 'en');

  String _(String ar, String en) => isAr ? ar : en;

  /// Inline bilingual label for screen-specific texts.
  String t(String ar, String en) => isAr ? ar : en;

  // App and shell
  String get appName => 'HR';
  String get appTagline => _('نظام إدارة الموارد البشرية', 'Human Resources Management System');
  String get home => _('الرئيسية', 'Home');
  String get myRequests => _('طلباتي', 'My requests');
  String get inbox => _('بانتظار إجرائي', 'Awaiting my action');
  String get leaves => _('الإجازات', 'Leave');
  String get attendance => _('حضوري', 'My attendance');
  String get notifications => _('الإشعارات', 'Notifications');
  String get profile => _('ملفي', 'My profile');
  String get settings => _('الإعدادات', 'Settings');
  String get signOut => _('تسجيل الخروج', 'Sign out');
  String get language => _('اللغة', 'Language');
  String get theme => _('المظهر', 'Appearance');
  String get themeSystem => _('حسب الجهاز', 'System');
  String get themeLight => _('فاتح', 'Light');
  String get themeDark => _('داكن', 'Dark');
  String get menu => _('القائمة', 'Menu');

  // Common
  String get retry => _('إعادة المحاولة', 'Retry');
  String get cancel => _('إلغاء', 'Cancel');
  String get save => _('حفظ', 'Save');
  String get submit => _('تقديم', 'Submit');
  String get confirm => _('تأكيد', 'Confirm');
  String get close => _('إغلاق', 'Close');
  String get loadMore => _('عرض المزيد', 'Load more');
  String get empty => _('لا توجد بيانات', 'Nothing here yet');
  String get requiredField => _('هذا الحقل مطلوب', 'This field is required');
  String get reason => _('السبب', 'Reason');
  String get note => _('ملاحظة (اختيارية)', 'Note (optional)');
  String get all => _('الكل', 'All');
  String get from => _('من', 'From');
  String get to => _('إلى', 'To');
  String get date => _('التاريخ', 'Date');
  String get status => _('الحالة', 'Status');
  String get details => _('التفاصيل', 'Details');
  String get done => _('تم', 'Done');
  String get saved => _('تم الحفظ', 'Saved');
  String get days => _('يوم', 'days');
  String get hours => _('ساعة', 'hours');
  String get minutes => _('دقيقة', 'min');

  // Errors and states
  String get networkError => _('تعذر الاتصال بالخادم. تحقق من الشبكة وأعد المحاولة.',
      'Could not reach the server. Check your connection and try again.');
  String get unknownError => _('حدث خطأ غير متوقع.', 'Something went wrong.');
  String get forbidden => _('ليس لديك صلاحية لهذا الإجراء.', 'You do not have permission for this.');
  String get notFound => _('العنصر غير موجود أو لا يمكنك الوصول إليه.', 'Not found, or you cannot access it.');
  String get conflict => _('تغيرت البيانات منذ فتحها. أعد التحميل ثم حاول مرة أخرى.',
      'This data changed since you opened it. Reload and try again.');
  String get validation => _('تحقق من الحقول المدخلة.', 'Check the highlighted fields.');
  String get sessionExpired => _('انتهت الجلسة. سجل الدخول مرة أخرى.', 'Your session ended. Please sign in again.');

  String error(Object error) {
    if (error is ApiException) {
      final code = error.code;
      if (code != null) {
        final mapped = (isAr ? hrErrorMessagesAr : hrErrorMessagesEn)[code];
        if (mapped != null) return mapped;
      }
      if (error.isNetworkError) return networkError;
      if (error.isUnauthorized) return sessionExpired;
      if (error.isForbidden) return forbidden;
      if (error.isNotFound) return notFound;
      if (error.isConflict) return conflict;
      if (error.isValidation) {
        final msgs = error.validationErrors.map((v) => v.message).whereType<String>().toList();
        return msgs.isEmpty ? validation : msgs.join('\n');
      }
      return error.message ?? unknownError;
    }
    return unknownError;
  }

  // Auth
  String get signIn => _('تسجيل الدخول', 'Sign in');
  String get signInHint => _('يتم الدخول عبر صفحة الدخول الآمنة على الخادم.',
      'You sign in on the secure server login page.');
  String get signInFailed => _('لم يكتمل تسجيل الدخول.', 'Sign-in was not completed.');
  String get changePassword => _('تغيير كلمة السر', 'Change password');
  String get changePasswordRequired => _('يجب تغيير كلمة السر المؤقتة قبل المتابعة.',
      'You must change your temporary password before continuing.');
  String get currentPassword => _('كلمة السر الحالية', 'Current password');
  String get newPassword => _('كلمة السر الجديدة', 'New password');
  String get confirmPassword => _('تأكيد كلمة السر الجديدة', 'Confirm new password');
  String get passwordsDoNotMatch => _('كلمتا السر غير متطابقتين', 'Passwords do not match');
  String get passwordChanged => _('تم تغيير كلمة السر', 'Password changed');
  String get mfa => _('التحقق بخطوتين', 'Two-step verification');
  String get mfaRequired => _('دورك يتطلب تفعيل التحقق بخطوتين قبل المتابعة.',
      'Your role requires two-step verification before you continue.');
  String get mfaStep1 => _('امسح الرمز بتطبيق المصادقة أو أدخل المفتاح يدوياً.',
      'Scan the code with your authenticator app, or enter the key manually.');
  String get mfaStep2 => _('أدخل الرمز المكوّن من 6 أرقام من التطبيق.', 'Enter the 6-digit code from the app.');
  String get mfaStart => _('بدء التفعيل', 'Start setup');
  String get mfaCode => _('رمز التحقق', 'Verification code');
  String get mfaKey => _('المفتاح', 'Key');
  String get mfaEnabled => _('مفعّل', 'Enabled');
  String get mfaDisabled => _('غير مفعّل', 'Not enabled');
  String get recoveryCodes => _('رموز الاسترداد', 'Recovery codes');
  String get recoveryCodesHint => _('احفظ هذه الرموز الآن. لن تظهر مرة أخرى، وكل رمز يعمل مرة واحدة.',
      'Save these codes now. They will not be shown again, and each works once.');
  String get regenerateCodes => _('إصدار رموز جديدة', 'New recovery codes');
  String get continueLabel => _('متابعة', 'Continue');
  String get security => _('الأمان', 'Security');

  // Home
  String greeting(String name) => _('مرحباً، $name', 'Welcome, $name');
  String get pendingActions => _('بانتظار إجرائي', 'Awaiting my action');
  String get nothingPending => _('لا شيء بانتظارك الآن.', 'Nothing is waiting for you.');
  String get viaDelegation => _('عبر تفويض', 'via delegation');
  String get quickActions => _('إجراءات سريعة', 'Quick actions');
  String get remainingBalance => _('الرصيد المتبقي', 'Remaining balance');
  String get unread => _('غير مقروءة', 'Unread');

  String inboxCategory(InboxCategory c) => switch (c) {
        InboxCategory.request => _('طلبات', 'Requests'),
        InboxCategory.memo => _('مطالعات', 'Memos'),
        InboxCategory.incident => _('بلاغات', 'Incidents'),
        InboxCategory.stockIssue => _('صرف مخزن', 'Stock issues'),
        InboxCategory.evaluation => _('تقييمات', 'Evaluations'),
        InboxCategory.departmentReport => _('تقارير الأقسام', 'Department reports'),
        InboxCategory.evaluationObjection => _('اعتراضات التقييم', 'Evaluation objections'),
        InboxCategory.evaluationAcknowledgement => _('اطلاع على التقييم', 'Evaluation acknowledgements'),
        InboxCategory.circularAcknowledgement => _('اطلاع على التعاميم', 'Circular acknowledgements'),
        InboxCategory.custodyAcknowledgement => _('استلام العهد', 'Custody acknowledgements'),
        InboxCategory.warningResponse => _('رد على إنذار', 'Warning responses'),
        InboxCategory.warningDecision => _('قرارات الإنذار', 'Warning decisions'),
        InboxCategory.contractDecision => _('قرارات العقود', 'Contract decisions'),
        InboxCategory.appreciationLetterAcknowledgement => _('كتب الشكر', 'Appreciation letters'),
        InboxCategory.$unknown => _('أخرى', 'Other'),
      };

  // Requests
  String get newRequest => _('طلب جديد', 'New request');
  String get requestType => _('نوع الطلب', 'Request type');
  String get description => _('الوصف', 'Description');
  String get item => _('الصنف', 'Item');
  String get quantity => _('الكمية', 'Quantity');
  String get approve => _('موافقة', 'Approve');
  String get reject => _('رفض', 'Reject');
  String get cancelRequest => _('إلغاء الطلب', 'Cancel request');
  String get approved => _('تمت الموافقة', 'Approved');
  String get rejected => _('تم الرفض', 'Rejected');
  String get cancelled => _('تم الإلغاء', 'Cancelled');
  String get submitted => _('تم تقديم الطلب', 'Request submitted');
  String get steps => _('خطوات الموافقة', 'Approval steps');
  String get timeline => _('السجل الزمني', 'Timeline');
  String get rejectionReason => _('سبب الرفض', 'Rejection reason');
  String get submittedBy => _('قدّمه', 'Submitted by');
  String get onBehalf => _('نيابة عن الموظف', 'On behalf');
  String get employee => _('الموظف', 'Employee');
  String get waitingSince => _('بانتظار منذ', 'Waiting since');
  String get delegatedFrom => _('مفوّض من', 'Delegated by');
  String get stockIssueFlow => _('يُنفذ من شاشة صرف المخزن', 'Handled in the stock issue screen');
  String get dedicatedFlow => _('هذا النوع يُقدم من شاشته الخاصة', 'This type has its own form');

  String role(AppRole? r) => switch (r) {
        AppRole.employee => _('موظف', 'Employee'),
        AppRole.departmentManager => _('مدير القسم', 'Department manager'),
        AppRole.hrManager => _('الموارد البشرية', 'HR manager'),
        AppRole.executiveDirector => _('المدير التنفيذي', 'Executive director'),
        AppRole.reviewer => _('مراجع', 'Reviewer'),
        AppRole.accountant => _('المحاسب', 'Accountant'),
        AppRole.storeOfficer => _('مسؤول المخزن', 'Store officer'),
        AppRole.systemAdmin => _('مسؤول النظام', 'System admin'),
        _ => '—',
      };

  String requestStatus(RequestStatus s) => switch (s) {
        RequestStatus.pendingManager => _('بانتظار المدير', 'Pending manager'),
        RequestStatus.pendingHr => _('بانتظار الموارد البشرية', 'Pending HR'),
        RequestStatus.pendingExecutive => _('بانتظار المدير التنفيذي', 'Pending executive'),
        RequestStatus.pendingDestination => _('بانتظار جهة التنفيذ', 'Pending destination'),
        RequestStatus.approved => _('موافق عليه', 'Approved'),
        RequestStatus.rejected => _('مرفوض', 'Rejected'),
        RequestStatus.completed => _('مكتمل', 'Completed'),
        RequestStatus.cancelled => _('ملغى', 'Cancelled'),
        RequestStatus.$unknown => '—',
      };

  String employeeStatus(RequestEmployeeStatus s) => switch (s) {
        RequestEmployeeStatus.pendingManager => _('بانتظار مدير القسم', 'Pending manager'),
        RequestEmployeeStatus.pendingHr => _('بانتظار الموارد البشرية', 'Pending HR'),
        RequestEmployeeStatus.pendingExecutive => _('بانتظار المدير التنفيذي', 'Pending executive'),
        RequestEmployeeStatus.approved => _('موافق عليه', 'Approved'),
        RequestEmployeeStatus.rejected => _('مرفوض', 'Rejected'),
        RequestEmployeeStatus.completed => _('مكتمل', 'Completed'),
        RequestEmployeeStatus.cancelled => _('ملغى', 'Cancelled'),
        RequestEmployeeStatus.$unknown => '—',
      };

  String stepDecision(RequestStepDecision d) => switch (d) {
        RequestStepDecision.notReached => _('لم تصل بعد', 'Not reached'),
        RequestStepDecision.pending => _('بانتظار القرار', 'Pending'),
        RequestStepDecision.approved => _('موافقة', 'Approved'),
        RequestStepDecision.approvedBySubmission => _('موافقة بالتقديم', 'Approved by submission'),
        RequestStepDecision.rejected => _('رفض', 'Rejected'),
        RequestStepDecision.$unknown => '—',
      };

  // Leave
  String get myBalance => _('رصيدي', 'My balance');
  String get myLeaves => _('إجازاتي', 'My leave requests');
  String get newLeave => _('طلب إجازة', 'Request leave');
  String get entitlement => _('استحقاق السنة', 'Year entitlement');
  String get carried => _('المضافة من السنة الماضية', 'Carried from last year');
  String get used => _('المستخدم', 'Used');
  String get held => _('المحجوز', 'Held');
  String get remaining => _('المتبقي', 'Remaining');
  String get hourlyQuota => _('حصة الزمنيات هذا الشهر', 'Hourly quota this month');
  String get nextYearStart => _('بداية السنة القادمة', 'Next year starts');
  String get entitlementNotSet => _('لم يحدد مدير الموارد البشرية رصيدك بعد.', 'HR has not set your entitlement yet.');
  String get leaveKind => _('نوع الإجازة', 'Leave kind');
  String get otherLeaveType => _('نوع الإجازة الأخرى', 'Other leave type');
  String get fromTime => _('من الساعة', 'From time');
  String get toTime => _('إلى الساعة', 'To time');
  String get startDate => _('من تاريخ', 'Start date');
  String get endDate => _('إلى تاريخ', 'End date');
  String get notes => _('ملاحظات', 'Notes');
  String get attachments => _('المرفقات', 'Attachments');
  String get addAttachment => _('إضافة مرفق', 'Add attachment');
  String get attachmentRequired => _('هذا النوع يتطلب مرفقاً', 'This type requires an attachment');
  String get preview => _('احسب', 'Calculate');
  String get workingDays => _('أيام الدوام', 'Working days');
  String get annualDays => _('من الرصيد السنوي', 'From annual balance');
  String get remainingAfter => _('المتبقي بعد الطلب', 'Remaining after');
  String get shortage => _('النقص', 'Shortage');
  String get requestedHours => _('الساعات المطلوبة', 'Requested hours');

  String leaveKindLabel(LeaveKind k) => switch (k) {
        LeaveKind.hourly => _('زمنية', 'Hourly'),
        LeaveKind.annual => _('اعتيادية من الرصيد', 'Annual'),
        LeaveKind.other => _('نوع آخر', 'Other type'),
        LeaveKind.$unknown => '—',
      };

  String balanceEffect(LeaveBalanceEffect e) => switch (e) {
        LeaveBalanceEffect.none => _('بدون أثر', 'No effect'),
        LeaveBalanceEffect.held => _('محجوز', 'Held'),
        LeaveBalanceEffect.deducted => _('مخصوم', 'Deducted'),
        LeaveBalanceEffect.released => _('مفكوك', 'Released'),
        LeaveBalanceEffect.$unknown => '—',
      };

  // Attendance
  String get correction => _('طلب تصحيح بصمة', 'Punch correction');
  String get punchType => _('نوع البصمة', 'Punch type');
  String get correctTime => _('الوقت الصحيح', 'Correct time');
  String get checkIn => _('دخول', 'Check-in');
  String get checkOut => _('خروج', 'Check-out');
  String get lateLabel => _('تأخر', 'Late');
  String get worked => _('عمل', 'Worked');
  String get overtime => _('إضافي', 'Overtime');
  String get corrected => _('مصحح', 'Corrected');

  String punch(AttendancePunchType t) => t == AttendancePunchType.checkOut ? checkOut : checkIn;

  String dayStatus(AttendanceDayStatus s) => switch (s) {
        AttendanceDayStatus.present => _('حاضر', 'Present'),
        AttendanceDayStatus.late => _('متأخر', 'Late'),
        AttendanceDayStatus.absent => _('غائب', 'Absent'),
        AttendanceDayStatus.incomplete => _('ناقص', 'Incomplete'),
        AttendanceDayStatus.onLeave => _('إجازة', 'On leave'),
        AttendanceDayStatus.holiday => _('عطلة رسمية', 'Holiday'),
        AttendanceDayStatus.weeklyOff => _('عطلة أسبوعية', 'Weekly off'),
        AttendanceDayStatus.noShift => _('بدون وردية', 'No shift'),
        AttendanceDayStatus.$unknown => '—',
      };

  // Notifications
  String get markAllHint => _('اضغط على الإشعار لتعليمه كمقروء', 'Tap a notification to mark it read');
  String get onlyUnread => _('غير المقروءة فقط', 'Unread only');

  // Profile
  String get account => _('الحساب', 'Account');
  String get department => _('القسم', 'Department');
  String get jobTitle => _('المسمى الوظيفي', 'Job title');
  String get hireDate => _('تاريخ المباشرة', 'Hire date');
  String get manager => _('المدير المباشر', 'Manager');
  String get email => _('البريد الإلكتروني', 'Email');
  String get phone => _('الهاتف', 'Phone');
  String get employeeNumber => _('الرقم الوظيفي', 'Employee number');
  String get roles => _('الأدوار', 'Roles');
  String get shift => _('الوردية', 'Shift');
  String get notificationLanguage => _('لغة الإشعارات والبريد', 'Notification and email language');
  String get readOnlyProfile => _('ملفك للقراءة فقط، والتعديل لدى الموارد البشرية.',
      'Your file is read-only; HR makes changes.');
}
