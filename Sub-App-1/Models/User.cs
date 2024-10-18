namespace Sub_App_1.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public enum AccountType {
    Regular, FoodProducer
}

public class User : IdentityUser {
    [Required]
    public AccountType AccountType { get; set; }
}