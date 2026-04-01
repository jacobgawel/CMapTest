namespace CMapTest.Core.DTOs.Timesheet;

public class TimesheetResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal HoursWorked { get; set; }
    public string Duration { get; set; } = string.Empty;
}
