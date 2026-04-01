using System.ComponentModel.DataAnnotations;

namespace CMapTest.Core.DTOs.Timesheet;

public class UpdateTimesheetRequest
{
    [Required] 
    public int UserId { get; set; }

    [Required] 
    public int ProjectId { get; set; }

    [Required] 
    public DateOnly Date { get; set; }

    [MaxLength(500)] 
    public string? Description { get; set; }

    [Required] 
    public TimeOnly StartTime { get; set; }

    [Required] 
    public TimeOnly EndTime { get; set; }
}