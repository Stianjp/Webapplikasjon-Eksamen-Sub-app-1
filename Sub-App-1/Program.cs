using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sub_App_1.DAL;
using Sub_App_1.DAL.Interfaces;
using Sub_App_1.DAL.Repositories;

/// <summary>
/// Entry point for the ASP.NET Core application.
/// Configures services, logging, middleware, and routing.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Configures Serilog for application logging.
/// </summary>
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

/// <summary>
/// Configures services for the application, including controllers, views, database context, identity, and repositories.
/// </summary>
builder.Services.AddControllersWithViews().AddViewOptions(options =>
{
    options.HtmlHelperOptions.ClientValidationEnabled = true;
});

/// <summary>
/// Adds the ApplicationDbContext using SQLite as the database provider.
/// </summary>
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

/// <summary>
/// Configures ASP.NET Core Identity with custom password options and adds default token providers.
/// </summary>
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

/// <summary>
/// Registers application-specific repositories for dependency injection.
/// </summary>
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

/// <summary>
/// Configures Kestrel server options using application configuration.
/// </summary>
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

/// <summary>
/// Builds the application and seeds the database with default data.
/// </summary>
var app = builder.Build();

await DBInit.SeedAsync(app);

/// <summary>
/// Configures middleware and routing for the HTTP request pipeline.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Configures the default route and additional custom routes for different roles and controllers.
/// </summary>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "foodproducer",
    pattern: "{controller=FoodProducer}/{action=Dashboard}/{id?}");

app.MapControllerRoute(
    name: "regularuser",
    pattern: "RegularUser/{action=Dashboard}/{id?}",
    defaults: new { controller = "RegularUser" });

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=UserManager}/{id?}",
    defaults: new { controller = "Admin" });

/// <summary>
/// Registers an application shutdown handler to flush Serilog logs on termination.
/// </summary>
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

/// <summary>
/// Starts the application.
/// </summary>
app.Run();
