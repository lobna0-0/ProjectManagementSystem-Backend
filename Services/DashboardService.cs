using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.Dashboard;
using ProjectManagementSystem.Enums;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services;

public class DashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponseDto> GetDashboardAsync(
        int userId,
        UserRoles role)
    {
        IQueryable<Project> projectsQuery =
            _context.Projects.AsNoTracking();

        IQueryable<TaskItem> tasksQuery =
            _context.Tasks.AsNoTracking();

        // تحديد المشاريع التي يستطيع المستخدم الوصول إليها
        if (role == UserRoles.Manager)
        {
            projectsQuery = projectsQuery
                .Where(p => p.CreatedById == userId);
        }
        else if (role == UserRoles.Member)
        {
            projectsQuery = projectsQuery
                .Where(p =>
                    p.Members.Any(pm =>
                        pm.UserId == userId));
        }

        var projectIds = await projectsQuery
            .Select(p => p.Id)
            .ToListAsync();

        tasksQuery = tasksQuery
            .Where(t => projectIds.Contains(t.ProjectId));

        var now = DateTime.UtcNow;

        return new DashboardResponseDto
        {
            TotalProjects = projectIds.Count,

            TotalTasks = await tasksQuery.CountAsync(),

            TodoTasks = await tasksQuery.CountAsync(
                t => t.Status == ProjectManagementSystem.Enums.TaskStatus.Todo),

            DoingTasks = await tasksQuery.CountAsync(
                t => t.Status == ProjectManagementSystem.Enums.TaskStatus.Doing),

            DoneTasks = await tasksQuery.CountAsync(
                t => t.Status == ProjectManagementSystem.Enums.TaskStatus.Done),

            HighPriorityTasks = await tasksQuery.CountAsync(
                t => t.Priority == TaskPriority.High),

            OverdueTasks = await tasksQuery.CountAsync(
                t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value < now &&
                    t.Status != ProjectManagementSystem.Enums.TaskStatus.Done),

            MyAssignedTasks = await _context.Tasks
                .AsNoTracking()
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    projectIds.Contains(t.ProjectId)),

            UnreadNotifications = await _context.Notifications
                .AsNoTracking()
                .CountAsync(n =>
                    n.UserId == userId &&
                    !n.IsRead)
        };
    }
    public async Task<ProjectStatisticsResponseDto> GetProjectStatisticsAsync(
    int projectId,
    int userId,
    UserRoles role)
{
    var project = await _context.Projects
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == projectId);

    if (project == null)
    {
        throw new NotFoundException(
            "Project not found.");
    }

    var canAccess =
        role == UserRoles.Admin ||
        (role == UserRoles.Manager &&
         project.CreatedById == userId) ||
        (role == UserRoles.Member &&
         await _context.ProjectMembers.AnyAsync(pm =>
             pm.ProjectId == projectId &&
             pm.UserId == userId));

    if (!canAccess)
    {
        throw new BadRequestException(
            "You do not have access to this project.");
    }

    var tasksQuery = _context.Tasks
        .AsNoTracking()
        .Where(t => t.ProjectId == projectId);

    var now = DateTime.UtcNow;

    return new ProjectStatisticsResponseDto
    {
        ProjectId = project.Id,

        ProjectName = project.Name,

        TotalTasks = await tasksQuery.CountAsync(),

        TodoTasks = await tasksQuery.CountAsync(
            t => t.Status == ProjectManagementSystem.Enums.TaskStatus.Todo),

        DoingTasks = await tasksQuery.CountAsync(
            t => t.Status == ProjectManagementSystem.Enums.TaskStatus.Doing),

        DoneTasks = await tasksQuery.CountAsync(
            t => t.Status == ProjectManagementSystem.Enums.TaskStatus.Done),

        HighPriorityTasks = await tasksQuery.CountAsync(
            t => t.Priority == TaskPriority.High),

        OverdueTasks = await tasksQuery.CountAsync(
            t =>
                t.DueDate.HasValue &&
                t.DueDate.Value < now &&
                t.Status != ProjectManagementSystem.Enums.TaskStatus.Done),

        TotalMembers = await _context.ProjectMembers
            .CountAsync(pm =>
                pm.ProjectId == projectId)
    };
}
}