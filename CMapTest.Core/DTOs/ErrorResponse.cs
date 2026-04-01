namespace CMapTest.Core.DTOs;

public class ErrorResponse
{
    public required string Type { get; set; }
    public required List<string> Errors { get; set; }
}
