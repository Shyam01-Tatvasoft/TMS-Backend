namespace TMS.Service.Interfaces;

public interface IOtpService
{
    public string GenerateSecret();
    public string GenerateQrCodeUri(string username, string email, string issuer = "MyApp");
    public bool Validate(string email, string code);
}
