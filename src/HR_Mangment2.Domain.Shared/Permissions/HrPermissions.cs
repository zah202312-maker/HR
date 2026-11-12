using System.Collections.Generic;

namespace HR_Mangment2.Permissions;

/// <summary>Generated from x-permission and x-allowed-roles in api-contract/openapi.yaml.</summary>
public static class HrPermissions
{
    public static class AppreciationLetterPermissions
    {
        public const string Acknowledge = "Hr.AppreciationLetter.Acknowledge";
        public const string Issue = "Hr.AppreciationLetter.Issue";
        public const string Read = "Hr.AppreciationLetter.Read";
        public const string ReadEmployeeFile = "Hr.AppreciationLetter.ReadEmployeeFile";
        public const string ReadAll = "Hr.AppreciationLetter.ReadAll";
        public const string ReadOwn = "Hr.AppreciationLetter.ReadOwn";
    }

    public static class AttendancePermissions
    {
        public const string CorrectionCreate = "Hr.Attendance.Correction.Create";
        public const string CorrectionView = "Hr.Attendance.Correction.View";
        public const string DeviceManage = "Hr.Attendance.Device.Manage";
        public const string DeviceView = "Hr.Attendance.Device.View";
        public const string DeviceMapManage = "Hr.Attendance.DeviceMap.Manage";
        public const string GeoFenceManage = "Hr.Attendance.GeoFence.Manage";
        public const string GpsPunch = "Hr.Attendance.Gps.Punch";
        public const string GpsReviewFlagged = "Hr.Attendance.Gps.ReviewFlagged";
        public const string Import = "Hr.Attendance.Import";
        public const string SettingsManage = "Hr.Attendance.Settings.Manage";
        public const string SettingsView = "Hr.Attendance.Settings.View";
        public const string ViewDayDetail = "Hr.Attendance.ViewDayDetail";
        public const string ViewEmployee = "Hr.Attendance.ViewEmployee";
        public const string ViewOwn = "Hr.Attendance.ViewOwn";
        public const string ViewTeam = "Hr.Attendance.ViewTeam";
    }

    public static class AuditLogPermissions
    {
        public const string Read = "Hr.AuditLog.Read";
    }

    public static class CircularsPermissions
    {
        public const string Read = "Hr.Circulars.Read";
        public const string Send = "Hr.Circulars.Send";
    }

    public static class ReviewerEvaluationPermissions
    {
        public const string Manage = "Hr.ReviewerEvaluation.Manage";
        public const string ReadReports = "Hr.ReviewerEvaluation.ReadReports";
    }

    public static class HrEvaluationPermissions
    {
        public const string Manage = "Hr.HrEvaluation.Manage";
        public const string ReadReports = "Hr.HrEvaluation.ReadReports";
    }

    public static class NoticesPermissions
    {
        public const string Read = "Hr.Notices.Read";
        public const string Send = "Hr.Notices.Send";
    }

    public static class ComplaintsPermissions
    {
        public const string Attachment = "Hr.Complaints.Attachment";
        public const string Close = "Hr.Complaints.Close";
        public const string Handle = "Hr.Complaints.Handle";
        public const string Forward = "Hr.Complaints.Forward";
        public const string HandlersApprove = "Hr.Complaints.Handlers.Approve";
        public const string HandlersRead = "Hr.Complaints.Handlers.Read";
        public const string HandlersRequest = "Hr.Complaints.Handlers.Request";
        public const string RaiseLevel = "Hr.Complaints.RaiseLevel";
        public const string Submit = "Hr.Complaints.Submit";
        public const string ViewAccessLog = "Hr.Complaints.ViewAccessLog";
        public const string ViewOwn = "Hr.Complaints.ViewOwn";
    }

    public static class ContractPermissions
    {
        public const string Create = "Hr.Contract.Create";
        public const string Decide = "Hr.Contract.Decide";
        public const string Read = "Hr.Contract.Read";
        public const string ReadOverview = "Hr.Contract.ReadOverview";
        public const string ReadOwn = "Hr.Contract.ReadOwn";
        public const string Recommend = "Hr.Contract.Recommend";
    }

