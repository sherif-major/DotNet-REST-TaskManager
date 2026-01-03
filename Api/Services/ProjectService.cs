using Api.Data;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Project>> GetAllAsync()
    {
        return await _db.Projects.ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Project>> GetByUserIdAsync(int userId)
    {
        return await _db.Projects.Where(p => p.UserId == userId).ToListAsync();
    }

    public async Task<(bool ok, string? error, Project? project)> CreateAsync(string name, string? description, int userId)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            return (false, "UserId is invalid", null);

        var project = new Project
        {
            Name = name,
            Description = description,
            UserId = userId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return (true, null, project);
    }

    public async Task<(bool ok, string? error, Project? project)> UpdateAsync(int id, string name, string? description)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project is null) return (false, "Project not found", null);

        project.Name = name;
        project.Description = description;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null, project);
    }

    public async Task<(bool ok, string? error)> SoftDeleteAsync(int id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project is null) return (false, "Project not found");

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }
}
