using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Enums;
using TMS.Repository.Interfaces;
using TMS.Service.Constants;
using TMS.Service.Helpers;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserOtpRepository _userOtpRepository;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly IDataProtector _dataProtector;
    private readonly ISystemConfigurationRepository _systemConfigurationRepository;
    public AuthenticationService(IUserRepository userRepository, IDataProtectionProvider dataProtectionProvider, IUserOtpRepository userOtpRepository, IEmailService emailService, IOtpService otpService, ISystemConfigurationRepository systemConfigurationRepository)
    {
        _userRepository = userRepository;
        _dataProtector = dataProtectionProvider.CreateProtector("ResetPasswordProtector");
        _userOtpRepository = userOtpRepository;
        _emailService = emailService;
        _otpService = otpService;
        _systemConfigurationRepository = systemConfigurationRepository;
    }

    public async Task<(string, int)> RegisterAsync(UserRegisterDto dto)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing != null) return ("Email already registered.", existing.Id);
        var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existingUsername != null) return ("Username already exist.", existingUsername.Id);

        User user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Username = dto.Username.Trim(),
            Email = dto.Email,
            Phone = dto.Phone,
            FkCountryId = dto.CountryId,
            FkCountryTimezone = dto.Timezone,
            FkRoleId = 2,
            Password = "12345678",
            IsDeleted = false
        };

        await _userRepository.AddAsync(user);
        string resetToken = await GenerateResetToken(dto.Email);
        string? setupPath = await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.SetupPasswordUrl);
        var resetLink = setupPath + resetToken;
        string subject = "Password setup request";
        string body = await GetEmailTemplate(resetLink, "Forgot Password Template");
        SendMail(dto.Email, subject, body);
        return ("Account created successfully.", user.Id);
    }

    public async Task<(string, int)> ForgotPassword(string email)
    {
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing == null) return ("User not Exist.", 0);

        string resetToken = await GenerateResetToken(email);
        string? resetPath = await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.ResetPasswordUrl);
        var resetLink = resetPath + resetToken;
        string subject = "Password reset request";
        string body = await GetEmailTemplate(resetLink, "Forgot Password Template");
        SendMail(email, subject, body);
        return ("Mail sent successfully.", existing.Id);
    }

    public async Task<(bool, string)> ResetPasswordAsync(string email, ResetPasswordDto dto)
    {
        User? user = await _userRepository.GetByEmailAsync(email);
        if (VerifyPassword(dto.NewPassword, user?.Password))
        {
            return (false, "New password must be different from the old password.");
        }
        user!.Password = HashPassword(dto.NewPassword);
        int PasswordExpiryDuration = int.Parse(await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.PasswordExpiryDuration) ?? "60");
        AddEditUserDto userData = new()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Password = user.Password,
            Phone = user.Phone,
            IsDeleted = false,
            ModifiedAt = DateTime.Now,
            Username = user.Username,
            PasswordExpiryDate = DateTime.Now.Date.AddDays(PasswordExpiryDuration)
        };
        bool response = await _userRepository.UpdateAsync(userData);
        if (response)
            return (true, "Password reset successfully.");
        else
            return (false, "Failed to reset password.");
    }

    public async Task<(bool, string)> ChangePasswordAsync(ChangePasswordDto dto)
    {
        User? user = await _userRepository.GetByEmailAsync(dto.Email);

        if (VerifyPassword(dto.NewPassword, user?.Password))
        {
            return (false, "New password must be different from the old password.");
        }

        user!.Password = HashPassword(dto.NewPassword);
        int PasswordExpiryDuration = int.Parse(await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.PasswordExpiryDuration) ?? "60");
        AddEditUserDto userData = new()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Password = user.Password,
            Phone = user.Phone,
            IsDeleted = false,
            ModifiedAt = DateTime.Now,
            Username = user.Username,
            PasswordExpiryDate = DateTime.Now.Date.AddDays(PasswordExpiryDuration)
        };
        bool response = await _userRepository.UpdateAsync(userData);
        if (response)
            return (true, "Password changed successfully.");
        else
            return (false, "Failed to change password.");
    }

    public async Task<(bool, string, UserDto?)> LoginAsync(UserLoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !VerifyPassword(dto.Password, user.Password))
        {
            if (user != null)
            {
                user.InvalidLoginAttempts += 1;
                user.LastInvalidAttemptAt = DateTime.Now;
                if (user.IsBlocked == true)
                {
                    return (false, "Your account is blocked try again later.", null);
                }
                int userLockout = int.Parse(await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.UserLockup) ?? "5");
                if (user.InvalidLoginAttempts >= userLockout)
                {
                    user.IsBlocked = true;
                    user.BlockedAt = DateTime.Now;
                    await _userRepository.UpdateUserAsync(user);
                    return (false, "Your account has been blocked due to multiple failed attempts.", null);
                }

                await _userRepository.UpdateUserAsync(user);
            }

            return (false, "Invalid credentials.", null);
        }
        UserDto userData = new()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Phone = user.Phone,
            Role = user.FkRole.Name,
            CountryName = user.FkCountry?.Name,
            TimezoneName = user.FkCountryTimezoneNavigation.Timezone,
            IsTwoFaEnabled = user.IsTwoFaEnabled,
            AuthType = user.AuthType
        };

        if (user.IsBlocked == true)
        {
            return (false, "Your account is blocked please try again latter.", null);
        }

        if (DateTime.Now.Date >= user.PasswordExpiryDate)
        {
            return (false, "Password is Expired.", null);
        }
        if (user.IsTwoFaEnabled == true && user.AuthType == (int)AuthType.AuthTypeEnum.Email)
        {
            await SendOtp(dto.Email);
            return (true, "2FA is enabled.", userData);
        }

        return (true, "User Logged in successfully.", userData);
    }

    public async Task<bool> BlockUserAccount(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return false;
        user.IsBlocked = true;
        user.BlockedAt = DateTime.Now;

        return await _userRepository.UpdateUserAsync(user);
    }

    public async Task<bool> SendOtp(string email)
    {
        var otp = OtpHelper.GenerateOtp(); // e.g., "124567"
        var otpHash = HashHelper.HashString(otp);

        // Save new OTP
        var userOtp = new UserOtp
        {
            Email = email,
            OtpHash = otpHash,
            ExpiryTime = DateTime.Now.AddMinutes(5)
        };
        int id = await _userOtpRepository.AddOtpAsync(userOtp);
        if (id <= 0)
            return false;

        await _emailService.SendMail(email, "Verify your otp", $"Your OTP is: {otp}");

        return true;
    }

    public async Task<(bool, string, UserDto?)> VerifyOtp(OtpModel model)
    {
        var record = await _userOtpRepository.GetAsync(model.Email);
        User? user = await _userRepository.GetByEmailAsync(model.Email);

        if (record == null || record.ExpiryTime < DateTime.UtcNow)
            return (false, "OTP expired or not found.", null);

        if (!HashHelper.VerifyHash(model.OTP, record.OtpHash))
            return (false, "Invalid OTP.", null);
        UserDto userData = new()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Phone = user.Phone,
            Role = user.FkRole.Name,
            CountryName = user.FkCountry?.Name,
            TimezoneName = user.FkCountryTimezoneNavigation.Timezone,
            IsTwoFaEnabled = user.IsTwoFaEnabled,
            AuthType = user.AuthType
        };
        return (true, "OTP is valid.", userData);
    }

    public async Task<string?> Setup2FA(SetupAuthDto dto)
    {
        User? user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null) return null;
        if (dto.AuthType > 3) return null;
        string? qrImage = null;

        if (dto.IsEnabled)
        {
            if (dto.AuthType == (int)AuthType.AuthTypeEnum.AuthenticatorApp)
            {
                qrImage = _otpService.GenerateQrCodeUri(user.Username, user.Email!, "TMS");
            }
            else
            {
                user.AuthType = dto.AuthType;
                user.IsTwoFaEnabled = true;
            }
        }
        else
        {
            user.IsTwoFaEnabled = false;
            user.AuthType = 1;
        }

        await _userRepository.UpdateUserAsync(user);

        return qrImage;
    }

    public async Task<bool> Enable2Fa(Enable2FaDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || user.Email == null) return false;

        if (!_otpService.Validate(user.Email, dto.Code))
            return false;

        user.IsTwoFaEnabled = true;
        user.AuthType = (int)AuthType.AuthTypeEnum.AuthenticatorApp;

        await _userRepository.UpdateUserAsync(user);
        return true;
    }

    public async Task<(bool, string, UserDto?)> Login2Fa(OtpModel dto)
    {
        User? user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || user.Email == null || !_otpService.Validate(user.Email, dto.OTP))
        {
            if (user != null)
            {
                user.InvalidLoginAttempts += 1;
                if (user.IsBlocked == true)
                {
                    return (false, "Your account is blocked try again later.", null);
                }
                int userLockout = int.Parse(await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.UserLockup) ?? "5");
                if (user.InvalidLoginAttempts >= userLockout)
                {
                    await _userRepository.UpdateUserAsync(user);
                    await SendOtp(user.Email!);
                    return (false, "OTP sent to your email now login through that code.", null);
                }

                await _userRepository.UpdateUserAsync(user);
            }
            return (false, "Invalid code.", null);
        }

        if (user.IsBlocked == true)
            return (false, "Your account is blocked please try again latter.", null);
        UserDto userData = new()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Phone = user.Phone,
            Role = user.FkRole.Name,
            CountryName = user.FkCountry?.Name,
            TimezoneName = user.FkCountryTimezoneNavigation.Timezone,
            IsTwoFaEnabled = user.IsTwoFaEnabled,
            AuthType = user.AuthType
        };

        return (true, "Logged In successfully.", userData);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string? storedHash)
    {
        return HashPassword(password) == storedHash;
    }

    public async Task<string> GetEmailTemplate(string ResetLink, string templateName)
    {
        var template = await _emailService.GetEmailTemplateByName(templateName);
        var values = new Dictionary<string, string>
        {
            { "Link", ResetLink },
        };
        string emailbody = _emailService.BuildBody(template?.Body!,values);
        return emailbody.Replace("{{Link}}", ResetLink);
    }

    private void SendMail(string ToEmail, string subject, string body)
    {
        string SenderMail = "test.dotnet@etatvasoft.com";
        string SenderPassword = "P}N^{z-]7Ilp";
        string Host = "mail.etatvasoft.com";
        int Port = 587;

        var smtpClient = new SmtpClient(Host)
        {
            Port = Port,
            Credentials = new NetworkCredential(SenderMail, SenderPassword),
        };

        MailMessage mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(SenderMail);
        mailMessage.To.Add(ToEmail);
        mailMessage.Subject = subject;
        mailMessage.IsBodyHtml = true;
        mailMessage.Body = body;

        smtpClient.Send(mailMessage);
    }

    public async Task<string> GenerateResetToken(string email)
    {
        int ResetPasswordLinkExpiry = int.Parse(await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.ResetPasswordLinkExpiry) ?? "24");
        DateTime expiry = DateTime.UtcNow.AddHours(ResetPasswordLinkExpiry);
        string tokenData = $"{email} | {expiry.Ticks}";
        return _dataProtector.Protect(tokenData);
    }

    public async Task<string?> ValidateResetToken(string token, string validationType)
    {
        string unprotectedToken;
        try
        {
            unprotectedToken = _dataProtector.Unprotect(token);
        }
        catch
        {
            return null;
        }

        // Token format: {email}|{expiryTicks}
        var parts = unprotectedToken.Split('|');
        if (parts.Length != 2 || !long.TryParse(parts[1], out long expiryTicks))
            return null;

        DateTime expiryDate = new DateTime(expiryTicks, DateTimeKind.Utc);
        if (expiryDate < DateTime.UtcNow)
        {
            return null;
        }

        string email = parts[0].Trim();

        User user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        // check for validation
        if (validationType == "ResetPassword")
        {
            DateTime? updatedDate = user.ModifiedAt;
            if (updatedDate.HasValue)
            {
                DateTime updatedDateUtc = TimeZoneInfo.ConvertTimeToUtc(updatedDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                if (updatedDateUtc > expiryDate.AddHours(-24))
                {
                    return null;
                }
            }
        }
        else
        {
            if (user.Password != "12345678")
                return null;
        }

        return email;
    }

    public async System.Threading.Tasks.Task UnblockUser()
    {
        List<User> users = await _userRepository.GetUsers();

        foreach (var user in users)
        {
            if (user.IsBlocked == true)
            {
                DateTime currentTime = DateTime.Now;
                int LockupDuration = int.Parse((await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.LockupDuration))!);
                if (currentTime.Subtract(user.BlockedAt!.Value).TotalMinutes >= LockupDuration)
                {
                    user.IsBlocked = false;
                    user.BlockedAt = null;
                    user.InvalidLoginAttempts = 0;
                    user.LastInvalidAttemptAt = null;
                    await _userRepository.UpdateUserAsync(user);
                }
            }
        }
    }

    public async System.Threading.Tasks.Task ResetInvalidLoginAttempt()
    {
        List<User> users = await _userRepository.GetUsers();

        foreach (var user in users)
        {
            if (user.InvalidLoginAttempts > 0 && user.IsBlocked == false)
            {
                DateTime currentTime = DateTime.Now;
                int LockupDuration = int.Parse((await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.LockupDuration))!);
                // if (currentTime.Date >= user.LastInvalidAttemptAt!.Value.Date.AddDays(1))
                if (user.LastInvalidAttemptAt.HasValue && currentTime.Subtract((DateTime)user.LastInvalidAttemptAt).TotalMinutes >= LockupDuration)
                {
                    user.InvalidLoginAttempts = 0;
                    user.LastInvalidAttemptAt = null;
                    await _userRepository.UpdateUserAsync(user);
                }
            }
        }
    }
}
