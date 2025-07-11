using System.ComponentModel.DataAnnotations;

namespace TMS.Repository.Dtos;

public class UserRegisterDto
{
    [Required(ErrorMessage = "First Name is required.")]
    [MaxLength(100, ErrorMessage = "Fist Name should be less then 100 character")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "First Name can only contain letters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required")]
    [MaxLength(100, ErrorMessage = "Last name should be less then 100 character")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Last Name can only contain letters.")]
    public string LastName { get; set; } = null!;

    [Required]
    [MaxLength(50, ErrorMessage = "Username should be less than 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(200, ErrorMessage = "Email should be less then 200 character")]
    [RegularExpression(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$", ErrorMessage = "Invalid Email")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter a valid phone number.")]
    [MaxLength(10)]
    public string? Phone { get; set; }

    [Required]
    public int CountryId { get; set; }

    [Required]
    public int Timezone { get; set; }
}
