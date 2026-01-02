using Api.Data;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var anyUser = await db.Users.AnyAsync();
        if (!anyUser)
        {
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = "admin123",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
