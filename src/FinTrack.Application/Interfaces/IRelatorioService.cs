using FinTrack.Application.DTOs;

namespace FinTrack.Application.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioMensalDto> GerarMensalAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<RelatorioAnualDto> GerarAnualAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default);
}
