using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.ProjectMembers;
using ProjectManagementSystem.Services;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/projects/{projectId}/members")]
[Authorize]
public class ProjectMembersController : ProjectAuthorizedController
{
    private readonly ProjectMemberService _memberService;
    public ProjectMembersController(
        ProjectMemberService memberService,
        ProjectAuthorizationService projectAuthorizationService)
        : base(projectAuthorizationService)
    {
        _memberService = memberService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectMemberResponseDto>> AddMember(int projectId, AddProjectMemberDto dto)
    {
        await EnsureCanAccessProjectAsync(projectId);
        var result = await _memberService.AddMemeberAsync(projectId, dto);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectMemberResponseDto>>> GetMembers(int projectId)
    {
        await EnsureCanAccessProjectAsync(projectId);
        var result = await _memberService.GetMembersAsync(projectId);
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveMember(int projectId, int userId)
    {
        await EnsureCanAccessProjectAsync(projectId);
        await _memberService.RemoveMemberAsync(projectId, userId);
        return NoContent();
    }
}