    public static class CustodyPermissions
    {
        public const string Acknowledge = "Hr.Custody.Acknowledge";
        public const string Count = "Hr.Custody.Count";
        public const string Create = "Hr.Custody.Create";
        public const string Return = "Hr.Custody.Return";
        public const string View = "Hr.Custody.View";
        public const string ViewAll = "Hr.Custody.ViewAll";
        public const string ViewCount = "Hr.Custody.ViewCount";
        public const string ViewOwn = "Hr.Custody.ViewOwn";
        public const string ViewTeam = "Hr.Custody.ViewTeam";
    }

    public static class DelegationPermissions
    {
        public const string Manage = "Hr.Delegation.Manage";
        public const string ReadAll = "Hr.Delegation.ReadAll";
    }

    public static class DepartmentPermissions
    {
        public const string Lookup = "Hr.Department.Lookup";
        public const string Manage = "Hr.Department.Manage";
        public const string Read = "Hr.Department.Read";
    }

    public static class DepartmentReportsPermissions
    {
        public const string Manage = "Hr.DepartmentReports.Manage";
        public const string Read = "Hr.DepartmentReports.Read";
    }

    public static class EmployeePermissions
    {
        public const string AssignManager = "Hr.Employee.AssignManager";
        public const string CorrectHireDate = "Hr.Employee.CorrectHireDate";
        public const string Create = "Hr.Employee.Create";
        public const string DocumentDelete = "Hr.Employee.Document.Delete";
        public const string DocumentRead = "Hr.Employee.Document.Read";
        public const string DocumentUpload = "Hr.Employee.Document.Upload";
        public const string Lookup = "Hr.Employee.Lookup";
        public const string Read = "Hr.Employee.Read";
        public const string ReadManagerHistory = "Hr.Employee.ReadManagerHistory";
        public const string ReadOwnTaskLog = "Hr.Employee.ReadOwnTaskLog";
        public const string ReadSelf = "Hr.Employee.ReadSelf";
        public const string ReadTaskLog = "Hr.Employee.ReadTaskLog";
        public const string ReadTeam = "Hr.Employee.ReadTeam";
        public const string Update = "Hr.Employee.Update";
    }

    public static class EmployeeShiftPermissions
    {
        public const string Assign = "Hr.EmployeeShift.Assign";
        public const string View = "Hr.EmployeeShift.View";
    }

    public static class EvaluationsPermissions
    {
        public const string AnnualSummaryRead = "Hr.Evaluations.AnnualSummary.Read";
        public const string CycleRead = "Hr.Evaluations.Cycle.Read";
        public const string DecideObjection = "Hr.Evaluations.DecideObjection";
        public const string HrRead = "Hr.Evaluations.HrRead";
        public const string InternalNoteRead = "Hr.Evaluations.InternalNote.Read";
        public const string InternalNoteWrite = "Hr.Evaluations.InternalNote.Write";
        public const string Manage = "Hr.Evaluations.Manage";
        public const string ViewOwn = "Hr.Evaluations.ViewOwn";
    }

