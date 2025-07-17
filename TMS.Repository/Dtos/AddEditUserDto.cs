using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TMS.Repository.Dtos;

public class AddEditUserDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    [MaxLength(100, ErrorMessage = "Fist Name should be less then 100 character")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "First Name can only contain letters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required")]
    [MaxLength(100, ErrorMessage = "Last name should be less then 100 character")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Last Name can only contain letters.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(200, ErrorMessage = "Email should be less then 200 character")]
    [RegularExpression(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$", ErrorMessage = "Invalid Email")]
    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter a valid phone number.")]
    [MaxLength(10)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Country is required.")]
    public int? FkCountryId { get; set; }

    [Required(ErrorMessage = "Timezone is required.")]
    public int? FkCountryTimezone { get; set; }

    public string? Role { get; set; }

    public bool? IsDeleted { get; set; }

    [Required]
    [MaxLength(50, ErrorMessage = "Username should be less than 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    public string Username { get; set; } = null!;

    public DateTime? ModifiedAt { get; set; }

    public IFormFile? ProfileImage { get; set; }
    public DateTime? PasswordExpiryDate { get; set; }

    public string? ProfileImagePath { get; set; }
}
