using System;

namespace HR_Mangment2.Employees;

public class EmployeeRefDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? DepartmentName { get; set; }

    /// <summary>The profile photo of the employee's account (decision 2026-11-11).</summary>
    public Guid? PhotoId { get; set; }
}
