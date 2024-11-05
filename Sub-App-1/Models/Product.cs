namespace Sub_App_1.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;


public class Product {
    [Key]
    public int Id { get; set; } // Primary Key

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Description { get; set; }

    public string? Category { get; set; }

    // Nutritional Information
    [Required]
    public double? Calories { get; set; } // kcal per 100g

    [Required]
    public double? Protein { get; set; } // grams per 100g

    [Required]
    public double? Carbohydrates { get; set; } // grams per 100g

    [Required]
    public double? Fat { get; set; } // grams per 100g
    
    public string? Allergens { get; set; }


    //Behov for kategorier av type mat
    // public string Category {get; set; } // Skal vi bruke en liste med kategorier som er alt satt av oss?

    // Foreign Key to the producer
    [JsonIgnore]
    public string? ProducerId { get; set; }
    public IdentityUser? Producer { get; set; }
}
