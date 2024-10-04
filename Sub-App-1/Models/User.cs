namespace Sub_App_1.Models;

using System.ComponentModel.DataAnnotations;

public enum AccountType {
    Regular, FoodProducer
}

public class User {
    public int Id { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public AccountType AccountType { get; set; }
}
