using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.DTOs.Attachments;
using ProjectManagementSystem.Exceptions;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Services;
public class AttachmentService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedExtensions =
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".png",
        ".jpg",
        ".jpeg",
        ".txt"
    };

    private const long MaxFileSize = 10 * 1024 * 1024;
    public AttachmentService(
        AppDbContext context,
        IWebHostEnvironment environment
    )
    {
        _context = context;
        _environment = environment;
    }
    public async Task<AttachmentResponseDto> UploadAsync(
        int taskId,
        int userId,
        CreateAttachmentDto dto
    )
    {
        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
        if (!taskExists)
        {
            throw new NotFoundException("Task not found.");
        }

        var userExists = await _context.Users
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            throw new NotFoundException("User not found.");
        }
        if (dto.File == null || dto.File.Length == 0)
        {
            throw new BadRequestException(
                "File is required.");
        }

        if (dto.File.Length > MaxFileSize)
        {
            throw new BadRequestException(
                "File size cannot exceed 10 MB.");
        }
        var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException(
                "File type is not allowed.");
        }
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsFolder);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await dto.File.CopyToAsync(stream);
         var fileUrl =
            $"/uploads/{uniqueFileName}";

        var attachment = new Attachment
        {
            FileName = dto.File.FileName,
            FileUrl = fileUrl,
            TaskItemId = taskId,
            UploadedById = userId
        };

        _context.Attachments.Add(attachment);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(attachment.Id)
            ?? throw new NotFoundException(
                "Attachment not found.");
    }
    
    public async Task<List<AttachmentResponseDto>> GetByTaskAsync(
        int taskId)
    {
        var taskExists = await _context.Tasks
            .AnyAsync(t => t.Id == taskId);

        if (!taskExists)
        {
            throw new NotFoundException(
                "Task not found.");
        }

        var attachments = await _context.Attachments
            .AsNoTracking()
            .Include(a => a.UploadedBy)
            .Where(a => a.TaskItemId == taskId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();

        return attachments
            .Select(MapToDto)
            .ToList();
    }
    public async Task<AttachmentResponseDto?> GetByIdAsync(
        int id)
    {
        var attachment = await _context.Attachments
            .AsNoTracking()
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment == null)
        {
            return null;
        }

        return MapToDto(attachment);
    }

    public async Task DeleteAsync(
        int id,
        int userId)
    {
        var attachment = await _context.Attachments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment == null)
        {
            throw new NotFoundException(
                "Attachment not found.");
        }

        if (attachment.UploadedById != userId)
        {
            throw new BadRequestException(
                "You can only delete your own attachments.");
        }

        var filePath = Path.Combine(
            _environment.WebRootPath ?? "wwwroot",
            attachment.FileUrl.TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.Attachments.Remove(attachment);

        await _context.SaveChangesAsync();
    }

    private static AttachmentResponseDto MapToDto(
        Attachment attachment)
    {
        return new AttachmentResponseDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FileUrl = attachment.FileUrl,
            UploadedAt = attachment.UploadedAt,
            TaskItemId = attachment.TaskItemId,
            UploadedById = attachment.UploadedById,
            UploadedByName = attachment.UploadedBy.FullName
        };
    }
}