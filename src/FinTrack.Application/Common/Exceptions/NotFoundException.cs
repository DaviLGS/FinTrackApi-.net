namespace FinTrack.Application.Common.Exceptions;

/// <summary>Recurso solicitado não existe (mapeado para HTTP 404).</summary>
public class NotFoundException(string recurso, object chave)
    : Exception($"{recurso} com id '{chave}' não foi encontrado(a).");
