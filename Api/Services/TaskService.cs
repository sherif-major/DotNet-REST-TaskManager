using Api.Data;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class TaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskItem>> GetByProjectIdAsync(int projectId)
    {
        return await _db.TaskItems
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<(bool ok, string? error, TaskItem? task)> CreateAsync(int projectId, string title, string? description)
    {
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId);
        if (!projectExists) return (false, "Project not found", null);

        var task = new TaskItem
        {
            Title = title,
            Description = description,
            Status = "Todo",
            ProjectId = projectId
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();

        return (true, null, task);
    }

    public async Task<(bool ok, string? error, TaskItem? task)> UpdateAsync(int id, string title, string? description, string status)
    {
        var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return (false, "Task not found", null);

        var allowed = new[] { "Todo", "InProgress", "Done" };
        if (!allowed.Contains(status))
            return (false, "Invalid status. Use: Todo, InProgress, Done", null);

        task.Title = title;
        task.Description = description;
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null, task);
    }

    public async Task<(bool ok, string? error)> SoftDeleteAsync(int id)
    {
        var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return (false, "Task not found");

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }
}
