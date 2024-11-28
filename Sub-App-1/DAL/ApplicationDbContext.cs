namespace Sub_App_1.DAL;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sub_App_1.Models;

/// <summary>
/// Represents the application's database context, which includes Identity and custom entities.
/// </summary>
public class ApplicationDbContext : IdentityDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to configure the database context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the collection of products in the database.
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Configures the relationships and constraints for the entity models in the database.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the entity models.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call the base method to ensure Identity relationships are configured.
        base.OnModelCreating(modelBuilder);

        // Configure the relationship between Product and IdentityUser.
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Producer)
            .WithMany()
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
