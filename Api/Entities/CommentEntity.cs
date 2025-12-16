namespace Api.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; } = null!;

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
