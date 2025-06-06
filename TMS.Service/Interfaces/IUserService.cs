using TMS.Repository.Data;

namespace TMS.Service.Interfaces;

public interface IUserService
{
    public Task<List<User>> GetUsers();
    public Task<User?> GetUserByEmail(string? email);
}
