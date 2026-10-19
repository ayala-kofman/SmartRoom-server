using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartRoom.Data;

public static class DependencyInjection
{
    /// <summary>Registers the Data layer. The connection string comes from User Secrets (never from the repo).</summary>
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing. Set it with: " +
                "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\" --project SmartRoom.API");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
