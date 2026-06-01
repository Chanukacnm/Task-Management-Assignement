using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using TaskManagement.Api.Authentication;
using TaskManagement.Api.Middleware;
using TaskManagement.Application;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "AngularClient";
const string AuthRateLimitPolicy = "auth";

// --- Application & Infrastructure (Clean Architecture composition root) -------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Controllers & JSON -------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums (e.g. Priority) as strings such as "Medium" rather than 1.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- HTTP Basic authentication & authorization --------------------------------
builder.Services
    .AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
        BasicAuthenticationDefaults.AuthenticationScheme, _ => { });

builder.Services.AddAuthorization();

// --- Rate limiting (throttles brute-force attempts on the login endpoint) -----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(AuthRateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// --- CORS for the Angular dev server -----------------------------------------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// --- Swagger / OpenAPI with Basic auth support --------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "RESTful API for managing tasks. Secured with HTTP Basic authentication."
    });

    var basicScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "HTTP Basic authentication. Default credentials: admin / Passw0rd!",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "basic"
        }
    };

    options.AddSecurityDefinition("basic", basicScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { basicScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// --- Apply migrations and seed data on startup --------------------------------
using (var scope = app.Services.CreateScope())
{
    var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
    await initialiser.InitialiseAsync();
    await initialiser.SeedAsync();
}

// --- HTTP request pipeline -----------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        options.DocumentTitle = "Task Management API";
    });
}
else
{
    // HTTP Basic credentials must only travel over TLS in production.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors(CorsPolicyName);

// Rate limiter runs after CORS (so 429s carry CORS headers) and before
// authentication (so failed login attempts are counted and throttled).
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
