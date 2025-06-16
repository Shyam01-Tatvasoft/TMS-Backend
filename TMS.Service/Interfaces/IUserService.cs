using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface IUserService
{
    public Task<List<UserDto>> GetUsers();
    public Task<(List<UserDto>,int count)> GetUsers(int skip, int take, string? search, string? sorting = null, string? sortDirection = null);
    public Task<UserDto?> GetUserByEmail(string? email);
    public Task<(bool success, string message)> UpdateUser(AddEditUserDto user);
    public Task<(bool success, string message)> DeleteUser(int id);
    public Task<(bool success, string message)> AddUser(AddEditUserDto user);
    public Task<UserDto?> GetUserById(int id);
}
