using System;
using System.Collections.Generic;
using System.Linq;
using HR_Mangment2.Errors;
using Volo.Abp;

namespace HR_Mangment2.Identity;

/// <summary>
/// The list from Role_UpdatePermissions: these are hidden when reading and rejected when writing,
/// because no settings screen may reach them.
/// </summary>
public static class ProtectedPermissions
{
    private static readonly string[] ExactNames = { "Hr.Evaluations.HrRead", "Hr.Reports.Review", "Hr.Warning.Read" };

    private static readonly string[] Prefixes = { "Hr.Complaint", "Hr.Finance", "Hr.Evaluations.InternalNote.", "Hr.Identity." };

    public static bool IsProtected(string name) =>
        ExactNames.Contains(name) || Prefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal));

    /// <summary>Throws the contract error for a permission that cannot be granted to the given roles.</summary>
    public static void EnsureGrantable(string name, IReadOnlyCollection<string> targetRoleNames)
    {
        if (name.StartsWith("Hr.Complaint", StringComparison.Ordinal))
        {
            throw new BusinessException(HrErrorCodes.Identity.ProtectedPermissionNotGrantable);
        }

        if (name.StartsWith("Hr.Finance", StringComparison.Ordinal) &&
            (targetRoleNames.Contains(HrRoles.DepartmentManager) || targetRoleNames.Contains(HrRoles.StoreOfficer)))
        {
            throw new BusinessException(HrErrorCodes.Identity.FinancialPermissionNotGrantable);
        }

        if (IsProtected(name))
        {
            throw new BusinessException(HrErrorCodes.Identity.ProtectedPermission);
        }
    }
}
