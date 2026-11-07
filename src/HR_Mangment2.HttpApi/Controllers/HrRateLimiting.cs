namespace HR_Mangment2.Controllers;

public static class HrRateLimiting
{
    public const string SensitivePolicy = "hr-sensitive";
    public const string DevicePolicy = "hr-device";

    /// <summary>Q38: per IP. A device agent sends far more often than a person signs in.</summary>
    public const int DevicePermitPerMinute = 120;
}
