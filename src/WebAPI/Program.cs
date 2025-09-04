using Application.Common.Configurations;
using Application.Common.Interface;
using Application.Features.Users.Queries.GetUserList;
using Infrastructure.Data;
using Infrastructure.Seeding;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Globalization;
using System.Text;
using WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure OpenAPI/Swagger service for API documentation
builder.Services.AddOpenApi();

// 2. Configure strongly typed JwtSettings options from configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

// 3. Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

// 4. Add Authorization policy service
builder.Services.AddAuthorization();

// 5. Configure EF Core DbContext with connection string and register as interface
builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 6. Register HttpContextAccessor (for accessing HTTP context inside services)
builder.Services.AddHttpContextAccessor();

// 7. Add controllers support (API endpoints)
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// Define CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // ?? your Angular app URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 8. Add scoped services for dependency injection
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<Seeder>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserService, UserService>();

// 9. Enable API explorer for Swagger
builder.Services.AddEndpointsApiExplorer();
// Mediator
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetUsersListQueryHandler).Assembly));
// 10. Configure Localization services and supported cultures
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// 11. Configure Swagger generator with JWT support in UI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "User Management",
        Version = "v1",
        Description = "API documentation for User Management"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\r\nExample: \"Bearer eyJhb...\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#region S E R I L O G

// -------------------- 🧾 Serilog Logging --------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

#endregion

// 12. Build the app
var app = builder.Build();

// 13. Run database seeding at startup for roles, etc.
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
    await seeder.SeedAsync();
}

// 14. Configure middleware pipeline

if (app.Environment.IsDevelopment())
{
    // Enable OpenAPI and Swagger UI only in Development
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
        c.RoutePrefix = string.Empty; // Swagger UI at root URL
    });
}

app.UseHttpsRedirection();

// 15. Use custom global exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

// 16. Add Swagger middleware (if not in development, optional)
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngularApp");

// 17. Add Authentication and Authorization middlewares
app.UseAuthentication();
app.UseAuthorization();

// 18. Map API controllers endpoints
app.MapControllers();
app.MapHealthChecks("/health").WithName("HealthCheck");

// 19. Enable localization middleware for culture-aware responses
app.UseRequestLocalization();

// 21. Run the application
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
