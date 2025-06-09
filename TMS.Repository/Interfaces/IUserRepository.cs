using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Repository.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetByEmailAsync(string email);
    public Task<User> AddAsync(User user);
    public Task<List<User>> GetUsers();
    public Task<bool> UpdateAsync(UserDto user);
    public Task<User?> GetByIdAsync(int id);
}
