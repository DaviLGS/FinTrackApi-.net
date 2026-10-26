using FinTrack.Domain.Entities;

namespace FinTrack.Application.Interfaces;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Categoria?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
}
