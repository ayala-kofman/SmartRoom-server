namespace SmartRoom.Core.Options;

/// <summary>The React client addresses allowed to call the API from a browser, from appsettings.json.</summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>The policy name used by AddCors / UseCors.</summary>
    public const string PolicyName = "ReactClient";

    public string[] AllowedOrigins { get; set; } = [];
}
