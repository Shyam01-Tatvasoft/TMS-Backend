using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class JWTService:IJWTService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JWTService(IConfiguration config,IUserRepository userRepository)
    {
        _key = config["Jwt:Key"]!;
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
        _userRepository = userRepository;
    }


    public async Task<string> GenerateToken(string email, bool rememberMe)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        User? user = await _userRepository.GetByEmailAsync(email);
        string? userRole = user?.FkRoleId == 1 ? "Admin" : "User";
        
        var authClaims = new List<Claim>{
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, userRole),
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
         };
        DateTime expiry = rememberMe ? DateTime.UtcNow.AddHours(24) : DateTime.UtcNow.AddHours(3);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: authClaims,
            expires: expiry,
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string?, string?, string?) ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_key);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var email = principal.FindFirst(ClaimTypes.Email)?.Value.Trim();
            var role = principal.FindFirst(ClaimTypes.Role)?.Value.Trim();
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Trim().ToString();
            return (email, role, userId);
        }
        catch (Exception ex)
        {
            return (null, null, null);
        }
    }
}
