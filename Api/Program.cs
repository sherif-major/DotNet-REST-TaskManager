using Api.Data;
using Api.DTOs.User;
using Api.Entities;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Api.Seed;
using Api.DTOs.Project;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

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
        PasswordHash = dto.Password,
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

app.MapPost("/projects", async (CreateProjectDto dto, AppDbContext db) =>
{
    var userExists = await db.Users.AnyAsync(u => u.Id == dto.UserId);
    if (!userExists)
    {
        return Results.BadRequest(
            ApiResponse<string>.Fail("UserId is invalid")
        );
    }

    var project = new Project
    {
        Name = dto.Name,
        Description = dto.Description,
        UserId = dto.UserId
    };

    db.Projects.Add(project);
    await db.SaveChangesAsync();

    var response = new ProjectResponseDto
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        UserId = project.UserId
    };

    return Results.Created(
        $"/projects/{project.Id}",
        ApiResponse<ProjectResponseDto>.SuccessResponse(response, "Project created successfully")
    );
});

app.MapGet("/projects", async (AppDbContext db) =>
{
    var projects = await db.Projects
        .Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            UserId = p.UserId
        })
        .ToListAsync();

    return Results.Ok(
        ApiResponse<List<ProjectResponseDto>>.SuccessResponse(
            projects,
            "Projects listed successfully"
        )
    );
});

app.MapGet("/projects/{id:int}", async (int id, AppDbContext db) =>
{
    var project = await db.Projects
        .Where(p => p.Id == id)
        .Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            UserId = p.UserId
        })
        .FirstOrDefaultAsync();

    if (project is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Project not found")
        );
    }

    return Results.Ok(
        ApiResponse<ProjectResponseDto>.SuccessResponse(
            project,
            "Project retrieved successfully"
        )
    );
});


app.MapGet("/users/{userId:int}/projects", async (int userId, AppDbContext db) =>
{
    var userExists = await db.Users.AnyAsync(u => u.Id == userId);
    if (!userExists)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("User not found")
        );
    }

    var projects = await db.Projects
        .Where(p => p.UserId == userId)
        .Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            UserId = p.UserId
        })
        .ToListAsync();

    return Results.Ok(
        ApiResponse<List<ProjectResponseDto>>.SuccessResponse(
            projects,
            "User projects listed successfully"
        )
    );
});

app.MapPut("/projects/{id:int}", async (int id, UpdateProjectDto dto, AppDbContext db) =>
{
    var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

    if (project is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Project not found")
        );
    }

    project.Name = dto.Name;
    project.Description = dto.Description;
    project.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    var response = new ProjectResponseDto
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        UserId = project.UserId
    };

    return Results.Ok(
        ApiResponse<ProjectResponseDto>.SuccessResponse(
            response,
            "Project updated successfully"
        )
    );
});

app.MapDelete("/projects/{id:int}", async (int id, AppDbContext db) =>
{
    var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

    if (project is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Project not found")
        );
    }

    project.IsDeleted = true;
    project.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(
        ApiResponse<string>.SuccessResponse(
            "Project deleted successfully",
            "Project deleted successfully"
        )
    );
});


app.Run();
