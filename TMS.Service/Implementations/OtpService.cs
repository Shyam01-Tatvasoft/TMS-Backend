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

    public string GenerateQrCodeUri(string username, string secret, string issuer = "TMS")
    {
        var keyBytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(keyBytes);
        string barcode = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(username)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
        using var qr = new QRCodeGenerator().CreateQrCode(barcode, QRCodeGenerator.ECCLevel.M);
        return new Base64QRCode(qr).GetGraphic(20);
    }

    public bool Validate(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        return isValid;
    }

}
