using System.Text;
using EduAssist.Authentication.Configuration;
using EduAssist.Authentication.Services;
using EduAssist.AngularWebApi.Middleware;
using EduAssist.AngularWebApi.Services;
using EduAssist.EFCore.Context;
using EduAssist.EFCore.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 1. DATABASE CONFIGURATION
// =============================================

// Core Database (Business tables: UserRequests, AIResponses, Categories, Bookmarks)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CoreDatabase")));

// Auth Database (Identity tables: Users, Roles, Claims)
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDatabase")));

// =============================================
// 2. IDENTITY CONFIGURATION
// =============================================

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// =============================================
// 3. JWT AUTHENTICATION CONFIGURATION
// =============================================

// Bind JwtSettings from appsettings.json
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // No clock skew tolerance — token expires exactly on time
    };
});

builder.Services.AddAuthorization();

// =============================================
// 4. SERVICE REGISTRATION (Dependency Injection)
// =============================================

// Authentication services (from EduAssist.Authentication project)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Business services (local to this project)
builder.Services.AddHttpClient("OpenAI"); // HttpClientFactory for OpenAI calls
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// =============================================
// 5. CORS CONFIGURATION
// =============================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// =============================================
// 6. CONTROLLER & SWAGGER CONFIGURATION
// =============================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EduAssist-AI API",
        Version = "v1",
        Description = "AI-Powered Education Assistant API - Ask questions, get AI explanations, track progress."
    });

    // Add JWT auth to Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =============================================
// BUILD & CONFIGURE MIDDLEWARE PIPELINE
// =============================================

var app = builder.Build();

// Development: Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EduAssist-AI API v1");
    });
}

app.UseHttpsRedirection();

// CORS must come before auth
app.UseCors("AllowAngular");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom Middleware
app.UseMiddleware<RateLimitingMiddleware>();

// Map Controllers
app.MapControllers();

// =============================================
// SEED DEFAULT ADMIN USER ON FIRST RUN
// =============================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create User role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Create default admin user if it doesn't exist
        var adminEmail = "admin@eduassist.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                DisplayName = "System Admin",
                IsActive = true,
                JoinedOn = DateTime.UtcNow,
                LastActiveOn = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
