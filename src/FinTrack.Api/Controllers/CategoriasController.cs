using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoriaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar(CancellationToken cancellationToken) =>
        Ok(await categoriaService.ListarAsync(cancellationToken));
}
