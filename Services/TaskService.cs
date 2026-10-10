using ProjectManagementSystem.DTOs.Common;
// using ProjectManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.Tasks;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Models;
using ProjectTaskStatus = ProjectManagementSystem.Enums.TaskStatus;

namespace ProjectManagementSystem.Services;

public class TaskService
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;

    public TaskService(
        AppDbContext context,
        NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }


    // =========================================================
    // Create Task
    // =========================================================

    public async Task<TaskResponseDto?> CreateAsync(
        int projectId,
        CreateTaskDto dto)
    {
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == projectId);

        if (!projectExists)
        {
            return null;
        }

        if (dto.AssignedToId.HasValue)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == dto.AssignedToId.Value);

            if (!userExists)
            {
                throw new BadRequestException(
                    "Assigned user not found.");
            }
        }

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            AssignedToId = dto.AssignedToId,
            ProjectId = projectId
        };

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        if (task.AssignedToId.HasValue)
        {
            await _notificationService.CreateAsync(
                task.AssignedToId.Value,
                $"You have been assigned to task: {task.Title}");
        }

        return await GetByIdAsync(task.Id);
    }


    // =========================================================
    // Get All Tasks
    // =========================================================

    public async Task<PaginatedResponseDto<TaskResponseDto>> GetAllAsync(
        IEnumerable<int> accessibleProjectIds,
        TaskQueryDto query)
    {
        var projectIds = accessibleProjectIds.ToList();

        var tasksQuery = _context.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Where(t => projectIds.Contains(t.ProjectId));

        return await BuildPaginatedResultAsync(
            tasksQuery,
            query);
    }


    // =========================================================
    // Get Tasks By Project
    // =========================================================

    public async Task<PaginatedResponseDto<TaskResponseDto>> GetByProjectAsync(
        int projectId,
        TaskQueryDto query)
    {
        var tasksQuery = _context.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Where(t => t.ProjectId == projectId);

        return await BuildPaginatedResultAsync(
            tasksQuery,
            query);
    }


    // =========================================================
    // Get Task By ID
    // =========================================================

    public async Task<TaskResponseDto?> GetByIdAsync(int id)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return null;
        }

        return MapToDto(task);
    }


    // =========================================================
    // Update Task
    // =========================================================

    public async Task<TaskResponseDto?> UpdateAsync(
        int id,
        UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return null;
        }

        if (dto.AssignedToId.HasValue)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == dto.AssignedToId.Value);

            if (!userExists)
            {
                throw new BadRequestException(
                    "Assigned user not found.");
            }
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.AssignedToId = dto.AssignedToId;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(task.Id);
    }


    // =========================================================
    // Update Task Status
    // =========================================================

    public async Task<TaskResponseDto?> UpdateStatusAsync(
    int id,
    ProjectTaskStatus status)
{
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id);

    if (task == null)
        return null;

    task.Status = status;

    await _context.SaveChangesAsync();

    return await GetByIdAsync(task.Id);
}


    // =========================================================
    // Delete Task
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // Filtering + Sorting + Pagination
    // =========================================================

    private static async Task<
        PaginatedResponseDto<TaskResponseDto>>
        BuildPaginatedResultAsync(
            IQueryable<TaskItem> tasksQuery,
            TaskQueryDto query)
    {
        // -------------------------
        // Search
        // -------------------------

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            tasksQuery = tasksQuery.Where(t =>
                t.Title.Contains(query.Search) ||
                t.Description.Contains(query.Search));
        }


        // -------------------------
        // Filter by Status
        // -------------------------

        if (query.Status.HasValue)
        {
            tasksQuery = tasksQuery.Where(t =>
                t.Status == query.Status.Value);
        }


        // -------------------------
        // Filter by Priority
        // -------------------------

        if (query.Priority.HasValue)
        {
            tasksQuery = tasksQuery.Where(t =>
                t.Priority == query.Priority.Value);
        }


        // -------------------------
        // Sorting
        // -------------------------

        tasksQuery = query.SortBy.ToLower() switch
        {
            "title" => query.Descending
                ? tasksQuery.OrderByDescending(t => t.Title)
                : tasksQuery.OrderBy(t => t.Title),

            "duedate" => query.Descending
                ? tasksQuery.OrderByDescending(t => t.DueDate)
                : tasksQuery.OrderBy(t => t.DueDate),

            "priority" => query.Descending
                ? tasksQuery.OrderByDescending(t => t.Priority)
                : tasksQuery.OrderBy(t => t.Priority),

            "status" => query.Descending
                ? tasksQuery.OrderByDescending(t => t.Status)
                : tasksQuery.OrderBy(t => t.Status),

            _ => query.Descending
                ? tasksQuery.OrderByDescending(t => t.CreatedAt)
                : tasksQuery.OrderBy(t => t.CreatedAt)
        };


        // -------------------------
        // Pagination
        // -------------------------

        var totalItems =
            await tasksQuery.CountAsync();

        var page = query.Page < 1
            ? 1
            : query.Page;

        var pageSize = query.PageSize < 1
            ? 10
            : Math.Min(query.PageSize, 50);

        var tasks = await tasksQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages =
            (int)Math.Ceiling(
                totalItems / (double)pageSize);


        return new PaginatedResponseDto<TaskResponseDto>
        {
            Data = tasks
                .Select(MapToDto)
                .ToList(),

            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }


    // =========================================================
    // Mapping
    // =========================================================

    private static TaskResponseDto MapToDto(
        TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,

            ProjectId = task.ProjectId,
            ProjectName = task.Project.Name,

            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo?.FullName
        };
    }
}