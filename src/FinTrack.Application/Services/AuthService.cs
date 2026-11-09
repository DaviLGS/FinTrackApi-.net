using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Validators;
using FinTrack.Domain.Entities;
using FluentValidation;

namespace FinTrack.Application.Services;

public class AuthService(
    IUsuarioRepository usuarioRepository,
    ISenhaHasher senhaHasher,
    ITokenService tokenService,
    TimeProvider timeProvider,
    RegistroValidator registroValidator,
    LoginValidator loginValidator) : IAuthService
{
    public async Task<UsuarioDto> RegistrarAsync(CredenciaisDto dto, CancellationToken cancellationToken = default)
    {
        await registroValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var email = NormalizarEmail(dto.Email);

        if (await usuarioRepository.ExisteEmailAsync(email, cancellationToken))
            throw new ConflictException("Já existe um usuário com este e-mail.");

        var usuario = new Usuario
        {
            Email = email,
            SenhaHash = senhaHasher.Hash(dto.Senha),
            CriadoEm = timeProvider.GetUtcNow().UtcDateTime
        };

        await usuarioRepository.AdicionarAsync(usuario, cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.Email, usuario.CriadoEm);
    }

    public async Task<TokenDto> LoginAsync(CredenciaisDto dto, CancellationToken cancellationToken = default)
    {
        await loginValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var usuario = await usuarioRepository.ObterPorEmailAsync(NormalizarEmail(dto.Email), cancellationToken);

        // Verifica mesmo quando o usuário não existe, para o tempo de resposta não revelar quais e-mails estão cadastrados
        if (!senhaHasher.Verificar(usuario?.SenhaHash, dto.Senha) || usuario is null)
            throw new CredenciaisInvalidasException();

        return tokenService.Gerar(usuario);
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
