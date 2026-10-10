using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.Projects;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services;
public class ProjectService
{
    private readonly AppDbContext _context;
    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, int userId)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            DueDate = dto.DueDate,
            CreatedById = userId
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        await _context.Entry(project)
            .Reference(p => p.CreatedBy)
            .LoadAsync();
        return MapToDto(project);
    }
    public async Task<List<ProjectResponseDto>> GetAllAsync(
        IReadOnlyCollection<int> accessibleProjectIds)
    {
        var projects = await _context.Projects
            .Include(p => p.CreatedBy)
            .Where(p => accessibleProjectIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return projects
            .Select(MapToDto)
            .ToList();
    }
    public async Task<ProjectResponseDto?> GetByIdAsync(int id)
    {
        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .FirstAsync(p => p.Id == id);
        if (project == null)
        {
            return null;
        }
        return MapToDto(project);
    }
    public async Task<ProjectResponseDto?> UpdateAsync(int id, UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return null;
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.DueDate = dto.DueDate;

        await _context.SaveChangesAsync();

        return MapToDto(project);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return false;
        }

        _context.Projects.Remove(project);

        await _context.SaveChangesAsync();

        return true;
    }
    private static ProjectResponseDto MapToDto(Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            DueDate = project.DueDate,
            CreatedById = project.CreatedById,
            CreatedByName = project.CreatedBy.FullName
        };
    }
}