namespace FinTrack.Application.Common.Exceptions;

/// <summary>
/// E-mail ou senha incorretos (mapeado para HTTP 401). A mensagem é a mesma nos dois casos,
/// para não revelar quais e-mails estão cadastrados.
/// </summary>
public class CredenciaisInvalidasException() : Exception("E-mail ou senha inválidos.");
