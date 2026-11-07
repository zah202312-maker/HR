// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

/// Contract metadata per operation, used to show or hide UI.
/// x-permission is the server check; x-allowed-roles is the UI contract.
class HrOperationMeta {
  const HrOperationMeta({required this.method, required this.path, required this.permission, required this.roles, required this.release, required this.errorCodes});
  final String method;
  final String path;
  final String? permission;
  final List<String> roles;
  final String release;
  final List<String> errorCodes;
}

class HrOps {
  static const String OpenIdConfiguration_Get = 'OpenIdConfiguration_Get';
  static const String ConnectToken_Issue = 'ConnectToken_Issue';
  static const String ApplicationConfiguration_Get = 'ApplicationConfiguration_Get';
  static const String Me_Get = 'Me_Get';
  static const String Me_Update = 'Me_Update';
  static const String Me_GetEmployeeProfile = 'Me_GetEmployeeProfile';
  static const String Me_GetInboxSummary = 'Me_GetInboxSummary';
  static const String Me_GetAdministrativeTaskLog = 'Me_GetAdministrativeTaskLog';
  static const String Account_ChangePassword = 'Account_ChangePassword';
  static const String Mfa_GetStatus = 'Mfa_GetStatus';
  static const String Mfa_StartEnrollment = 'Mfa_StartEnrollment';
  static const String Mfa_ConfirmEnrollment = 'Mfa_ConfirmEnrollment';
  static const String Mfa_RegenerateRecoveryCodes = 'Mfa_RegenerateRecoveryCodes';
  static const String UserAccount_ResetMfa = 'UserAccount_ResetMfa';
  static const String UserAccount_List = 'UserAccount_List';
  static const String UserAccount_Create = 'UserAccount_Create';
  static const String UserAccount_Get = 'UserAccount_Get';
  static const String UserAccount_ResetPassword = 'UserAccount_ResetPassword';
  static const String UserAccount_SetRoles = 'UserAccount_SetRoles';
  static const String UserAccount_GetPermissions = 'UserAccount_GetPermissions';
  static const String UserAccount_UpdatePermissions = 'UserAccount_UpdatePermissions';
  static const String Role_List = 'Role_List';
  static const String Role_Create = 'Role_Create';
  static const String Role_Get = 'Role_Get';
  static const String Role_Update = 'Role_Update';
  static const String Role_Delete = 'Role_Delete';
  static const String Role_GetPermissions = 'Role_GetPermissions';
  static const String Role_UpdatePermissions = 'Role_UpdatePermissions';
  static const String Department_List = 'Department_List';
  static const String Department_Create = 'Department_Create';
  static const String Department_Lookup = 'Department_Lookup';
  static const String Department_Get = 'Department_Get';
  static const String Department_Update = 'Department_Update';
  static const String Department_Delete = 'Department_Delete';
  static const String Employee_List = 'Employee_List';
  static const String Employee_Create = 'Employee_Create';
  static const String Employee_Lookup = 'Employee_Lookup';
  static const String Employee_Get = 'Employee_Get';
  static const String Employee_Update = 'Employee_Update';
  static const String Employee_CorrectHireDate = 'Employee_CorrectHireDate';
  static const String Employee_ChangeManager = 'Employee_ChangeManager';
  static const String Employee_GetManagerHistory = 'Employee_GetManagerHistory';
  static const String Employee_GetAdministrativeTaskLog = 'Employee_GetAdministrativeTaskLog';
  static const String Manager_AssignEmployees = 'Manager_AssignEmployees';
  static const String Manager_GetMyTeam = 'Manager_GetMyTeam';
  static const String Manager_GetTeamMember = 'Manager_GetTeamMember';
  static const String EmployeeDocument_List = 'EmployeeDocument_List';
  static const String EmployeeDocument_Upload = 'EmployeeDocument_Upload';
  static const String EmployeeDocument_GetDownloadUrl = 'EmployeeDocument_GetDownloadUrl';
  static const String EmployeeDocument_Delete = 'EmployeeDocument_Delete';
  static const String Delegation_List = 'Delegation_List';
  static const String Delegation_Create = 'Delegation_Create';
  static const String Delegation_GetMy = 'Delegation_GetMy';
  static const String Delegation_GetCandidates = 'Delegation_GetCandidates';
  static const String Delegation_Cancel = 'Delegation_Cancel';
  static const String WorkShift_List = 'WorkShift_List';
  static const String WorkShift_Create = 'WorkShift_Create';
  static const String WorkShift_Get = 'WorkShift_Get';
  static const String WorkShift_Update = 'WorkShift_Update';
  static const String EmployeeShift_Assign = 'EmployeeShift_Assign';
  static const String EmployeeShift_GetHistory = 'EmployeeShift_GetHistory';
  static const String Holiday_List = 'Holiday_List';
  static const String Holiday_Create = 'Holiday_Create';
  static const String Holiday_Update = 'Holiday_Update';
  static const String Holiday_Delete = 'Holiday_Delete';
  static const String WeeklyOffSetting_Get = 'WeeklyOffSetting_Get';
  static const String WeeklyOffSetting_Update = 'WeeklyOffSetting_Update';
  static const String Attendance_GetMy = 'Attendance_GetMy';
  static const String Attendance_GetTeam = 'Attendance_GetTeam';
  static const String Attendance_GetByEmployee = 'Attendance_GetByEmployee';
  static const String Attendance_GetDayDetail = 'Attendance_GetDayDetail';
  static const String AttendanceDevice_IngestPunches = 'AttendanceDevice_IngestPunches';
  static const String AttendanceImport_Create = 'AttendanceImport_Create';
  static const String AttendanceImport_List = 'AttendanceImport_List';
  static const String AttendanceImport_Get = 'AttendanceImport_Get';
  static const String AttendanceSettings_Get = 'AttendanceSettings_Get';
  static const String AttendanceSettings_Update = 'AttendanceSettings_Update';
  static const String AttendanceDevice_List = 'AttendanceDevice_List';
  static const String AttendanceDevice_Create = 'AttendanceDevice_Create';
  static const String AttendanceDevice_Update = 'AttendanceDevice_Update';
  static const String AttendanceDevice_RotateKey = 'AttendanceDevice_RotateKey';
  static const String DeviceUserMap_List = 'DeviceUserMap_List';
  static const String DeviceUserMap_Create = 'DeviceUserMap_Create';
  static const String DeviceUserMap_Delete = 'DeviceUserMap_Delete';
  static const String DeviceUserMap_ListUnmapped = 'DeviceUserMap_ListUnmapped';
  static const String AttendanceCorrection_Create = 'AttendanceCorrection_Create';
  static const String AttendanceCorrection_Get = 'AttendanceCorrection_Get';
  static const String Attendance_CreateGpsPunch = 'Attendance_CreateGpsPunch';
  static const String Attendance_ListFlaggedGpsPunches = 'Attendance_ListFlaggedGpsPunches';
  static const String GeoFence_List = 'GeoFence_List';
  static const String GeoFence_Create = 'GeoFence_Create';
  static const String GeoFence_Update = 'GeoFence_Update';
  static const String GeoFence_Delete = 'GeoFence_Delete';
  static const String LeaveBalance_GetMy = 'LeaveBalance_GetMy';
  static const String LeaveBalance_GetByEmployee = 'LeaveBalance_GetByEmployee';
  static const String LeaveBalance_SetEntitlement = 'LeaveBalance_SetEntitlement';
  static const String LeaveBalance_Adjust = 'LeaveBalance_Adjust';
  static const String LeaveLedger_GetByEmployee = 'LeaveLedger_GetByEmployee';
  static const String LeaveLedger_GetMy = 'LeaveLedger_GetMy';
  static const String LeaveRequest_Preview = 'LeaveRequest_Preview';
  static const String LeaveRequest_Create = 'LeaveRequest_Create';
  static const String LeaveRequest_ListMy = 'LeaveRequest_ListMy';
  static const String LeaveRequest_ListByEmployee = 'LeaveRequest_ListByEmployee';
  static const String LeaveRequest_Reverse = 'LeaveRequest_Reverse';
  static const String LeaveRequest_Get = 'LeaveRequest_Get';
  static const String LeaveRequest_UploadAttachment = 'LeaveRequest_UploadAttachment';
  static const String LeaveRequest_GetAttachmentDownloadUrl = 'LeaveRequest_GetAttachmentDownloadUrl';
  static const String OtherLeaveType_List = 'OtherLeaveType_List';
  static const String OtherLeaveType_Create = 'OtherLeaveType_Create';
  static const String OtherLeaveType_Update = 'OtherLeaveType_Update';
  static const String OtherLeaveType_ListAvailable = 'OtherLeaveType_ListAvailable';
  static const String LeaveCalendar_GetTeam = 'LeaveCalendar_GetTeam';
  static const String RequestType_GetList = 'RequestType_GetList';
  static const String RequestType_Create = 'RequestType_Create';
  static const String RequestType_GetAdminList = 'RequestType_GetAdminList';
  static const String RequestType_Get = 'RequestType_Get';
  static const String RequestType_Update = 'RequestType_Update';
  static const String Request_Submit = 'Request_Submit';
  static const String Request_GetList = 'Request_GetList';
  static const String Request_GetMy = 'Request_GetMy';
  static const String Request_GetInbox = 'Request_GetInbox';
  static const String Request_Get = 'Request_Get';
  static const String Request_Approve = 'Request_Approve';
  static const String Request_Reject = 'Request_Reject';
  static const String Request_Cancel = 'Request_Cancel';
  static const String Memo_Create = 'Memo_Create';
  static const String Memo_GetMy = 'Memo_GetMy';
  static const String Memo_GetInbox = 'Memo_GetInbox';
  static const String Memo_Get = 'Memo_Get';
  static const String Memo_Forward = 'Memo_Forward';
  static const String Memo_Convert = 'Memo_Convert';
  static const String Memo_Close = 'Memo_Close';
  static const String Incident_Create = 'Incident_Create';
  static const String Incident_GetMy = 'Incident_GetMy';
  static const String Incident_GetInbox = 'Incident_GetInbox';
  static const String Incident_Get = 'Incident_Get';
  static const String Incident_UpdateStatus = 'Incident_UpdateStatus';
  static const String Incident_Forward = 'Incident_Forward';
  static const String Item_GetList = 'Item_GetList';
  static const String Item_Create = 'Item_Create';
  static const String Item_Lookup = 'Item_Lookup';
  static const String Item_Get = 'Item_Get';
  static const String Item_Update = 'Item_Update';
  static const String Stock_Receive = 'Stock_Receive';
  static const String Stock_Adjust = 'Stock_Adjust';
  static const String Stock_GetMovements = 'Stock_GetMovements';
  static const String StockIssue_GetList = 'StockIssue_GetList';
  static const String StockIssue_Get = 'StockIssue_Get';
  static const String StockIssue_Acknowledge = 'StockIssue_Acknowledge';
  static const String StockIssue_MarkPurchasing = 'StockIssue_MarkPurchasing';
  static const String StockIssue_Deliver = 'StockIssue_Deliver';
  static const String Custody_Create = 'Custody_Create';
  static const String Custody_GetList = 'Custody_GetList';
  static const String Custody_GetMy = 'Custody_GetMy';
  static const String Custody_GetTeam = 'Custody_GetTeam';
  static const String Custody_Get = 'Custody_Get';
  static const String Custody_Acknowledge = 'Custody_Acknowledge';
  static const String Custody_Return = 'Custody_Return';
  static const String CustodyCount_Create = 'CustodyCount_Create';
  static const String CustodyCount_GetList = 'CustodyCount_GetList';
  static const String Complaint_GetRespondents = 'Complaint_GetRespondents';
  static const String Complaint_PreviewRouting = 'Complaint_PreviewRouting';
  static const String Complaint_Submit = 'Complaint_Submit';
  static const String Complaint_GetMyList = 'Complaint_GetMyList';
  static const String Complaint_GetMyDetail = 'Complaint_GetMyDetail';
  static const String Complaint_GetAssignedList = 'Complaint_GetAssignedList';
  static const String Complaint_GetDetail = 'Complaint_GetDetail';
  static const String Complaint_RaiseLevel = 'Complaint_RaiseLevel';
  static const String Complaint_AddAction = 'Complaint_AddAction';
  static const String Complaint_Forward = 'Complaint_Forward';
  static const String Complaint_Close = 'Complaint_Close';
  static const String Complaint_GetAccessLog = 'Complaint_GetAccessLog';
  static const String Complaint_UploadAttachment = 'Complaint_UploadAttachment';
  static const String Complaint_DownloadAttachment = 'Complaint_DownloadAttachment';
  static const String ComplaintHandler_Request = 'ComplaintHandler_Request';
  static const String ComplaintHandler_GetList = 'ComplaintHandler_GetList';
  static const String ComplaintHandler_Approve = 'ComplaintHandler_Approve';
  static const String ComplaintHandler_Reject = 'ComplaintHandler_Reject';
  static const String ComplaintHandler_Revoke = 'ComplaintHandler_Revoke';
  static const String EvaluationCycle_GetCurrent = 'EvaluationCycle_GetCurrent';
  static const String Evaluation_GetTeamList = 'Evaluation_GetTeamList';
  static const String Evaluation_GetManagerForm = 'Evaluation_GetManagerForm';
  static const String Evaluation_SaveDraft = 'Evaluation_SaveDraft';
  static const String Evaluation_Submit = 'Evaluation_Submit';
  static const String Evaluation_GetMyList = 'Evaluation_GetMyList';
  static const String Evaluation_GetMyDetail = 'Evaluation_GetMyDetail';
  static const String Evaluation_Acknowledge = 'Evaluation_Acknowledge';
  static const String Evaluation_AddResponse = 'Evaluation_AddResponse';
  static const String Evaluation_DecideObjection = 'Evaluation_DecideObjection';
  static const String Evaluation_GetHrList = 'Evaluation_GetHrList';
  static const String Evaluation_GetHrView = 'Evaluation_GetHrView';
  static const String EvaluationInternalNote_GetList = 'EvaluationInternalNote_GetList';
  static const String EvaluationInternalNote_Add = 'EvaluationInternalNote_Add';
  static const String EvaluationInternalNote_SaveDraft = 'EvaluationInternalNote_SaveDraft';
  static const String EvaluationAnnualSummary_GetMy = 'EvaluationAnnualSummary_GetMy';
  static const String EvaluationAnnualSummary_GetForEmployee = 'EvaluationAnnualSummary_GetForEmployee';
  static const String EvaluationCriterion_GetList = 'EvaluationCriterion_GetList';
  static const String EvaluationCriterion_Create = 'EvaluationCriterion_Create';
  static const String EvaluationCriterion_Update = 'EvaluationCriterion_Update';
  static const String EvaluationGrade_GetList = 'EvaluationGrade_GetList';
  static const String EvaluationGrade_Create = 'EvaluationGrade_Create';
  static const String EvaluationGrade_Update = 'EvaluationGrade_Update';
  static const String DepartmentReport_GetList = 'DepartmentReport_GetList';
  static const String DepartmentReport_Get = 'DepartmentReport_Get';
  static const String DepartmentReport_UpdateNotes = 'DepartmentReport_UpdateNotes';
  static const String DepartmentReport_Submit = 'DepartmentReport_Submit';
  static const String ReviewReport_GetList = 'ReviewReport_GetList';
  static const String ReviewReport_Create = 'ReviewReport_Create';
  static const String ReviewReport_Get = 'ReviewReport_Get';
  static const String ReviewReport_Update = 'ReviewReport_Update';
  static const String ReviewReport_Delete = 'ReviewReport_Delete';
  static const String ReviewReport_Submit = 'ReviewReport_Submit';
  static const String Report_GetTeamAttendance = 'Report_GetTeamAttendance';
  static const String Report_GetTeamLatenessAbsence = 'Report_GetTeamLatenessAbsence';
  static const String Report_GetTeamLeaves = 'Report_GetTeamLeaves';
  static const String Report_GetTeamPending = 'Report_GetTeamPending';
  static const String Report_GetTeamCustody = 'Report_GetTeamCustody';
  static const String Report_GetTeamEvaluations = 'Report_GetTeamEvaluations';
  static const String Report_GetReviewDashboard = 'Report_GetReviewDashboard';
  static const String Report_GetEvaluationConsistency = 'Report_GetEvaluationConsistency';
  static const String Report_GetHrAttendance = 'Report_GetHrAttendance';
  static const String Report_GetHrLeaveBalances = 'Report_GetHrLeaveBalances';
  static const String Report_GetHrRequests = 'Report_GetHrRequests';
  static const String Report_GetHrContracts = 'Report_GetHrContracts';
  static const String Report_GetHrWarnings = 'Report_GetHrWarnings';
  static const String Report_Export = 'Report_Export';
  static const String Circular_GetReceivedList = 'Circular_GetReceivedList';
  static const String Circular_Create = 'Circular_Create';
  static const String Circular_GetSentList = 'Circular_GetSentList';
  static const String Circular_Get = 'Circular_Get';
  static const String Circular_GetReceipts = 'Circular_GetReceipts';
  static const String Circular_Acknowledge = 'Circular_Acknowledge';
  static const String Contract_List = 'Contract_List';
  static const String Contract_Create = 'Contract_Create';
  static const String Contract_GetExpiring = 'Contract_GetExpiring';
  static const String Contract_Get = 'Contract_Get';
  static const String Contract_Recommend = 'Contract_Recommend';
  static const String Contract_Decide = 'Contract_Decide';
  static const String Contract_GetMyList = 'Contract_GetMyList';
  static const String Contract_GetMy = 'Contract_GetMy';
  static const String Warning_Create = 'Warning_Create';
  static const String Warning_List = 'Warning_List';
  static const String Warning_GetObjections = 'Warning_GetObjections';
  static const String Warning_GetRecommendedByMe = 'Warning_GetRecommendedByMe';
  static const String Warning_GetMyList = 'Warning_GetMyList';
  static const String Warning_GetMy = 'Warning_GetMy';
  static const String Warning_GetEmployeeFile = 'Warning_GetEmployeeFile';
  static const String Warning_Get = 'Warning_Get';
  static const String Warning_GetAttachmentDownloadUrl = 'Warning_GetAttachmentDownloadUrl';
  static const String Warning_Notify = 'Warning_Notify';
  static const String Warning_Respond = 'Warning_Respond';
  static const String Warning_Decide = 'Warning_Decide';
  static const String Warning_Object = 'Warning_Object';
  static const String Warning_ResolveObjection = 'Warning_ResolveObjection';
  static const String AppreciationLetter_Create = 'AppreciationLetter_Create';
  static const String AppreciationLetter_List = 'AppreciationLetter_List';
  static const String AppreciationLetter_GetMyList = 'AppreciationLetter_GetMyList';
  static const String AppreciationLetter_GetEmployeeFile = 'AppreciationLetter_GetEmployeeFile';
  static const String AppreciationLetter_Get = 'AppreciationLetter_Get';
  static const String AppreciationLetter_GetDownloadUrl = 'AppreciationLetter_GetDownloadUrl';
  static const String AppreciationLetter_Acknowledge = 'AppreciationLetter_Acknowledge';
  static const String SalaryProfile_List = 'SalaryProfile_List';
  static const String SalaryProfile_Get = 'SalaryProfile_Get';
  static const String SalaryProfile_Update = 'SalaryProfile_Update';
  static const String SalaryProfile_GetHistory = 'SalaryProfile_GetHistory';
  static const String Adjustment_List = 'Adjustment_List';
  static const String Adjustment_Create = 'Adjustment_Create';
  static const String Adjustment_Reverse = 'Adjustment_Reverse';
  static const String Adjustment_GetAttachmentDownloadUrl = 'Adjustment_GetAttachmentDownloadUrl';
  static const String AdjustmentSuggestion_List = 'AdjustmentSuggestion_List';
  static const String AdjustmentSuggestion_Approve = 'AdjustmentSuggestion_Approve';
  static const String AdjustmentSuggestion_Reject = 'AdjustmentSuggestion_Reject';
  static const String PayrollRun_List = 'PayrollRun_List';
  static const String PayrollRun_Create = 'PayrollRun_Create';
  static const String PayrollRun_Get = 'PayrollRun_Get';
  static const String PayrollRun_Recalculate = 'PayrollRun_Recalculate';
  static const String PayrollRun_RequestApproval = 'PayrollRun_RequestApproval';
  static const String PayrollRun_Approve = 'PayrollRun_Approve';
  static const String PayrollRun_Close = 'PayrollRun_Close';
  static const String PayrollRun_Export = 'PayrollRun_Export';
  static const String Payslip_GetMyList = 'Payslip_GetMyList';
  static const String Payslip_GetMy = 'Payslip_GetMy';
  static const String Payslip_DownloadMyPdf = 'Payslip_DownloadMyPdf';
  static const String Expense_List = 'Expense_List';
  static const String Expense_Create = 'Expense_Create';
  static const String Expense_Export = 'Expense_Export';
  static const String Expense_Get = 'Expense_Get';
  static const String Expense_Reverse = 'Expense_Reverse';
  static const String Expense_GetAttachmentDownloadUrl = 'Expense_GetAttachmentDownloadUrl';
  static const String AccountantRequest_List = 'AccountantRequest_List';
  static const String AccountantRequest_Get = 'AccountantRequest_Get';
  static const String FinanceReport_GetMonthlyStatement = 'FinanceReport_GetMonthlyStatement';
  static const String FinanceReport_GetAdjustments = 'FinanceReport_GetAdjustments';
  static const String FinanceReport_GetStoreExpenses = 'FinanceReport_GetStoreExpenses';
  static const String FinanceReport_Export = 'FinanceReport_Export';
  static const String Notification_List = 'Notification_List';
  static const String Notification_GetUnreadCount = 'Notification_GetUnreadCount';
  static const String Notification_MarkRead = 'Notification_MarkRead';
  static const String DeviceToken_Register = 'DeviceToken_Register';
  static const String DeviceToken_Unregister = 'DeviceToken_Unregister';
  static const String SystemSettings_ListSections = 'SystemSettings_ListSections';
  static const String SystemSettings_GetReminders = 'SystemSettings_GetReminders';
  static const String SystemSettings_UpdateReminders = 'SystemSettings_UpdateReminders';
  static const String SystemSettings_GetContracts = 'SystemSettings_GetContracts';
  static const String SystemSettings_UpdateContracts = 'SystemSettings_UpdateContracts';
  static const String SystemSettings_GetComplaints = 'SystemSettings_GetComplaints';
  static const String SystemSettings_UpdateComplaints = 'SystemSettings_UpdateComplaints';
  static const String SystemSettings_GetHourlyLeave = 'SystemSettings_GetHourlyLeave';
  static const String SystemSettings_UpdateHourlyLeave = 'SystemSettings_UpdateHourlyLeave';
  static const String SystemSettings_GetFinance = 'SystemSettings_GetFinance';
  static const String SystemSettings_UpdateFinance = 'SystemSettings_UpdateFinance';
  static const String AuditLog_List = 'AuditLog_List';
  static const String UserAccount_Lookup = 'UserAccount_Lookup';
  static const String EvaluationCycle_GetList = 'EvaluationCycle_GetList';
  static const String PayrollRun_ReturnForChanges = 'PayrollRun_ReturnForChanges';
}

