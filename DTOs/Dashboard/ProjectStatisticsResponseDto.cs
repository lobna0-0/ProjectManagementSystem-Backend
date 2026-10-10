namespace ProjectManagementSystem.DTOs.Dashboard;

public class ProjectStatisticsResponseDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public int TotalTasks { get; set; }

    public int TodoTasks { get; set; }

    public int DoingTasks { get; set; }

    public int DoneTasks { get; set; }

    public int HighPriorityTasks { get; set; }

    public int OverdueTasks { get; set; }

    public int TotalMembers { get; set; }
}