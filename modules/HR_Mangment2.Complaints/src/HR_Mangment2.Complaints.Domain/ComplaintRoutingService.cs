using System.Collections.Generic;
using System.Linq;
using HR_Mangment2.Identity;

namespace HR_Mangment2.Complaints;

/// <param name="SkipsDepartmentManager">The complaint goes to the deciding handler without passing a department manager.</param>
public record ComplaintRespondentProfile(ComplaintRespondentCategory? Category, ComplaintLevel MinimumLevel, bool SkipsDepartmentManager);

/// <summary>
/// The routing table of document section 5.8. These rules live in code on purpose: no setting and no client input
/// can change them. A person holding several roles gets the strictest rule that applies.
/// </summary>
public static class ComplaintRoutingService
{
    public static readonly ComplaintRespondentProfile GeneralComplaint = new(null, ComplaintLevel.Normal, false);

    /// <returns>Null when no complaint can name this person: there is no one above them in the system.</returns>
    public static ComplaintRespondentProfile? Classify(IReadOnlyCollection<string> respondentRoles)
    {
        if (respondentRoles.Contains(HrRoles.ExecutiveDirector) || respondentRoles.Contains(HrRoles.SystemAdmin))
        {
            return null;
        }

        if (respondentRoles.Contains(HrRoles.HrManager))
        {
            return new ComplaintRespondentProfile(ComplaintRespondentCategory.HrManager, ComplaintLevel.HighlyConfidential, true);
        }

        if (respondentRoles.Contains(HrRoles.Reviewer))
        {
            return new ComplaintRespondentProfile(ComplaintRespondentCategory.Reviewer, ComplaintLevel.HighlyConfidential, true);
        }

        if (respondentRoles.Contains(HrRoles.DepartmentManager))
        {
            return new ComplaintRespondentProfile(ComplaintRespondentCategory.DepartmentManager, ComplaintLevel.Confidential, true);
        }

        // QUESTIONS.md item 33: the accountant follows the store officer's route.
        if (respondentRoles.Contains(HrRoles.StoreOfficer) || respondentRoles.Contains(HrRoles.Accountant))
        {
            var category = respondentRoles.Contains(HrRoles.StoreOfficer) ? ComplaintRespondentCategory.StoreOfficer : ComplaintRespondentCategory.Accountant;
            return new ComplaintRespondentProfile(category, ComplaintLevel.Normal, true);
        }

        return new ComplaintRespondentProfile(ComplaintRespondentCategory.Colleague, ComplaintLevel.Normal, false);
    }

    /// <summary>
    /// QUESTIONS.md item 34: the HR manager's own complaint goes to the executive, who only ever sees the highest level,
    /// so it starts at that level.
    /// </summary>
    public static ComplaintLevel MinimumLevelFor(ComplaintRespondentProfile profile, IReadOnlyCollection<string> complainantRoles) =>
        complainantRoles.Contains(HrRoles.HrManager) ? ComplaintLevel.HighlyConfidential : profile.MinimumLevel;

    /// <param name="throughManager">False when the route cannot pass a department manager.</param>
    public static IReadOnlyList<ComplaintHandlerRole> RouteFor(ComplaintLevel level, bool throughManager) => level switch
    {
        ComplaintLevel.HighlyConfidential => new[] { ComplaintHandlerRole.ExecutiveDirector },
        ComplaintLevel.Confidential => new[] { ComplaintHandlerRole.HrManager },
        _ => throughManager
            ? new[] { ComplaintHandlerRole.DepartmentManager, ComplaintHandlerRole.HrManager }
            : new[] { ComplaintHandlerRole.HrManager }
    };

    public static ComplaintStatus FirstStatus(IReadOnlyList<ComplaintHandlerRole> route) => route[0] switch
    {
        ComplaintHandlerRole.DepartmentManager => ComplaintStatus.PendingDepartmentManager,
        ComplaintHandlerRole.HrManager => ComplaintStatus.PendingHrManager,
        _ => ComplaintStatus.PendingExecutiveDirector
    };

    public static IReadOnlyList<ComplaintLevel> SelectableLevels(ComplaintLevel minimum)
    {
        var levels = new List<ComplaintLevel>();
        for (var level = minimum; level <= ComplaintLevel.HighlyConfidential; level++)
        {
            levels.Add(level);
        }

        return levels;
    }
}
