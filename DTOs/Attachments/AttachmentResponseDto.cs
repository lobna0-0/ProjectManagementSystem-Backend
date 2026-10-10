namespace ProjectManagementSystem.DTOs.Attachments;

public class AttachmentResponseDto
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public int TaskItemId { get; set; }

    public int UploadedById { get; set; }

    public string UploadedByName { get; set; } = string.Empty;
}