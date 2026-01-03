namespace Api.DTOs.Comment;

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public int TaskItemId { get; set; }
    public int UserId { get; set; }
}
