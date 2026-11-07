using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HR_Mangment2.Employees;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Identity;

public class MyAccountDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Email { get; set; }
    public AppLanguage PreferredLanguage { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public EmployeeRefDto? Employee { get; set; }
    public int DirectReportCount { get; set; }
    public AccessGate AccessGate { get; set; }
    public bool MustChangePassword { get; set; }
    public bool MfaRequired { get; set; }
    public bool MfaEnabled { get; set; }
}

public class MyAccountUpdateDto
{
    [Required]
    public AppLanguage? PreferredLanguage { get; set; }
}

public class ChangePasswordInputDto
{
    [Required, StringLength(128, MinimumLength = 1), DisableAuditing]
    public string CurrentPassword { get; set; } = null!;

    [Required, StringLength(128, MinimumLength = 1), DisableAuditing]
    public string NewPassword { get; set; } = null!;
}

public class ChangePasswordResultDto
{
    public AccessGate AccessGate { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class MfaStatusDto
{
    public bool IsRequired { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public int RecoveryCodesRemaining { get; set; }
}

public class MfaEnrollmentDto
{
    public string SharedKey { get; set; } = null!;
    public string AuthenticatorUri { get; set; } = null!;
}

public class MfaCodeInputDto
{
    [Required, RegularExpression("^[0-9]{6}$"), DisableAuditing]
    public string Code { get; set; } = null!;
}

public class MfaRecoveryCodesDto
{
    public List<string> RecoveryCodes { get; set; } = new();
    public AccessGate AccessGate { get; set; }
}

public class ReasonInputDto
{
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Reason { get; set; } = null!;
}

public class RoleCreateDto
{
    [Required, RegularExpression("^[a-z][a-zA-Z0-9]{1,63}$")]
    public string Name { get; set; } = null!;

    [Required, StringLength(128, MinimumLength = 1)]
    public string NameAr { get; set; } = null!;

    [Required, StringLength(128, MinimumLength = 1)]
    public string NameEn { get; set; } = null!;
}

public class RoleUpdateDto
{
    [Required, StringLength(128, MinimumLength = 1)]
    public string NameAr { get; set; } = null!;

    [Required, StringLength(128, MinimumLength = 1)]
    public string NameEn { get; set; } = null!;

    [Required]
    public string ConcurrencyStamp { get; set; } = null!;
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsStatic { get; set; }
    public int UserCount { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class RolePagedResultDto
{
    public long TotalCount { get; set; }
    public List<RoleDto> Items { get; set; } = new();
}

public class PermissionGrantChangeDto
{
    [Required]
    public string Name { get; set; } = null!;

    public bool IsGranted { get; set; }
}

public class PermissionGrantUpdateDto
{
    [Required, MinLength(1), MaxLength(500)]
    public List<PermissionGrantChangeDto> Permissions { get; set; } = new();
}

public class PermissionGrantInfoDto
{
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ParentName { get; set; }
    public bool IsGranted { get; set; }
    public bool IsDirectlyGranted { get; set; }
    public List<string> GrantedByRoleNames { get; set; } = new();
}

public class PermissionGroupDto
{
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public List<PermissionGrantInfoDto> Permissions { get; set; } = new();
}

public class PermissionListResultDto
{
    public List<PermissionGroupDto> Groups { get; set; } = new();
}
