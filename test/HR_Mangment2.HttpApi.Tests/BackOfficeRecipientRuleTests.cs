using HR_Mangment2.Identity;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// Who receives no circular or notice (decisions of 2026-11-10 and 2026-11-11): a system admin, an accountant or a
/// store officer with no other business role. CircularAppService and NoticeAppService both use this rule.
/// </summary>
public class BackOfficeRecipientRuleTests
{
    [Theory]
    [InlineData(true, HrRoles.Employee, HrRoles.Accountant)]
    [InlineData(true, HrRoles.Employee, HrRoles.StoreOfficer)]
    [InlineData(true, HrRoles.Accountant, HrRoles.StoreOfficer)]
    [InlineData(true, HrRoles.SystemAdmin)]
    [InlineData(true, HrRoles.Employee, HrRoles.SystemAdmin)]
    [InlineData(false, HrRoles.Employee, HrRoles.HrManager, HrRoles.StoreOfficer)]
    [InlineData(false, HrRoles.Employee, HrRoles.DepartmentManager, HrRoles.Accountant)]
    [InlineData(false, HrRoles.Employee)]
    [InlineData(false, HrRoles.Employee, HrRoles.HrManager)]
    public void Receives_no_circulars(bool expected, params string[] roles) =>
        AccountProvisioner.ReceivesNoCirculars(roles).ShouldBe(expected);
}
