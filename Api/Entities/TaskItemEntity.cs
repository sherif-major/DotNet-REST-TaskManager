namespace Api.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public string Status { get; set; } = "Todo";

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
