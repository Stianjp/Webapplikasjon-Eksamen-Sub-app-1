using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sub_App_1.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging() // debug
    .EnableDetailedErrors()); // debug

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false; // defautlt = true | (discuss?)
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

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

builder.WebHost.ConfigureKestrel((context, options) => {
    options.Configure(context.Configuration.GetSection("Kestrel"));
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

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);    

app.Run();