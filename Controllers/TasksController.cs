using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.Tasks;
using ProjectManagementSystem.Services;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ProjectAuthorizedController
{
    private readonly TaskService _taskService;

    public TasksController(
        TaskService taskService,
        ProjectAuthorizationService projectAuthorizationService)
        : base(projectAuthorizationService)
    {
        _taskService = taskService;
    }


    // GET: api/Tasks
    [HttpGet]
    public async Task<ActionResult<List<TaskResponseDto>>> GetAll(
        [FromQuery] TaskQueryDto query)
    {
        var accessibleProjectIds =
            await GetAccessibleProjectIdsAsync();

        var result =
            await _taskService.GetAllAsync(
                accessibleProjectIds,
                query);

        return Ok(result);
    }


    // GET: api/Tasks/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponseDto>> GetById(
        int id)
    {
        var result =
            await _taskService.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        await EnsureCanAccessProjectAsync(
            result.ProjectId);

        return Ok(result);
    }


    // POST: api/Tasks
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TaskResponseDto>> Create(
        CreateTaskDto dto)
    {
        await EnsureCanAccessProjectAsync(
            dto.ProjectId);

        var result =
            await _taskService.CreateAsync(
                dto.ProjectId,
                dto);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }


    // PUT: api/Tasks/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TaskResponseDto>> Update(
        int id,
        UpdateTaskDto dto)
    {
        var existingTask =
            await _taskService.GetByIdAsync(id);

        if (existingTask == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        await EnsureCanAccessProjectAsync(
            existingTask.ProjectId);

        var result =
            await _taskService.UpdateAsync(
                id,
                dto);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        return Ok(result);
    }


    // PATCH: api/Tasks/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TaskResponseDto>> UpdateStatus(
        int id,
        UpdateTaskStatusDto dto)
    {
        var existingTask =
            await _taskService.GetByIdAsync(id);

        if (existingTask == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        await EnsureCanAccessProjectAsync(
            existingTask.ProjectId);

        var result =
            await _taskService.UpdateStatusAsync(
                id,
                dto.Status);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        return Ok(result);
    }


    // DELETE: api/Tasks/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var existingTask =
            await _taskService.GetByIdAsync(id);

        if (existingTask == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        await EnsureCanAccessProjectAsync(
            existingTask.ProjectId);

        var deleted =
            await _taskService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        return NoContent();
    }
}