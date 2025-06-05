using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetByEmailAsync(string email);
    public Task<User> AddAsync(User user);
    public Task<List<User>> GetUsers();
}
