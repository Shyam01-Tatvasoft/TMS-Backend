using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Repository.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetByEmailAsync(string email);
    public Task<User?> GetByUsernameAsync(string username);
    public Task<User> AddAsync(User user);
    public Task<List<User>> GetUsers();
    public Task<bool> UpdateAsync(AddEditUserDto user);
    public Task<User?> GetByIdAsync(int id);
}
