using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Cadastra um usuário com e-mail e senha (mínimo de 8 caracteres).</summary>
    [HttpPost("registrar")]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> Registrar([FromBody] CredenciaisDto dto, CancellationToken cancellationToken)
    {
        var usuario = await authService.RegistrarAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    /// <summary>Autentica com e-mail e senha e retorna o token JWT para as demais rotas.</summary>
    [HttpPost("login")]
    [ProducesResponseType<TokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> Login([FromBody] CredenciaisDto dto, CancellationToken cancellationToken) =>
        Ok(await authService.LoginAsync(dto, cancellationToken));
}
