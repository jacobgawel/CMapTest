using FluentResults;

namespace CMapTest.Core.Errors;

public class ValidationError(string message) : Error(message);
