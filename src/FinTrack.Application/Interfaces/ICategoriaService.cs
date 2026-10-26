using FinTrack.Application.DTOs;

namespace FinTrack.Application.Interfaces;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken cancellationToken = default);
}
