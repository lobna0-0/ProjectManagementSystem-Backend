using ProjectManagementSystem.Enums;

namespace ProjectManagementSystem.Models;
public class User
{
    public int Id {get; set;}
    public string FullName {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string PasswordHash {get; set;} = string.Empty;
    public UserRoles Role {get; set;} = UserRoles.Member;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    // Navigation Properties
    public ICollection<Project> CreatedProjects { get; set; }
        = new List<Project>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; }
        = new List<ProjectMember>();
    public ICollection<TaskItem> AssignedTasks { get; set; }
        = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; }
        = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; }
        = new List<Attachment>();
    public ICollection<Notification> Notifications { get; set; }
        = new List<Notification>();
}