namespace Sub_App_1.Data;

using Microsoft.EntityFrameworkCore;
using Sub_App_1.Models;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}
