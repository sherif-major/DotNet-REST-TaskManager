namespace Api.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
