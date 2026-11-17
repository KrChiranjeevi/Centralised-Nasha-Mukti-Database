using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Razor Views ────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Cookie Authentication (Phase 3 will add Identity + EF) ──
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Account/Login";
        options.LogoutPath       = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name      = "NashaMukti.Auth";
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// ── CSRF ─────────────────────────────────────────────────────
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");

// ── Session ──────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Swagger (will expand in Phase 8) ─────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Nasha Mukti DB API",
        Version     = "v1",
        Description = "REST API for the Centralised Nasha Mukti Database — Academic Demonstration"
    });
});

// ── HttpContext accessor (for audit logging later) ───────────
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ── Error Handling ───────────────────────────────────────────
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

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// ── Status Code Pages ────────────────────────────────────────
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

// ── Routes ───────────────────────────────────────────────────
// API routes
app.MapControllerRoute(
    name: "api",
    pattern: "api/v1/{controller}/{action=Index}/{id?}");

// MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
