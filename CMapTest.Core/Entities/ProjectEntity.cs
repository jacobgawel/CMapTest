namespace CMapTest.Core.Entities;

public class ProjectEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public ICollection<TimesheetEntity> Timesheets { get; set; } = [];
}