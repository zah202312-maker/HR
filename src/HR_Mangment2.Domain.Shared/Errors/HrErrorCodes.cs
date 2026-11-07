namespace HR_Mangment2.Errors;

/// <summary>Generated from api-contract/openapi.yaml. Format: Hr:&lt;Module&gt;:&lt;PascalCaseName&gt;.</summary>
public static class HrErrorCodes
{
    public const string Prefix = "Hr:";

    public static class Appreciation
    {
        public const string EmployeeNotFound = "Hr:Appreciation:EmployeeNotFound";
        public const string FileRejected = "Hr:Appreciation:FileRejected";
        public const string SelfIssueNotAllowed = "Hr:Appreciation:SelfIssueNotAllowed";
    }

    public static class Attendance
    {
        public const string CorrectionAlreadyPending = "Hr:Attendance:CorrectionAlreadyPending";
        public const string CorrectionDateInFuture = "Hr:Attendance:CorrectionDateInFuture";
        public const string CorrectionTimeInvalid = "Hr:Attendance:CorrectionTimeInvalid";
        public const string DeviceInactive = "Hr:Attendance:DeviceInactive";
        public const string ExecutiveDirectorNotCovered = "Hr:Attendance:ExecutiveDirectorNotCovered";
        public const string GpsMethodDisabled = "Hr:Attendance:GpsMethodDisabled";
        public const string GpsNotAvailable = "Hr:Attendance:GpsNotAvailable";
        public const string ImportFileInvalid = "Hr:Attendance:ImportFileInvalid";
        public const string OutsideGeoFence = "Hr:Attendance:OutsideGeoFence";
    }

    public static class Circulars
    {
        public const string NoRecipients = "Hr:Circulars:NoRecipients";
        public const string ScopeNotAllowed = "Hr:Circulars:ScopeNotAllowed";
    }

    public static class Common
    {
        public const string IdempotencyKeyReused = "Hr:Common:IdempotencyKeyReused";
    }

    public static class Complaints
    {
        public const string AssignmentNotActive = "Hr:Complaints:AssignmentNotActive";
        public const string AssignmentNotPending = "Hr:Complaints:AssignmentNotPending";
        public const string AttachmentRejected = "Hr:Complaints:AttachmentRejected";
        public const string AttachmentTooLarge = "Hr:Complaints:AttachmentTooLarge";
        public const string ComplaintClosed = "Hr:Complaints:ComplaintClosed";
        public const string HandlerCannotBeSystemAdmin = "Hr:Complaints:HandlerCannotBeSystemAdmin";
        public const string HandlerRoleNotAllowed = "Hr:Complaints:HandlerRoleNotAllowed";
        public const string InvalidAssignmentPeriod = "Hr:Complaints:InvalidAssignmentPeriod";
        public const string InvalidTransition = "Hr:Complaints:InvalidTransition";
        public const string LevelNotHigher = "Hr:Complaints:LevelNotHigher";
        public const string NoEligibleHandler = "Hr:Complaints:NoEligibleHandler";
        public const string RespondentNotAllowed = "Hr:Complaints:RespondentNotAllowed";
        public const string RespondentRequired = "Hr:Complaints:RespondentRequired";
        public const string SelfComplaintNotAllowed = "Hr:Complaints:SelfComplaintNotAllowed";
    }

    public static class Contract
    {
        public const string EmployeeNotFound = "Hr:Contract:EmployeeNotFound";
        public const string InvalidDateRange = "Hr:Contract:InvalidDateRange";
        public const string InvalidPreviousContract = "Hr:Contract:InvalidPreviousContract";
        public const string InvalidStatus = "Hr:Contract:InvalidStatus";
        public const string OverlappingContract = "Hr:Contract:OverlappingContract";
        public const string SelfActionNotAllowed = "Hr:Contract:SelfActionNotAllowed";
    }

    public static class Custody
    {
        public const string AlreadyAcknowledged = "Hr:Custody:AlreadyAcknowledged";
        public const string AlreadyReturned = "Hr:Custody:AlreadyReturned";
        public const string CountLineInvalid = "Hr:Custody:CountLineInvalid";
        public const string CountLinesIncomplete = "Hr:Custody:CountLinesIncomplete";
        public const string DepartmentNotInScope = "Hr:Custody:DepartmentNotInScope";
        public const string InvalidHolder = "Hr:Custody:InvalidHolder";
        public const string ItemNotReturnable = "Hr:Custody:ItemNotReturnable";
        public const string ReturnNoteRequired = "Hr:Custody:ReturnNoteRequired";
    }

