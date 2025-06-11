using System.ComponentModel.DataAnnotations;

namespace TMS.Repository.Dtos;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = null!;
}
