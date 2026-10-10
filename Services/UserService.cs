using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.Users;
using ProjectManagementSystem.Enums;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return users
            .Select(MapToDto)
            .ToList();
    }

    public async Task<UserResponseDto> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        return MapToDto(user);
    }

    public async Task<UserResponseDto> UpdateAsync(
        int id,
        UpdateUserDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        var emailExists = await _context.Users
            .AnyAsync(u =>
                u.Email == dto.Email &&
                u.Id != id);

        if (emailExists)
        {
            throw new BadRequestException(
                "Email is already registered.");
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserResponseDto> ChangeRoleAsync(
        int id,
        ChangeUserRoleDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        user.Role = dto.Role;

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }

    private static UserResponseDto MapToDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };
    }
    public async Task ChangePasswordAsync(
        int id,
        ChangePasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        var currentPasswordValid =
            BCrypt.Net.BCrypt.Verify(
                dto.CurrentPassword,
                user.PasswordHash);

        if (!currentPasswordValid)
        {
            throw new BadRequestException(
                "Current password is incorrect.");
        }

        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(
                dto.NewPassword);

        await _context.SaveChangesAsync();
    }
    public async Task<List<UserResponseDto>> GetAssignableUsersAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return users
            .Select(MapToDto)
            .ToList();
    }
}