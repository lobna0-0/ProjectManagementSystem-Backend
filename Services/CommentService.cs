using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.Comments;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services;

public class CommentService
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;
    public CommentService(
        AppDbContext context,
        NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }
    public async Task<CommentResponseDto> CreateAsync(int taskId, int userId, CreateCommentDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            throw new NotFoundException("User not found.");
        }
        var comment = new Comment
        {
            Content = dto.Content,
            TaskItemId = taskId,
            UserId = userId
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        if (task.AssignedToId.HasValue && task.AssignedToId != userId)
        {
            await _notificationService.CreateAsync(
                task.AssignedToId.Value,
                $"New comment added to task: {task.Title}");
        }

        return await GetByIdAsync(comment.Id)
            ?? throw new NotFoundException("Comment not found.");
    }
    public async Task<List<CommentResponseDto>> GetByTaskAsync(int taskId)
    {
        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
        if (!taskExists)
        {
            throw new NotFoundException("Task not found.");
        }

        var comments = await _context.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments
            .Select(MapToDto)
            .ToList();
    }
    public async Task<CommentResponseDto?> GetByIdAsync(
        int id)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return null;
        }

        return MapToDto(comment);
    }

    public async Task<CommentResponseDto> UpdateAsync(
        int id,
        int userId,
        UpdateCommentDto dto)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            throw new NotFoundException("Comment not found.");
        }

        if (comment.UserId != userId)
        {
            throw new BadRequestException(
                "You can only update your own comments.");
        }

        comment.Content = dto.Content;

        await _context.SaveChangesAsync();

        return MapToDto(comment);
    }

    public async Task DeleteAsync(
        int id,
        int userId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            throw new NotFoundException("Comment not found.");
        }

        if (comment.UserId != userId)
        {
            throw new BadRequestException(
                "You can only delete your own comments.");
        }

        _context.Comments.Remove(comment);

        await _context.SaveChangesAsync();
    }

    private static CommentResponseDto MapToDto(
        Comment comment)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            TaskItemId = comment.TaskItemId,
            UserId = comment.UserId,
            UserName = comment.User.FullName
        };
    }
}