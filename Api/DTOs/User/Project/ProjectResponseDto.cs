namespace Api.DTOs.Project;

public class ProjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int UserId { get; set; }
}
