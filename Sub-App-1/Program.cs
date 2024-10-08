using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sub_App_1.Data;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add cookie authentification (Todo: test)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
    options.LoginPath = "/Account/Index"; // Login path - where to redirect if authentication is a success
    options.AccessDeniedPath = "/Account/AccessDenied"; // TODO: Access denied path - where to redirect if authorization fails

    options.Cookie.HttpOnly = true; // prevents client-side scripts from accessing the cookie. Security against XSS attacks.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // cookie only travels accoss HTTPS.
    options.Cookie.SameSite = SameSiteMode.Strict; // prevents the cookie from being sent in cross-site requests. Security against CSRF.
    options.Cookie.Name = "SubApp1_Auth_Cookie"; // todo: Unique cookie name subject to change (or are we calling it this?)
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // how long should the user stay logged in.
    options.SlidingExpiration = true; // if the user interacts with the server in this timespan, the timer refreshes so the user stays logged in.
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 7041; // Hardcoded port taken from launchSettings.json
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5289); // HTTP port - hardcoded from launchSettings.json
    options.ListenLocalhost(7041, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS port - hardcoded from launchSettings.json
    });
});

var app = builder.Build();
    
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();