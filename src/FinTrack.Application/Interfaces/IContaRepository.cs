using FinTrack.Domain.Entities;

namespace FinTrack.Application.Interfaces;

public interface IContaRepository
{
    Task<IReadOnlyList<Conta>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Conta?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
}
