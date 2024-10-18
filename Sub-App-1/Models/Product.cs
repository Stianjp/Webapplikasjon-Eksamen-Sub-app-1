namespace Sub_App_1.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public class Product {
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    // Nutritional Information
    [Required]
    public double Calories { get; set; } // kcal per 100g

    [Required]
    public double Protein { get; set; } // grams per 100g

    [Required]
    public double Carbohydrates { get; set; } // grams per 100g

    [Required]
    public double Fat { get; set; } // grams per 100g

    // Foreign Key to the producer
    public string ProducerId { get; set; }
    public IdentityUser Producer { get; set; } // who created the product
}