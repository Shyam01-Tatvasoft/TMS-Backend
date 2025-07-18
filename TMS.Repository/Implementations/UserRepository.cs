using System.Collections;
using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class UserRepository : IUserRepository
{
    private readonly TmsContext _context;

    public UserRepository(TmsContext context) => _context = context;

    public async Task<List<User>> GetUsers()
    {
        return await _context.Users.Where(u => u.IsDeleted == false)
        .Include(u => u.FkRole)
        .Include(u => u.FkCountry)
        .Include(u => u.FkCountryTimezoneNavigation)
        .ToListAsync();
    }

    public async Task<(List<UserDto>,int count)> GetUsers(int skip, int take, string? search, string? sorting = null, string? sortDirection = null)
    {
        var query = _context.Users.Where(u => u.IsDeleted == false)
            .Include(u => u.FkRole)
            .Include(u => u.FkCountry)
            .Include(u => u.FkCountryTimezoneNavigation)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                u.Username.ToLower().Contains(search));
        }

        if (!string.IsNullOrEmpty(sorting) && !string.IsNullOrEmpty(sortDirection))
        {
            if (sortDirection.ToLower() == "asc")
            {
                query = sorting switch
                {
                    "0" => query.OrderBy(u => u.FirstName),
                    "1" => query.OrderBy(u => u.LastName),
                    "2" => query.OrderBy(u => u.Username),
                    "3" => query.OrderBy(u => u.Email),
                    _ => query.OrderBy(u => u.FirstName)
                };
            }
            else
            {
                query = sorting switch
                {
                     "0" => query.OrderByDescending(u => u.FirstName),
                    "1" => query.OrderByDescending(u => u.LastName),
                    "2" => query.OrderByDescending(u => u.Username),
                    "3" => query.OrderByDescending(u => u.Email),
                    _ => query.OrderByDescending(u => u.FirstName)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(u => u.Id);
        }
        int totalCount = await query.CountAsync();

        List<UserDto> users = await query
            .Skip(skip)
            .Take(take)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FkCountryId = u.FkCountryId,
                FkCountryTimezone = u.FkCountryTimezone,
                Phone = u.Phone,
                Role = u.FkRole.Name,
                CountryName = u.FkCountry.Name,
                TimezoneName = u.FkCountryTimezoneNavigation.Timezone,
                ProfileImagePath = u.ProfileImage
            })
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Where(u => u.Email == email.Trim() && u.IsDeleted == false)
        .Include(u => u.FkRole)
        .Include(u => u.FkCountry)
        .Include(u => u.FkCountryTimezoneNavigation)
        .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.Where(u => u.Username == username && u.IsDeleted == false)
        .Include(u => u.FkRole)
        .Include(u => u.FkCountry)
        .Include(u => u.FkCountryTimezoneNavigation)
        .FirstOrDefaultAsync();
    }
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.Where(u => u.Id == id && u.IsDeleted == false)
        .Include(u => u.FkRole)
        .Include(u => u.FkCountry)
        .Include(u => u.FkCountryTimezoneNavigation)
        .FirstOrDefaultAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(AddEditUserDto user)
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
        existingUser.IsDeleted = user.IsDeleted;
        existingUser.Password = user.Password;
        existingUser.Username = user.Username;
        existingUser.ModifiedAt = user.ModifiedAt;
        existingUser.ProfileImage = user.ProfileImagePath;
        existingUser.PasswordExpiryDate = user.PasswordExpiryDate;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
