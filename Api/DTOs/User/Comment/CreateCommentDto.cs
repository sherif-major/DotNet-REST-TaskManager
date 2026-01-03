namespace Api.DTOs.Comment;

public class CreateCommentDto
{
    public string Content { get; set; } = null!;
    public int UserId { get; set; }  // şimdilik buradan alıyoruz
}
