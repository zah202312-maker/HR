namespace HR_Mangment2.Auditing;

public enum AuditActionType
{
    Create,
    Update,
    Delete,
    Read,
    Export,
    Login,
    LoginFailed,
    PasswordReset,
    RoleChanged,
    PermissionChanged,
    SettingChanged,
    ComplaintHandlerAssignment
}
