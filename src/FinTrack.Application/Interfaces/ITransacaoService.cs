using FinTrack.Application.Common;
using FinTrack.Application.DTOs;

namespace FinTrack.Application.Interfaces;

public interface ITransacaoService
{
    Task<TransacaoResponseDto> CriarAsync(TransacaoCreateDto dto, CancellationToken cancellationToken = default);
    Task<TransacaoResponseDto> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<TransacaoResponseDto>> ListarAsync(TransacaoFiltroDto filtro, CancellationToken cancellationToken = default);
}
