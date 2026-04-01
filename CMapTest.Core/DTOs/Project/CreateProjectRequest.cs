using System.ComponentModel.DataAnnotations;

namespace CMapTest.Core.DTOs.Project;

public class CreateProjectRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
}
