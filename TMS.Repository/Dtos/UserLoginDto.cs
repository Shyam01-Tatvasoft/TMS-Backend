using System.ComponentModel.DataAnnotations;

namespace TMS.Repository.Dtos;

public class UserLoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(50, ErrorMessage = "Email should be less then 50 character")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
    
    public bool RememberMe { get; set; }
}
