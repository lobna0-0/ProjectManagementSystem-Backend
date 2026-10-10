using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Enums;
using ProjectManagementSystem.Services;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ProjectAuthorizedController : ControllerBase
{
    private readonly ProjectAuthorizationService _projectAuthorizationService;

    protected ProjectAuthorizedController(
        ProjectAuthorizationService projectAuthorizationService)
    {
        _projectAuthorizationService = projectAuthorizationService;
    }

    protected Task EnsureCanAccessProjectAsync(int projectId)
    {
        var (userId, role) = GetCurrentUser();
        return _projectAuthorizationService.EnsureCanAccessProjectAsync(
            projectId,
            userId,
            role);
    }

    protected Task<List<int>> GetAccessibleProjectIdsAsync()
    {
        var (userId, role) = GetCurrentUser();
        return _projectAuthorizationService.GetAccessibleProjectIdsAsync(
            userId,
            role);
    }

    protected Task EnsureCanAccessTaskAsync(int taskId)
    {
        var (userId, role) = GetCurrentUser();
        return _projectAuthorizationService.EnsureCanAccessTaskAsync(
            taskId,
            userId,
            role);
    }

    protected Task EnsureCanAccessCommentAsync(int commentId)
    {
        var (userId, role) = GetCurrentUser();
        return _projectAuthorizationService.EnsureCanAccessCommentAsync(
            commentId,
            userId,
            role);
    }

    protected Task EnsureCanAccessAttachmentAsync(int attachmentId)
    {
        var (userId, role) = GetCurrentUser();
        return _projectAuthorizationService.EnsureCanAccessAttachmentAsync(
            attachmentId,
            userId,
            role);
    }

    protected (int UserId, UserRoles Role) GetCurrentUser()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = Enum.Parse<UserRoles>(
            User.FindFirstValue(ClaimTypes.Role)!);

        return (userId, role);
    }
}