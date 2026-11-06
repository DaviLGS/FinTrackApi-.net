using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransacoesController(ITransacaoService transacaoService) : ControllerBase
{
    /// <summary>Lista transações com filtros opcionais e paginação (mais recentes primeiro).</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<TransacaoResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TransacaoResponseDto>>> Listar(
        [FromQuery] TransacaoFiltroDto filtro, CancellationToken cancellationToken) =>
        Ok(await transacaoService.ListarAsync(filtro, cancellationToken));

    /// <summary>Obtém uma transação pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TransacaoResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransacaoResponseDto>> ObterPorId(int id, CancellationToken cancellationToken) =>
        Ok(await transacaoService.ObterPorIdAsync(id, cancellationToken));

    /// <summary>Cria uma transação. O tipo deve ser compatível com o tipo da categoria.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<TransacaoResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TransacaoResponseDto>> Criar(
        [FromBody] TransacaoCreateDto dto, CancellationToken cancellationToken)
    {
        var criada = await transacaoService.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = criada.Id }, criada);
    }

    /// <summary>Substitui os dados de uma transação. Segue as mesmas regras da criação.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<TransacaoResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TransacaoResponseDto>> Atualizar(
        int id, [FromBody] TransacaoCreateDto dto, CancellationToken cancellationToken) =>
        Ok(await transacaoService.AtualizarAsync(id, dto, cancellationToken));

    /// <summary>Remove uma transação.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await transacaoService.RemoverAsync(id, cancellationToken);
        return NoContent();
    }
}
