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
    [HttpGet("mensal")]
    [ProducesResponseType<RelatorioMensalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RelatorioMensalDto>> Mensal([FromQuery] RelatorioMensalFiltroDto filtro, CancellationToken cancellationToken) =>
        Ok(await relatorioService.GerarMensalAsync(filtro, cancellationToken));

    [HttpGet("anual")]
    [ProducesResponseType<RelatorioAnualDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RelatorioAnualDto>> Anual([FromQuery] RelatorioAnualFiltroDto filtro, CancellationToken cancellationToken) =>
        Ok(await relatorioService.GerarAnualAsync(filtro, cancellationToken));
}
