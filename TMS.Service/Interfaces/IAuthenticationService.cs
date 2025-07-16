using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface IAuthenticationService
{
    public Task<(string, int)> RegisterAsync(UserRegisterDto dto);
    public Task<(bool, string, UserDto?)> LoginAsync(UserLoginDto dto);
    public Task<string> GenerateResetToken(string email);
    public Task<string?> ValidateResetToken(string token, string validationType);
    public Task<User> ResetPasswordAsync(string email, ResetPasswordDto dto);
    public Task<(string, int)> ForgotPassword(string email);
    public Task<bool> SendOtp(string email);
    public Task<(bool, string, UserDto?)> VerifyOtp(OtpModel model);
    public Task<string?> Setup2FA(SetupAuthDto dto);
    public Task<bool> Enable2Fa(Enable2FaDto dto);
    public Task<UserDto?> Login2Fa(OtpModel dto);
    public Task<bool> BlockUserAccount(string email);
    public System.Threading.Tasks.Task UnblockUser();
}
