using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.DTOs.Users;

public class ChangeUserRoleDto
{
    [Required]
    public UserRoles Role { get; set; }
}