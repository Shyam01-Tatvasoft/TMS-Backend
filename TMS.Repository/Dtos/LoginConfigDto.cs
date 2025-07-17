namespace TMS.Repository.Dtos;

public class LoginConfigDto
{
    public string JWTSecret { get; set; } = string.Empty;
    public string JWTIssuer { get; set; } = string.Empty;
    public string JWTAudience { get; set; } = string.Empty;
    public string JWTSubject { get; set; } = string.Empty;
    public int JWTExpiry { get; set; }
    public int JWTExpiryLong { get; set; }
    public int UserLockup { get; set; }
    public int LockupDuration { get; set; }
    public int ResetPasswordLinkExpiry { get; set; }
    public int SetupPasswordLinkExpiry { get; set; }
    public int PasswordExpiryDuration { get; set; }
    public string ResetPasswordUrl { get; set; } = string.Empty;
    public string SetupPasswordUrl { get; set; } = string.Empty;
}