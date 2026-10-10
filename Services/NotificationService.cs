using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.Notifications;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services;
public class NotificationService
{
    private readonly AppDbContext _context;
    public NotificationService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        return notifications
            .Select(MapToDto)
            .ToList();
    }
    public async Task<int> GetUnreadCountAsync(
        int userId)
    {
        return await _context.Notifications
            .CountAsync(n =>
                n.UserId == userId &&
                !n.IsRead);
    }
    public async Task MarkAsReadAsync(
        int id,
        int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId);

        if (notification == null)
        {
            throw new NotFoundException(
                "Notification not found.");
        }

        notification.IsRead = true;

        await _context.SaveChangesAsync();
    }
    public async Task MarkAllAsReadAsync(
        int userId)
    {
        var notifications = await _context.Notifications
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }
public async Task CreateAsync(
        int userId,
        string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        int id,
        int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId);

        if (notification == null)
        {
            throw new NotFoundException(
                "Notification not found.");
        }

        _context.Notifications.Remove(notification);

        await _context.SaveChangesAsync();
    }

    private static NotificationResponseDto MapToDto(
        Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}