    public static class FinancePermissions
    {
        public const string AccountantRequestRead = "Hr.Finance.AccountantRequest.Read";
        public const string AdjustmentCreate = "Hr.Finance.Adjustment.Create";
        public const string AdjustmentRead = "Hr.Finance.Adjustment.Read";
        public const string AdjustmentReview = "Hr.Finance.Adjustment.Review";
        public const string ExpenseCreate = "Hr.Finance.Expense.Create";
        public const string ExpenseRead = "Hr.Finance.Expense.Read";
        public const string PayrollRunApprove = "Hr.Finance.PayrollRun.Approve";
        public const string PayrollRunClose = "Hr.Finance.PayrollRun.Close";
        public const string PayrollRunDeliver = "Hr.Finance.PayrollRun.Deliver";
        public const string PettyCashManage = "Hr.Finance.PettyCash.Manage";
        public const string PettyCashRead = "Hr.Finance.PettyCash.Read";
        public const string PayrollRunExport = "Hr.Finance.PayrollRun.Export";
        public const string PayrollRunPrepare = "Hr.Finance.PayrollRun.Prepare";
        public const string PayrollRunRead = "Hr.Finance.PayrollRun.Read";
        public const string PayslipReadOwn = "Hr.Finance.Payslip.ReadOwn";
        public const string ReportRead = "Hr.Finance.Report.Read";
        public const string SalaryProfileRead = "Hr.Finance.SalaryProfile.Read";
        public const string SalaryProfileReadOverview = "Hr.Finance.SalaryProfile.ReadOverview";
        public const string SalaryProfileUpdate = "Hr.Finance.SalaryProfile.Update";
    }

    public static class HolidayPermissions
    {
        public const string Manage = "Hr.Holiday.Manage";
        public const string View = "Hr.Holiday.View";
    }

    public static class IdentityPermissions
    {
        public const string PermissionManage = "Hr.Identity.Permission.Manage";
        public const string RoleManage = "Hr.Identity.Role.Manage";
        public const string RoleRead = "Hr.Identity.Role.Read";
        public const string RoleReadDetail = "Hr.Identity.Role.ReadDetail";
        public const string UserAccountCreate = "Hr.Identity.UserAccount.Create";
        public const string UserAccountLookup = "Hr.Identity.UserAccount.Lookup";
        public const string UserAccountRead = "Hr.Identity.UserAccount.Read";
        public const string UserAccountResetMfa = "Hr.Identity.UserAccount.ResetMfa";
        public const string UserAccountResetPassword = "Hr.Identity.UserAccount.ResetPassword";
        public const string UserAccountSetRoles = "Hr.Identity.UserAccount.SetRoles";
    }

    public static class IncidentPermissions
    {
        public const string Create = "Hr.Incident.Create";
        public const string Process = "Hr.Incident.Process";
        public const string Forward = "Hr.Incident.Forward";
        public const string UpdateStatus = "Hr.Incident.UpdateStatus";
        public const string View = "Hr.Incident.View";
        public const string ViewOwn = "Hr.Incident.ViewOwn";
    }

    public static class LeavePermissions
    {
        public const string BalanceAdjust = "Hr.Leave.Balance.Adjust";
        public const string RequestReverse = "Hr.Leave.Request.Reverse";
        public const string BalanceView = "Hr.Leave.Balance.View";
        public const string BalanceViewOwn = "Hr.Leave.Balance.ViewOwn";
        public const string CalendarViewTeam = "Hr.Leave.Calendar.ViewTeam";
        public const string LedgerView = "Hr.Leave.Ledger.View";
        public const string LedgerViewOwn = "Hr.Leave.Ledger.ViewOwn";
        public const string OtherTypeManage = "Hr.Leave.OtherType.Manage";
        public const string RequestCreate = "Hr.Leave.Request.Create";
        public const string RequestView = "Hr.Leave.Request.View";
        public const string RequestViewEmployee = "Hr.Leave.Request.ViewEmployee";
        public const string RequestViewOwn = "Hr.Leave.Request.ViewOwn";
    }

    public static class MemoPermissions
    {
        public const string Close = "Hr.Memo.Close";
        public const string Create = "Hr.Memo.Create";
        public const string Process = "Hr.Memo.Process";
        public const string Forward = "Hr.Memo.Forward";
        public const string View = "Hr.Memo.View";
        public const string ViewOwn = "Hr.Memo.ViewOwn";
    }

    public static class NotificationPermissions
    {
        public const string Device = "Hr.Notification.Device";
        public const string Read = "Hr.Notification.Read";
    }

