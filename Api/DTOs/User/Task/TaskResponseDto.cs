namespace Api.DTOs.Task;

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public int ProjectId { get; set; }
}
