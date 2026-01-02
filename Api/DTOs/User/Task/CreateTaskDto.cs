namespace Api.DTOs.Task;

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
