using System;
using System.ComponentModel.DataAnnotations;

public class Customer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    [StringLength(20)]
    public string? Telephone { get; set; }

    [Required]
    public string? PasswordHash { get; set; }
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}