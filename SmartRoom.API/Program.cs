using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

// Validates the token the generator writes: signature, issuer, audience and expiry,
// using the same validated JwtOptions, so the key is read from one place.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            // An expired token is rejected right away (the default allows 5 more minutes).
            ClockSkew = TimeSpan.Zero,
            // Same claim types the generator writes, so [Authorize(Roles = "...")] works.
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

// Every endpoint requires a logged-in user unless it is marked [AllowAnonymous] (register, login).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Injected clock, so tests can fake "now" (tomorrow-only, no changes in the past).
builder.Services.AddSingleton(TimeProvider.System);

// Enums travel as text ("Teacher", "Booked"), in responses and in requests.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartRoom API", Version = "v1" });

    // "Authorize" button: paste the token from POST login (without the "Bearer " prefix),
    // and Swagger sends it in the Authorization header of every request.
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token returned by the login endpoint."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
