using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.DTOs.Tasks;

public class UpdateTaskStatusDto
{
    public ProjectManagementSystem.Enums.TaskStatus Status { get; set; }
}