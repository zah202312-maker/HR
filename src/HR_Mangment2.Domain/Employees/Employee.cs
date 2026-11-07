using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Employees;

/// <summary>The employee file. The account, manager link and shift each change through their own operation.</summary>
[Audited]
public class Employee : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxFullNameLength = 200;
    public const int MaxPhoneNumberLength = 32;
    public const int MaxAddressLength = 500;
    public const int MaxJobTitleLength = 128;
    public const int MaxEmployeeNumberLength = 32;
    public const int MaxEmailLength = 256;

    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string FullNameAr { get; private set; } = null!;
    public string? FullNameEn { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public Guid DepartmentId { get; private set; }
    public string JobTitle { get; private set; } = null!;
    public DateOnly HireDate { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public Guid? ManagerEmployeeId { get; private set; }
    public AppGender? Gender { get; private set; }
    public string? EmployeeNumber { get; private set; }

    protected Employee()
    {
    }

    public Employee(Guid id, Guid? tenantId, Guid? userId)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Status = EmployeeStatus.Active;
    }

    public void SetProfile(
        string fullNameAr,
        string? fullNameEn,
        string? email,
        string? phoneNumber,
        DateOnly? dateOfBirth,
        string? address,
        Guid departmentId,
        string jobTitle,
        DateOnly hireDate,
        AppGender? gender,
        string? employeeNumber)
    {
        FullNameAr = fullNameAr;
        FullNameEn = fullNameEn;
        Email = email;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Address = address;
        DepartmentId = departmentId;
        JobTitle = jobTitle;
        HireDate = hireDate;
        Gender = gender;
        EmployeeNumber = employeeNumber;
    }

    public void LinkAccount(Guid userId) => UserId = userId;

    public void SetStatus(EmployeeStatus status) => Status = status;

    /// <summary>Only through the correction route, which records the reason.</summary>
    public void CorrectHireDate(DateOnly hireDate) => HireDate = hireDate;

    public void SetManager(Guid? managerEmployeeId) => ManagerEmployeeId = managerEmployeeId;
}

/// <summary>QUESTIONS.md item 6: every correction of a hire date keeps its reason. Rows are never changed.</summary>
public class EmployeeHireDateCorrection : Volo.Abp.Domain.Entities.Entity<Guid>, Volo.Abp.MultiTenancy.IMultiTenant
{
    public const int MaxReasonLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateOnly OldHireDate { get; private set; }
    public DateOnly NewHireDate { get; private set; }
    public string Reason { get; private set; } = null!;
    public Guid CorrectedByUserId { get; private set; }
    public DateTime CorrectedAt { get; private set; }

    protected EmployeeHireDateCorrection()
    {
    }

    public EmployeeHireDateCorrection(Guid id, Guid? tenantId, Guid employeeId, DateOnly oldHireDate, DateOnly newHireDate, string reason, Guid correctedByUserId, DateTime correctedAt)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        OldHireDate = oldHireDate;
        NewHireDate = newHireDate;
        Reason = reason;
        CorrectedByUserId = correctedByUserId;
        CorrectedAt = correctedAt;
    }
}