    public static class Delegation
    {
        public const string DelegatorNotEligible = "Hr:Delegation:DelegatorNotEligible";
        public const string AlreadyEnded = "Hr:Delegation:AlreadyEnded";
        public const string CannotDelegateToSelf = "Hr:Delegation:CannotDelegateToSelf";
        public const string DelegateNotEligible = "Hr:Delegation:DelegateNotEligible";
        public const string InvalidPeriod = "Hr:Delegation:InvalidPeriod";
        public const string OverlappingDelegation = "Hr:Delegation:OverlappingDelegation";
    }

    public static class Department
    {
        public const string HasEmployees = "Hr:Department:HasEmployees";
        public const string HasSharedCustody = "Hr:Department:HasSharedCustody";
        public const string NotFound = "Hr:Department:NotFound";
    }

    public static class DepartmentReports
    {
        public const string AlreadySubmitted = "Hr:DepartmentReports:AlreadySubmitted";
    }

    public static class Document
    {
        public const string FileTooLarge = "Hr:Document:FileTooLarge";
        public const string FileTypeNotAllowed = "Hr:Document:FileTypeNotAllowed";
        public const string NotAvailable = "Hr:Document:NotAvailable";
    }

    public static class Employee
    {
        public const string HireDateUnchanged = "Hr:Employee:HireDateUnchanged";
        public const string HasOutstandingCustody = "Hr:Employee:HasOutstandingCustody";
        public const string CannotManageSelf = "Hr:Employee:CannotManageSelf";
        public const string EmployeeAlreadyHasAccount = "Hr:Employee:EmployeeAlreadyHasAccount";
        public const string EmployeeEnded = "Hr:Employee:EmployeeEnded";
        public const string EmployeeNotFound = "Hr:Employee:EmployeeNotFound";
        public const string ExecutiveCannotHaveManager = "Hr:Employee:ExecutiveCannotHaveManager";
        public const string HasDirectReports = "Hr:Employee:HasDirectReports";
        public const string HireDateLocked = "Hr:Employee:HireDateLocked";
        public const string InvalidManagerForRole = "Hr:Employee:InvalidManagerForRole";
        public const string ManagerCycle = "Hr:Employee:ManagerCycle";
        public const string ManagerNotActive = "Hr:Employee:ManagerNotActive";
        public const string ManagerNotFound = "Hr:Employee:ManagerNotFound";
        public const string ManagerRequired = "Hr:Employee:ManagerRequired";
        public const string ManagerUnchanged = "Hr:Employee:ManagerUnchanged";
        public const string WorkShiftNotFound = "Hr:Employee:WorkShiftNotFound";
        public const string WorkShiftRequired = "Hr:Employee:WorkShiftRequired";
    }

    public static class Evaluations
    {
        public const string ObjectionWindowClosed = "Hr:Evaluations:ObjectionWindowClosed";
        public const string CycleNotOpen = "Hr:Evaluations:CycleNotOpen";
        public const string EvaluationNotSubmitted = "Hr:Evaluations:EvaluationNotSubmitted";
        public const string IncompleteScores = "Hr:Evaluations:IncompleteScores";
        public const string InvalidCriterion = "Hr:Evaluations:InvalidCriterion";
        public const string InvalidGrade = "Hr:Evaluations:InvalidGrade";
        public const string LastActiveCriterion = "Hr:Evaluations:LastActiveCriterion";
        public const string MinimumActiveGrades = "Hr:Evaluations:MinimumActiveGrades";
        public const string NotAnObjection = "Hr:Evaluations:NotAnObjection";
        public const string NotEditable = "Hr:Evaluations:NotEditable";
        public const string ObjectionAlreadyDecided = "Hr:Evaluations:ObjectionAlreadyDecided";
        public const string ObjectionAlreadyPending = "Hr:Evaluations:ObjectionAlreadyPending";
        public const string SelfDecisionNotAllowed = "Hr:Evaluations:SelfDecisionNotAllowed";
    }

