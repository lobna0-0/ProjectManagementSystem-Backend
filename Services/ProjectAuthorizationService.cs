using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Enums;
using ProjectManagementSystem.Exceptions;

namespace ProjectManagementSystem.Services;

public class ProjectAuthorizationService
{
    private readonly AppDbContext _context;

    public ProjectAuthorizationService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAccessProjectAsync(
        int projectId,
        int userId,
        UserRoles role)
    {
        if (role == UserRoles.Admin)
        {
            return true;
        }

        if (role == UserRoles.Manager)
        {
            return await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.CreatedById == userId);
        }

        return await _context.ProjectMembers
            .AnyAsync(pm =>
                pm.ProjectId == projectId &&
                pm.UserId == userId);
    }

    public async Task<List<int>> GetAccessibleProjectIdsAsync(
        int userId,
        UserRoles role)
    {
        if (role == UserRoles.Admin)
        {
            return await _context.Projects
                .Select(p => p.Id)
                .ToListAsync();
        }

        if (role == UserRoles.Manager)
        {
            return await _context.Projects
                .Where(p => p.CreatedById == userId)
                .Select(p => p.Id)
                .ToListAsync();
        }

        return await _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync();
    }

    public async Task EnsureCanAccessProjectAsync(
        int projectId,
        int userId,
        UserRoles role)
    {
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == projectId);

        if (!projectExists)
        {
            throw new NotFoundException(
                "Project not found.");
        }

        var canAccess = await CanAccessProjectAsync(
            projectId,
            userId,
            role);

        if (!canAccess)
        {
            throw new BadRequestException(
                "You do not have access to this project.");
        }
    }

    public async Task EnsureCanAccessTaskAsync(
        int taskId,
        int userId,
        UserRoles role)
    {
        var projectId = await _context.Tasks
            .Where(t => t.Id == taskId)
            .Select(t => (int?)t.ProjectId)
            .FirstOrDefaultAsync();

        if (!projectId.HasValue)
        {
            throw new NotFoundException("Task not found.");
        }

        await EnsureCanAccessProjectAsync(projectId.Value, userId, role);
    }

    public async Task EnsureCanAccessCommentAsync(
        int commentId,
        int userId,
        UserRoles role)
    {
        var projectId = await _context.Comments
            .Where(c => c.Id == commentId)
            .Select(c => (int?)c.TaskItem.ProjectId)
            .FirstOrDefaultAsync();

        if (!projectId.HasValue)
        {
            throw new NotFoundException("Comment not found.");
        }

        await EnsureCanAccessProjectAsync(projectId.Value, userId, role);
    }

    public async Task EnsureCanAccessAttachmentAsync(
        int attachmentId,
        int userId,
        UserRoles role)
    {
        var projectId = await _context.Attachments
            .Where(a => a.Id == attachmentId)
            .Select(a => (int?)a.TaskItem.ProjectId)
            .FirstOrDefaultAsync();

        if (!projectId.HasValue)
        {
            throw new NotFoundException("Attachment not found.");
        }

        await EnsureCanAccessProjectAsync(projectId.Value, userId, role);
    }
}