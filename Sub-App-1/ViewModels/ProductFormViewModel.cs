namespace Sub_App_1.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Sub_App_1.Models;


/// <summary>
/// Represents the view model used for creating and editing products.
/// </summary>
public class ProductFormViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    [Required(ErrorMessage = "Description is required")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the list of categories for the product.
    /// </summary>
    [Required(ErrorMessage = "At least one category must be selected")]
    public List<string> CategoryList { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the categories as a comma-separated string.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the caloric value of the product in kilocalories per 100 grams.
    /// </summary>
    [Required(ErrorMessage = "Calories value is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid calories value")]
    public double Calories { get; set; }

    /// <summary>
    /// Gets or sets the protein content of the product in grams per 100 grams.
    /// </summary>
    [Required(ErrorMessage = "Protein value is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid protein value")]
    public double Protein { get; set; }

    /// <summary>
    /// Gets or sets the fat content of the product in grams per 100 grams.
    /// </summary>
    [Required(ErrorMessage = "Fat value is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid fat value")]
    public double Fat { get; set; }

    /// <summary>
    /// Gets or sets the carbohydrate content of the product in grams per 100 grams.
    /// </summary>
    [Required(ErrorMessage = "Carbohydrates value is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid carbohydrates value")]
    public double Carbohydrates { get; set; }

    /// <summary>
    /// Gets or sets the allergens associated with the product as a comma-separated string.
    /// </summary>
    public string? Allergens { get; set; }

    /// <summary>
    /// Gets or sets the selected allergens for the product as a list.
    /// </summary>
    public List<string> SelectedAllergens { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the producer's unique identifier associated with the product.
    /// </summary>
    [Required]
    public string? ProducerId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the product is being edited.
    /// </summary>
    public bool IsEdit => Id != 0;

    /// <summary>
    /// Gets the title to display on the page based on whether the product is being created or edited.
    /// </summary>
    public string PageTitle => IsEdit ? "Edit Product" : "Create Product";

    /// <summary>
    /// Gets the text to display on the submit button based on whether the product is being created or edited.
    /// </summary>
    public string SubmitButtonText => IsEdit ? "Save changes" : "Save";

    /// <summary>
    /// Creates a <see cref="ProductFormViewModel"/> from a <see cref="Product"/> object.
    /// </summary>
    /// <param name="product">The product to convert.</param>
    /// <returns>A new instance of <see cref="ProductFormViewModel"/>.</returns>
    public static ProductFormViewModel FromProduct(Product product)
    {
        return new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryList = product.CategoryList,
            Category = product.Category,
            Calories = product.Calories,
            Protein = product.Protein,
            Fat = product.Fat,
            Carbohydrates = product.Carbohydrates,
            Allergens = product.Allergens,
            SelectedAllergens = product.Allergens?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
            ProducerId = product.ProducerId
        };
    }

    /// <summary>
    /// Converts the view model to a <see cref="Product"/> object.
    /// </summary>
    /// <returns>A new instance of <see cref="Product"/>.</returns>
    public Product ToProduct()
    {
        return new Product
        {
            Id = Id,
            Name = Name,
            Description = Description,
            CategoryList = CategoryList,
            Calories = Calories,
            Protein = Protein,
            Fat = Fat,
            Carbohydrates = Carbohydrates,
            Allergens = SelectedAllergens?.Any() == true ? string.Join(",", SelectedAllergens) : null,
            ProducerId = ProducerId
        };
    }

    /// <summary>
    /// Updates an existing <see cref="Product"/> object with the values from this view model.
    /// </summary>
    /// <param name="product">The product to update.</param>
    public void UpdateProduct(Product product)
    {
        product.Name = Name;
        product.Description = Description;
        product.CategoryList = CategoryList;
        product.Calories = Calories;
        product.Protein = Protein;
        product.Fat = Fat;
        product.Carbohydrates = Carbohydrates;
        product.Allergens = SelectedAllergens?.Any() == true ? string.Join(",", SelectedAllergens) : null;

        if (!string.IsNullOrEmpty(ProducerId))
        {
            product.ProducerId = ProducerId;
        }
    }
}