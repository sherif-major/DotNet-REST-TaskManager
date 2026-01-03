namespace Api.DTOs.Project;

public class CreateProjectDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public int UserId { get; set; }
}
