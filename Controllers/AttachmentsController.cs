using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.Attachments;
using ProjectManagementSystem.Services;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AttachmentsController : ProjectAuthorizedController
{
    private readonly AttachmentService _attachmentService;

    public AttachmentsController(
        AttachmentService attachmentService,
        ProjectAuthorizationService projectAuthorizationService)
        : base(projectAuthorizationService)
    {
        _attachmentService = attachmentService;
    }

    [HttpPost("tasks/{taskId}/attachments")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<AttachmentResponseDto>> Upload(
        int taskId,
        [FromForm] CreateAttachmentDto dto)
    {
        await EnsureCanAccessTaskAsync(taskId);
        var userId = int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        var result = await _attachmentService.UploadAsync(
            taskId,
            userId,
            dto);

        return Ok(result);
    }

    [HttpGet("tasks/{taskId}/attachments")]
    public async Task<
        ActionResult<List<AttachmentResponseDto>>> GetByTask(
        int taskId)
    {
        await EnsureCanAccessTaskAsync(taskId);
        var result =
            await _attachmentService.GetByTaskAsync(taskId);

        return Ok(result);
    }

    [HttpDelete("attachments/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await EnsureCanAccessAttachmentAsync(id);
        var userId = int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        await _attachmentService.DeleteAsync(
            id,
            userId);

        return NoContent();
    }
}