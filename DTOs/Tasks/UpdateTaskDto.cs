using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.DTOs.Tasks;

public class UpdateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public Enums.TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public int? AssignedToId { get; set; }
}