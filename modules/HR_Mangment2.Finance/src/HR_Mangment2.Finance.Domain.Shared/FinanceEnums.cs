namespace HR_Mangment2.Finance;

public enum SalaryComponentType
{
    Allowance,
    FixedDeduction
}

public enum AdjustmentType
{
    Addition,
    Deduction
}

public enum AdjustmentSource
{
    Manual,
    AttendanceSuggestion,
    Reversal
}

public enum AdjustmentStatus
{
    Open,
    IncludedInRun,
    Reversed
}

public enum AdjustmentSuggestionBasis
{
    Lateness,
    Absence,
    Overtime,
    AttendanceCorrectionSettlement
}

public enum AdjustmentSuggestionQuantityUnit
{
    Minutes,
    Hours,
    Days
}

public enum AdjustmentSuggestionStatus
{
    PendingReview,
    Approved,
    Rejected
}

public enum PayrollRunStatus
{
    Draft,
    Approved,
    Closed
}

public enum PayrollLineKind
{
    BaseSalary,
    Allowance,
    FixedDeduction,
    Addition,
    Deduction
}

public enum ExpenseSource
{
    General,
    StorePurchase
}

public enum ExpenseStatus
{
    Active,
    Reversed
}

public enum AccountantRequestStatus
{
    PendingAccountant,
    Completed,
    Rejected
}

public enum FinanceExportFormat
{
    Pdf,
    Excel
}

public enum FinanceReportCode
{
    MonthlyStatement,
    Adjustments,
    StoreExpenses
}

public static class FinanceConsts
{
    public const int PeriodLength = 7;
    public const int CurrencyLength = 3;
    public const int MaxNameLength = 200;
    public const int MaxCategoryLength = 100;
    public const int MaxTextLength = 2000;
    public const string PeriodPattern = "^[0-9]{4}-(0[1-9]|1[0-2])$";
    public const string CurrencyPattern = "^[A-Z]{3}$";
}