const Map<String, HrOperationMeta> hrOperations = <String, HrOperationMeta>{
  'OpenIdConfiguration_Get': HrOperationMeta(method: 'GET', path: '/.well-known/openid-configuration', permission: null, roles: <String>[], release: '1', errorCodes: <String>[]),
  'ConnectToken_Issue': HrOperationMeta(method: 'POST', path: '/connect/token', permission: null, roles: <String>[], release: '1', errorCodes: <String>[]),
  'ApplicationConfiguration_Get': HrOperationMeta(method: 'GET', path: '/api/abp/application-configuration', permission: null, roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Me_Get': HrOperationMeta(method: 'GET', path: '/api/app/me', permission: null, roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Me_Update': HrOperationMeta(method: 'PATCH', path: '/api/app/me', permission: null, roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Me_GetEmployeeProfile': HrOperationMeta(method: 'GET', path: '/api/app/me/employee-profile', permission: 'Hr.Employee.ReadSelf', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Me_GetInboxSummary': HrOperationMeta(method: 'GET', path: '/api/app/me/inbox-summary', permission: null, roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Me_GetAdministrativeTaskLog': HrOperationMeta(method: 'GET', path: '/api/app/me/administrative-task-log', permission: 'Hr.Employee.ReadOwnTaskLog', roles: <String>['departmentManager'], release: '1', errorCodes: <String>[]),
  'Account_ChangePassword': HrOperationMeta(method: 'POST', path: '/api/app/account/change-password', permission: null, roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Identity:CurrentPasswordIncorrect', 'Hr:Identity:PasswordPolicyViolation', 'Hr:Identity:NewPasswordSameAsCurrent']),
  'Mfa_GetStatus': HrOperationMeta(method: 'GET', path: '/api/app/account/mfa', permission: null, roles: <String>['hrManager', 'executiveDirector', 'accountant', 'systemAdmin', 'employee'], release: '1', errorCodes: <String>[]),
  'Mfa_StartEnrollment': HrOperationMeta(method: 'POST', path: '/api/app/account/mfa/enroll', permission: null, roles: <String>['hrManager', 'executiveDirector', 'accountant', 'systemAdmin', 'employee'], release: '1', errorCodes: <String>['Hr:Identity:MfaAlreadyEnabled']),
  'Mfa_ConfirmEnrollment': HrOperationMeta(method: 'POST', path: '/api/app/account/mfa/confirm', permission: null, roles: <String>['hrManager', 'executiveDirector', 'accountant', 'systemAdmin', 'employee'], release: '1', errorCodes: <String>['Hr:Identity:MfaEnrollmentNotStarted', 'Hr:Identity:MfaAlreadyEnabled', 'Hr:Identity:MfaCodeInvalid']),
  'Mfa_RegenerateRecoveryCodes': HrOperationMeta(method: 'POST', path: '/api/app/account/mfa/recovery-codes', permission: null, roles: <String>['hrManager', 'executiveDirector', 'accountant', 'systemAdmin', 'employee'], release: '1', errorCodes: <String>['Hr:Identity:MfaNotEnabled', 'Hr:Identity:MfaCodeInvalid']),
  'UserAccount_ResetMfa': HrOperationMeta(method: 'POST', path: '/api/app/user-account/{id}/mfa/reset', permission: 'Hr.Identity.UserAccount.ResetMfa', roles: <String>['systemAdmin', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Identity:CannotResetOwnMfa', 'Hr:Identity:MfaResetNotAllowedForTarget', 'Hr:Identity:MfaNotEnabled']),
  'UserAccount_List': HrOperationMeta(method: 'GET', path: '/api/app/user-account', permission: 'Hr.Identity.UserAccount.Read', roles: <String>['hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'UserAccount_Create': HrOperationMeta(method: 'POST', path: '/api/app/user-account', permission: 'Hr.Identity.UserAccount.Create', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Employee:EmployeeAlreadyHasAccount', 'Hr:Identity:NoVerifiedEmail', 'Hr:Employee:EmployeeEnded', 'Hr:Identity:RoleNotFound', 'Hr:Identity:SingletonRoleAlreadyAssigned', 'Hr:Identity:ProtectedRoleChangeRequiresApproval', 'Hr:Identity:ForbiddenRoleCombination']),
  'UserAccount_Get': HrOperationMeta(method: 'GET', path: '/api/app/user-account/{id}', permission: 'Hr.Identity.UserAccount.Read', roles: <String>['hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'UserAccount_ResetPassword': HrOperationMeta(method: 'POST', path: '/api/app/user-account/{id}/reset-password', permission: 'Hr.Identity.UserAccount.ResetPassword', roles: <String>['hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Identity:CannotResetOwnPassword', 'Hr:Identity:PasswordResetNotAllowedForTarget', 'Hr:Identity:AccountInactive', 'Hr:Identity:NoVerifiedEmail']),
  'UserAccount_SetRoles': HrOperationMeta(method: 'PUT', path: '/api/app/user-account/{id}/roles', permission: 'Hr.Identity.UserAccount.SetRoles', roles: <String>['hrManager', 'systemAdmin', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Identity:RoleNotFound', 'Hr:Identity:SingletonRoleAlreadyAssigned', 'Hr:Identity:LastSystemAdminCannotBeRemoved', 'Hr:Identity:ManagerRoleHasDirectReports', 'Hr:Employee:ManagerRequired', 'Hr:Employee:ExecutiveCannotHaveManager', 'Hr:Employee:ManagerNotFound', 'Hr:Employee:ManagerNotActive', 'Hr:Employee:InvalidManagerForRole', 'Hr:Employee:CannotManageSelf', 'Hr:Employee:ManagerCycle', 'Hr:Employee:EmployeeEnded', 'Hr:Identity:ProtectedRoleChangeRequiresApproval', 'Hr:Identity:ForbiddenRoleCombination', 'Hr:Identity:CannotChangeOwnRoles']),
  'UserAccount_GetPermissions': HrOperationMeta(method: 'GET', path: '/api/app/user-account/{id}/permission', permission: 'Hr.Identity.Permission.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'UserAccount_UpdatePermissions': HrOperationMeta(method: 'PUT', path: '/api/app/user-account/{id}/permission', permission: 'Hr.Identity.Permission.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>['Hr:Identity:ProtectedPermissionNotGrantable', 'Hr:Identity:FinancialPermissionNotGrantable', 'Hr:Identity:PermissionNotFound', 'Hr:Identity:ProtectedPermission', 'Hr:Identity:CannotChangeOwnPermissions']),
  'Role_List': HrOperationMeta(method: 'GET', path: '/api/app/role', permission: 'Hr.Identity.Role.Read', roles: <String>['hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Role_Create': HrOperationMeta(method: 'POST', path: '/api/app/role', permission: 'Hr.Identity.Role.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'Role_Get': HrOperationMeta(method: 'GET', path: '/api/app/role/{id}', permission: 'Hr.Identity.Role.ReadDetail', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'Role_Update': HrOperationMeta(method: 'PUT', path: '/api/app/role/{id}', permission: 'Hr.Identity.Role.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'Role_Delete': HrOperationMeta(method: 'DELETE', path: '/api/app/role/{id}', permission: 'Hr.Identity.Role.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>['Hr:Identity:StaticRoleCannotBeDeleted', 'Hr:Identity:RoleInUse']),
  'Role_GetPermissions': HrOperationMeta(method: 'GET', path: '/api/app/role/{id}/permission', permission: 'Hr.Identity.Permission.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'Role_UpdatePermissions': HrOperationMeta(method: 'PUT', path: '/api/app/role/{id}/permission', permission: 'Hr.Identity.Permission.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>['Hr:Identity:ProtectedPermissionNotGrantable', 'Hr:Identity:FinancialPermissionNotGrantable', 'Hr:Identity:PermissionNotFound', 'Hr:Identity:ProtectedPermission']),
  'Department_List': HrOperationMeta(method: 'GET', path: '/api/app/department', permission: 'Hr.Department.Read', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Department_Create': HrOperationMeta(method: 'POST', path: '/api/app/department', permission: 'Hr.Department.Manage', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Department_Lookup': HrOperationMeta(method: 'GET', path: '/api/app/department/lookup', permission: 'Hr.Department.Lookup', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Department_Get': HrOperationMeta(method: 'GET', path: '/api/app/department/{id}', permission: 'Hr.Department.Read', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Department_Update': HrOperationMeta(method: 'PUT', path: '/api/app/department/{id}', permission: 'Hr.Department.Manage', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Department_Delete': HrOperationMeta(method: 'DELETE', path: '/api/app/department/{id}', permission: 'Hr.Department.Manage', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Department:HasEmployees', 'Hr:Department:HasSharedCustody']),
  'Employee_List': HrOperationMeta(method: 'GET', path: '/api/app/employee', permission: 'Hr.Employee.Read', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Employee_Create': HrOperationMeta(method: 'POST', path: '/api/app/employee', permission: 'Hr.Employee.Create', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Department:NotFound', 'Hr:Employee:WorkShiftRequired', 'Hr:Employee:WorkShiftNotFound', 'Hr:Employee:ManagerRequired', 'Hr:Employee:ExecutiveCannotHaveManager', 'Hr:Employee:ManagerNotFound', 'Hr:Employee:ManagerNotActive', 'Hr:Employee:InvalidManagerForRole', 'Hr:Identity:RoleNotFound', 'Hr:Identity:SingletonRoleAlreadyAssigned', 'Hr:Identity:ProtectedRoleChangeRequiresApproval', 'Hr:Identity:ForbiddenRoleCombination', 'Hr:Shift:ShiftInactive']),
  'Employee_Lookup': HrOperationMeta(method: 'GET', path: '/api/app/employee/lookup', permission: 'Hr.Employee.Lookup', roles: <String>['storeOfficer', 'hrManager', 'executiveDirector', 'accountant', 'reviewer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Employee_Get': HrOperationMeta(method: 'GET', path: '/api/app/employee/{id}', permission: 'Hr.Employee.Read', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Employee_Update': HrOperationMeta(method: 'PUT', path: '/api/app/employee/{id}', permission: 'Hr.Employee.Update', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Department:NotFound', 'Hr:Employee:HasDirectReports', 'Hr:Employee:HireDateLocked', 'Hr:Employee:HasOutstandingCustody']),
  'Employee_CorrectHireDate': HrOperationMeta(method: 'POST', path: '/api/app/employee/{id}/correct-hire-date', permission: 'Hr.Employee.CorrectHireDate', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Employee:EmployeeEnded', 'Hr:Employee:HireDateUnchanged']),
  'Employee_ChangeManager': HrOperationMeta(method: 'PUT', path: '/api/app/employee/{id}/manager', permission: 'Hr.Employee.AssignManager', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Employee:ExecutiveCannotHaveManager', 'Hr:Employee:ManagerNotFound', 'Hr:Employee:ManagerNotActive', 'Hr:Employee:InvalidManagerForRole', 'Hr:Employee:CannotManageSelf', 'Hr:Employee:ManagerCycle', 'Hr:Employee:EmployeeEnded', 'Hr:Employee:ManagerUnchanged']),
  'Employee_GetManagerHistory': HrOperationMeta(method: 'GET', path: '/api/app/employee/{id}/manager-history', permission: 'Hr.Employee.ReadManagerHistory', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Employee_GetAdministrativeTaskLog': HrOperationMeta(method: 'GET', path: '/api/app/employee/{id}/administrative-task-log', permission: 'Hr.Employee.ReadTaskLog', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Manager_AssignEmployees': HrOperationMeta(method: 'POST', path: '/api/app/manager/{id}/assign-employees', permission: 'Hr.Employee.AssignManager', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Employee:ExecutiveCannotHaveManager', 'Hr:Employee:ManagerNotActive', 'Hr:Employee:InvalidManagerForRole', 'Hr:Employee:CannotManageSelf', 'Hr:Employee:ManagerCycle', 'Hr:Employee:EmployeeEnded', 'Hr:Employee:EmployeeNotFound']),
  'Manager_GetMyTeam': HrOperationMeta(method: 'GET', path: '/api/app/manager/my-team', permission: 'Hr.Employee.ReadTeam', roles: <String>['departmentManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Manager_GetTeamMember': HrOperationMeta(method: 'GET', path: '/api/app/manager/my-team/{id}', permission: 'Hr.Employee.ReadTeam', roles: <String>['departmentManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'EmployeeDocument_List': HrOperationMeta(method: 'GET', path: '/api/app/employee/{id}/documents', permission: 'Hr.Employee.Document.Read', roles: <String>['employee', 'departmentManager', 'systemAdmin', 'hrManager'], release: '1', errorCodes: <String>[]),
  'EmployeeDocument_Upload': HrOperationMeta(method: 'POST', path: '/api/app/employee/{id}/documents', permission: 'Hr.Employee.Document.Upload', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Document:FileTooLarge', 'Hr:Document:FileTypeNotAllowed', 'Hr:Employee:EmployeeEnded']),
  'EmployeeDocument_GetDownloadUrl': HrOperationMeta(method: 'GET', path: '/api/app/document/{id}/download-url', permission: 'Hr.Employee.Document.Read', roles: <String>['employee', 'departmentManager', 'systemAdmin', 'hrManager'], release: '1', errorCodes: <String>['Hr:Document:NotAvailable']),
  'EmployeeDocument_Delete': HrOperationMeta(method: 'DELETE', path: '/api/app/document/{id}', permission: 'Hr.Employee.Document.Delete', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Delegation_List': HrOperationMeta(method: 'GET', path: '/api/app/delegation', permission: 'Hr.Delegation.ReadAll', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Delegation_Create': HrOperationMeta(method: 'POST', path: '/api/app/delegation', permission: 'Hr.Delegation.Manage', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin', 'accountant'], release: '1', errorCodes: <String>['Hr:Delegation:CannotDelegateToSelf', 'Hr:Delegation:InvalidPeriod', 'Hr:Delegation:DelegateNotEligible', 'Hr:Delegation:OverlappingDelegation', 'Hr:Delegation:DelegatorNotEligible']),
  'Delegation_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/delegation/my', permission: null, roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Delegation_GetCandidates': HrOperationMeta(method: 'GET', path: '/api/app/delegation/candidate', permission: 'Hr.Delegation.Manage', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin', 'accountant'], release: '1', errorCodes: <String>[]),
  'Delegation_Cancel': HrOperationMeta(method: 'DELETE', path: '/api/app/delegation/{id}', permission: 'Hr.Delegation.Manage', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin', 'accountant'], release: '1', errorCodes: <String>['Hr:Delegation:AlreadyEnded']),
  'WorkShift_List': HrOperationMeta(method: 'GET', path: '/api/app/work-shift', permission: 'Hr.WorkShift.View', roles: <String>['hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'WorkShift_Create': HrOperationMeta(method: 'POST', path: '/api/app/work-shift', permission: 'Hr.WorkShift.Manage', roles: <String>['hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Shift:InvalidSchedule', 'Hr:Shift:CrossesMidnightMismatch']),
  'WorkShift_Get': HrOperationMeta(method: 'GET', path: '/api/app/work-shift/{id}', permission: 'Hr.WorkShift.View', roles: <String>['hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'WorkShift_Update': HrOperationMeta(method: 'PUT', path: '/api/app/work-shift/{id}', permission: 'Hr.WorkShift.Manage', roles: <String>['hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Shift:InvalidSchedule', 'Hr:Shift:CrossesMidnightMismatch']),
  'EmployeeShift_Assign': HrOperationMeta(method: 'PUT', path: '/api/app/employee/{id}/shift', permission: 'Hr.EmployeeShift.Assign', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Shift:ShiftInactive', 'Hr:Shift:StartDateBeforeHireDate', 'Hr:Shift:StartDateBeforeCurrentAssignment', 'Hr:Attendance:ExecutiveDirectorNotCovered']),
  'EmployeeShift_GetHistory': HrOperationMeta(method: 'GET', path: '/api/app/employee/{id}/shift', permission: 'Hr.EmployeeShift.View', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Holiday_List': HrOperationMeta(method: 'GET', path: '/api/app/holiday', permission: 'Hr.Holiday.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Holiday_Create': HrOperationMeta(method: 'POST', path: '/api/app/holiday', permission: 'Hr.Holiday.Manage', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Holiday_Update': HrOperationMeta(method: 'PUT', path: '/api/app/holiday/{id}', permission: 'Hr.Holiday.Manage', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Holiday_Delete': HrOperationMeta(method: 'DELETE', path: '/api/app/holiday/{id}', permission: 'Hr.Holiday.Manage', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'WeeklyOffSetting_Get': HrOperationMeta(method: 'GET', path: '/api/app/weekly-off-setting', permission: 'Hr.Holiday.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'WeeklyOffSetting_Update': HrOperationMeta(method: 'PUT', path: '/api/app/weekly-off-setting', permission: 'Hr.Holiday.Manage', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Attendance_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/attendance/my', permission: 'Hr.Attendance.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Attendance:ExecutiveDirectorNotCovered']),
  'Attendance_GetTeam': HrOperationMeta(method: 'GET', path: '/api/app/attendance/team', permission: 'Hr.Attendance.ViewTeam', roles: <String>['departmentManager'], release: '1', errorCodes: <String>[]),
  'Attendance_GetByEmployee': HrOperationMeta(method: 'GET', path: '/api/app/attendance/employee/{id}', permission: 'Hr.Attendance.ViewEmployee', roles: <String>['departmentManager', 'hrManager'], release: '1', errorCodes: <String>['Hr:Attendance:ExecutiveDirectorNotCovered']),
  'Attendance_GetDayDetail': HrOperationMeta(method: 'GET', path: '/api/app/attendance/day/{id}', permission: 'Hr.Attendance.ViewDayDetail', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'AttendanceDevice_IngestPunches': HrOperationMeta(method: 'POST', path: '/api/attendance-devices/punches', permission: null, roles: <String>[], release: '1', errorCodes: <String>[]),
  'AttendanceImport_Create': HrOperationMeta(method: 'POST', path: '/api/app/attendance-import', permission: 'Hr.Attendance.Import', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Attendance:ImportFileInvalid', 'Hr:Attendance:DeviceInactive']),
  'AttendanceImport_List': HrOperationMeta(method: 'GET', path: '/api/app/attendance-import', permission: 'Hr.Attendance.Import', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'AttendanceImport_Get': HrOperationMeta(method: 'GET', path: '/api/app/attendance-import/{id}', permission: 'Hr.Attendance.Import', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'AttendanceSettings_Get': HrOperationMeta(method: 'GET', path: '/api/app/attendance-settings', permission: 'Hr.Attendance.Settings.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'AttendanceSettings_Update': HrOperationMeta(method: 'PUT', path: '/api/app/attendance-settings', permission: 'Hr.Attendance.Settings.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>['Hr:Attendance:GpsNotAvailable']),
  'AttendanceDevice_List': HrOperationMeta(method: 'GET', path: '/api/app/attendance-device', permission: 'Hr.Attendance.Device.View', roles: <String>['systemAdmin', 'hrManager'], release: '1', errorCodes: <String>[]),
  'AttendanceDevice_Create': HrOperationMeta(method: 'POST', path: '/api/app/attendance-device', permission: 'Hr.Attendance.Device.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'AttendanceDevice_Update': HrOperationMeta(method: 'PUT', path: '/api/app/attendance-device/{id}', permission: 'Hr.Attendance.Device.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'AttendanceDevice_RotateKey': HrOperationMeta(method: 'POST', path: '/api/app/attendance-device/{id}/rotate-key', permission: 'Hr.Attendance.Device.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'DeviceUserMap_List': HrOperationMeta(method: 'GET', path: '/api/app/attendance-device/{id}/user-map', permission: 'Hr.Attendance.DeviceMap.Manage', roles: <String>['systemAdmin', 'hrManager'], release: '1', errorCodes: <String>[]),
  'DeviceUserMap_Create': HrOperationMeta(method: 'POST', path: '/api/app/attendance-device/{id}/user-map', permission: 'Hr.Attendance.DeviceMap.Manage', roles: <String>['systemAdmin', 'hrManager'], release: '1', errorCodes: <String>['Hr:Attendance:ExecutiveDirectorNotCovered']),
  'DeviceUserMap_Delete': HrOperationMeta(method: 'DELETE', path: '/api/app/attendance-device/{id}/user-map/{mapId}', permission: 'Hr.Attendance.DeviceMap.Manage', roles: <String>['systemAdmin', 'hrManager'], release: '1', errorCodes: <String>[]),
  'DeviceUserMap_ListUnmapped': HrOperationMeta(method: 'GET', path: '/api/app/attendance-device/{id}/unmapped-user', permission: 'Hr.Attendance.DeviceMap.Manage', roles: <String>['systemAdmin', 'hrManager'], release: '1', errorCodes: <String>[]),
  'AttendanceCorrection_Create': HrOperationMeta(method: 'POST', path: '/api/app/attendance-correction', permission: 'Hr.Attendance.Correction.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Attendance:ExecutiveDirectorNotCovered', 'Hr:Attendance:CorrectionDateInFuture', 'Hr:Attendance:CorrectionAlreadyPending', 'Hr:Attendance:CorrectionTimeInvalid', 'Hr:Request:NoApproverAvailable']),
  'AttendanceCorrection_Get': HrOperationMeta(method: 'GET', path: '/api/app/attendance-correction/{id}', permission: 'Hr.Attendance.Correction.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Attendance_CreateGpsPunch': HrOperationMeta(method: 'POST', path: '/api/app/attendance/gps-punch', permission: 'Hr.Attendance.Gps.Punch', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: 'later', errorCodes: <String>['Hr:Attendance:ExecutiveDirectorNotCovered', 'Hr:Attendance:GpsMethodDisabled', 'Hr:Attendance:OutsideGeoFence']),
  'Attendance_ListFlaggedGpsPunches': HrOperationMeta(method: 'GET', path: '/api/app/attendance/gps-punch/flagged', permission: 'Hr.Attendance.Gps.ReviewFlagged', roles: <String>['hrManager'], release: 'later', errorCodes: <String>[]),
  'GeoFence_List': HrOperationMeta(method: 'GET', path: '/api/app/geo-fence', permission: 'Hr.Attendance.GeoFence.Manage', roles: <String>['systemAdmin'], release: 'later', errorCodes: <String>[]),
  'GeoFence_Create': HrOperationMeta(method: 'POST', path: '/api/app/geo-fence', permission: 'Hr.Attendance.GeoFence.Manage', roles: <String>['systemAdmin'], release: 'later', errorCodes: <String>[]),
  'GeoFence_Update': HrOperationMeta(method: 'PUT', path: '/api/app/geo-fence/{id}', permission: 'Hr.Attendance.GeoFence.Manage', roles: <String>['systemAdmin'], release: 'later', errorCodes: <String>[]),
  'GeoFence_Delete': HrOperationMeta(method: 'DELETE', path: '/api/app/geo-fence/{id}', permission: 'Hr.Attendance.GeoFence.Manage', roles: <String>['systemAdmin'], release: 'later', errorCodes: <String>[]),
  'LeaveBalance_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/leave-balance/my', permission: 'Hr.Leave.Balance.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible']),
  'LeaveBalance_GetByEmployee': HrOperationMeta(method: 'GET', path: '/api/app/leave-balance/employee/{id}', permission: 'Hr.Leave.Balance.View', roles: <String>['departmentManager', 'hrManager', 'systemAdmin', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible']),
  'LeaveBalance_SetEntitlement': HrOperationMeta(method: 'PUT', path: '/api/app/leave-balance/employee/{id}/entitlement', permission: 'Hr.Leave.Balance.SetEntitlement', roles: <String>['hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible', 'Hr:Leave:SelfAdjustmentNotAllowed']),
  'LeaveBalance_Adjust': HrOperationMeta(method: 'POST', path: '/api/app/leave-balance/employee/{id}/adjust', permission: 'Hr.Leave.Balance.Adjust', roles: <String>['hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible', 'Hr:Leave:EntitlementNotSet', 'Hr:Leave:AdjustmentExceedsBalance', 'Hr:Leave:SelfAdjustmentNotAllowed']),
  'LeaveLedger_GetByEmployee': HrOperationMeta(method: 'GET', path: '/api/app/leave-ledger/employee/{id}', permission: 'Hr.Leave.Ledger.View', roles: <String>['departmentManager', 'hrManager', 'systemAdmin', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible']),
  'LeaveLedger_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/leave-ledger/my', permission: 'Hr.Leave.Ledger.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible']),
  'LeaveRequest_Preview': HrOperationMeta(method: 'POST', path: '/api/app/leave-request/preview', permission: 'Hr.Leave.Request.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible', 'Hr:Leave:EmployeeNotActive', 'Hr:Leave:NoShiftAssigned', 'Hr:Leave:InvalidDateRange']),
  'LeaveRequest_Create': HrOperationMeta(method: 'POST', path: '/api/app/leave-request', permission: 'Hr.Leave.Request.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible', 'Hr:Leave:EmployeeNotActive', 'Hr:Leave:EntitlementNotSet', 'Hr:Leave:NoShiftAssigned', 'Hr:Leave:InvalidDateRange', 'Hr:Leave:NoWorkingDaysInRange', 'Hr:Leave:RangeBeyondNextYear', 'Hr:Leave:InsufficientBalance', 'Hr:Leave:OverlappingLeave', 'Hr:Leave:HourlySettingsNotConfigured', 'Hr:Leave:HourlyOutsideShift', 'Hr:Leave:OtherLeaveTypeInactive', 'Hr:Leave:NotEligibleForLeaveType', 'Hr:Leave:OtherLeaveTypeMaxExceeded', 'Hr:Leave:AttachmentRequired', 'Hr:Leave:InvalidAttachment', 'Hr:Request:NoApproverAvailable']),
  'LeaveRequest_ListMy': HrOperationMeta(method: 'GET', path: '/api/app/leave-request/my', permission: 'Hr.Leave.Request.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible']),
  'LeaveRequest_ListByEmployee': HrOperationMeta(method: 'GET', path: '/api/app/leave-request/employee/{id}', permission: 'Hr.Leave.Request.ViewEmployee', roles: <String>['departmentManager', 'hrManager'], release: '1', errorCodes: <String>[]),
  'LeaveRequest_Reverse': HrOperationMeta(method: 'POST', path: '/api/app/leave-request/{id}/reverse', permission: 'Hr.Leave.Request.Reverse', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Leave:RequestNotReversible', 'Hr:Leave:SelfAdjustmentNotAllowed']),
  'LeaveRequest_Get': HrOperationMeta(method: 'GET', path: '/api/app/leave-request/{id}', permission: 'Hr.Leave.Request.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'LeaveRequest_UploadAttachment': HrOperationMeta(method: 'POST', path: '/api/app/leave-request/attachment', permission: 'Hr.Leave.Request.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Leave:AttachmentRejected']),
  'LeaveRequest_GetAttachmentDownloadUrl': HrOperationMeta(method: 'GET', path: '/api/app/leave-request/{id}/attachment/{attachmentId}/download-url', permission: 'Hr.Leave.Request.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'OtherLeaveType_List': HrOperationMeta(method: 'GET', path: '/api/app/other-leave-type', permission: 'Hr.Leave.OtherType.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'OtherLeaveType_Create': HrOperationMeta(method: 'POST', path: '/api/app/other-leave-type', permission: 'Hr.Leave.OtherType.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'OtherLeaveType_Update': HrOperationMeta(method: 'PUT', path: '/api/app/other-leave-type/{id}', permission: 'Hr.Leave.OtherType.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'OtherLeaveType_ListAvailable': HrOperationMeta(method: 'GET', path: '/api/app/other-leave-type/available', permission: 'Hr.Leave.Request.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Leave:ExecutiveDirectorNotEligible']),
  'LeaveCalendar_GetTeam': HrOperationMeta(method: 'GET', path: '/api/app/leave-calendar/team', permission: 'Hr.Leave.Calendar.ViewTeam', roles: <String>['departmentManager'], release: '1', errorCodes: <String>['Hr:Leave:InvalidDateRange']),
  'RequestType_GetList': HrOperationMeta(method: 'GET', path: '/api/app/request-type', permission: 'Hr.Request.Type.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'RequestType_Create': HrOperationMeta(method: 'POST', path: '/api/app/request-type', permission: 'Hr.Request.Type.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>['Hr:Request:TypeCodeAlreadyExists']),
  'RequestType_GetAdminList': HrOperationMeta(method: 'GET', path: '/api/app/request-type/admin', permission: 'Hr.Request.Type.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'RequestType_Get': HrOperationMeta(method: 'GET', path: '/api/app/request-type/{id}', permission: 'Hr.Request.Type.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'RequestType_Update': HrOperationMeta(method: 'PUT', path: '/api/app/request-type/{id}', permission: 'Hr.Request.Type.Manage', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>['Hr:Request:SystemTypeDestinationLocked']),
  'Request_Submit': HrOperationMeta(method: 'POST', path: '/api/app/request', permission: 'Hr.Request.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Request:TypeInactive', 'Hr:Request:UseDedicatedEndpoint', 'Hr:Request:ExecutiveCannotSubmit', 'Hr:Request:OnBehalfNotAllowed', 'Hr:Request:ItemFieldsInvalid', 'Hr:Request:ItemInactive', 'Hr:Request:NoApproverAvailable']),
  'Request_GetList': HrOperationMeta(method: 'GET', path: '/api/app/request', permission: 'Hr.Request.ViewList', roles: <String>['hrManager', 'departmentManager'], release: '1', errorCodes: <String>[]),
  'Request_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/request/my', permission: 'Hr.Request.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Request_GetInbox': HrOperationMeta(method: 'GET', path: '/api/app/request/inbox', permission: 'Hr.Request.Inbox', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin', 'accountant', 'storeOfficer'], release: '1', errorCodes: <String>[]),
  'Request_Get': HrOperationMeta(method: 'GET', path: '/api/app/request/{id}', permission: 'Hr.Request.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Request_Approve': HrOperationMeta(method: 'POST', path: '/api/app/request/{id}/approve', permission: 'Hr.Request.Decide', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin', 'accountant'], release: '1', errorCodes: <String>['Hr:Request:SelfApprovalNotAllowed', 'Hr:Request:NotPending', 'Hr:Request:UseStockIssueFlow']),
  'Request_Reject': HrOperationMeta(method: 'POST', path: '/api/app/request/{id}/reject', permission: 'Hr.Request.Reject', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin', 'accountant', 'storeOfficer'], release: '1', errorCodes: <String>['Hr:Request:SelfApprovalNotAllowed', 'Hr:Request:NotPending']),
  'Request_Cancel': HrOperationMeta(method: 'POST', path: '/api/app/request/{id}/cancel', permission: 'Hr.Request.Cancel', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Request:CannotCancelAtThisStage']),
  'Memo_Create': HrOperationMeta(method: 'POST', path: '/api/app/memo', permission: 'Hr.Memo.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Memo:NoManagerLinked']),
  'Memo_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/memo/my', permission: 'Hr.Memo.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Memo_GetInbox': HrOperationMeta(method: 'GET', path: '/api/app/memo/inbox', permission: 'Hr.Memo.Process', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Memo_Get': HrOperationMeta(method: 'GET', path: '/api/app/memo/{id}', permission: 'Hr.Memo.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Memo_Forward': HrOperationMeta(method: 'POST', path: '/api/app/memo/{id}/forward', permission: 'Hr.Memo.Forward', roles: <String>['departmentManager', 'systemAdmin', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Memo:InvalidState', 'Hr:Memo:SelfActionNotAllowed']),
  'Memo_Convert': HrOperationMeta(method: 'POST', path: '/api/app/memo/{id}/convert', permission: 'Hr.Memo.Process', roles: <String>['departmentManager', 'hrManager', 'executiveDirector', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Memo:InvalidState', 'Hr:Memo:ConvertTargetMismatch', 'Hr:Memo:LeaveDetailsInvalid', 'Hr:Memo:LeaveAttachmentRequired', 'Hr:Memo:SelfActionNotAllowed', 'Hr:Leave:InsufficientBalance', 'Hr:Leave:EmployeeNotActive', 'Hr:Leave:EntitlementNotSet', 'Hr:Leave:HourlyOutsideShift', 'Hr:Leave:HourlySettingsNotConfigured', 'Hr:Leave:InvalidDateRange', 'Hr:Leave:NoShiftAssigned', 'Hr:Leave:NotEligibleForLeaveType', 'Hr:Leave:NoWorkingDaysInRange', 'Hr:Leave:OtherLeaveTypeInactive', 'Hr:Leave:OtherLeaveTypeMaxExceeded', 'Hr:Leave:OverlappingLeave', 'Hr:Leave:RangeBeyondNextYear', 'Hr:Request:NoApproverAvailable']),
  'Memo_Close': HrOperationMeta(method: 'POST', path: '/api/app/memo/{id}/close', permission: 'Hr.Memo.Close', roles: <String>['hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Memo:InvalidState', 'Hr:Memo:SelfActionNotAllowed']),
  'Incident_Create': HrOperationMeta(method: 'POST', path: '/api/app/incident', permission: 'Hr.Incident.Create', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>['Hr:Incident:NoManagerLinked']),
  'Incident_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/incident/my', permission: 'Hr.Incident.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Incident_GetInbox': HrOperationMeta(method: 'GET', path: '/api/app/incident/inbox', permission: 'Hr.Incident.Process', roles: <String>['departmentManager', 'hrManager'], release: '1', errorCodes: <String>[]),
  'Incident_Get': HrOperationMeta(method: 'GET', path: '/api/app/incident/{id}', permission: 'Hr.Incident.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Incident_UpdateStatus': HrOperationMeta(method: 'PATCH', path: '/api/app/incident/{id}', permission: 'Hr.Incident.UpdateStatus', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Incident:InvalidTransition']),
  'Incident_Forward': HrOperationMeta(method: 'POST', path: '/api/app/incident/{id}/forward', permission: 'Hr.Incident.Forward', roles: <String>['departmentManager'], release: '1', errorCodes: <String>['Hr:Incident:InvalidState']),
  'Item_GetList': HrOperationMeta(method: 'GET', path: '/api/app/item', permission: 'Hr.Store.Item.View', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>[]),
  'Item_Create': HrOperationMeta(method: 'POST', path: '/api/app/item', permission: 'Hr.Store.Item.Manage', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Item:NameAlreadyExists']),
  'Item_Lookup': HrOperationMeta(method: 'GET', path: '/api/app/item/lookup', permission: 'Hr.Store.Item.Lookup', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '2', errorCodes: <String>[]),
  'Item_Get': HrOperationMeta(method: 'GET', path: '/api/app/item/{id}', permission: 'Hr.Store.Item.View', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>[]),
  'Item_Update': HrOperationMeta(method: 'PUT', path: '/api/app/item/{id}', permission: 'Hr.Store.Item.Manage', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Item:KindLocked', 'Hr:Item:NameAlreadyExists']),
  'Stock_Receive': HrOperationMeta(method: 'POST', path: '/api/app/stock/receive', permission: 'Hr.Store.Stock.Receive', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Stock:ItemInactive', 'Hr:Stock:PurchaseCostRequired', 'Hr:Stock:IssueNotPurchasing', 'Hr:Stock:IssueItemMismatch']),
  'Stock_Adjust': HrOperationMeta(method: 'POST', path: '/api/app/stock/adjust', permission: 'Hr.Store.Stock.Adjust', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Stock:InsufficientStock', 'Hr:Stock:ZeroQuantity', 'Hr:Stock:MovementAlreadyReversed']),
  'Stock_GetMovements': HrOperationMeta(method: 'GET', path: '/api/app/stock/movements', permission: 'Hr.Store.Stock.View', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>[]),
  'StockIssue_GetList': HrOperationMeta(method: 'GET', path: '/api/app/stock-issue', permission: 'Hr.Store.Issue.View', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>[]),
  'StockIssue_Get': HrOperationMeta(method: 'GET', path: '/api/app/stock-issue/{id}', permission: 'Hr.Store.Issue.View', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>[]),
  'StockIssue_Acknowledge': HrOperationMeta(method: 'POST', path: '/api/app/stock-issue/{id}/acknowledge', permission: 'Hr.Store.Issue.Process', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Stock:IssueInvalidState']),
  'StockIssue_MarkPurchasing': HrOperationMeta(method: 'POST', path: '/api/app/stock-issue/{id}/mark-purchasing', permission: 'Hr.Store.Issue.Process', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Stock:IssueInvalidState']),
  'StockIssue_Deliver': HrOperationMeta(method: 'POST', path: '/api/app/stock-issue/{id}/deliver', permission: 'Hr.Store.Issue.Deliver', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Stock:IssueInvalidState', 'Hr:Stock:InsufficientStock']),
  'Custody_Create': HrOperationMeta(method: 'POST', path: '/api/app/custody', permission: 'Hr.Custody.Create', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Custody:InvalidHolder', 'Hr:Custody:ItemNotReturnable', 'Hr:Stock:InsufficientStock']),
  'Custody_GetList': HrOperationMeta(method: 'GET', path: '/api/app/custody', permission: 'Hr.Custody.ViewAll', roles: <String>['storeOfficer', 'hrManager'], release: '2', errorCodes: <String>[]),
  'Custody_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/custody/my', permission: 'Hr.Custody.ViewOwn', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '2', errorCodes: <String>[]),
  'Custody_GetTeam': HrOperationMeta(method: 'GET', path: '/api/app/custody/team', permission: 'Hr.Custody.ViewTeam', roles: <String>['departmentManager'], release: '2', errorCodes: <String>[]),
  'Custody_Get': HrOperationMeta(method: 'GET', path: '/api/app/custody/{id}', permission: 'Hr.Custody.View', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '2', errorCodes: <String>[]),
  'Custody_Acknowledge': HrOperationMeta(method: 'POST', path: '/api/app/custody/{id}/acknowledge', permission: 'Hr.Custody.Acknowledge', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '2', errorCodes: <String>['Hr:Custody:AlreadyAcknowledged', 'Hr:Custody:AlreadyReturned']),
  'Custody_Return': HrOperationMeta(method: 'POST', path: '/api/app/custody/{id}/return', permission: 'Hr.Custody.Return', roles: <String>['storeOfficer'], release: '2', errorCodes: <String>['Hr:Custody:AlreadyReturned', 'Hr:Custody:ReturnNoteRequired']),
  'CustodyCount_Create': HrOperationMeta(method: 'POST', path: '/api/app/custody-count', permission: 'Hr.Custody.Count', roles: <String>['departmentManager'], release: '2', errorCodes: <String>['Hr:Custody:CountLinesIncomplete', 'Hr:Custody:CountLineInvalid', 'Hr:Custody:DepartmentNotInScope']),
  'CustodyCount_GetList': HrOperationMeta(method: 'GET', path: '/api/app/custody-count', permission: 'Hr.Custody.ViewCount', roles: <String>['departmentManager', 'storeOfficer', 'hrManager'], release: '2', errorCodes: <String>[]),
  'Complaint_GetRespondents': HrOperationMeta(method: 'GET', path: '/api/app/complaint/respondents', permission: 'Hr.Complaints.Submit', roles: <String>['employee', 'departmentManager'], release: '1', errorCodes: <String>[]),
  'Complaint_PreviewRouting': HrOperationMeta(method: 'POST', path: '/api/app/complaint/preview-routing', permission: 'Hr.Complaints.Submit', roles: <String>['employee', 'departmentManager'], release: '1', errorCodes: <String>['Hr:Complaints:RespondentRequired', 'Hr:Complaints:RespondentNotAllowed', 'Hr:Complaints:SelfComplaintNotAllowed']),
  'Complaint_Submit': HrOperationMeta(method: 'POST', path: '/api/app/complaint', permission: 'Hr.Complaints.Submit', roles: <String>['employee', 'departmentManager'], release: '1', errorCodes: <String>['Hr:Complaints:RespondentRequired', 'Hr:Complaints:RespondentNotAllowed', 'Hr:Complaints:SelfComplaintNotAllowed', 'Hr:Complaints:NoEligibleHandler']),
  'Complaint_GetMyList': HrOperationMeta(method: 'GET', path: '/api/app/complaint/my', permission: 'Hr.Complaints.ViewOwn', roles: <String>['employee', 'departmentManager'], release: '1', errorCodes: <String>[]),
  'Complaint_GetMyDetail': HrOperationMeta(method: 'GET', path: '/api/app/complaint/my/{id}', permission: 'Hr.Complaints.ViewOwn', roles: <String>['employee', 'departmentManager'], release: '1', errorCodes: <String>[]),
  'Complaint_GetAssignedList': HrOperationMeta(method: 'GET', path: '/api/app/complaint/assigned', permission: 'Hr.Complaints.Handle', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '1', errorCodes: <String>[]),
  'Complaint_GetDetail': HrOperationMeta(method: 'GET', path: '/api/app/complaint/{id}', permission: 'Hr.Complaints.Handle', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '1', errorCodes: <String>[]),
  'Complaint_RaiseLevel': HrOperationMeta(method: 'POST', path: '/api/app/complaint/{id}/raise-level', permission: 'Hr.Complaints.RaiseLevel', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Complaints:LevelNotHigher', 'Hr:Complaints:ComplaintClosed']),
  'Complaint_AddAction': HrOperationMeta(method: 'POST', path: '/api/app/complaint/{id}/action', permission: 'Hr.Complaints.Handle', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Complaints:ComplaintClosed']),
  'Complaint_Forward': HrOperationMeta(method: 'POST', path: '/api/app/complaint/{id}/forward', permission: 'Hr.Complaints.Forward', roles: <String>['departmentManager'], release: '1', errorCodes: <String>['Hr:Complaints:ComplaintClosed', 'Hr:Complaints:InvalidTransition']),
  'Complaint_Close': HrOperationMeta(method: 'POST', path: '/api/app/complaint/{id}/close', permission: 'Hr.Complaints.Close', roles: <String>['hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Complaints:ComplaintClosed']),
  'Complaint_GetAccessLog': HrOperationMeta(method: 'GET', path: '/api/app/complaint/{id}/access-log', permission: 'Hr.Complaints.ViewAccessLog', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '1', errorCodes: <String>[]),
  'Complaint_UploadAttachment': HrOperationMeta(method: 'POST', path: '/api/app/complaint/{id}/attachment', permission: 'Hr.Complaints.Attachment', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector'], release: '1', errorCodes: <String>['Hr:Complaints:ComplaintClosed', 'Hr:Complaints:AttachmentRejected', 'Hr:Complaints:AttachmentTooLarge', 'Hr:Document:FileTypeNotAllowed']),
  'Complaint_DownloadAttachment': HrOperationMeta(method: 'GET', path: '/api/app/complaint/{id}/attachment/{attachmentId}', permission: 'Hr.Complaints.Attachment', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector'], release: '1', errorCodes: <String>[]),
  'ComplaintHandler_Request': HrOperationMeta(method: 'POST', path: '/api/app/complaint-handler', permission: 'Hr.Complaints.Handlers.Request', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Complaints:HandlerCannotBeSystemAdmin', 'Hr:Complaints:InvalidAssignmentPeriod', 'Hr:Complaints:HandlerRoleNotAllowed']),
  'ComplaintHandler_GetList': HrOperationMeta(method: 'GET', path: '/api/app/complaint-handler', permission: 'Hr.Complaints.Handlers.Read', roles: <String>['hrManager', 'executiveDirector'], release: '1', errorCodes: <String>[]),
  'ComplaintHandler_Approve': HrOperationMeta(method: 'POST', path: '/api/app/complaint-handler/{id}/approve', permission: 'Hr.Complaints.Handlers.Approve', roles: <String>['executiveDirector'], release: '1', errorCodes: <String>['Hr:Complaints:AssignmentNotPending', 'Hr:Complaints:HandlerCannotBeSystemAdmin', 'Hr:Complaints:HandlerRoleNotAllowed']),
  'ComplaintHandler_Reject': HrOperationMeta(method: 'POST', path: '/api/app/complaint-handler/{id}/reject', permission: 'Hr.Complaints.Handlers.Approve', roles: <String>['executiveDirector'], release: '1', errorCodes: <String>['Hr:Complaints:AssignmentNotPending']),
  'ComplaintHandler_Revoke': HrOperationMeta(method: 'POST', path: '/api/app/complaint-handler/{id}/revoke', permission: 'Hr.Complaints.Handlers.Approve', roles: <String>['executiveDirector'], release: '1', errorCodes: <String>['Hr:Complaints:AssignmentNotActive']),
  'EvaluationCycle_GetCurrent': HrOperationMeta(method: 'GET', path: '/api/app/evaluation-cycle/current', permission: 'Hr.Evaluations.Cycle.Read', roles: <String>['departmentManager', 'hrManager', 'reviewer'], release: '2', errorCodes: <String>[]),
  'Evaluation_GetTeamList': HrOperationMeta(method: 'GET', path: '/api/app/evaluation/team', permission: 'Hr.Evaluations.Manage', roles: <String>['departmentManager'], release: '2', errorCodes: <String>[]),
  'Evaluation_GetManagerForm': HrOperationMeta(method: 'GET', path: '/api/app/evaluation/{id}', permission: 'Hr.Evaluations.Manage', roles: <String>['departmentManager'], release: '2', errorCodes: <String>[]),
  'Evaluation_SaveDraft': HrOperationMeta(method: 'PUT', path: '/api/app/evaluation/{id}', permission: 'Hr.Evaluations.Manage', roles: <String>['departmentManager'], release: '2', errorCodes: <String>['Hr:Evaluations:NotEditable', 'Hr:Evaluations:CycleNotOpen', 'Hr:Evaluations:InvalidCriterion', 'Hr:Evaluations:InvalidGrade']),
  'Evaluation_Submit': HrOperationMeta(method: 'POST', path: '/api/app/evaluation/{id}/submit', permission: 'Hr.Evaluations.Manage', roles: <String>['departmentManager'], release: '2', errorCodes: <String>['Hr:Evaluations:NotEditable', 'Hr:Evaluations:CycleNotOpen', 'Hr:Evaluations:IncompleteScores']),
  'Evaluation_GetMyList': HrOperationMeta(method: 'GET', path: '/api/app/evaluation/my', permission: 'Hr.Evaluations.ViewOwn', roles: <String>['employee'], release: '2', errorCodes: <String>[]),
  'Evaluation_GetMyDetail': HrOperationMeta(method: 'GET', path: '/api/app/evaluation/my/{id}', permission: 'Hr.Evaluations.ViewOwn', roles: <String>['employee'], release: '2', errorCodes: <String>[]),
  'Evaluation_Acknowledge': HrOperationMeta(method: 'POST', path: '/api/app/evaluation/{id}/acknowledge', permission: 'Hr.Evaluations.ViewOwn', roles: <String>['employee'], release: '2', errorCodes: <String>[]),
  'Evaluation_AddResponse': HrOperationMeta(method: 'POST', path: '/api/app/evaluation/{id}/response', permission: 'Hr.Evaluations.ViewOwn', roles: <String>['employee'], release: '2', errorCodes: <String>['Hr:Evaluations:ObjectionAlreadyPending', 'Hr:Evaluations:ObjectionWindowClosed']),
  'Evaluation_DecideObjection': HrOperationMeta(method: 'POST', path: '/api/app/evaluation/{id}/response/{rid}/decide', permission: 'Hr.Evaluations.DecideObjection', roles: <String>['hrManager', 'executiveDirector'], release: '2', errorCodes: <String>['Hr:Evaluations:NotAnObjection', 'Hr:Evaluations:ObjectionAlreadyDecided', 'Hr:Evaluations:SelfDecisionNotAllowed']),
  'Evaluation_GetHrList': HrOperationMeta(method: 'GET', path: '/api/app/evaluation', permission: 'Hr.Evaluations.HrRead', roles: <String>['hrManager'], release: '2', errorCodes: <String>[]),
  'Evaluation_GetHrView': HrOperationMeta(method: 'GET', path: '/api/app/evaluation/{id}/hr-view', permission: 'Hr.Evaluations.HrRead', roles: <String>['hrManager'], release: '2', errorCodes: <String>[]),
  'EvaluationInternalNote_GetList': HrOperationMeta(method: 'GET', path: '/api/app/evaluation/{id}/internal-note', permission: 'Hr.Evaluations.InternalNote.Read', roles: <String>['departmentManager', 'hrManager'], release: '2', errorCodes: <String>[]),
  'EvaluationInternalNote_Add': HrOperationMeta(method: 'POST', path: '/api/app/evaluation/{id}/internal-note', permission: 'Hr.Evaluations.InternalNote.Write', roles: <String>['departmentManager'], release: '2', errorCodes: <String>['Hr:Evaluations:EvaluationNotSubmitted']),
  'EvaluationInternalNote_SaveDraft': HrOperationMeta(method: 'PUT', path: '/api/app/evaluation/{id}/internal-note/draft', permission: 'Hr.Evaluations.InternalNote.Write', roles: <String>['departmentManager'], release: '2', errorCodes: <String>['Hr:Evaluations:NotEditable']),
  'EvaluationAnnualSummary_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/evaluation-annual-summary/my', permission: 'Hr.Evaluations.ViewOwn', roles: <String>['employee'], release: '2', errorCodes: <String>[]),
  'EvaluationAnnualSummary_GetForEmployee': HrOperationMeta(method: 'GET', path: '/api/app/evaluation-annual-summary/employee/{employeeId}', permission: 'Hr.Evaluations.AnnualSummary.Read', roles: <String>['departmentManager', 'hrManager'], release: '2', errorCodes: <String>[]),
  'EvaluationCriterion_GetList': HrOperationMeta(method: 'GET', path: '/api/app/evaluation-criterion', permission: 'Hr.Evaluations.Criteria.Manage', roles: <String>['systemAdmin'], release: '2', errorCodes: <String>[]),
  'EvaluationCriterion_Create': HrOperationMeta(method: 'POST', path: '/api/app/evaluation-criterion', permission: 'Hr.Evaluations.Criteria.Manage', roles: <String>['systemAdmin'], release: '2', errorCodes: <String>[]),
  'EvaluationCriterion_Update': HrOperationMeta(method: 'PUT', path: '/api/app/evaluation-criterion/{id}', permission: 'Hr.Evaluations.Criteria.Manage', roles: <String>['systemAdmin'], release: '2', errorCodes: <String>['Hr:Evaluations:LastActiveCriterion']),
  'EvaluationGrade_GetList': HrOperationMeta(method: 'GET', path: '/api/app/evaluation-grade', permission: 'Hr.Evaluations.Grades.Manage', roles: <String>['systemAdmin'], release: '2', errorCodes: <String>[]),
  'EvaluationGrade_Create': HrOperationMeta(method: 'POST', path: '/api/app/evaluation-grade', permission: 'Hr.Evaluations.Grades.Manage', roles: <String>['systemAdmin'], release: '2', errorCodes: <String>[]),
  'EvaluationGrade_Update': HrOperationMeta(method: 'PUT', path: '/api/app/evaluation-grade/{id}', permission: 'Hr.Evaluations.Grades.Manage', roles: <String>['systemAdmin'], release: '2', errorCodes: <String>['Hr:Evaluations:MinimumActiveGrades']),
  'DepartmentReport_GetList': HrOperationMeta(method: 'GET', path: '/api/app/department-report', permission: 'Hr.DepartmentReports.Read', roles: <String>['departmentManager', 'hrManager'], release: '2', errorCodes: <String>[]),
  'DepartmentReport_Get': HrOperationMeta(method: 'GET', path: '/api/app/department-report/{id}', permission: 'Hr.DepartmentReports.Read', roles: <String>['departmentManager', 'hrManager'], release: '2', errorCodes: <String>[]),
  'DepartmentReport_UpdateNotes': HrOperationMeta(method: 'PATCH', path: '/api/app/department-report/{id}', permission: 'Hr.DepartmentReports.Manage', roles: <String>['departmentManager'], release: '2', errorCodes: <String>['Hr:DepartmentReports:AlreadySubmitted']),
  'DepartmentReport_Submit': HrOperationMeta(method: 'POST', path: '/api/app/department-report/{id}/submit', permission: 'Hr.DepartmentReports.Manage', roles: <String>['departmentManager'], release: '2', errorCodes: <String>['Hr:DepartmentReports:AlreadySubmitted']),
  'ReviewReport_GetList': HrOperationMeta(method: 'GET', path: '/api/app/review-report', permission: 'Hr.ReviewReports.Read', roles: <String>['reviewer', 'executiveDirector'], release: '2', errorCodes: <String>[]),
  'ReviewReport_Create': HrOperationMeta(method: 'POST', path: '/api/app/review-report', permission: 'Hr.ReviewReports.Manage', roles: <String>['reviewer'], release: '2', errorCodes: <String>['Hr:ReviewReports:InvalidPeriod', 'Hr:ReviewReports:NoTeams']),
  'ReviewReport_Get': HrOperationMeta(method: 'GET', path: '/api/app/review-report/{id}', permission: 'Hr.ReviewReports.Read', roles: <String>['reviewer', 'executiveDirector'], release: '2', errorCodes: <String>[]),
  'ReviewReport_Update': HrOperationMeta(method: 'PUT', path: '/api/app/review-report/{id}', permission: 'Hr.ReviewReports.Manage', roles: <String>['reviewer'], release: '2', errorCodes: <String>['Hr:ReviewReports:AlreadySubmitted', 'Hr:ReviewReports:InvalidPeriod', 'Hr:ReviewReports:UnknownTeam']),
  'ReviewReport_Delete': HrOperationMeta(method: 'DELETE', path: '/api/app/review-report/{id}', permission: 'Hr.ReviewReports.Manage', roles: <String>['reviewer'], release: '2', errorCodes: <String>['Hr:ReviewReports:AlreadySubmitted']),
  'ReviewReport_Submit': HrOperationMeta(method: 'POST', path: '/api/app/review-report/{id}/submit', permission: 'Hr.ReviewReports.Manage', roles: <String>['reviewer'], release: '2', errorCodes: <String>['Hr:ReviewReports:AlreadySubmitted', 'Hr:ReviewReports:IncompleteTeams']),
  'Report_GetTeamAttendance': HrOperationMeta(method: 'GET', path: '/api/app/report/team-attendance', permission: 'Hr.Reports.Team', roles: <String>['departmentManager'], release: '1', errorCodes: <String>['Hr:Reports:InvalidPeriod']),
  'Report_GetTeamLatenessAbsence': HrOperationMeta(method: 'GET', path: '/api/app/report/team-lateness-absence', permission: 'Hr.Reports.Team', roles: <String>['departmentManager'], release: '1', errorCodes: <String>['Hr:Reports:InvalidPeriod']),
  'Report_GetTeamLeaves': HrOperationMeta(method: 'GET', path: '/api/app/report/team-leaves', permission: 'Hr.Reports.Team', roles: <String>['departmentManager'], release: '1', errorCodes: <String>['Hr:Reports:InvalidPeriod']),
  'Report_GetTeamPending': HrOperationMeta(method: 'GET', path: '/api/app/report/team-pending', permission: 'Hr.Reports.Team', roles: <String>['departmentManager'], release: '1', errorCodes: <String>[]),
  'Report_GetTeamCustody': HrOperationMeta(method: 'GET', path: '/api/app/report/team-custody', permission: 'Hr.Reports.Team', roles: <String>['departmentManager'], release: '2', errorCodes: <String>[]),
  'Report_GetTeamEvaluations': HrOperationMeta(method: 'GET', path: '/api/app/report/team-evaluations', permission: 'Hr.Reports.Team', roles: <String>['departmentManager'], release: '2', errorCodes: <String>[]),
  'Report_GetReviewDashboard': HrOperationMeta(method: 'GET', path: '/api/app/report/review-dashboard', permission: 'Hr.Reports.Review', roles: <String>['reviewer'], release: '1', errorCodes: <String>[]),
  'Report_GetEvaluationConsistency': HrOperationMeta(method: 'GET', path: '/api/app/report/evaluation-consistency', permission: 'Hr.Reports.Review', roles: <String>['reviewer'], release: '2', errorCodes: <String>[]),
  'Report_GetHrAttendance': HrOperationMeta(method: 'GET', path: '/api/app/report/hr-attendance', permission: 'Hr.Reports.Hr', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Reports:InvalidPeriod']),
  'Report_GetHrLeaveBalances': HrOperationMeta(method: 'GET', path: '/api/app/report/hr-leave-balances', permission: 'Hr.Reports.Hr', roles: <String>['hrManager'], release: '1', errorCodes: <String>[]),
  'Report_GetHrRequests': HrOperationMeta(method: 'GET', path: '/api/app/report/hr-requests', permission: 'Hr.Reports.Hr', roles: <String>['hrManager'], release: '1', errorCodes: <String>['Hr:Reports:InvalidPeriod']),
  'Report_GetHrContracts': HrOperationMeta(method: 'GET', path: '/api/app/report/hr-contracts', permission: 'Hr.Reports.Hr', roles: <String>['hrManager'], release: '3', errorCodes: <String>[]),
  'Report_GetHrWarnings': HrOperationMeta(method: 'GET', path: '/api/app/report/hr-warnings', permission: 'Hr.Reports.Hr', roles: <String>['hrManager'], release: '3', errorCodes: <String>['Hr:Reports:InvalidPeriod']),
  'Report_Export': HrOperationMeta(method: 'GET', path: '/api/app/report/{code}/export', permission: 'Hr.Reports.Export', roles: <String>['departmentManager', 'reviewer', 'hrManager'], release: '1', errorCodes: <String>['Hr:Reports:InvalidPeriod', 'Hr:Reports:MissingFilter']),
  'Circular_GetReceivedList': HrOperationMeta(method: 'GET', path: '/api/app/circular', permission: 'Hr.Circulars.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '2', errorCodes: <String>[]),
  'Circular_Create': HrOperationMeta(method: 'POST', path: '/api/app/circular', permission: 'Hr.Circulars.Send', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '2', errorCodes: <String>['Hr:Circulars:ScopeNotAllowed', 'Hr:Circulars:NoRecipients']),
  'Circular_GetSentList': HrOperationMeta(method: 'GET', path: '/api/app/circular/sent', permission: 'Hr.Circulars.Send', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '2', errorCodes: <String>[]),
  'Circular_Get': HrOperationMeta(method: 'GET', path: '/api/app/circular/{id}', permission: 'Hr.Circulars.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '2', errorCodes: <String>[]),
  'Circular_GetReceipts': HrOperationMeta(method: 'GET', path: '/api/app/circular/{id}/receipt', permission: 'Hr.Circulars.Send', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '2', errorCodes: <String>[]),
  'Circular_Acknowledge': HrOperationMeta(method: 'POST', path: '/api/app/circular/{id}/acknowledge', permission: 'Hr.Circulars.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '2', errorCodes: <String>[]),
  'Contract_List': HrOperationMeta(method: 'GET', path: '/api/app/contract', permission: 'Hr.Contract.Read', roles: <String>['hrManager'], release: '3', errorCodes: <String>[]),
  'Contract_Create': HrOperationMeta(method: 'POST', path: '/api/app/contract', permission: 'Hr.Contract.Create', roles: <String>['hrManager'], release: '3', errorCodes: <String>['Hr:Contract:EmployeeNotFound', 'Hr:Contract:InvalidDateRange', 'Hr:Contract:OverlappingContract', 'Hr:Contract:InvalidPreviousContract']),
  'Contract_GetExpiring': HrOperationMeta(method: 'GET', path: '/api/app/contract/expiring', permission: 'Hr.Contract.Read', roles: <String>['hrManager'], release: '3', errorCodes: <String>[]),
  'Contract_Get': HrOperationMeta(method: 'GET', path: '/api/app/contract/{id}', permission: 'Hr.Contract.Read', roles: <String>['hrManager'], release: '3', errorCodes: <String>[]),
  'Contract_Recommend': HrOperationMeta(method: 'POST', path: '/api/app/contract/{id}/recommend', permission: 'Hr.Contract.Recommend', roles: <String>['hrManager'], release: '3', errorCodes: <String>['Hr:Contract:InvalidStatus', 'Hr:Contract:SelfActionNotAllowed']),
  'Contract_Decide': HrOperationMeta(method: 'POST', path: '/api/app/contract/{id}/decide', permission: 'Hr.Contract.Decide', roles: <String>['hrManager'], release: '3', errorCodes: <String>['Hr:Contract:InvalidStatus', 'Hr:Contract:SelfActionNotAllowed']),
  'Contract_GetMyList': HrOperationMeta(method: 'GET', path: '/api/app/contract/my', permission: 'Hr.Contract.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'Contract_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/contract/my/{id}', permission: 'Hr.Contract.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'Warning_Create': HrOperationMeta(method: 'POST', path: '/api/app/warning', permission: 'Hr.Warning.Recommend', roles: <String>['departmentManager', 'reviewer'], release: '3', errorCodes: <String>['Hr:Warning:EmployeeNotFound', 'Hr:Warning:TargetNotInTeam', 'Hr:Warning:TargetIsDepartmentManager', 'Hr:Warning:TargetNotDepartmentManager', 'Hr:Warning:SelfRecommendationNotAllowed', 'Hr:Warning:IncidentDateInFuture', 'Hr:Warning:AttachmentRejected', 'Hr:Document:FileTooLarge', 'Hr:Document:FileTypeNotAllowed']),
  'Warning_List': HrOperationMeta(method: 'GET', path: '/api/app/warning', permission: 'Hr.Warning.ReadAll', roles: <String>['hrManager'], release: '3', errorCodes: <String>[]),
  'Warning_GetObjections': HrOperationMeta(method: 'GET', path: '/api/app/warning/objection', permission: 'Hr.Warning.ResolveObjection', roles: <String>['executiveDirector'], release: '3', errorCodes: <String>[]),
  'Warning_GetRecommendedByMe': HrOperationMeta(method: 'GET', path: '/api/app/warning/recommended-by-me', permission: 'Hr.Warning.Recommend', roles: <String>['departmentManager', 'reviewer'], release: '3', errorCodes: <String>[]),
  'Warning_GetMyList': HrOperationMeta(method: 'GET', path: '/api/app/warning/my', permission: 'Hr.Warning.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'Warning_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/warning/my/{id}', permission: 'Hr.Warning.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'Warning_GetEmployeeFile': HrOperationMeta(method: 'GET', path: '/api/app/warning/employee/{employeeId}', permission: 'Hr.Warning.ReadEmployeeFile', roles: <String>['departmentManager', 'hrManager'], release: '3', errorCodes: <String>[]),
  'Warning_Get': HrOperationMeta(method: 'GET', path: '/api/app/warning/{id}', permission: 'Hr.Warning.Read', roles: <String>['hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'Warning_GetAttachmentDownloadUrl': HrOperationMeta(method: 'GET', path: '/api/app/warning/{id}/attachment/{attachmentId}/download-url', permission: 'Hr.Warning.Read', roles: <String>['hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'Warning_Notify': HrOperationMeta(method: 'POST', path: '/api/app/warning/{id}/notify', permission: 'Hr.Warning.Notify', roles: <String>['hrManager'], release: '3', errorCodes: <String>['Hr:Warning:InvalidStatus', 'Hr:Warning:SelfDecisionNotAllowed']),
  'Warning_Respond': HrOperationMeta(method: 'POST', path: '/api/app/warning/{id}/respond', permission: 'Hr.Warning.Respond', roles: <String>['employee'], release: '3', errorCodes: <String>['Hr:Warning:InvalidStatus', 'Hr:Warning:ResponseWindowExpired', 'Hr:Warning:ResponseAlreadySubmitted']),
  'Warning_Decide': HrOperationMeta(method: 'POST', path: '/api/app/warning/{id}/decide', permission: 'Hr.Warning.Decide', roles: <String>['hrManager'], release: '3', errorCodes: <String>['Hr:Warning:InvalidStatus', 'Hr:Warning:ResponseWindowOpen', 'Hr:Warning:LevelRequired', 'Hr:Warning:SelfDecisionNotAllowed']),
  'Warning_Object': HrOperationMeta(method: 'POST', path: '/api/app/warning/{id}/object', permission: 'Hr.Warning.Object', roles: <String>['employee'], release: '3', errorCodes: <String>['Hr:Warning:InvalidStatus', 'Hr:Warning:ObjectionAlreadySubmitted']),
  'Warning_ResolveObjection': HrOperationMeta(method: 'POST', path: '/api/app/warning/{id}/resolve-objection', permission: 'Hr.Warning.ResolveObjection', roles: <String>['executiveDirector'], release: '3', errorCodes: <String>['Hr:Warning:InvalidStatus']),
  'AppreciationLetter_Create': HrOperationMeta(method: 'POST', path: '/api/app/appreciation-letter', permission: 'Hr.AppreciationLetter.Issue', roles: <String>['executiveDirector'], release: '3', errorCodes: <String>['Hr:Appreciation:EmployeeNotFound', 'Hr:Appreciation:SelfIssueNotAllowed', 'Hr:Appreciation:FileRejected', 'Hr:Document:FileTooLarge', 'Hr:Document:FileTypeNotAllowed']),
  'AppreciationLetter_List': HrOperationMeta(method: 'GET', path: '/api/app/appreciation-letter', permission: 'Hr.AppreciationLetter.ReadAll', roles: <String>['hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'AppreciationLetter_GetMyList': HrOperationMeta(method: 'GET', path: '/api/app/appreciation-letter/my', permission: 'Hr.AppreciationLetter.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'AppreciationLetter_GetEmployeeFile': HrOperationMeta(method: 'GET', path: '/api/app/appreciation-letter/employee/{employeeId}', permission: 'Hr.AppreciationLetter.ReadEmployeeFile', roles: <String>['departmentManager', 'hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'AppreciationLetter_Get': HrOperationMeta(method: 'GET', path: '/api/app/appreciation-letter/{id}', permission: 'Hr.AppreciationLetter.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'AppreciationLetter_GetDownloadUrl': HrOperationMeta(method: 'GET', path: '/api/app/appreciation-letter/{id}/download-url', permission: 'Hr.AppreciationLetter.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'AppreciationLetter_Acknowledge': HrOperationMeta(method: 'POST', path: '/api/app/appreciation-letter/{id}/acknowledge', permission: 'Hr.AppreciationLetter.Acknowledge', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'SalaryProfile_List': HrOperationMeta(method: 'GET', path: '/api/app/finance/salary-profile', permission: 'Hr.Finance.SalaryProfile.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'SalaryProfile_Get': HrOperationMeta(method: 'GET', path: '/api/app/finance/salary-profile/{employeeId}', permission: 'Hr.Finance.SalaryProfile.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'SalaryProfile_Update': HrOperationMeta(method: 'PUT', path: '/api/app/finance/salary-profile/{employeeId}', permission: 'Hr.Finance.SalaryProfile.Update', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:EffectiveDateNotAfterLatest', 'Hr:Finance:EffectiveDateInLockedPeriod', 'Hr:Finance:DuplicateComponentName', 'Hr:Finance:SelfChangeNotAllowed']),
  'SalaryProfile_GetHistory': HrOperationMeta(method: 'GET', path: '/api/app/finance/salary-profile/{employeeId}/history', permission: 'Hr.Finance.SalaryProfile.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'Adjustment_List': HrOperationMeta(method: 'GET', path: '/api/app/finance/adjustment', permission: 'Hr.Finance.Adjustment.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'Adjustment_Create': HrOperationMeta(method: 'POST', path: '/api/app/finance/adjustment', permission: 'Hr.Finance.Adjustment.Create', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:EmployeeNotFound', 'Hr:Finance:SalaryProfileMissing', 'Hr:Finance:PeriodLocked', 'Hr:Finance:AttachmentRejected', 'Hr:Finance:RequestNotAtAccountantStep', 'Hr:Finance:SelfChangeNotAllowed', 'Hr:Document:FileTooLarge', 'Hr:Document:FileTypeNotAllowed']),
  'Adjustment_Reverse': HrOperationMeta(method: 'POST', path: '/api/app/finance/adjustment/{id}/reverse', permission: 'Hr.Finance.Adjustment.Create', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:AdjustmentAlreadyReversed', 'Hr:Finance:PeriodLocked', 'Hr:Finance:SelfChangeNotAllowed']),
  'Adjustment_GetAttachmentDownloadUrl': HrOperationMeta(method: 'GET', path: '/api/app/finance/adjustment/{id}/attachment-download-url', permission: 'Hr.Finance.Adjustment.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'AdjustmentSuggestion_List': HrOperationMeta(method: 'GET', path: '/api/app/finance/adjustment/suggestions', permission: 'Hr.Finance.Adjustment.Review', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'AdjustmentSuggestion_Approve': HrOperationMeta(method: 'POST', path: '/api/app/finance/adjustment/suggestions/{id}/approve', permission: 'Hr.Finance.Adjustment.Review', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:SuggestionAlreadyReviewed', 'Hr:Finance:SuggestionAmountRequired', 'Hr:Finance:SalaryProfileMissing', 'Hr:Finance:PeriodLocked', 'Hr:Finance:SelfChangeNotAllowed']),
  'AdjustmentSuggestion_Reject': HrOperationMeta(method: 'POST', path: '/api/app/finance/adjustment/suggestions/{id}/reject', permission: 'Hr.Finance.Adjustment.Review', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:SuggestionAlreadyReviewed', 'Hr:Finance:SelfChangeNotAllowed']),
  'PayrollRun_List': HrOperationMeta(method: 'GET', path: '/api/app/finance/payroll-run', permission: 'Hr.Finance.PayrollRun.Read', roles: <String>['accountant', 'hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'PayrollRun_Create': HrOperationMeta(method: 'POST', path: '/api/app/finance/payroll-run', permission: 'Hr.Finance.PayrollRun.Prepare', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'PayrollRun_Get': HrOperationMeta(method: 'GET', path: '/api/app/finance/payroll-run/{id}', permission: 'Hr.Finance.PayrollRun.Read', roles: <String>['accountant', 'hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'PayrollRun_Recalculate': HrOperationMeta(method: 'POST', path: '/api/app/finance/payroll-run/{id}/recalculate', permission: 'Hr.Finance.PayrollRun.Prepare', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:InvalidPayrollRunStatus']),
  'PayrollRun_RequestApproval': HrOperationMeta(method: 'POST', path: '/api/app/finance/payroll-run/{id}/request-approval', permission: 'Hr.Finance.PayrollRun.Prepare', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:InvalidPayrollRunStatus', 'Hr:Finance:PayrollRunStale', 'Hr:Finance:PayrollApproverNotConfigured']),
  'PayrollRun_Approve': HrOperationMeta(method: 'POST', path: '/api/app/finance/payroll-run/{id}/approve', permission: 'Hr.Finance.PayrollRun.Approve', roles: <String>['hrManager', 'executiveDirector'], release: '3', errorCodes: <String>['Hr:Finance:InvalidPayrollRunStatus', 'Hr:Finance:ApprovalNotRequested', 'Hr:Finance:PayrollRunStale', 'Hr:Finance:PayrollApproverNotConfigured', 'Hr:Finance:NotConfiguredPayrollApprover', 'Hr:Finance:ApproverIsPreparer']),
  'PayrollRun_Close': HrOperationMeta(method: 'POST', path: '/api/app/finance/payroll-run/{id}/close', permission: 'Hr.Finance.PayrollRun.Close', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:InvalidPayrollRunStatus']),
  'PayrollRun_Export': HrOperationMeta(method: 'GET', path: '/api/app/finance/payroll-run/{id}/export', permission: 'Hr.Finance.PayrollRun.Export', roles: <String>['accountant', 'hrManager', 'executiveDirector'], release: '3', errorCodes: <String>[]),
  'Payslip_GetMyList': HrOperationMeta(method: 'GET', path: '/api/app/finance/payslip/my', permission: 'Hr.Finance.Payslip.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'Payslip_GetMy': HrOperationMeta(method: 'GET', path: '/api/app/finance/payslip/my/{id}', permission: 'Hr.Finance.Payslip.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'Payslip_DownloadMyPdf': HrOperationMeta(method: 'GET', path: '/api/app/finance/payslip/my/{id}/pdf', permission: 'Hr.Finance.Payslip.ReadOwn', roles: <String>['employee'], release: '3', errorCodes: <String>[]),
  'Expense_List': HrOperationMeta(method: 'GET', path: '/api/app/finance/expense', permission: 'Hr.Finance.Expense.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'Expense_Create': HrOperationMeta(method: 'POST', path: '/api/app/finance/expense', permission: 'Hr.Finance.Expense.Create', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:AttachmentRejected', 'Hr:Finance:RequestNotAtAccountantStep', 'Hr:Document:FileTooLarge', 'Hr:Document:FileTypeNotAllowed']),
  'Expense_Export': HrOperationMeta(method: 'GET', path: '/api/app/finance/expense/export', permission: 'Hr.Finance.Expense.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'Expense_Get': HrOperationMeta(method: 'GET', path: '/api/app/finance/expense/{id}', permission: 'Hr.Finance.Expense.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'Expense_Reverse': HrOperationMeta(method: 'POST', path: '/api/app/finance/expense/{id}/reverse', permission: 'Hr.Finance.Expense.Create', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:ExpenseAlreadyReversed']),
  'Expense_GetAttachmentDownloadUrl': HrOperationMeta(method: 'GET', path: '/api/app/finance/expense/{id}/attachment-download-url', permission: 'Hr.Finance.Expense.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'AccountantRequest_List': HrOperationMeta(method: 'GET', path: '/api/app/finance/accountant-request', permission: 'Hr.Finance.AccountantRequest.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'AccountantRequest_Get': HrOperationMeta(method: 'GET', path: '/api/app/finance/accountant-request/{id}', permission: 'Hr.Finance.AccountantRequest.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'FinanceReport_GetMonthlyStatement': HrOperationMeta(method: 'GET', path: '/api/app/finance/report/monthly-statement', permission: 'Hr.Finance.Report.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:PayrollRunNotFoundForPeriod']),
  'FinanceReport_GetAdjustments': HrOperationMeta(method: 'GET', path: '/api/app/finance/report/adjustments', permission: 'Hr.Finance.Report.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'FinanceReport_GetStoreExpenses': HrOperationMeta(method: 'GET', path: '/api/app/finance/report/store-expenses', permission: 'Hr.Finance.Report.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>[]),
  'FinanceReport_Export': HrOperationMeta(method: 'GET', path: '/api/app/finance/report/{code}/export', permission: 'Hr.Finance.Report.Read', roles: <String>['accountant'], release: '3', errorCodes: <String>['Hr:Finance:ReportFilterRequired', 'Hr:Finance:PayrollRunNotFoundForPeriod']),
  'Notification_List': HrOperationMeta(method: 'GET', path: '/api/app/notification', permission: 'Hr.Notification.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Notification_GetUnreadCount': HrOperationMeta(method: 'GET', path: '/api/app/notification/unread-count', permission: 'Hr.Notification.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'Notification_MarkRead': HrOperationMeta(method: 'POST', path: '/api/app/notification/{id}/read', permission: 'Hr.Notification.Read', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'DeviceToken_Register': HrOperationMeta(method: 'POST', path: '/api/app/device-token', permission: 'Hr.Notification.Device', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'DeviceToken_Unregister': HrOperationMeta(method: 'POST', path: '/api/app/device-token/unregister', permission: 'Hr.Notification.Device', roles: <String>['employee', 'departmentManager', 'hrManager', 'executiveDirector', 'reviewer', 'accountant', 'storeOfficer', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'SystemSettings_ListSections': HrOperationMeta(method: 'GET', path: '/api/app/system-settings', permission: 'Hr.Settings.Read', roles: <String>['systemAdmin', 'hrManager'], release: '1', errorCodes: <String>[]),
  'SystemSettings_GetReminders': HrOperationMeta(method: 'GET', path: '/api/app/system-settings/reminders', permission: 'Hr.Settings.Reminders', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'SystemSettings_UpdateReminders': HrOperationMeta(method: 'PUT', path: '/api/app/system-settings/reminders', permission: 'Hr.Settings.Reminders', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>['Hr:Settings:InvalidEscalationDay']),
  'SystemSettings_GetContracts': HrOperationMeta(method: 'GET', path: '/api/app/system-settings/contracts', permission: 'Hr.Settings.Contracts', roles: <String>['systemAdmin'], release: '3', errorCodes: <String>[]),
  'SystemSettings_UpdateContracts': HrOperationMeta(method: 'PUT', path: '/api/app/system-settings/contracts', permission: 'Hr.Settings.Contracts', roles: <String>['systemAdmin'], release: '3', errorCodes: <String>[]),
  'SystemSettings_GetComplaints': HrOperationMeta(method: 'GET', path: '/api/app/system-settings/complaints', permission: 'Hr.Settings.Complaints', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'SystemSettings_UpdateComplaints': HrOperationMeta(method: 'PUT', path: '/api/app/system-settings/complaints', permission: 'Hr.Settings.Complaints', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'SystemSettings_GetHourlyLeave': HrOperationMeta(method: 'GET', path: '/api/app/system-settings/hourly-leave', permission: 'Hr.Settings.HourlyLeave', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'SystemSettings_UpdateHourlyLeave': HrOperationMeta(method: 'PUT', path: '/api/app/system-settings/hourly-leave', permission: 'Hr.Settings.HourlyLeave', roles: <String>['systemAdmin'], release: '1', errorCodes: <String>[]),
  'SystemSettings_GetFinance': HrOperationMeta(method: 'GET', path: '/api/app/system-settings/finance', permission: 'Hr.Settings.Finance', roles: <String>['systemAdmin'], release: '3', errorCodes: <String>[]),
  'SystemSettings_UpdateFinance': HrOperationMeta(method: 'PUT', path: '/api/app/system-settings/finance', permission: 'Hr.Settings.Finance', roles: <String>['systemAdmin'], release: '3', errorCodes: <String>[]),
  'AuditLog_List': HrOperationMeta(method: 'GET', path: '/api/app/audit-log', permission: 'Hr.AuditLog.Read', roles: <String>['systemAdmin', 'executiveDirector'], release: '1', errorCodes: <String>[]),
  'UserAccount_Lookup': HrOperationMeta(method: 'GET', path: '/api/app/user-account/lookup', permission: 'Hr.Identity.UserAccount.Lookup', roles: <String>['executiveDirector', 'hrManager', 'systemAdmin'], release: '1', errorCodes: <String>[]),
  'EvaluationCycle_GetList': HrOperationMeta(method: 'GET', path: '/api/app/evaluation-cycle', permission: 'Hr.Evaluations.Cycle.Read', roles: <String>['departmentManager', 'hrManager', 'reviewer'], release: '2', errorCodes: <String>[]),
  'PayrollRun_ReturnForChanges': HrOperationMeta(method: 'POST', path: '/api/app/finance/payroll-run/{id}/return', permission: 'Hr.Finance.PayrollRun.Approve', roles: <String>['hrManager', 'executiveDirector'], release: '3', errorCodes: <String>['Hr:Finance:ApprovalNotRequested']),
};
