using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface IUserService
{
    public Task<List<User>> GetUsers();
    public Task<UserDto?> GetUserByEmail(string? email);
    public Task<(bool success, string message)> UpdateUser(UserDto user);
    public Task<(bool success, string message)> DeleteUser(int id);
    public Task<(bool success, string message)> AddUser(UserDto user);
    public Task<UserDto?> GetUserById(int id);
}
