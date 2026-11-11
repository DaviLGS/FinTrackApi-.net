using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FinTrack.Infrastructure.Auth;

public class SenhaHasher : ISenhaHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Usuario = new();

    private static readonly Lazy<string> HashFicticio = new(() => Hasher.HashPassword(Usuario, Guid.NewGuid().ToString()));

    public string Hash(string senha) => Hasher.HashPassword(Usuario, senha);

    public bool Verificar(string? hash, string senha)
    {
        var resultado = Hasher.VerifyHashedPassword(Usuario, hash ?? HashFicticio.Value, senha);
        return hash is not null && resultado != PasswordVerificationResult.Failed;
    }
}