    public static class ReportsPermissions
    {
        public const string Export = "Hr.Reports.Export";
        public const string Hr = "Hr.Reports.Hr";
        public const string TodaySummary = "Hr.Reports.TodaySummary";
        public const string Review = "Hr.Reports.Review";
        public const string Team = "Hr.Reports.Team";
    }

    public static class ReviewReportsPermissions
    {
        public const string Manage = "Hr.ReviewReports.Manage";
        public const string Read = "Hr.ReviewReports.Read";
    }

    public static class RequestPermissions
    {
        public const string Cancel = "Hr.Request.Cancel";
        public const string Create = "Hr.Request.Create";
        public const string Decide = "Hr.Request.Decide";
        public const string Reject = "Hr.Request.Reject";
        public const string Inbox = "Hr.Request.Inbox";
        public const string TypeManage = "Hr.Request.Type.Manage";
        public const string TypeView = "Hr.Request.Type.View";
        public const string View = "Hr.Request.View";
        public const string ViewList = "Hr.Request.ViewList";
        public const string ViewOwn = "Hr.Request.ViewOwn";
    }

    public static class SettingsPermissions
    {
        public const string AnnualLeave = "Hr.Settings.AnnualLeave";
        public const string HrEvaluationAlert = "Hr.Settings.HrEvaluationAlert";
        public const string Complaints = "Hr.Settings.Complaints";
        public const string Contracts = "Hr.Settings.Contracts";
        public const string Finance = "Hr.Settings.Finance";
        public const string HourlyLeave = "Hr.Settings.HourlyLeave";
        public const string Read = "Hr.Settings.Read";
        public const string Reminders = "Hr.Settings.Reminders";
    }

    public static class StorePermissions
    {
        public const string IssueDeliver = "Hr.Store.Issue.Deliver";
        public const string IssueProcess = "Hr.Store.Issue.Process";
        public const string IssueView = "Hr.Store.Issue.View";
        public const string ItemLookup = "Hr.Store.Item.Lookup";
        public const string ItemManage = "Hr.Store.Item.Manage";
        public const string ItemView = "Hr.Store.Item.View";
        public const string StockAdjust = "Hr.Store.Stock.Adjust";
        public const string StockReceive = "Hr.Store.Stock.Receive";
        public const string StockView = "Hr.Store.Stock.View";
        public const string PurchaseView = "Hr.Store.Purchase.View";
        public const string PurchaseManage = "Hr.Store.Purchase.Manage";
        public const string PurchaseApprove = "Hr.Store.Purchase.Approve";
        public const string PurchaseReverse = "Hr.Store.Purchase.Reverse";
        public const string TeamView = "Hr.Store.Team.View";
        public const string TeamManage = "Hr.Store.Team.Manage";
        public const string IssueVoucherView = "Hr.Store.IssueVoucher.View";
        public const string IssueVoucherManage = "Hr.Store.IssueVoucher.Manage";
        public const string IssueVoucherApprove = "Hr.Store.IssueVoucher.Approve";
        public const string IssueVoucherReverse = "Hr.Store.IssueVoucher.Reverse";
        public const string ReportView = "Hr.Store.Report.View";
        public const string ReportExport = "Hr.Store.Report.Export";
    }

    public static class WarningPermissions
    {
        public const string Decide = "Hr.Warning.Decide";
        public const string Notify = "Hr.Warning.Notify";
        public const string Object = "Hr.Warning.Object";
        public const string Read = "Hr.Warning.Read";
        public const string ReadAll = "Hr.Warning.ReadAll";
        public const string ReadEmployeeFile = "Hr.Warning.ReadEmployeeFile";
        public const string ReadOwn = "Hr.Warning.ReadOwn";
        public const string Recommend = "Hr.Warning.Recommend";
        public const string ResolveObjection = "Hr.Warning.ResolveObjection";
        public const string Respond = "Hr.Warning.Respond";
    }

    public static class WorkShiftPermissions
    {
        public const string Manage = "Hr.WorkShift.Manage";
        public const string View = "Hr.WorkShift.View";
    }

