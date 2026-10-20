using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using SmartRoom.API.Middleware;
using SmartRoom.API.Models;
using SmartRoom.Core.Results;
using SmartRoom.Core.Options;
using SmartRoom.Data;
using SmartRoom.Data.Seed;
using SmartRoom.Service;

var builder = WebApplication.CreateBuilder(args);

// NLog (nlog.config) replaces the default providers; the code keeps using ILogger.
// The levels stay in appsettings "Logging:LogLevel" (Debug for our code in development).
builder.Logging.ClearProviders();
builder.Host.UseNLog(new NLogAspNetCoreOptions { RemoveLoggerFactoryFilter = false });

// Add services to the container.
builder.Services.AddDataLayer(builder.Configuration);
builder.Services.AddServiceLayer(builder.Configuration["AutoMapper:LicenseKey"]);
builder.Services.Configure<BookingRulesOptions>(builder.Configuration.GetSection(BookingRulesOptions.SectionName));
builder.Services.Configure<PagingOptions>(builder.Configuration.GetSection(PagingOptions.SectionName));

// Only the React client may call the API from a browser. The client reads the correlation id
// and Location from the response, so those headers are exposed to it.
var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
builder.Services.AddCors(options => options.AddPolicy(CorsOptions.PolicyName, policy => policy
    .WithOrigins(corsOptions.AllowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders(CorrelationIdMiddleware.HeaderName, "Location")));

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

        // 401 / 403 in the uniform ErrorResponse shape (the default is an empty body), with a Warning line.
        bearer.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Unauthenticated request to {Method} {Path} (missing, invalid or expired token).",
                    context.Request.Method, context.Request.Path);
                await ErrorResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized,
                    ErrorCodes.Unauthorized, "Please log in to continue.");
            },
            OnForbidden = async context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("User {UserId} with role {Role} was denied {Method} {Path}.",
                    context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    context.HttpContext.User.FindFirstValue(ClaimTypes.Role),
                    context.Request.Method, context.Request.Path);
                await ErrorResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden,
                    ErrorCodes.Forbidden, "You do not have permission to perform this action.");
            }
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
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    // A 400 from model validation (missing field, bad JSON, wrong enum) in the uniform shape, with the invalid fields.
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = context =>
    {
        // Field names as the client sends them (camelCase); "$.roomSlotId" from the JSON reader and "RoomSlotId"
        // from [Required] become one key, and errors about the body as a whole go under "request".
        var errors = context.ModelState
            .Where(e => e.Value is { Errors.Count: > 0 })
            .GroupBy(e => ToClientFieldName(e.Key))
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(e => e.Value!.Errors)
                    .Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? "The value is invalid." : x.ErrorMessage)
                    .Distinct()
                    .ToArray());

        var body = new ErrorResponse(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError,
            "One or more fields are invalid.", context.HttpContext.TraceIdentifier, errors);
        return new BadRequestObjectResult(body);
    });
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
// Errors first, so they catch everything; then the CorrelationId, so every later log line carries it.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

// CORS before the Shabbat check: otherwise the browser hides the 503 from the client
// (no CORS headers) and the client cannot show "closed for Shabbat".
app.UseCors(CorsOptions.PolicyName);
app.UseMiddleware<ShabbatClosureMiddleware>();

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

static string ToClientFieldName(string modelStateKey)
{
    var key = modelStateKey.TrimStart('$', '.');
    return key.Length == 0 || key == "request"
        ? "request"
        : JsonNamingPolicy.CamelCase.ConvertName(key);
}
