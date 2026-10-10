using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.ProjectMembers;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services;
public class ProjectMemberService
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;
    public ProjectMemberService(
        AppDbContext context,
        NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }
    public async Task<ProjectMemberResponseDto> AddMemeberAsync(int projectId, AddProjectMemberDto dto)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
        {
            throw new NotFoundException(
                "Project not found.");
        }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }
        var alreadyMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == dto.UserId);
        if (alreadyMember)
        {
            throw new BadRequestException(
                "User is already a member of this project.");
        }
        var projectMember = new ProjectMember
        {
            ProjectId = projectId,
            UserId = dto.UserId
        };
        _context.ProjectMembers.Add(projectMember);
        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            user.Id,
            $"You have been added to project: {project.Name}");

        return new ProjectMemberResponseDto
        {
            Id = projectMember.Id,
            ProjectId = projectMember.ProjectId,
            UserId = user.Id,
            UserName = user.FullName,
            UserEmail = user.Email,
            Role = user.Role.ToString(),
            JoinedAt = projectMember.JoinedAt
        };
    }
    public async Task<List<ProjectMemberResponseDto>> GetMembersAsync(int projectId)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
        if (!projectExists)
        {
            throw new NotFoundException(
                "Project not found.");
        }
        var members = await _context.ProjectMembers
            .AsNoTracking()
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .OrderBy(pm => pm.JoinedAt)
            .ToListAsync();
        return members
            .Select(pm => new ProjectMemberResponseDto
            {
                Id = pm.Id,
                ProjectId = pm.ProjectId,
                UserId = pm.UserId,
                UserName = pm.User.FullName,
                UserEmail = pm.User.Email,
                Role = pm.User.Role.ToString(),
                JoinedAt = pm.JoinedAt
            })
            .ToList();
    }
    public async Task RemoveMemberAsync(
        int projectId,
        int userId)
    {
        var member = await _context.ProjectMembers
            .Include(pm => pm.Project)
            .FirstOrDefaultAsync(pm =>
                pm.ProjectId == projectId &&
                pm.UserId == userId);

        if (member == null)
        {
            throw new NotFoundException(
                "Project member not found.");
        }

        _context.ProjectMembers.Remove(member);

        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            userId,
            $"You have been removed from project: {member.Project.Name}");
    }
}