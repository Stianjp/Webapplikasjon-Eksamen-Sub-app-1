using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sub_App_1.Data;
using Serilog;
using Sub_App_1.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;


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
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") + ";Cache=Shared"), ServiceLifetime.Scoped);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.WebHost.ConfigureKestrel((context, options) => {
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

builder.Services.AddControllersWithViews().AddViewOptions(options => {
    options.HtmlHelperOptions.ClientValidationEnabled = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "foodproducer",
    pattern: "{controller=FoodProducer}/{action=ProducerDashboard}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=UserManager}/{id?}",
    defaults: new { controller = "Admin" });

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

using (var scope = app.Services.CreateScope()) {
    var serviceProvider = scope.ServiceProvider;
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { UserRoles.RegularUser, UserRoles.FoodProducer, UserRoles.Administrator };
    
    foreach (var role in roles) {
        if (!await roleManager.RoleExistsAsync(role)) {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    await EnsureAdminUserExists(serviceProvider);
}

app.Run();

static async Task EnsureAdminUserExists(IServiceProvider serviceProvider) {
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var adminUsername = "Admin";

    var adminUser = await userManager.FindByNameAsync(adminUsername);
    if (adminUser == null) {
        adminUser = new IdentityUser {
            UserName = adminUsername,
        };

        var result = await userManager.CreateAsync(adminUser, "OsloMet2024");
        if (result.Succeeded) {
            await userManager.AddToRoleAsync(adminUser, UserRoles.Administrator);
        } else {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create admin user. Errors: {errors}");
        }
    } else {
        // Ensure the existing admin user has the Administrator role
        if (!await userManager.IsInRoleAsync(adminUser, UserRoles.Administrator)) {
            await userManager.AddToRoleAsync(adminUser, UserRoles.Administrator);
        }
    }
}   