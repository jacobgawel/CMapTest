namespace CMapTest.Core.DTOs.Timesheet;

public class ProjectHoursSummary
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
}
