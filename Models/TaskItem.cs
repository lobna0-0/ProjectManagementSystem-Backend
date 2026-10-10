using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.Models;
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Foreign Keys
    public int ProjectId { get; set; }
    public int? AssignedToId { get; set; }
    // Navigation Properties
    public Project Project { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public ICollection<Comment> Comments { get; set; }
        = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; }
        = new List<Attachment>();
}