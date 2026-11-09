using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FinTrack.Infrastructure.Auth;

/// <summary>Seção "Jwt" da configuração.</summary>
public class JwtOptions
{
    public const string Secao = "Jwt";
    public const int ChaveTamanhoMinimo = 32;

    public string Issuer { get; set; } = "FinTrack.Api";
    public string Audience { get; set; } = "FinTrack.Api";

    /// <summary>Chave secreta HS256, com pelo menos 32 caracteres. Em produção, vem de variável de ambiente.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiracaoMinutos { get; set; } = 60;

    public SymmetricSecurityKey ObterChave() => new(Encoding.UTF8.GetBytes(Key));
}
