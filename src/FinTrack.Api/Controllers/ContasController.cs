using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContasController(IContaService contaService) : ControllerBase
{
    /// <summary>Lista as contas disponíveis.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ContaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContaDto>>> Listar(CancellationToken cancellationToken) =>
        Ok(await contaService.ListarAsync(cancellationToken));
}
