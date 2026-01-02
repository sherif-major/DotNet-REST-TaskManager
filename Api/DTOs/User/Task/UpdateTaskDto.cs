namespace Api.DTOs.Task;

public class UpdateTaskDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo";
}
