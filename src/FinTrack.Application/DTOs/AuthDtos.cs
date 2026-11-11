namespace FinTrack.Application.DTOs;

public record CredenciaisDto
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

public record UsuarioDto(int Id, string Email, DateTime CriadoEm);

public record TokenDto(string AccessToken, string TokenType, DateTime ExpiraEm);
