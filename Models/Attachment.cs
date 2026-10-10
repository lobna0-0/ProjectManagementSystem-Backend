namespace ProjectManagementSystem.Models;
public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int TaskItemId { get; set; }
    public int UploadedById { get; set; }
    // Navigation Properties
    public TaskItem TaskItem {get; set;} = null!;
    public User UploadedBy {get; set;} = null!;
}