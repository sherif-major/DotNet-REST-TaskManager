using Api.Data;
using Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<(bool ok, string? token, string? role, string? username)> LoginAsync(string username, string password)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

        if (user is null)
            return (false, null, null, null);

        var token = JwtTokenHelper.GenerateToken(user, _config);
        return (true, token, user.Role, user.Username);
    }
}
