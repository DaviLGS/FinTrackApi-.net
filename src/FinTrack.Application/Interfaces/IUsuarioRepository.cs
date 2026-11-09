using FinTrack.Domain.Entities;

namespace FinTrack.Application.Interfaces;

public interface IUsuarioRepository
{
    /// <summary>O e-mail deve chegar já normalizado (sem espaços, em minúsculas).</summary>
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Lança <see cref="Common.Exceptions.ConflictException"/> se o e-mail já estiver cadastrado.</summary>
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
