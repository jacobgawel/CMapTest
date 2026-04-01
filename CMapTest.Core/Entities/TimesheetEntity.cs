namespace CMapTest.Core.Entities;

public class TimesheetEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public UserEntity? User { get; set; }
    public ProjectEntity? Project { get; set; }
}