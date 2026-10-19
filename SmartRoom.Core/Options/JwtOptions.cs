namespace SmartRoom.Core.Options;

/// <summary>
/// JWT settings. Issuer, Audience and ExpiryMinutes come from appsettings.json;
/// Key comes from User Secrets only and is never committed.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Signing key. Checked at startup: must exist and be at least 32 characters.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
