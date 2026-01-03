using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Api.Data;
using Api.DTOs.User;
using Api.Entities;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Api.Seed;
using Api.DTOs.Project;
using Api.DTOs.Task;
using Api.DTOs.Comment;
using Api.DTOs.Auth;
using Api.Helpers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token} şeklinde gir. Örn: Bearer eyJhbGciOi..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "API çalışıyor");
app.UseAuthentication();
app.UseAuthorization();
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
})
.RequireAuthorization();


app.MapGet("/users/{id:int}", async (int id, AppDbContext db) => //controller yerine yazdığım endpointler. 
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
}).RequireAuthorization();

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
}).RequireAuthorization(p => p.RequireRole("Admin"));

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
}).RequireAuthorization(p => p.RequireRole("Admin"));


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
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.MapPost("/auth/login", async (
    LoginDto dto,
    AppDbContext db,
    IConfiguration config) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(u =>
            u.Username == dto.Username &&
            u.PasswordHash == dto.Password);

    if (user is null)
    {
        return Results.Json(
            ApiResponse<string>.Fail("Invalid username or password"),
            statusCode: StatusCodes.Status401Unauthorized
        );
    }

    var token = JwtTokenHelper.GenerateToken(user, config);

    return Results.Ok(
        ApiResponse<object>.SuccessResponse(
            new
            {
                token,
                user.Username,
                user.Role
            },
            "Login successful"
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
}).RequireAuthorization(p => p.RequireRole("Admin"));

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
}).RequireAuthorization();

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
}).RequireAuthorization();


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
}).RequireAuthorization();

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
}).RequireAuthorization(p => p.RequireRole("Admin"));

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
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.MapPost("/projects/{projectId:int}/tasks", async (
    int projectId,
    CreateTaskDto dto,
    AppDbContext db) =>
{
    var projectExists = await db.Projects.AnyAsync(p => p.Id == projectId);
    if (!projectExists)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Project not found")
        );
    }

    var task = new TaskItem
    {
        Title = dto.Title,
        Description = dto.Description,
        Status = "Todo",
        ProjectId = projectId
    };

    db.TaskItems.Add(task);
    await db.SaveChangesAsync();

    var response = new TaskResponseDto
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        ProjectId = task.ProjectId
    };

    return Results.Created(
        $"/tasks/{task.Id}",
        ApiResponse<TaskResponseDto>.SuccessResponse(response, "Task created successfully")
    );
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.MapGet("/projects/{projectId:int}/tasks", async (int projectId, AppDbContext db) =>
{
    var projectExists = await db.Projects.AnyAsync(p => p.Id == projectId);
    if (!projectExists)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Project not found")
        );
    }

    var tasks = await db.TaskItems
        .Where(t => t.ProjectId == projectId)
        .Select(t => new TaskResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            ProjectId = t.ProjectId
        })
        .ToListAsync();

    return Results.Ok(
        ApiResponse<List<TaskResponseDto>>.SuccessResponse(tasks, "Tasks listed successfully")
    );
}).RequireAuthorization();

app.MapPut("/tasks/{id:int}", async (int id, UpdateTaskDto dto, AppDbContext db) =>
{
    var task = await db.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
    if (task is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Task not found")
        );
    }
    
    var allowed = new[] { "Todo", "InProgress", "Done" };
    if (!allowed.Contains(dto.Status))
    {
        return Results.BadRequest(
            ApiResponse<string>.Fail("Invalid status. Use: Todo, InProgress, Done")
        );
    }

    task.Title = dto.Title;
    task.Description = dto.Description;
    task.Status = dto.Status;
    task.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    var response = new TaskResponseDto
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        ProjectId = task.ProjectId
    };

    return Results.Ok(
        ApiResponse<TaskResponseDto>.SuccessResponse(response, "Task updated successfully")
    );
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.MapDelete("/tasks/{id:int}", async (int id, AppDbContext db) =>
{
    var task = await db.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
    if (task is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Task not found")
        );
    }

    task.IsDeleted = true;
    task.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(
        ApiResponse<string>.SuccessResponse("Task deleted successfully", "Task deleted successfully")
    );
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.MapPost("/tasks/{taskId:int}/comments", async (
    int taskId,
    CreateCommentDto dto,
    AppDbContext db) =>
{
    var taskExists = await db.TaskItems.AnyAsync(t => t.Id == taskId);
    if (!taskExists)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Task not found")
        );
    }

    var userExists = await db.Users.AnyAsync(u => u.Id == dto.UserId);
    if (!userExists)
    {
        return Results.BadRequest(
            ApiResponse<string>.Fail("UserId is invalid")
        );
    }

    var comment = new Comment
    {
        Content = dto.Content,
        TaskItemId = taskId,
        UserId = dto.UserId
    };

    db.Comments.Add(comment);
    await db.SaveChangesAsync();

    var response = new CommentResponseDto
    {
        Id = comment.Id,
        Content = comment.Content,
        TaskItemId = comment.TaskItemId,
        UserId = comment.UserId
    };

    return Results.Created(
        $"/comments/{comment.Id}",
        ApiResponse<CommentResponseDto>.SuccessResponse(response, "Comment created successfully")
    );
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.MapGet("/tasks/{taskId:int}/comments", async (int taskId, AppDbContext db) =>
{
    var taskExists = await db.TaskItems.AnyAsync(t => t.Id == taskId);
    if (!taskExists)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Task not found")
        );
    }

    var comments = await db.Comments
        .Where(c => c.TaskItemId == taskId)
        .Select(c => new CommentResponseDto
        {
            Id = c.Id,
            Content = c.Content,
            TaskItemId = c.TaskItemId,
            UserId = c.UserId
        })
        .ToListAsync();

    return Results.Ok(
        ApiResponse<List<CommentResponseDto>>.SuccessResponse(
            comments,
            "Comments listed successfully"
        )
    );
}).RequireAuthorization();

app.MapPut("/comments/{id:int}", async (int id, UpdateCommentDto dto, AppDbContext db) =>
{
    var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
    if (comment is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Comment not found")
        );
    }

    comment.Content = dto.Content;
    comment.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    var response = new CommentResponseDto
    {
        Id = comment.Id,
        Content = comment.Content,
        TaskItemId = comment.TaskItemId,
        UserId = comment.UserId
    };

    return Results.Ok(
        ApiResponse<CommentResponseDto>.SuccessResponse(
            response,
            "Comment updated successfully"
        )
    );
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.MapDelete("/comments/{id:int}", async (int id, AppDbContext db) =>
{
    var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
    if (comment is null)
    {
        return Results.NotFound(
            ApiResponse<string>.Fail("Comment not found")
        );
    }

    comment.IsDeleted = true;
    comment.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(
        ApiResponse<string>.SuccessResponse(
            "Comment deleted successfully",
            "Comment deleted successfully"
        )
    );
}).RequireAuthorization(p => p.RequireRole("Admin"));



app.Run();
