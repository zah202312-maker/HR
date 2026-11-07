using System;

namespace HR_Mangment2.Employees;

public class EmployeeRefDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? DepartmentName { get; set; }
}
