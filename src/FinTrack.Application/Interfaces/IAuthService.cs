using FinTrack.Application.DTOs;

namespace FinTrack.Application.Interfaces;

public interface IAuthService
{
    Task<UsuarioDto> RegistrarAsync(CredenciaisDto dto, CancellationToken cancellationToken = default);
    Task<TokenDto> LoginAsync(CredenciaisDto dto, CancellationToken cancellationToken = default);
}
