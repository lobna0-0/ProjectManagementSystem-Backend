using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DTOs.Attachments;

public class CreateAttachmentDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}