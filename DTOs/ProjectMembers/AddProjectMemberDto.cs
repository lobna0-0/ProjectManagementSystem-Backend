using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DTOs.ProjectMembers;

public class AddProjectMemberDto
{
    [Required]
    public int UserId { get; set; }
}