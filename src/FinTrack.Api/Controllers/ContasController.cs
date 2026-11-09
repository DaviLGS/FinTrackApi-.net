using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContasController(IContaService contaService) : ControllerBase
{
    /// <summary>Lista as contas disponíveis.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ContaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContaDto>>> Listar(CancellationToken cancellationToken) =>
        Ok(await contaService.ListarAsync(cancellationToken));

    /// <summary>Calcula o saldo da conta: saldo inicial + receitas - despesas.</summary>
    [HttpGet("{id:int}/saldo")]
    [ProducesResponseType<ContaSaldoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContaSaldoDto>> ObterSaldo(int id, CancellationToken cancellationToken) =>
        Ok(await contaService.ObterSaldoAsync(id, cancellationToken));
}
