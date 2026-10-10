using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.DTOs.Users;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Services;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserResponseDto>>> GetAll()
    {
        var result = await _userService.GetAllAsync();

        return Ok(result);
    }
    
    [HttpGet("assignable")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<UserResponseDto>>> GetAssignableUsers()
    {
        var result = await _userService.GetAssignableUsersAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponseDto>> Update(
        int id,
        UpdateUserDto dto)
    {
        var result = await _userService.UpdateAsync(
            id,
            dto);

        return Ok(result);
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponseDto>> ChangeRole(
        int id,
        ChangeUserRoleDto dto)
    {
        var result = await _userService.ChangeRoleAsync(
            id,
            dto);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        if (currentUserId == id)
        {
            throw new BadRequestException(
                "You cannot delete your own account.");
        }

        await _userService.DeleteAsync(id);

        return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordDto dto)
    {
        var userId = int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        await _userService.ChangePasswordAsync(
            userId,
            dto);

        return Ok(new
        {
            message = "Password changed successfully."
        });
    }
}