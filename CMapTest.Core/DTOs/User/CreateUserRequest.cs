using System.ComponentModel.DataAnnotations;

namespace CMapTest.Core.DTOs.User;

public class CreateUserRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
}
