using FluentResults;

namespace CMapTest.Core.Errors;

public class NotFoundError(string message) : Error(message);
