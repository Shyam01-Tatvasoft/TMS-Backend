using System.ComponentModel.DataAnnotations;

namespace TMS.Repository.Dtos;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Token is Required.")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "New Password is required.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*#?&]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, number and special character.")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Confirm New Password is required.")]
    [Compare("NewPassword", ErrorMessage = "Password doesn't match")]
    public string ConfirmNewPassword { get; set; } = null!;
}
