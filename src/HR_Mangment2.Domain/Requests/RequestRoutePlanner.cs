using System.Collections.Generic;
using System.Linq;
using HR_Mangment2.Identity;

namespace HR_Mangment2.Requests;

/// <param name="ApproverRole">The role named by the matrix.</param>
/// <param name="IsLinkedManager">The step belongs to the owner's linked manager by name, not to any holder of the role.</param>
public record PlannedRequestStep(RequestStatus Stage, string ApproverRole, bool IsLinkedManager);

/// <summary>The approval matrix of document section 4. No client input can change a route.</summary>
public static class RequestRoutePlanner
{
    /// <summary>
    /// QUESTIONS.md item 21: executive, then HR, then system admin, then department manager, then reviewer, then employee.
    /// The reviewer row is the owner's decision of 2026-11-07.
    /// </summary>
    public static RequestOwnerRow ResolveRow(IReadOnlyCollection<string> roles)
    {
        if (roles.Contains(HrRoles.ExecutiveDirector))
        {
            return RequestOwnerRow.ExecutiveDirector;
        }

        if (roles.Contains(HrRoles.HrManager))
        {
            return RequestOwnerRow.HrManager;
        }

        if (roles.Contains(HrRoles.SystemAdmin))
        {
            return RequestOwnerRow.SystemAdmin;
        }

        if (roles.Contains(HrRoles.DepartmentManager))
        {
            return RequestOwnerRow.DepartmentManager;
        }

        return roles.Contains(HrRoles.Reviewer) ? RequestOwnerRow.Reviewer : RequestOwnerRow.Employee;
    }

    /// <returns>The ordered steps, or null when this owner cannot submit to this destination at all.</returns>
    public static IReadOnlyList<PlannedRequestStep>? Plan(RequestOwnerRow row, RequestDestination destination)
    {
        var steps = new List<PlannedRequestStep>();

        switch (row)
        {
            case RequestOwnerRow.Employee:
                steps.Add(new PlannedRequestStep(RequestStatus.PendingManager, HrRoles.DepartmentManager, true));
                steps.Add(new PlannedRequestStep(RequestStatus.PendingHr, HrRoles.HrManager, false));
                break;

            case RequestOwnerRow.DepartmentManager:
                // Decision 2026-11-10: the department manager's manager is the HR manager, so the request starts at HR.
                steps.Add(new PlannedRequestStep(RequestStatus.PendingHr, HrRoles.HrManager, false));
                break;

            case RequestOwnerRow.SystemAdmin:
                steps.Add(new PlannedRequestStep(RequestStatus.PendingExecutive, HrRoles.ExecutiveDirector, false));
                break;

            case RequestOwnerRow.Reviewer:
                // The reviewer's manager is the executive or the HR manager, so the request starts at HR.
                steps.Add(new PlannedRequestStep(RequestStatus.PendingHr, HrRoles.HrManager, false));
                break;

            case RequestOwnerRow.HrManager:
                // The HR manager goes straight to the store, but through the executive for everything else.
                if (destination != RequestDestination.Store)
                {
                    steps.Add(new PlannedRequestStep(RequestStatus.PendingExecutive, HrRoles.ExecutiveDirector, false));
                }

                break;

            case RequestOwnerRow.ExecutiveDirector:
                if (destination == RequestDestination.None)
                {
                    return null;
                }

                break;
        }

        if (destination == RequestDestination.Store)
        {
            steps.Add(new PlannedRequestStep(RequestStatus.PendingDestination, HrRoles.StoreOfficer, false));
        }
        else if (destination == RequestDestination.Accountant)
        {
            steps.Add(new PlannedRequestStep(RequestStatus.PendingDestination, HrRoles.Accountant, false));
        }

        return steps;
    }

    /// <summary>What the person who submitted the request sees: a request at its destination already reads as approved.</summary>
    public static RequestEmployeeStatus ToEmployeeStatus(RequestStatus status) => status switch
    {
        RequestStatus.PendingManager => RequestEmployeeStatus.PendingManager,
        RequestStatus.PendingHr => RequestEmployeeStatus.PendingHr,
        RequestStatus.PendingExecutive => RequestEmployeeStatus.PendingExecutive,
        RequestStatus.PendingDestination or RequestStatus.Approved => RequestEmployeeStatus.Approved,
        RequestStatus.Rejected => RequestEmployeeStatus.Rejected,
        RequestStatus.Completed => RequestEmployeeStatus.Completed,
        _ => RequestEmployeeStatus.Cancelled
    };
}
