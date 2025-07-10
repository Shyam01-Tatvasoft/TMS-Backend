namespace TMS.Service.Interfaces;

public interface IOtpService
{
    public string GenerateSecret();
    public string GenerateQrCodeUri(string username, string secret, string issuer = "MyApp");
    public bool Validate(string secret, string code);
}
