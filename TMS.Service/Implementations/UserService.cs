using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    // private readonly IMapper _mapper;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        // _mapper = mapper;
    }

    public async Task<List<User>> GetUsers()
    {
        List<User> users = await _userRepository.GetUsers();
        return users;
        // return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto?> GetUserByEmail(string? email)
    {
        User? user = await _userRepository.GetByEmailAsync(email);
        UserDto userData = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Phone = user.Phone,
            Role = user.FkRoleId == 1 ? "Admin" : "User"
        };
        return userData;
        // return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserById(int id)
    {
        User? user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        UserDto userData = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Phone = user.Phone,
            Role = user.FkRoleId == 1 ? "Admin" : "User"
        };
        return userData;
    }
    
    public async Task<(bool success, string message)> AddUser(UserDto user)
    {
        User newUser = new User{
            FirstName = user.FirstName.Trim(),
            Username = user.Username.Trim(),
            LastName = user.LastName.Trim(),
            Email = user.Email,
            Phone = user.Phone,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            FkRoleId = 2,
            Password = "12345678",
            IsDeleted = false
        };
        User response = await _userRepository.AddAsync(newUser);
        return response != null ? (true, "User added successfully.") : (false, "Error while add new user.");
    }

    public async Task<(bool success, string message)> UpdateUser(UserDto user)
    {
        bool response = await _userRepository.UpdateAsync(user);

        return response ? (true, "User updated successfully.") : (false, "User Not Found.");
    }


    public async Task<(bool success, string message)> DeleteUser(int id)
    {
        User? user = await _userRepository.GetByIdAsync(id);
        if(user == null)
            return (false, "User Not Found.");
        UserDto userData = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Phone = user.Phone,
            Role = user.FkRoleId == 1 ? "Admin" : "User",
            Password = user.Password,
            IsDeleted = true
        };
        bool response = await _userRepository.UpdateAsync(userData);

        return response ? (true, "User deleted successfully.") : (false, "User Not Found.");
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
