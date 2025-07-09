using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface IUserOtpRepository
{
    public Task<int> AddOtpAsync(UserOtp userOtp);
    public Task<UserOtp?> GetAsync(string email);
}