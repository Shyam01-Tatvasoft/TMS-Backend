namespace TMS.Service.Interfaces;

public interface IJWTService
{
    public Task<(string, int)> GenerateToken(string email, bool rememberMe);
    public (string?, string?, string?) ValidateToken(string token);
}
