using FinTrack.Application.DTOs;

namespace FinTrack.Application.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioMensalDto> GerarMensalAsync(RelatorioMensalFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<RelatorioAnualDto> GerarAnualAsync(RelatorioAnualFiltroDto filtro, CancellationToken cancellationToken = default);
}
