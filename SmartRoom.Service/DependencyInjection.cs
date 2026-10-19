using Microsoft.Extensions.DependencyInjection;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Service.Security;
using SmartRoom.Service.Services;

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

        services.AddScoped<IBookingWindowService, BookingWindowService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IRoomSlotService, RoomSlotService>();
        services.AddScoped<IBookingPeriodService, BookingPeriodService>();
        services.AddScoped<IReferenceDataService, ReferenceDataService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        // Stateless, so one instance serves everyone.
        services.AddSingleton<IIdentityNumberValidator, IdentityNumberValidator>();

        // Singleton too: depends only on JwtOptions and the clock (both singletons).
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
