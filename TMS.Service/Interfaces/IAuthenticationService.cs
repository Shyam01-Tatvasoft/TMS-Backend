using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface IAuthenticationService
{
    public Task<string> RegisterAsync(UserRegisterDto dto);
    public Task<UserDto?> LoginAsync(UserLoginDto dto);
    public string GenerateResetToken(string email);
    public Task<string?> ValidateResetToken(string token,string validationType);
    public Task<bool> ResetPasswordAsync(string email,ResetPasswordDto dto);
    public Task<string> ForgotPassword(string email);
}
