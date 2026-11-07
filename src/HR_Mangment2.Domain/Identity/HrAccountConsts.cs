using System;

namespace HR_Mangment2.Identity;

public static class HrAccountConsts
{
    /// <summary>How long a password reset link works. The Host sets the token lifespan from this value.</summary>
    public static readonly TimeSpan PasswordResetLinkLifetime = TimeSpan.FromHours(2);
}
