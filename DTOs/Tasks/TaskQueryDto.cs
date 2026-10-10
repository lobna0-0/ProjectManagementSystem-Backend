using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.DTOs.Tasks;

public class TaskQueryDto
{
    public string? Search { get; set; }

    public Enums.TaskStatus? Status { get; set; }

    public TaskPriority? Priority { get; set; }

    public string SortBy { get; set; } = "createdAt";

    public bool Descending { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}