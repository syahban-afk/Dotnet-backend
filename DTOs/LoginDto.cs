using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    [Required]
    public required string Telephone { get; set; }

    [Required]
    public required string Password { get; set; }
}