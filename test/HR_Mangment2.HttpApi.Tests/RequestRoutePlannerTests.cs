using System.Linq;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>Every cell of the approval matrix in the contract's info.description, without a database.</summary>
public class RequestRoutePlannerTests
{
    [Theory]
    [InlineData(RequestOwnerRow.Employee, RequestDestination.None, "PendingManager:departmentManager:named,PendingHr:hrManager")]
    [InlineData(RequestOwnerRow.Employee, RequestDestination.Store, "PendingManager:departmentManager:named,PendingHr:hrManager,PendingDestination:storeOfficer")]
    [InlineData(RequestOwnerRow.Employee, RequestDestination.Accountant, "PendingManager:departmentManager:named,PendingHr:hrManager,PendingDestination:accountant")]
    [InlineData(RequestOwnerRow.DepartmentManager, RequestDestination.None, "PendingHr:hrManager")]
    [InlineData(RequestOwnerRow.DepartmentManager, RequestDestination.Store, "PendingHr:hrManager,PendingDestination:storeOfficer")]
    [InlineData(RequestOwnerRow.DepartmentManager, RequestDestination.Accountant, "PendingHr:hrManager,PendingDestination:accountant")]
    [InlineData(RequestOwnerRow.HrManager, RequestDestination.None, "PendingExecutive:executiveDirector")]
    [InlineData(RequestOwnerRow.HrManager, RequestDestination.Store, "PendingDestination:storeOfficer")]
    [InlineData(RequestOwnerRow.HrManager, RequestDestination.Accountant, "PendingExecutive:executiveDirector,PendingDestination:accountant")]
    [InlineData(RequestOwnerRow.ExecutiveDirector, RequestDestination.Store, "PendingDestination:storeOfficer")]
    [InlineData(RequestOwnerRow.ExecutiveDirector, RequestDestination.Accountant, "PendingDestination:accountant")]
    [InlineData(RequestOwnerRow.SystemAdmin, RequestDestination.None, "PendingExecutive:executiveDirector")]
    [InlineData(RequestOwnerRow.SystemAdmin, RequestDestination.Store, "PendingExecutive:executiveDirector,PendingDestination:storeOfficer")]
    [InlineData(RequestOwnerRow.SystemAdmin, RequestDestination.Accountant, "PendingExecutive:executiveDirector,PendingDestination:accountant")]
    [InlineData(RequestOwnerRow.Reviewer, RequestDestination.None, "PendingHr:hrManager")]
    [InlineData(RequestOwnerRow.Reviewer, RequestDestination.Store, "PendingHr:hrManager,PendingDestination:storeOfficer")]
    [InlineData(RequestOwnerRow.Reviewer, RequestDestination.Accountant, "PendingHr:hrManager,PendingDestination:accountant")]
    public void Each_cell_of_the_matrix_builds_its_route(RequestOwnerRow row, RequestDestination destination, string expected)
    {
        var plan = RequestRoutePlanner.Plan(row, destination)!;

        string.Join(",", plan.Select(x => $"{x.Stage}:{x.ApproverRole}" + (x.IsLinkedManager ? ":named" : ""))).ShouldBe(expected);
    }

    [Fact]
    public void The_executive_has_no_route_without_a_destination()
    {
        RequestRoutePlanner.Plan(RequestOwnerRow.ExecutiveDirector, RequestDestination.None).ShouldBeNull();
    }