    public static class Finance
    {
        public const string AdjustmentAlreadyReversed = "Hr:Finance:AdjustmentAlreadyReversed";
        public const string ApprovalNotRequested = "Hr:Finance:ApprovalNotRequested";
        public const string ApproverIsPreparer = "Hr:Finance:ApproverIsPreparer";
        public const string AttachmentRejected = "Hr:Finance:AttachmentRejected";
        public const string DuplicateComponentName = "Hr:Finance:DuplicateComponentName";
        public const string EffectiveDateInLockedPeriod = "Hr:Finance:EffectiveDateInLockedPeriod";
        public const string EffectiveDateNotAfterLatest = "Hr:Finance:EffectiveDateNotAfterLatest";
        public const string EmployeeNotFound = "Hr:Finance:EmployeeNotFound";
        public const string ExpenseAlreadyReversed = "Hr:Finance:ExpenseAlreadyReversed";
        public const string InvalidPayrollRunStatus = "Hr:Finance:InvalidPayrollRunStatus";
        public const string NotConfiguredPayrollApprover = "Hr:Finance:NotConfiguredPayrollApprover";
        public const string PayrollApproverNotConfigured = "Hr:Finance:PayrollApproverNotConfigured";
        public const string PayrollRunNotFoundForPeriod = "Hr:Finance:PayrollRunNotFoundForPeriod";
        public const string PayrollRunStale = "Hr:Finance:PayrollRunStale";
        public const string PeriodLocked = "Hr:Finance:PeriodLocked";
        public const string ReportFilterRequired = "Hr:Finance:ReportFilterRequired";
        public const string RequestNotAtAccountantStep = "Hr:Finance:RequestNotAtAccountantStep";
        public const string SalaryProfileMissing = "Hr:Finance:SalaryProfileMissing";
        public const string SelfChangeNotAllowed = "Hr:Finance:SelfChangeNotAllowed";
        public const string SuggestionAlreadyReviewed = "Hr:Finance:SuggestionAlreadyReviewed";
        public const string SuggestionAmountRequired = "Hr:Finance:SuggestionAmountRequired";
    }

    public static class Identity
    {
        public const string AccountInactive = "Hr:Identity:AccountInactive";
        public const string CannotChangeOwnPermissions = "Hr:Identity:CannotChangeOwnPermissions";
        public const string CannotChangeOwnRoles = "Hr:Identity:CannotChangeOwnRoles";
        public const string CannotResetOwnMfa = "Hr:Identity:CannotResetOwnMfa";
        public const string CannotResetOwnPassword = "Hr:Identity:CannotResetOwnPassword";
        public const string CurrentPasswordIncorrect = "Hr:Identity:CurrentPasswordIncorrect";
        public const string FinancialPermissionNotGrantable = "Hr:Identity:FinancialPermissionNotGrantable";
        public const string ForbiddenRoleCombination = "Hr:Identity:ForbiddenRoleCombination";
        public const string LastSystemAdminCannotBeRemoved = "Hr:Identity:LastSystemAdminCannotBeRemoved";
        public const string ManagerRoleHasDirectReports = "Hr:Identity:ManagerRoleHasDirectReports";
        public const string MfaAlreadyEnabled = "Hr:Identity:MfaAlreadyEnabled";
        public const string MfaCodeInvalid = "Hr:Identity:MfaCodeInvalid";
        public const string MfaEnrollmentNotStarted = "Hr:Identity:MfaEnrollmentNotStarted";
        public const string MfaEnrollmentRequired = "Hr:Identity:MfaEnrollmentRequired";
        public const string MfaNotEnabled = "Hr:Identity:MfaNotEnabled";
        public const string MfaResetNotAllowedForTarget = "Hr:Identity:MfaResetNotAllowedForTarget";
        public const string NewPasswordSameAsCurrent = "Hr:Identity:NewPasswordSameAsCurrent";
        public const string NoVerifiedEmail = "Hr:Identity:NoVerifiedEmail";
        public const string PasswordChangeRequired = "Hr:Identity:PasswordChangeRequired";
        public const string PasswordPolicyViolation = "Hr:Identity:PasswordPolicyViolation";
        public const string PasswordResetNotAllowedForTarget = "Hr:Identity:PasswordResetNotAllowedForTarget";
        public const string PermissionNotFound = "Hr:Identity:PermissionNotFound";
        public const string ProtectedPermission = "Hr:Identity:ProtectedPermission";
        public const string ProtectedPermissionNotGrantable = "Hr:Identity:ProtectedPermissionNotGrantable";
        public const string ProtectedRoleChangeRequiresApproval = "Hr:Identity:ProtectedRoleChangeRequiresApproval";
        public const string RoleInUse = "Hr:Identity:RoleInUse";
        public const string RoleNotFound = "Hr:Identity:RoleNotFound";
        public const string SingletonRoleAlreadyAssigned = "Hr:Identity:SingletonRoleAlreadyAssigned";
        public const string StaticRoleCannotBeDeleted = "Hr:Identity:StaticRoleCannotBeDeleted";
    }

