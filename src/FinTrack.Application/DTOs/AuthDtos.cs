namespace FinTrack.Application.DTOs;

/// <summary>Corpo do cadastro e do login: apenas e-mail e senha.</summary>
public record CredenciaisDto
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

public record UsuarioDto(int Id, string Email, DateTime CriadoEm);

public record TokenDto(string AccessToken, string TokenType, DateTime ExpiraEm);
