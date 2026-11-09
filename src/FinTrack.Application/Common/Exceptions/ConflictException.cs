namespace FinTrack.Application.Common.Exceptions;

/// <summary>O recurso já existe (mapeado para HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);
