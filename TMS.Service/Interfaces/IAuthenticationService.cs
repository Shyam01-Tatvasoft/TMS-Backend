using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface IAuthenticationService
{
    public Task<string> RegisterAsync(UserRegisterDto dto);
    public Task<User?> LoginAsync(UserLoginDto dto);
}
