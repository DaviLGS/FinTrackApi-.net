using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class RelatoriosController(IRelatorioService relatorioService) : ControllerBase
{
    /// <summary>Despesas e receitas por categoria no mês (ex.: ?ano=2026&amp;mes=10).</summary>
    [HttpGet("mensal")]
    [ProducesResponseType<RelatorioMensalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RelatorioMensalDto>> Mensal([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken) =>
        Ok(await relatorioService.GerarMensalAsync(filtro, cancellationToken));

    /// <summary>Despesas e receitas por categoria no ano, com o resumo de cada mês (ex.: ?ano=2026).</summary>
    [HttpGet("anual")]
    [ProducesResponseType<RelatorioAnualDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RelatorioAnualDto>> Anual([FromQuery] RelatorioFiltroDto filtro, CancellationToken cancellationToken) =>
        Ok(await relatorioService.GerarAnualAsync(filtro, cancellationToken));
}
