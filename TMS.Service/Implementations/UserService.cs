using TMS.Repository.Data;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class UserService: IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;

    }

    public async Task<List<User>> GetUsers()
    {
        
        return await _userRepository.GetUsers();
    }
}
