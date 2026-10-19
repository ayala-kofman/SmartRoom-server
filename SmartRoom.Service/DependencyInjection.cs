using Microsoft.Extensions.DependencyInjection;

namespace SmartRoom.Service;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Service layer. The AutoMapper license key comes from User Secrets
    /// ("AutoMapper:LicenseKey"); without it AutoMapper still works but logs a license warning.
    /// </summary>
    public static IServiceCollection AddServiceLayer(this IServiceCollection services, string? autoMapperLicenseKey)
    {
        // Scans every Profile in this project (Mapping folder).
        services.AddAutoMapper(cfg => cfg.LicenseKey = autoMapperLicenseKey, typeof(DependencyInjection).Assembly);

        return services;
    }
}
