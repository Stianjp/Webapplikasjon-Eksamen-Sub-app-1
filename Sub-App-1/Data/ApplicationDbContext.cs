namespace Sub_App_1.Data {
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Sub_App_1.Models;
    using Microsoft.EntityFrameworkCore;  // Dette legger til ToListAsync

    public class ApplicationDbContext : IdentityDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Product> Products { get; set; }
    }
}

