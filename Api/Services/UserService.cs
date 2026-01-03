using Api.Data;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<(bool ok, string? error, User? user)> CreateAsync(string username, string password, string role = "User")
    {
        var exists = await _db.Users.AnyAsync(u => u.Username == username);
        if (exists)
            return (false, "Username already exists", null);

        var user = new User
        {
            Username = username,
            PasswordHash = password, // şimdilik düz
            Role = role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, null, user);
    }

    public async Task<(bool ok, string? error)> UpdateRoleAsync(int id, string role)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return (false, "User not found");

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ok, string? error)> SoftDeleteAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return (false, "User not found");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }
}
