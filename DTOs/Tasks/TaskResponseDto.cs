using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.DTOs.Tasks;

public class TaskResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Enums.TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public int? AssignedToId { get; set; }

    public string? AssignedToName { get; set; }
}