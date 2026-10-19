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
