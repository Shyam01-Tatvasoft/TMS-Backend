using System.Security.Cryptography;
using System.Text;

namespace TMS.Service.Helpers;

public class HashHelper
{
    public static string HashString(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyHash(string password, string? storedHash)
    {
        return HashString(password) == storedHash;
    }

}
