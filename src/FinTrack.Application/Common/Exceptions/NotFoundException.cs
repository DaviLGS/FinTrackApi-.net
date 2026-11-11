namespace FinTrack.Application.Common.Exceptions;

public class NotFoundException(string recurso, object chave)
    : Exception($"{recurso} com id '{chave}' não foi encontrado(a).");
