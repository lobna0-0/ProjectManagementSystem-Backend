namespace ProjectManagementSystem.DTOs.Projects;

public class ProjectResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public int CreatedById { get; set; }

    public string CreatedByName { get; set; } = string.Empty;
}