    /// <summary>The default role seed: permission name to the roles that hold it.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> DefaultRoles = new Dictionary<string, string[]>
    {
        ["Hr.AppreciationLetter.Acknowledge"] = new[] { "employee" },
        ["Hr.AppreciationLetter.Issue"] = new[] { "executiveDirector" },
        ["Hr.AppreciationLetter.Read"] = new[] { "hrManager", "executiveDirector", "employee", "departmentManager" },
        ["Hr.AppreciationLetter.ReadEmployeeFile"] = new[] { "departmentManager", "hrManager", "executiveDirector" },
        ["Hr.AppreciationLetter.ReadAll"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.AppreciationLetter.ReadOwn"] = new[] { "employee" },
        ["Hr.Attendance.Correction.Create"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Attendance.Correction.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "systemAdmin" },
        ["Hr.Attendance.Device.Manage"] = new[] { "systemAdmin" },
        ["Hr.Attendance.Device.View"] = new[] { "systemAdmin", "hrManager" },
        ["Hr.Attendance.DeviceMap.Manage"] = new[] { "systemAdmin", "hrManager" },
        ["Hr.Attendance.GeoFence.Manage"] = new[] { "systemAdmin" },
        ["Hr.Attendance.Gps.Punch"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Attendance.Gps.ReviewFlagged"] = new[] { "hrManager" },
        ["Hr.Attendance.Import"] = new[] { "hrManager" },
        ["Hr.Attendance.Settings.Manage"] = new[] { "systemAdmin" },
        ["Hr.Attendance.Settings.View"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Attendance.ViewDayDetail"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Attendance.ViewEmployee"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Attendance.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Attendance.ViewTeam"] = new[] { "departmentManager" },
        ["Hr.AuditLog.Read"] = new[] { "systemAdmin", "executiveDirector" },
        ["Hr.Circulars.Read"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Circulars.Send"] = new[] { "departmentManager", "hrManager", "executiveDirector" },
        ["Hr.HrEvaluation.Manage"] = new[] { "hrManager" },
        ["Hr.HrEvaluation.ReadReports"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.ReviewerEvaluation.Manage"] = new[] { "reviewer" },
        ["Hr.ReviewerEvaluation.ReadReports"] = new[] { "reviewer", "executiveDirector" },
        ["Hr.Notices.Read"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Notices.Send"] = new[] { "hrManager", "executiveDirector", "reviewer" },
        ["Hr.Complaints.Attachment"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector" },
        ["Hr.Complaints.Close"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Complaints.Handle"] = new[] { "departmentManager", "hrManager", "executiveDirector" },
        ["Hr.Complaints.Forward"] = new[] { "departmentManager" },
        ["Hr.Complaints.Handlers.Approve"] = new[] { "executiveDirector" },
        ["Hr.Complaints.Handlers.Read"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Complaints.Handlers.Request"] = new[] { "hrManager" },
        ["Hr.Complaints.RaiseLevel"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector" },
        ["Hr.Complaints.Submit"] = new[] { "employee", "departmentManager" },
        ["Hr.Complaints.ViewAccessLog"] = new[] { "departmentManager", "hrManager", "executiveDirector" },
        ["Hr.Complaints.ViewOwn"] = new[] { "employee", "departmentManager" },
        ["Hr.Contract.Create"] = new[] { "hrManager" },
        ["Hr.Contract.Decide"] = new[] { "hrManager" },
        ["Hr.Contract.Read"] = new[] { "hrManager" },
        ["Hr.Contract.ReadOverview"] = new[] { "executiveDirector" },
        ["Hr.Contract.ReadOwn"] = new[] { "employee" },
        ["Hr.Contract.Recommend"] = new[] { "hrManager" },
        ["Hr.Custody.Acknowledge"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Custody.Count"] = new[] { "departmentManager" },
        ["Hr.Custody.Create"] = new[] { "storeOfficer" },
        ["Hr.Custody.Return"] = new[] { "storeOfficer" },
        ["Hr.Custody.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Custody.ViewAll"] = new[] { "storeOfficer", "hrManager" },
        ["Hr.Custody.ViewCount"] = new[] { "departmentManager", "storeOfficer", "hrManager" },
        ["Hr.Custody.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Custody.ViewTeam"] = new[] { "departmentManager" },
        ["Hr.Delegation.Manage"] = new[] { "departmentManager", "hrManager", "executiveDirector", "systemAdmin", "accountant", "reviewer" },
        ["Hr.Delegation.ReadAll"] = new[] { "hrManager" },
        ["Hr.Department.Lookup"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Department.Manage"] = new[] { "hrManager" },
        ["Hr.Department.Read"] = new[] { "hrManager" },
        ["Hr.DepartmentReports.Manage"] = new[] { "departmentManager" },
        ["Hr.DepartmentReports.Read"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Employee.AssignManager"] = new[] { "hrManager" },
        ["Hr.Employee.Create"] = new[] { "hrManager" },
        ["Hr.Employee.Document.Delete"] = new[] { "hrManager" },
        ["Hr.Employee.Document.Read"] = new[] { "employee", "departmentManager", "systemAdmin", "hrManager" },
        ["Hr.Employee.Document.Upload"] = new[] { "hrManager" },
        ["Hr.Employee.Lookup"] = new[] { "storeOfficer", "hrManager", "executiveDirector", "accountant", "reviewer", "systemAdmin" },
        ["Hr.Employee.Read"] = new[] { "hrManager" },
        ["Hr.Employee.ReadManagerHistory"] = new[] { "hrManager" },
        ["Hr.Employee.ReadOwnTaskLog"] = new[] { "departmentManager" },
        ["Hr.Employee.ReadSelf"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Employee.ReadTaskLog"] = new[] { "hrManager" },
        ["Hr.Employee.ReadTeam"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Employee.Update"] = new[] { "hrManager" },
        ["Hr.Employee.CorrectHireDate"] = new[] { "hrManager" },
        ["Hr.EmployeeShift.Assign"] = new[] { "hrManager" },
        ["Hr.EmployeeShift.View"] = new[] { "hrManager" },
        ["Hr.Evaluations.AnnualSummary.Read"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Evaluations.Cycle.Read"] = new[] { "departmentManager", "hrManager", "reviewer" },
        ["Hr.Evaluations.DecideObjection"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Evaluations.HrRead"] = new[] { "hrManager" },
        ["Hr.Evaluations.InternalNote.Read"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Evaluations.InternalNote.Write"] = new[] { "departmentManager" },
        ["Hr.Evaluations.Manage"] = new[] { "departmentManager" },
        ["Hr.Evaluations.ViewOwn"] = new[] { "employee" },
        ["Hr.Finance.AccountantRequest.Read"] = new[] { "accountant" },
        ["Hr.Finance.Adjustment.Create"] = new[] { "accountant" },
        ["Hr.Finance.Adjustment.Read"] = new[] { "accountant" },
        ["Hr.Finance.Adjustment.Review"] = new[] { "accountant" },
        ["Hr.Finance.Expense.Create"] = new[] { "accountant" },
        ["Hr.Finance.Expense.Read"] = new[] { "accountant" },
        ["Hr.Finance.PayrollRun.Approve"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Finance.PayrollRun.Close"] = new[] { "accountant" },
        ["Hr.Finance.PayrollRun.Deliver"] = new[] { "accountant" },
        ["Hr.Finance.PettyCash.Manage"] = new[] { "accountant" },
        ["Hr.Finance.PettyCash.Read"] = new[] { "accountant" },
        ["Hr.Finance.PayrollRun.Export"] = new[] { "accountant", "hrManager", "executiveDirector" },
        ["Hr.Finance.PayrollRun.Prepare"] = new[] { "accountant" },
        ["Hr.Finance.PayrollRun.Read"] = new[] { "accountant", "hrManager", "executiveDirector" },
        ["Hr.Finance.Payslip.ReadOwn"] = new[] { "employee" },
        ["Hr.Finance.Report.Read"] = new[] { "accountant" },
        ["Hr.Finance.SalaryProfile.Read"] = new[] { "accountant" },
        ["Hr.Finance.SalaryProfile.ReadOverview"] = new[] { "executiveDirector" },
        ["Hr.Finance.SalaryProfile.Update"] = new[] { "accountant" },
        ["Hr.Holiday.Manage"] = new[] { "hrManager" },
        ["Hr.Holiday.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Identity.Permission.Manage"] = new[] { "systemAdmin" },
        ["Hr.Identity.Role.Manage"] = new[] { "systemAdmin" },
        ["Hr.Identity.Role.Read"] = new[] { "hrManager", "systemAdmin" },
        ["Hr.Identity.Role.ReadDetail"] = new[] { "systemAdmin" },
        ["Hr.Identity.UserAccount.Create"] = new[] { "hrManager" },
        ["Hr.Identity.UserAccount.Lookup"] = new[] { "executiveDirector", "hrManager", "systemAdmin" },
        ["Hr.Identity.UserAccount.Read"] = new[] { "hrManager", "systemAdmin" },
        ["Hr.Identity.UserAccount.ResetMfa"] = new[] { "systemAdmin", "executiveDirector" },
        ["Hr.Identity.UserAccount.ResetPassword"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Identity.UserAccount.SetRoles"] = new[] { "hrManager", "systemAdmin", "executiveDirector" },
        ["Hr.Incident.Create"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Incident.Process"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Incident.Forward"] = new[] { "departmentManager" },
        ["Hr.Incident.UpdateStatus"] = new[] { "hrManager" },
        ["Hr.Incident.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Incident.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Leave.Balance.Adjust"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Leave.Request.Reverse"] = new[] { "hrManager" },
        ["Hr.Leave.Balance.View"] = new[] { "departmentManager", "hrManager", "systemAdmin", "executiveDirector" },
        ["Hr.Leave.Balance.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Leave.Calendar.ViewTeam"] = new[] { "departmentManager" },
        ["Hr.Leave.Ledger.View"] = new[] { "departmentManager", "hrManager", "systemAdmin", "executiveDirector" },
        ["Hr.Leave.Ledger.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Leave.OtherType.Manage"] = new[] { "systemAdmin" },
        ["Hr.Leave.Request.Create"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Leave.Request.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "systemAdmin" },
        ["Hr.Leave.Request.ViewEmployee"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Leave.Request.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "systemAdmin" },
        ["Hr.Memo.Close"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Memo.Create"] = new[] { "employee", "departmentManager", "hrManager", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Memo.Process"] = new[] { "departmentManager", "hrManager", "executiveDirector", "systemAdmin" },
        ["Hr.Memo.Forward"] = new[] { "departmentManager", "systemAdmin", "executiveDirector" },
        ["Hr.Memo.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Memo.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Notification.Device"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Notification.Read"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Reports.Export"] = new[] { "departmentManager", "reviewer", "hrManager" },
        ["Hr.Reports.Hr"] = new[] { "hrManager" },
        ["Hr.Reports.TodaySummary"] = new[] { "executiveDirector" },
        ["Hr.Reports.Review"] = new[] { "reviewer" },
        ["Hr.Reports.Team"] = new[] { "departmentManager" },
        ["Hr.ReviewReports.Manage"] = new[] { "reviewer" },
        ["Hr.ReviewReports.Read"] = new[] { "reviewer", "executiveDirector" },
        ["Hr.Request.Cancel"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Request.Create"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Request.Decide"] = new[] { "departmentManager", "hrManager", "executiveDirector", "systemAdmin", "accountant" },
        ["Hr.Request.Reject"] = new[] { "departmentManager", "hrManager", "executiveDirector", "systemAdmin", "accountant", "storeOfficer" },
        ["Hr.Request.Inbox"] = new[] { "departmentManager", "hrManager", "executiveDirector", "systemAdmin", "accountant", "storeOfficer" },
        ["Hr.Request.Type.Manage"] = new[] { "systemAdmin" },
        ["Hr.Request.Type.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Request.View"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Request.ViewList"] = new[] { "hrManager", "departmentManager" },
        ["Hr.Request.ViewOwn"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Settings.Complaints"] = new[] { "systemAdmin" },
        ["Hr.Settings.Contracts"] = new[] { "systemAdmin" },
        ["Hr.Settings.Finance"] = new[] { "systemAdmin" },
        ["Hr.Settings.HourlyLeave"] = new[] { "systemAdmin" },
        ["Hr.Settings.AnnualLeave"] = new[] { "hrManager" },
        ["Hr.Settings.HrEvaluationAlert"] = new[] { "hrManager" },
        ["Hr.Settings.Read"] = new[] { "systemAdmin", "hrManager" },
        ["Hr.Settings.Reminders"] = new[] { "systemAdmin" },
        ["Hr.Store.Issue.Deliver"] = new[] { "storeOfficer" },
        ["Hr.Store.Issue.Process"] = new[] { "storeOfficer" },
        ["Hr.Store.Issue.View"] = new[] { "storeOfficer" },
        ["Hr.Store.Item.Lookup"] = new[] { "employee", "departmentManager", "hrManager", "executiveDirector", "reviewer", "accountant", "storeOfficer", "systemAdmin" },
        ["Hr.Store.Item.Manage"] = new[] { "storeOfficer" },
        ["Hr.Store.Item.View"] = new[] { "storeOfficer" },
        ["Hr.Store.Stock.Adjust"] = new[] { "storeOfficer" },
        ["Hr.Store.Stock.Receive"] = new[] { "storeOfficer" },
        ["Hr.Store.Stock.View"] = new[] { "storeOfficer" },
        ["Hr.Store.Purchase.View"] = new[] { "storeOfficer" },
        ["Hr.Store.Purchase.Manage"] = new[] { "storeOfficer" },
        ["Hr.Store.Purchase.Approve"] = new[] { "storeOfficer" },
        ["Hr.Store.Purchase.Reverse"] = new[] { "storeOfficer" },
        ["Hr.Store.Team.View"] = new[] { "storeOfficer" },
        ["Hr.Store.Team.Manage"] = new[] { "storeOfficer" },
        ["Hr.Store.IssueVoucher.View"] = new[] { "storeOfficer" },
        ["Hr.Store.IssueVoucher.Manage"] = new[] { "storeOfficer" },
        ["Hr.Store.IssueVoucher.Approve"] = new[] { "storeOfficer" },
        ["Hr.Store.IssueVoucher.Reverse"] = new[] { "storeOfficer" },
        ["Hr.Store.Report.View"] = new[] { "storeOfficer" },
        ["Hr.Store.Report.Export"] = new[] { "storeOfficer" },
        ["Hr.Warning.Decide"] = new[] { "hrManager" },
        ["Hr.Warning.Notify"] = new[] { "hrManager" },
        ["Hr.Warning.Object"] = new[] { "employee" },
        ["Hr.Warning.Read"] = new[] { "hrManager", "executiveDirector" },
        ["Hr.Warning.ReadAll"] = new[] { "hrManager" },
        ["Hr.Warning.ReadEmployeeFile"] = new[] { "departmentManager", "hrManager" },
        ["Hr.Warning.ReadOwn"] = new[] { "employee" },
        ["Hr.Warning.Recommend"] = new[] { "departmentManager", "reviewer" },
        ["Hr.Warning.ResolveObjection"] = new[] { "executiveDirector" },
        ["Hr.Warning.Respond"] = new[] { "employee" },
        ["Hr.WorkShift.Manage"] = new[] { "hrManager", "systemAdmin" },
        ["Hr.WorkShift.View"] = new[] { "hrManager", "systemAdmin" },
    };
}
