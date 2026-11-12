using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.ReviewReports;

/// <summary>
/// A reviewer's room (decision 2026-11-11): one department manager and the linked employees the reviewer picks.
/// The daily report takes one row per room. Deleting is soft, and reports keep their own snapshot.
/// </summary>
public class ReviewRoom : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ReviewerUserId { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid ManagerEmployeeId { get; private set; }
    public List<Guid> MemberEmployeeIds { get; private set; } = new();

    protected ReviewRoom()
    {
    }

    public ReviewRoom(Guid id, Guid? tenantId, Guid reviewerUserId, string name, Guid managerEmployeeId, List<Guid> memberEmployeeIds)
        : base(id)
    {
        TenantId = tenantId;
        ReviewerUserId = reviewerUserId;
        Set(name, managerEmployeeId, memberEmployeeIds);
    }

    /// <summary>The manager and member rules are checked by the application service.</summary>
    public void Set(string name, Guid managerEmployeeId, List<Guid> memberEmployeeIds)
    {
        Name = name.Trim();
        ManagerEmployeeId = managerEmployeeId;
        MemberEmployeeIds = memberEmployeeIds;
    }
}
