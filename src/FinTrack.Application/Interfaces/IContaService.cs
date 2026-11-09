using FinTrack.Application.DTOs;

namespace FinTrack.Application.Interfaces;

public interface IContaService
{
    Task<IReadOnlyList<ContaDto>> ListarAsync(CancellationToken cancellationToken = default);
    Task<ContaSaldoDto> ObterSaldoAsync(int id, CancellationToken cancellationToken = default);
}
