namespace ProjectManagementSystem.DTOs.Dashboard;

public class DashboardResponseDto
{
    public int TotalProjects { get; set; }

    public int TotalTasks { get; set; }

    public int TodoTasks { get; set; }

    public int DoingTasks { get; set; }

    public int DoneTasks { get; set; }

    public int HighPriorityTasks { get; set; }

    public int OverdueTasks { get; set; }

    public int MyAssignedTasks { get; set; }

    public int UnreadNotifications { get; set; }
}