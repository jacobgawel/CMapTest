using FluentResults;

namespace CMapTest.Core.Errors;

public class ConflictError(string message) : Error(message);
