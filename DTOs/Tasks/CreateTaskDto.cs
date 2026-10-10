using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.DTOs.Tasks;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TaskPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public int? AssignedToId { get; set; }

    public int ProjectId { get; set; }
}