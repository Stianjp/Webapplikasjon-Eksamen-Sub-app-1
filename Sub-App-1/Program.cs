using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sub_App_1.Data;
using Serilog;
using Sub_App_1.Models;

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

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

using (var scope = app.Services.CreateScope()) {
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { UserRoles.RegularUser, UserRoles.FoodProducer, UserRoles.Administrator };
    
    foreach (var role in roles) {
        if (!await roleManager.RoleExistsAsync(role)) {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();