    [Theory]
    [InlineData(RequestOwnerRow.ExecutiveDirector, HrRoles.Employee, HrRoles.HrManager, HrRoles.ExecutiveDirector)]
    [InlineData(RequestOwnerRow.HrManager, HrRoles.SystemAdmin, HrRoles.HrManager, HrRoles.DepartmentManager)]
    [InlineData(RequestOwnerRow.SystemAdmin, HrRoles.SystemAdmin, HrRoles.DepartmentManager, HrRoles.Employee)]
    [InlineData(RequestOwnerRow.DepartmentManager, HrRoles.Employee, HrRoles.Accountant, HrRoles.DepartmentManager)]
    [InlineData(RequestOwnerRow.Reviewer, HrRoles.Employee, HrRoles.Reviewer, HrRoles.Accountant)]
    [InlineData(RequestOwnerRow.DepartmentManager, HrRoles.Employee, HrRoles.Reviewer, HrRoles.DepartmentManager)]
    [InlineData(RequestOwnerRow.Employee, HrRoles.Employee, HrRoles.StoreOfficer, HrRoles.Employee)]
    public void Several_roles_use_the_highest_row(RequestOwnerRow expected, params string[] roles)
    {
        RequestRoutePlanner.ResolveRow(roles).ShouldBe(expected);
    }

    [Theory]
    [InlineData(RequestStatus.PendingDestination, RequestEmployeeStatus.Approved)]
    [InlineData(RequestStatus.Approved, RequestEmployeeStatus.Approved)]
    [InlineData(RequestStatus.PendingManager, RequestEmployeeStatus.PendingManager)]
    [InlineData(RequestStatus.Completed, RequestEmployeeStatus.Completed)]
    [InlineData(RequestStatus.Cancelled, RequestEmployeeStatus.Cancelled)]
    public void The_owner_sees_a_request_at_its_destination_as_approved(RequestStatus status, RequestEmployeeStatus expected)
    {
        RequestRoutePlanner.ToEmployeeStatus(status).ShouldBe(expected);
    }

    [Theory]
    [InlineData(new[] { HrRoles.Employee }, new[] { HrRoles.DepartmentManager })]
    [InlineData(new[] { HrRoles.Employee, HrRoles.Reviewer }, new[] { HrRoles.ExecutiveDirector, HrRoles.HrManager })]
    [InlineData(new[] { HrRoles.Employee, HrRoles.Reviewer, HrRoles.DepartmentManager }, new[] { HrRoles.HrManager })]
    [InlineData(new[] { HrRoles.Employee, HrRoles.HrManager }, new[] { HrRoles.ExecutiveDirector })]
    [InlineData(new[] { HrRoles.Employee, HrRoles.HrManager, HrRoles.SystemAdmin }, new[] { HrRoles.ExecutiveDirector })]
    public void The_manager_link_allows_the_roles_of_its_first_matching_rule(string[] employeeRoles, string[] expected)
    {
        HR_Mangment2.Employees.ManagerLinkService.AllowedManagerRoles(employeeRoles)!.ShouldBe(expected, ignoreOrder: true);
    }

    [Fact]
    public void The_executive_director_has_no_manager()
    {
        HR_Mangment2.Employees.ManagerLinkService.AllowedManagerRoles(new[] { HrRoles.ExecutiveDirector, HrRoles.Employee }).ShouldBeNull();
    }

    /// <summary>Decisions of 2026-11-10: these three have no shift and are outside attendance and leave.</summary>
    [Theory]
    [InlineData(new[] { HrRoles.Employee, HrRoles.ExecutiveDirector }, true)]
    [InlineData(new[] { HrRoles.Employee, HrRoles.HrManager }, true)]
    [InlineData(new[] { HrRoles.Employee, HrRoles.SystemAdmin }, true)]
    [InlineData(new[] { HrRoles.Employee, HrRoles.DepartmentManager }, false)]
    [InlineData(new[] { HrRoles.Employee, HrRoles.Reviewer }, false)]
    [InlineData(new[] { HrRoles.Employee }, false)]
    public void Only_the_executive_hr_manager_and_system_admin_are_outside_the_schedule(string[] roles, bool expected)
    {
        HR_Mangment2.Employees.ManagerLinkService.IsOutsideSchedule(roles).ShouldBe(expected);
    }

    /// <summary>Decision 2026-11-10: the system admin is responsible for the system and reports to no one.</summary>
    [Fact]
    public void The_system_admin_has_no_manager()
    {
        HR_Mangment2.Employees.ManagerLinkService.AllowedManagerRoles(new[] { HrRoles.SystemAdmin, HrRoles.Employee }).ShouldBeNull();
    }
}
