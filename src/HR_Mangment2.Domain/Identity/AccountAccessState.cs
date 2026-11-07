namespace HR_Mangment2.Identity;

public record AccountAccessState(bool Exists, bool IsActive, AccessGate Gate, bool MustChangePassword, bool MfaRequired, bool MfaEnabled)
{
    public static readonly AccountAccessState Missing = new(false, false, AccessGate.None, false, false, false);
}
