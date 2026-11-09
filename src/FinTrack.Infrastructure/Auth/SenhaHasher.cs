using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FinTrack.Infrastructure.Auth;

/// <summary>Usa o PasswordHasher do ASP.NET Core Identity (PBKDF2 com salt aleatório).</summary>
public class SenhaHasher : ISenhaHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Usuario = new();

    // Hash de uma senha qualquer, usado para gastar o mesmo tempo quando o usuário não existe
    private static readonly Lazy<string> HashFicticio = new(() => Hasher.HashPassword(Usuario, Guid.NewGuid().ToString()));

    public string Hash(string senha) => Hasher.HashPassword(Usuario, senha);

    public bool Verificar(string? hash, string senha)
    {
        var resultado = Hasher.VerifyHashedPassword(Usuario, hash ?? HashFicticio.Value, senha);
        return hash is not null && resultado != PasswordVerificationResult.Failed;
    }
}
