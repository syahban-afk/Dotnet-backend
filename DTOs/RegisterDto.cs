using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Telephone { get; set; }

    [Required]
    [MinLength(6)]
    public string? Password { get; set; }
}