namespace Sub_App_1.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Represents a product with details such as name, description, nutritional information, and producer.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    [Required]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the categories for the product as a comma-separated string.
    /// </summary>
    [Required]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the categories for the product as a list.
    /// </summary>
    /// <remarks>
    /// This property is not mapped to the database and is used for easier manipulation of categories in the application.
    /// </remarks>
    [NotMapped]
    public List<string> CategoryList
    {
        get => string.IsNullOrEmpty(Category) ? new List<string>() : Category.Split(',').ToList();
        set => Category = value != null ? string.Join(",", value) : null;
    }

    /// <summary>
    /// Gets or sets the caloric value of the product in kilocalories per 100 grams.
    /// </summary>
    [Required]
    public double Calories { get; set; }

    /// <summary>
    /// Gets or sets the protein content of the product in grams per 100 grams.
    /// </summary>
    [Required]
    public double Protein { get; set; }

    /// <summary>
    /// Gets or sets the carbohydrate content of the product in grams per 100 grams.
    /// </summary>
    [Required]
    public double Carbohydrates { get; set; }

    /// <summary>
    /// Gets or sets the fat content of the product in grams per 100 grams.
    /// </summary>
    [Required]
    public double Fat { get; set; }

    /// <summary>
    /// Gets or sets any allergens associated with the product.
    /// </summary>
    public string? Allergens { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the producer associated with the product.
    /// </summary>
    public string? ProducerId { get; set; }

    /// <summary>
    /// Gets or sets the producer of the product as a navigation property.
    /// </summary>
    /// <remarks>
    /// This is a foreign key relationship linking to the <see cref="IdentityUser"/> entity.
    /// </remarks>
    [ForeignKey(nameof(ProducerId))]
    public IdentityUser? Producer { get; set; }
}
