namespace Api.DTOs.User;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Role { get; set; } = null!;
}
