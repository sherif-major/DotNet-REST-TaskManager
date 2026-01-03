using Api.Data;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class CommentService
{
    private readonly AppDbContext _db;

    public CommentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Comment>> GetByTaskIdAsync(int taskId)
    {
        return await _db.Comments
            .Where(c => c.TaskItemId == taskId)
            .ToListAsync();
    }

    public async Task<(bool ok, string? error, Comment? comment)> CreateAsync(int taskId, int userId, string content)
    {
        var taskExists = await _db.TaskItems.AnyAsync(t => t.Id == taskId);
        if (!taskExists) return (false, "Task not found", null);

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists) return (false, "UserId is invalid", null);

        var comment = new Comment
        {
            Content = content,
            TaskItemId = taskId,
            UserId = userId
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return (true, null, comment);
    }

    public async Task<(bool ok, string? error, Comment? comment)> UpdateAsync(int id, string content)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return (false, "Comment not found", null);

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null, comment);
    }

    public async Task<(bool ok, string? error)> SoftDeleteAsync(int id)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return (false, "Comment not found");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }
}
