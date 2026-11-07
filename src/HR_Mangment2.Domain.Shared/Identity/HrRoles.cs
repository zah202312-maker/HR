using System.Collections.Generic;

namespace HR_Mangment2.Identity;

/// <summary>The eight built-in roles (AppRole in the contract). They always exist.</summary>
public static class HrRoles
{
    public const string Employee = "employee";
    public const string DepartmentManager = "departmentManager";
    public const string HrManager = "hrManager";
    public const string ExecutiveDirector = "executiveDirector";
    public const string Reviewer = "reviewer";
    public const string Accountant = "accountant";
    public const string StoreOfficer = "storeOfficer";
    public const string SystemAdmin = "systemAdmin";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Employee, DepartmentManager, HrManager, ExecutiveDirector, Reviewer, Accountant, StoreOfficer, SystemAdmin
    };

    /// <summary>Holders of these roles must enable two-step verification.</summary>
    public static readonly IReadOnlySet<string> MfaRequired = new HashSet<string>
    {
        HrManager, ExecutiveDirector, Accountant, SystemAdmin
    };
}
