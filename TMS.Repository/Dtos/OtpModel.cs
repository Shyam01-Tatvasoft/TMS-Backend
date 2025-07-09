using System.ComponentModel.DataAnnotations;

namespace TMS.Repository.Dtos;

public class OtpModel
{
    [Required]
    public string Email { get; set; } = null!;
    
    [Required]
    public string OTP { get; set; } = null!;
}
