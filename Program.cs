using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database Context Configuration (Entity Framework Core) ──
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

if (!EF.IsDesignTime)
{
    if (!useInMemory && !string.IsNullOrEmpty(connectionString))
    {
        try
        {
            var builderConn = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = 2
            };
            using var testConn = new Microsoft.Data.SqlClient.SqlConnection(builderConn.ConnectionString);
            testConn.Open();
            testConn.Close();
        }
        catch
        {
            // SQL Server is unreachable on localhost; seamlessly fallback to InMemory database
            useInMemory = true;
        }
    }
    else if (string.IsNullOrEmpty(connectionString))
    {
        useInMemory = true;
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("NashaMuktiDB");
    }
    else
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
    }
});

// ── 2. ASP.NET Core Identity Configuration ───────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password security policy
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── 3. Dual Authentication (Cookie for MVC + JWT Bearer for Web API) ───
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SmartScheme";
    options.DefaultChallengeScheme = "SmartScheme";
})
.AddPolicyScheme("SmartScheme", "Cookie or JWT", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (context.Request.Path.StartsWithSegments("/api") || 
            (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

        return IdentityConstants.ApplicationScheme;
    };
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "NashaMuktiPlatform_SecureJwtSecretKey_2026_AcademicDemoKey99!";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "NashaMuktiPortal";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "NashaMuktiClients";

    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.Principal!);
            if (user == null || !user.IsActive)
            {
                context.Fail("User account has been deactivated or does not exist.");
            }
        }
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = "NashaMukti.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.Principal!);
            if (user == null || !user.IsActive)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            }
        }
    };
});

// Dual Authorization: Authenticate transparently via Cookie OR JWT Bearer
builder.Services.AddAuthorization(options =>
{
    var defaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.DefaultPolicy = defaultPolicy;
});

// ── 4. Application Services & Dependency Injection ───────────
builder.Services.AddScoped<ICentreAuthorizationService, CentreAuthorizationService>();
builder.Services.AddScoped<ICentreService, CentreService>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
builder.Services.AddScoped<ICounsellingService, CounsellingService>();
builder.Services.AddScoped<IInterventionService, InterventionService>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ── 5. MVC + Razor Views ─────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── 6. CSRF Antiforgery ──────────────────────────────────────
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");

// ── 7. Distributed Cache & Session ───────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── 8. Swagger / OpenAPI ─────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Centralised Nasha Mukti Database API",
        Version = "v1",
        Description = "MoSJE Nasha Mukti Platform RESTful Web API for facility management, intake tracking, clinical interventions, and recovery analytics."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token obtained from /api/v1/auth/login"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ── 9. HttpContext accessor ──────────────────────────────────
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ── 10. Database Initialization & Seeding ────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.SeedAsync(app.Services, logger);
}

// ── 11. Middleware Pipeline ──────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger UI across environments for academic viva demonstration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nasha Mukti API v1");
    c.RoutePrefix = "swagger";
});

// ── HTTP Security Headers Middleware ─────────────────────────
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "SAMEORIGIN";
    headers["X-XSS-Protection"] = "1; mode=block";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; font-src 'self' https://cdn.jsdelivr.net data:; img-src 'self' data:; connect-src 'self'; frame-ancestors 'self';";

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication MUST precede Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Status Code Pages
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

// ── 12. Endpoint Routes ──────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
