using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class UserOtpRepository : IUserOtpRepository
{
    private readonly TmsContext _context;

    public UserOtpRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<int> AddOtpAsync(UserOtp userOtp)
    {
        await _context.AddAsync(userOtp);
        await _context.SaveChangesAsync();
        return userOtp.Id;
    }

    public async Task<UserOtp?> GetAsync(string email)
    {
        UserOtp? otp = await _context.UserOtps
        .Where(x => x.Email == email)
        .OrderByDescending(x => x.CreatedAt)
        .FirstOrDefaultAsync();

        return otp;
    }
}