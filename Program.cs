using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database Context Configuration (Entity Framework Core) ──
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
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

// ── 3. Cookie Authentication Settings ────────────────────────
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
});

// ── 4. Application Services & Dependency Injection ───────────
builder.Services.AddScoped<ICentreAuthorizationService, CentreAuthorizationService>();
builder.Services.AddScoped<ICentreService, CentreService>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
builder.Services.AddScoped<ICounsellingService, CounsellingService>();
builder.Services.AddScoped<IInterventionService, InterventionService>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();
builder.Services.AddScoped<ICentreService, CentreService>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
builder.Services.AddScoped<ICounsellingService, CounsellingService>();
builder.Services.AddScoped<IInterventionService, InterventionService>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();

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
    c.SwaggerDoc("v1", new()
    {
        Title = "Nasha Mukti DB API",
        Version = "v1",
        Description = "REST API for the Centralised Nasha Mukti Database — Academic Demonstration"
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
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nasha Mukti DB API v1");
        c.RoutePrefix = "api-docs";
    });
}

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
    name: "api",
    pattern: "api/v1/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
