using System.Security.Cryptography;
using System.Text;
using OtpNet;
using QRCoder;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class OtpService : IOtpService
{
    public string GenerateSecret()
    {
        return Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
    }

    public string GetEmailHashAsSecret(string email)
    {
        byte[] emailBytes = Encoding.UTF8.GetBytes(email);
        byte[] hashedBytes = SHA256.HashData(emailBytes);

        return Base32Encoding.ToString(hashedBytes);
    }
    
    public string GenerateQrCodeUri(string username, string email, string issuer = "TMS")
    {
        string secret = GetEmailHashAsSecret(email);
        // path type : otpauth://totp/{issuer}:{username}?secret={secret}&issuer={issuer}
        string barcode = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={Uri.EscapeDataString(secret)}&issuer={Uri.EscapeDataString(issuer)}";
        using var qr = new QRCodeGenerator().CreateQrCode(barcode, QRCodeGenerator.ECCLevel.M);
        return new Base64QRCode(qr).GetGraphic(20);
    }

    public bool Validate(string email, string code)
    {
        string secret = GetEmailHashAsSecret(email);
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        return isValid;
    }

}
