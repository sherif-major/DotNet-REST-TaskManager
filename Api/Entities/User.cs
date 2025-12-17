namespace Api.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "User";

    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
