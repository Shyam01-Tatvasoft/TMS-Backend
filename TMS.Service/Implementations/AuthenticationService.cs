using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class AuthenticationService:IAuthenticationService
{
     private readonly IUserRepository _userRepository;
    //  private readonly IMapper _mapper;

    public AuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<string> RegisterAsync(UserRegisterDto dto)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing != null) return "Email already registered.";

        User user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Password = HashPassword(dto.Password),
            Phone = dto.Phone,
            Country = dto.Country,
            Role = "User",
            CountryId = dto.CountryId,
            CountryTimezone = dto.Timezone,
            IsDeleted = false
        };

        await _userRepository.AddAsync(user);
        return "Registration successful.";
    }

    public async Task<User?> LoginAsync(UserLoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !VerifyPassword(dto.Password, user.Password)) return null;
        return user;
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
}
