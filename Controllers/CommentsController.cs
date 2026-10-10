using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.Comments;
using ProjectManagementSystem.Services;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class CommentsController : ProjectAuthorizedController
{
    private readonly CommentService _commentService;

    public CommentsController(
        CommentService commentService,
        ProjectAuthorizationService projectAuthorizationService)
        : base(projectAuthorizationService)
    {
        _commentService = commentService;
    }

    [HttpPost("tasks/{taskId}/comments")]
    public async Task<ActionResult<CommentResponseDto>> Create(
        int taskId,
        CreateCommentDto dto)
    {
        await EnsureCanAccessTaskAsync(taskId);
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _commentService.CreateAsync(
            taskId,
            userId,
            dto);

        return Ok(result);
    }

    [HttpGet("tasks/{taskId}/comments")]
    public async Task<ActionResult<List<CommentResponseDto>>> GetByTask(
        int taskId)
    {
        await EnsureCanAccessTaskAsync(taskId);
        var result = await _commentService.GetByTaskAsync(taskId);

        return Ok(result);
    }

    [HttpPut("comments/{id}")]
    public async Task<ActionResult<CommentResponseDto>> Update(
        int id,
        UpdateCommentDto dto)
    {
        await EnsureCanAccessCommentAsync(id);
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _commentService.UpdateAsync(
            id,
            userId,
            dto);

        return Ok(result);
    }

    [HttpDelete("comments/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await EnsureCanAccessCommentAsync(id);
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _commentService.DeleteAsync(id, userId);

        return NoContent();
    }
}