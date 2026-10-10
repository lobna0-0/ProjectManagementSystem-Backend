using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.Projects;
using ProjectManagementSystem.DTOs.Tasks;
using ProjectManagementSystem.Services;
using ProjectManagementSystem.DTOs.Dashboard;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ProjectAuthorizedController
{
    private readonly ProjectService _projectService;
    private readonly TaskService _taskService;
    private readonly DashboardService _dashboardService;
    public ProjectsController(
        ProjectService projectService,
        TaskService taskService,
        ProjectAuthorizationService projectAuthorizationService,
        DashboardService dashboardService)
        : base(projectAuthorizationService)
    {
        _projectService = projectService;
        _taskService = taskService;
        _dashboardService = dashboardService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectResponseDto>> Create(CreateProjectDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _projectService.CreateAsync(dto, userId);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPost("{projectId}/tasks")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(int projectId, CreateTaskDto dto)
    {
        await EnsureCanAccessProjectAsync(projectId);
        var result = await _taskService.CreateAsync(projectId, dto);
        if (result == null)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectResponseDto>>> GetAll()
    {
        var accessibleProjectIds = await GetAccessibleProjectIdsAsync();
        var result = await _projectService.GetAllAsync(accessibleProjectIds);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponseDto>> GetById(int id)
    {
        await EnsureCanAccessProjectAsync(id);
        var result = await _projectService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return Ok(result);
    }

    [HttpGet("{projectId}/tasks")]
    public async Task<ActionResult<List<TaskResponseDto>>> GetTasks(int projectId, [FromQuery] TaskQueryDto query)
    {
        await EnsureCanAccessProjectAsync(projectId);
        var result = await _taskService.GetByProjectAsync(projectId, query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectResponseDto>> Update(
        int id,
        UpdateProjectDto dto)
    {
        await EnsureCanAccessProjectAsync(id);
        var result = await _projectService.UpdateAsync(id, dto);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        await EnsureCanAccessProjectAsync(id);
        var deleted = await _projectService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return NoContent();
    }

    [HttpGet("{projectId}/statistics")]
    public async Task<
        ActionResult<ProjectStatisticsResponseDto>>
        GetStatistics(int projectId)
    {
        await EnsureCanAccessProjectAsync(projectId);
        var (userId, role) = GetCurrentUser();

        var result =
            await _dashboardService
                .GetProjectStatisticsAsync(
                    projectId,
                    userId,
                    role);

        return Ok(result);
    }
}