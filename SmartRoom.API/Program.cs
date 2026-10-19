using SmartRoom.Core.Options;
using SmartRoom.Data;
using SmartRoom.Data.Seed;
using SmartRoom.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataLayer(builder.Configuration);
builder.Services.AddServiceLayer(builder.Configuration["AutoMapper:LicenseKey"]);
builder.Services.Configure<BookingRulesOptions>(builder.Configuration.GetSection(BookingRulesOptions.SectionName));
builder.Services.Configure<PagingOptions>(builder.Configuration.GetSection(PagingOptions.SectionName));

// The signing key comes from User Secrets; stop at startup with a clear message if it is missing or too short.
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => o.Key.Length >= 32,
        "Jwt:Key is missing or shorter than 32 characters. Set it with: " +
        "dotnet user-secrets set \"Jwt:Key\" \"<at least 32 characters>\" --project SmartRoom.API")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience),
        "Jwt:Issuer and Jwt:Audience must be set in appsettings.json.")
    .ValidateOnStart();

// Injected clock, so tests can fake "now" (tomorrow-only, no changes in the past).
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create / migrate the database and seed demo data on startup, with no manual DB steps.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DbInitializer>().InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
