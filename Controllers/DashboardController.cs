using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.Dashboard;
using ProjectManagementSystem.Enums;
using ProjectManagementSystem.Services;
using System.Security.Claims;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(
        DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponseDto>>
        GetDashboard()
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        var result =
            await _dashboardService.GetDashboardAsync(
                userId,
                role);

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        return int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);
    }

    private UserRoles GetCurrentUserRole()
    {
        return Enum.Parse<UserRoles>(
            User.FindFirstValue(
                ClaimTypes.Role)!);
    }
}