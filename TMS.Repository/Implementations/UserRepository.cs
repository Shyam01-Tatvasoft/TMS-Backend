using System.Collections;
using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class UserRepository:IUserRepository
{
     private readonly TmsContext _context;

    public UserRepository(TmsContext context) => _context = context;

    public async Task<List<User>> GetUsers(){
        return await _context.Users.Where(u => u.IsDeleted == false).ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsDeleted == false);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsDeleted == false);
    }
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted == false);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(UserDto user)
    {
        var existingUser = await _context.Users.FindAsync(user.Id);
        if (existingUser == null)
        {
            return false;
        }

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName; 
        existingUser.Phone = user.Phone; 
        existingUser.FkCountryId = user.FkCountryId;
        existingUser.FkCountryTimezone = user.FkCountryTimezone;
        existingUser.IsDeleted = (bool)user.IsDeleted!;
        existingUser.Password = user.Password;
        await _context.SaveChangesAsync();
        return true;
    }
}
