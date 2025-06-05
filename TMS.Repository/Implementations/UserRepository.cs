using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class UserRepository:IUserRepository
{
     private readonly TmsContext _context;

    public UserRepository(TmsContext context) => _context = context;

    public async Task<List<User>> GetUsers(){
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
