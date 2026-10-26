namespace FinTrack.Application.Common.Exceptions;

/// <summary>Dados válidos no formato, mas que violam uma regra de negócio (mapeado para HTTP 422).</summary>
public class BusinessRuleException(string message) : Exception(message);
