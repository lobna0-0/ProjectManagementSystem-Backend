namespace ProjectManagementSystem.DTOs.ProjectMembers;

public class ProjectMemberResponseDto
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; }
}