    public static class Incident
    {
        public const string InvalidState = "Hr:Incident:InvalidState";
        public const string InvalidTransition = "Hr:Incident:InvalidTransition";
        public const string NoManagerLinked = "Hr:Incident:NoManagerLinked";
    }

    public static class Item
    {
        public const string KindLocked = "Hr:Item:KindLocked";
        public const string NameAlreadyExists = "Hr:Item:NameAlreadyExists";
    }

    public static class Leave
    {
        public const string AdjustmentExceedsBalance = "Hr:Leave:AdjustmentExceedsBalance";
        public const string RequestNotReversible = "Hr:Leave:RequestNotReversible";
        public const string AttachmentRejected = "Hr:Leave:AttachmentRejected";
        public const string AttachmentRequired = "Hr:Leave:AttachmentRequired";
        public const string EmployeeNotActive = "Hr:Leave:EmployeeNotActive";
        public const string EntitlementNotSet = "Hr:Leave:EntitlementNotSet";
        public const string ExecutiveDirectorNotEligible = "Hr:Leave:ExecutiveDirectorNotEligible";
        public const string HourlyOutsideShift = "Hr:Leave:HourlyOutsideShift";
        public const string HourlySettingsNotConfigured = "Hr:Leave:HourlySettingsNotConfigured";
        public const string InsufficientBalance = "Hr:Leave:InsufficientBalance";
        public const string InvalidAttachment = "Hr:Leave:InvalidAttachment";
        public const string InvalidDateRange = "Hr:Leave:InvalidDateRange";
        public const string NoShiftAssigned = "Hr:Leave:NoShiftAssigned";
        public const string NoWorkingDaysInRange = "Hr:Leave:NoWorkingDaysInRange";
        public const string NotEligibleForLeaveType = "Hr:Leave:NotEligibleForLeaveType";
        public const string OtherLeaveTypeInactive = "Hr:Leave:OtherLeaveTypeInactive";
        public const string OtherLeaveTypeMaxExceeded = "Hr:Leave:OtherLeaveTypeMaxExceeded";
        public const string OverlappingLeave = "Hr:Leave:OverlappingLeave";
        public const string RangeBeyondNextYear = "Hr:Leave:RangeBeyondNextYear";
        public const string SelfAdjustmentNotAllowed = "Hr:Leave:SelfAdjustmentNotAllowed";
    }

    public static class Memo
    {
        public const string ConvertTargetMismatch = "Hr:Memo:ConvertTargetMismatch";
        public const string InvalidState = "Hr:Memo:InvalidState";
        public const string LeaveAttachmentRequired = "Hr:Memo:LeaveAttachmentRequired";
        public const string LeaveDetailsInvalid = "Hr:Memo:LeaveDetailsInvalid";
        public const string NoManagerLinked = "Hr:Memo:NoManagerLinked";
        public const string SelfActionNotAllowed = "Hr:Memo:SelfActionNotAllowed";
    }

    public static class Reports
    {
        public const string InvalidPeriod = "Hr:Reports:InvalidPeriod";
        public const string MissingFilter = "Hr:Reports:MissingFilter";
    }

