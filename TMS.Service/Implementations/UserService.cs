using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoMapper;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Implementations;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskActionRepository _taskActionRepository;
    // private readonly IMapper _mapper;
    public UserService(IUserRepository userRepository, ITaskActionRepository taskActionRepository)
    {
        _userRepository = userRepository;
        _taskActionRepository = taskActionRepository;
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
                ProfileImagePath = user.ProfileImage,
            };
            userDtos.Add(userData);
        });
        return userDtos;
        // return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<(List<UserDto>, int count)> GetUsers(int skip, int take, string? search, string? sorting = null, string? sortDirection = null)
    {
        return await _userRepository.GetUsers(skip, take, search, sorting, sortDirection);
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
            ProfileImagePath = user.ProfileImage,
            IsTwoFaEnabled = user.IsTwoFaEnabled,
            AuthType = user.AuthType,
            HasSecret = !string.IsNullOrEmpty(user.OtpSecret),
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
            ProfileImagePath = user.ProfileImage,
        };
        return userData;
    }

    public async Task<(bool success, string message)> AddUser(AddEditUserDto user)
    {
        User? existingUser = await _userRepository.GetByUsernameAsync(user.Username);
        if (existingUser != null) return (false, "Username already exists.");
        var existing = await _userRepository.GetByEmailAsync(user.Email);
        if (existing != null) return (false, "Email already registered.");


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
        User? existingUserByEmail = await _userRepository.GetByEmailAsync(user.Email);

        string oldCombined = existingUserByEmail?.FirstName.Substring(0, 2) +
            existingUserByEmail?.LastName.Substring(existingUserByEmail.LastName.Length - 2) +
            existingUserByEmail?.Phone?.Substring(0, 1) +
            existingUserByEmail?.Email.Substring(0, 3);

        string newCombined = user?.FirstName.Substring(0, 2) +
            user?.LastName.Substring(user.LastName.Length - 2) +
            user?.Phone?.Substring(0, 1) +
            user?.Email.Substring(0, 3);

        if (oldCombined != newCombined)
        {
            bool isDocumentUpdated = await UpdateUserDocuments(oldCombined, newCombined, (int)user?.Id!);
            if (!isDocumentUpdated)
                return (false, "Error while update the user.");
        }

        // start profile image handling
        string ProfileImagePath = null;
        if (user?.ProfileImage != null && user.ProfileImage.Length > 0)
        {
            var folderPath = "D:/TMSFrontend/assets/images/ProfileImages";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filename = Guid.NewGuid().ToString() + Path.GetExtension(user.ProfileImage.FileName);
            var filePath = Path.Combine(folderPath, filename);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                user.ProfileImage.CopyTo(stream);
            }
            ProfileImagePath = "/ProfileImages/" + filename;
        }
        if (ProfileImagePath != null)
            user.ProfileImagePath = ProfileImagePath;
        // ending profile image handling

        user.Password = existingUserByEmail?.Password;
        user.ModifiedAt = DateTime.Now;
        user.IsDeleted = false;
        bool response = await _userRepository.UpdateAsync(user);

        return response ? (true, "User updated successfully.") : (false, "User Not Found.");
    }

    public async Task<bool> UpdateUserDocuments(string oldCombined, string newCombined, int userId)
    {
        string userFolder = Path.Combine(Directory.GetCurrentDirectory(), "Upload", userId.ToString());

        if (!Directory.Exists(userFolder))
            return true;

        // Generate old key,IV
        byte[] oldKey = SHA256.HashData(Encoding.UTF8.GetBytes(oldCombined));
        byte[] oldIv = MD5.HashData(Encoding.UTF8.GetBytes(oldCombined));

        // Generate new key,IV
        byte[] newKey = SHA256.HashData(Encoding.UTF8.GetBytes(newCombined));
        byte[] newIv = MD5.HashData(Encoding.UTF8.GetBytes(newCombined));

        var files = Directory.GetFiles(userFolder);

        foreach (var filePath in files)
        {
            try
            {
                // Decrypt with old key
                byte[] decryptedData;
                using (var inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var aes = Aes.Create())
                {
                    aes.Key = oldKey;
                    aes.IV = oldIv;
                    using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                    using var ms = new MemoryStream();
                    await cryptoStream.CopyToAsync(ms);
                    decryptedData = ms.ToArray();
                }

                // Encrypt with new key and overwrite original file
                using (var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (var aesNew = Aes.Create())
                {
                    aesNew.Key = newKey;
                    aesNew.IV = newIv;
                    using var cryptoStream = new CryptoStream(outputStream, aesNew.CreateEncryptor(), CryptoStreamMode.Write);
                    await cryptoStream.WriteAsync(decryptedData, 0, decryptedData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update file {ex.Message}");
                return false;
            }
        }

        return true;
    }

    public class TaskFileMetadata
    {
        public string OriginalFileName { get; set; }
        public string StoredFileName { get; set; }
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; }
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
