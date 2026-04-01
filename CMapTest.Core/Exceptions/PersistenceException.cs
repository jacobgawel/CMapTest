namespace CMapTest.Core.Exceptions;

public class PersistenceException(string message, Exception innerException) : Exception(message, innerException);
