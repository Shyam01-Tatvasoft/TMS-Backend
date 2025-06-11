using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    //  private readonly IMapper _mapper;
    private readonly IDataProtector _dataProtector;
    public AuthenticationService(IUserRepository userRepository, IDataProtectionProvider dataProtectionProvider)
    {
        _userRepository = userRepository;
        _dataProtector = dataProtectionProvider.CreateProtector("ResetPasswordProtector");

    }

    public async Task<string> RegisterAsync(UserRegisterDto dto)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing != null) return "Email already registered.";
        var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existingUsername != null) return "Username already exist.";

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
        string resetToken = GenerateResetToken(dto.Email);
        var resetLink = "http://127.0.0.1:5500/assets/templates/SetupPassword.html?token=" + resetToken;
        string subject = "Password setup request";
        string body = GetEmailTemplate(resetLink, "SetupPasswordTemplate");
        SendMail(dto.Email, subject, body);
        return "Account created successfully.";
    }

    public async Task<string> ForgotPassword(string email)
    {
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing == null) return "User not Exist.";

        string resetToken = GenerateResetToken(email);
        var resetLink = "http://127.0.0.1:5500/assets/templates/ResetPassword.html?token=" + resetToken;
        string subject = "Password reset request";
        string body = GetEmailTemplate(resetLink, "ForgotPasswordTemplate");
        SendMail(email, subject, body);
        return "Mail sent successfully.";
    }

    public async Task<bool> ResetPasswordAsync(string email, ResetPasswordDto dto)
    {
        User? user = await _userRepository.GetByEmailAsync(email);
        user.Password = HashPassword(dto.NewPassword);
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
            Username = user.Username
        };
        bool response = await _userRepository.UpdateAsync(userData);
        return response;
    }

    public async Task<UserDto?> LoginAsync(UserLoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !VerifyPassword(dto.Password, user.Password)) return null;
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
        };
        return userData;
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

    private static string GetEmailTemplate(string ResetLink, string templateName)
    {
        string templatePath = "";
        if (templateName == "ForgotPasswordTemplate")
        {
            templatePath = "d:/TMS/TMS.Service/Templates/ResetPasswordMailTemplate.html";
        }
        else
        {
            templatePath = "d:/TMS/TMS.Service/Templates/EmailTemplate.html";
        }
        if (!System.IO.File.Exists(templatePath))
        {
            return "<p>Email template Not Fount</p>";
        }
        string emailbody = System.IO.File.ReadAllText(templatePath);
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
        // StringBuilder mailBody = new StringBuilder();
        mailMessage.Body = body;

        smtpClient.Send(mailMessage);
    }

    public string GenerateResetToken(string email)
    {
        DateTime expiry = DateTime.UtcNow.AddHours(24);
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
}
