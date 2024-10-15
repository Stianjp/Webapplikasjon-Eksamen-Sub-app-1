using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace YourNamespace
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Denne metoden kalles av runtime. Bruk denne metoden til å legge til tjenester i beholderen.
        public void ConfigureServices(IServiceCollection services)
        {
            // Legger til støtte for Razor Pages
            services.AddRazorPages();

            // Du kan legge til flere tjenester her, som f.eks. database-kontekster eller autentisering
            // services.AddDbContext<YourDbContext>(options =>
            //     options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            
            // services.AddAuthentication(); // Legg til autentiseringstjenester om nødvendig
        }

        // Denne metoden kalles av runtime. Bruk denne metoden til å konfigurere HTTP-forespørselsrørledningen.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Viser detaljerte feilmeldinger under utvikling
            }
            else
            {
                app.UseExceptionHandler("/Error"); // Bruker en feilhåndteringsside i produksjon
                app.UseHsts(); // Bruker HSTS for sikkerhet i produksjon
            }

            app.UseHttpsRedirection(); // Tvinger bruk av HTTPS
            app.UseStaticFiles(); // Tillater bruk av statiske filer (CSS, JS, bilder, etc.)

            app.UseRouting(); // Bruker routing for å definere URL-er

            // Autentisering og autorisasjon
            app.UseAuthentication(); // Hvis du har autentisering
            app.UseAuthorization();

            // Konfigurer ruting for Razor Pages
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages(); // Kartlegger Razor Pages
            });
        }
    }
}