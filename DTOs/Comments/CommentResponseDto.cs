namespace ProjectManagementSystem.DTOs.Comments;

public class CommentResponseDto
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int TaskItemId { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;
}