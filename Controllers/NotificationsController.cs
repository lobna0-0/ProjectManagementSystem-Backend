using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.Notifications;
using ProjectManagementSystem.Services;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(
        NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<
        ActionResult<List<NotificationResponseDto>>> GetMyNotifications()
    {
        var userId = GetCurrentUserId();

        var result =
            await _notificationService
                .GetMyNotificationsAsync(userId);

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();

        var result =
            await _notificationService
                .GetUnreadCountAsync(userId);

        return Ok(new
        {
            unreadCount = result
        });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();

        await _notificationService.MarkAsReadAsync(
            id,
            userId);

        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();

        await _notificationService
            .MarkAllAsReadAsync(userId);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        await _notificationService.DeleteAsync(
            id,
            userId);

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        return int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);
    }
}