    public static class Request
    {
        public const string CannotCancelAtThisStage = "Hr:Request:CannotCancelAtThisStage";
        public const string ExecutiveCannotSubmit = "Hr:Request:ExecutiveCannotSubmit";
        public const string ItemFieldsInvalid = "Hr:Request:ItemFieldsInvalid";
        public const string ItemInactive = "Hr:Request:ItemInactive";
        public const string NoApproverAvailable = "Hr:Request:NoApproverAvailable";
        public const string NotPending = "Hr:Request:NotPending";
        public const string OnBehalfNotAllowed = "Hr:Request:OnBehalfNotAllowed";
        public const string SelfApprovalNotAllowed = "Hr:Request:SelfApprovalNotAllowed";
        public const string SystemTypeDestinationLocked = "Hr:Request:SystemTypeDestinationLocked";
        public const string TypeCodeAlreadyExists = "Hr:Request:TypeCodeAlreadyExists";
        public const string TypeInactive = "Hr:Request:TypeInactive";
        public const string UseDedicatedEndpoint = "Hr:Request:UseDedicatedEndpoint";
        public const string UseStockIssueFlow = "Hr:Request:UseStockIssueFlow";
    }

    public static class Settings
    {
        public const string InvalidEscalationDay = "Hr:Settings:InvalidEscalationDay";
    }

    public static class Shift
    {
        public const string CrossesMidnightMismatch = "Hr:Shift:CrossesMidnightMismatch";
        public const string InvalidSchedule = "Hr:Shift:InvalidSchedule";
        public const string ShiftInactive = "Hr:Shift:ShiftInactive";
        public const string StartDateBeforeCurrentAssignment = "Hr:Shift:StartDateBeforeCurrentAssignment";
        public const string StartDateBeforeHireDate = "Hr:Shift:StartDateBeforeHireDate";
    }

    public static class ReviewReports
    {
        public const string AlreadySubmitted = "Hr:ReviewReports:AlreadySubmitted";
        public const string IncompleteTeams = "Hr:ReviewReports:IncompleteTeams";
        public const string InvalidPeriod = "Hr:ReviewReports:InvalidPeriod";
        public const string NoTeams = "Hr:ReviewReports:NoTeams";
        public const string UnknownTeam = "Hr:ReviewReports:UnknownTeam";
    }

    public static class Stock
    {
        public const string InsufficientStock = "Hr:Stock:InsufficientStock";
        public const string IssueInvalidState = "Hr:Stock:IssueInvalidState";
        public const string IssueItemMismatch = "Hr:Stock:IssueItemMismatch";
        public const string IssueNotPurchasing = "Hr:Stock:IssueNotPurchasing";
        public const string ItemInactive = "Hr:Stock:ItemInactive";
        public const string MovementAlreadyReversed = "Hr:Stock:MovementAlreadyReversed";
        public const string PurchaseCostRequired = "Hr:Stock:PurchaseCostRequired";
        public const string ZeroQuantity = "Hr:Stock:ZeroQuantity";
    }

    public static class Warning
    {
        public const string AttachmentRejected = "Hr:Warning:AttachmentRejected";
        public const string EmployeeNotFound = "Hr:Warning:EmployeeNotFound";
        public const string IncidentDateInFuture = "Hr:Warning:IncidentDateInFuture";
        public const string InvalidStatus = "Hr:Warning:InvalidStatus";
        public const string LevelRequired = "Hr:Warning:LevelRequired";
        public const string ObjectionAlreadySubmitted = "Hr:Warning:ObjectionAlreadySubmitted";
        public const string ResponseAlreadySubmitted = "Hr:Warning:ResponseAlreadySubmitted";
        public const string ResponseWindowExpired = "Hr:Warning:ResponseWindowExpired";
        public const string ResponseWindowOpen = "Hr:Warning:ResponseWindowOpen";
        public const string SelfDecisionNotAllowed = "Hr:Warning:SelfDecisionNotAllowed";
        public const string SelfRecommendationNotAllowed = "Hr:Warning:SelfRecommendationNotAllowed";
        public const string TargetIsDepartmentManager = "Hr:Warning:TargetIsDepartmentManager";
        public const string TargetNotDepartmentManager = "Hr:Warning:TargetNotDepartmentManager";
        public const string TargetNotInTeam = "Hr:Warning:TargetNotInTeam";
    }
}
