using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DTOs.Projects;

public class UpdateProjectDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
}