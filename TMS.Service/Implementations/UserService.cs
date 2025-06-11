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

    public async Task<List<UserDto>> GetUsers()
    {
        List<User> users = await _userRepository.GetUsers();
        List<UserDto> userDtos = new List<UserDto>();
        users.ForEach(user =>
        {
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
                Role = user.FkRole.Name,
                CountryName = user.FkCountry?.Name,
                TimezoneName = user.FkCountryTimezoneNavigation.Timezone,
            };
            userDtos.Add(userData);
        });
        return userDtos;
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
            Role = user.FkRole.Name,
            CountryName = user.FkCountry?.Name,
            TimezoneName = user.FkCountryTimezoneNavigation.Timezone,
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
            Role = user.FkRole.Name,
            CountryName = user.FkCountry?.Name,
            TimezoneName = user.FkCountryTimezoneNavigation.Timezone,
        };
        return userData;
    }

    public async Task<(bool success, string message)> AddUser(AddEditUserDto user)
    {
        var existing = await _userRepository.GetByEmailAsync(user.Email);
        if (existing != null) return (false,"Email already registered.");
        User? existingUser = await _userRepository.GetByUsernameAsync(user.Username);
        if (existingUser != null)
            return (false, "Username already exists.");

        User newUser = new User
        {
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

    public async Task<(bool success, string message)> UpdateUser(AddEditUserDto user)
    {
        User? existingUser = await _userRepository.GetByUsernameAsync(user.Username);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return (false, "Username not available.");
        }
        User existingUserByEmail = await _userRepository.GetByEmailAsync(user.Email);

        user.Password = existingUserByEmail?.Password;
        user.ModifiedAt = DateTime.Now;
        user.IsDeleted = false;
        bool response = await _userRepository.UpdateAsync(user);

        return response ? (true, "User updated successfully.") : (false, "User Not Found.");
    }


    public async Task<(bool success, string message)> DeleteUser(int id)
    {
        User? user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return (false, "User Not Found.");
        AddEditUserDto userData = new()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FkCountryId = user.FkCountryId,
            FkCountryTimezone = user.FkCountryTimezone,
            Phone = user.Phone,
            Password = user.Password,
            Username = user.Username,
            IsDeleted = true,
            ModifiedAt = user.ModifiedAt
        };
        bool response = await _userRepository.UpdateAsync(userData);

        return response ? (true, "User deleted successfully.") : (false, "User Not Found.");
    }

}
