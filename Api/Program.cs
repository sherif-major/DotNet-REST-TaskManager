using Api.Data;
using Api.DTOs.User;
using Api.Entities;
using Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "API çalışıyor");
app.MapGet("/users", async (AppDbContext db) =>
{
    var users = await db.Users
        .Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role
        })
        .ToListAsync();

    return Results.Ok(
        ApiResponse<List<UserResponseDto>>.SuccessResponse(
            users,
            "Users listed successfully"
        )
    );
});

app.MapGet("/users/{id:int}", async (int id, AppDbContext db) =>
{
    var user = await db.Users
        .Where(u => u.Id == id)
        .Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role
        })
        .FirstOrDefaultAsync();

    if (user is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("User not found")
        );
    }

    return Results.Ok(
        ApiResponse<UserResponseDto>.SuccessResponse(
            user,
            "User retrieved successfully"
        )
    );
});

app.MapPut("/users/{id:int}", async (
    int id,
    UpdateUserDto dto,
    AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    if (user is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("User not found")
        );
    }

    var usernameExists = await db.Users
        .AnyAsync(u => u.Username == dto.Username && u.Id != id);

    if (usernameExists)
    {
        return Results.Conflict(
            ApiResponse<string>.Fail("Username already in use")
        );
    }

    user.Username = dto.Username;
    user.Role = dto.Role;
    user.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    var response = new UserResponseDto
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role
    };

    return Results.Ok(
        ApiResponse<UserResponseDto>.SuccessResponse(
            response,
            "User updated successfully"
        )
    );
});

app.MapPost("/users", async (CreateUserDto dto, AppDbContext db) =>
{
    var exists = await db.Users.AnyAsync(x => x.Username == dto.Username);
    if (exists)
    {
        return Results.BadRequest(
            ApiResponse<string>.Fail("Username already exists")
        );
    }

    var user = new User
    {
        Username = dto.Username,
        PasswordHash = dto.Password, // şimdilik düz yazı
        Role = "User"
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    var response = new UserResponseDto
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role
    };

    return Results.Created(
        $"/users/{user.Id}",
        ApiResponse<UserResponseDto>.SuccessResponse(response, "User created successfully")
    );
});


app.MapDelete("/users/{id:int}", async (
    int id,
    AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    if (user is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("User not found")
        );
    }

    user.IsDeleted = true;
    user.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(
        ApiResponse<string>.SuccessResponse(
            "User deleted successfully"
        )
    );
});

app.Run();
