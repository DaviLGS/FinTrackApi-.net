using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Services;
using FinTrack.Application.Validators;
using FinTrack.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace FinTrack.Tests.Services;

public class AuthServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ISenhaHasher _senhaHasher = Substitute.For<ISenhaHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _senhaHasher.Hash(Arg.Any<string>()).Returns(call => $"hash:{call.Arg<string>()}");

        _service = new AuthService(
            _usuarioRepository,
            _senhaHasher,
            _tokenService,
            new FakeTimeProvider(Agora),
            new RegistroValidator(),
            new LoginValidator());
    }

    private static CredenciaisDto Credenciais(string email = "davi@exemplo.com", string senha = "senha-forte-123") =>
        new() { Email = email, Senha = senha };

    [Fact]
    public async Task RegistrarAsync_normaliza_email_guarda_so_o_hash_e_retorna_usuario()
    {
        _usuarioRepository
            .When(r => r.AdicionarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>()))
            .Do(call => call.Arg<Usuario>().Id = 3);

        var usuario = await _service.RegistrarAsync(Credenciais(email: "  Davi@Exemplo.COM "));

        await _usuarioRepository.Received(1).AdicionarAsync(
            Arg.Is<Usuario>(u =>
                u.Email == "davi@exemplo.com" &&
                u.SenhaHash == "hash:senha-forte-123" &&
                u.CriadoEm == Agora.UtcDateTime),
            Arg.Any<CancellationToken>());
        Assert.Equal(3, usuario.Id);
        Assert.Equal("davi@exemplo.com", usuario.Email);
    }

    [Fact]
    public async Task RegistrarAsync_com_email_ja_cadastrado_lanca_ConflictException()
    {
        _usuarioRepository.ExisteEmailAsync("davi@exemplo.com", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegistrarAsync(Credenciais(email: "DAVI@exemplo.com")));

        await _usuarioRepository.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, default);
    }

    [Theory]
    [InlineData("nao-e-email", "senha-forte-123")]
    [InlineData("", "senha-forte-123")]
    [InlineData("davi@exemplo.com", "curta")]
    [InlineData("davi@exemplo.com", "")]
    public async Task RegistrarAsync_com_dados_invalidos_lanca_ValidationException(string email, string senha)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _service.RegistrarAsync(Credenciais(email, senha)));

        await _usuarioRepository.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, default);
    }

    [Fact]
    public async Task LoginAsync_com_credenciais_corretas_retorna_token()
    {
        var usuario = new Usuario { Id = 3, Email = "davi@exemplo.com", SenhaHash = "hash-salvo" };
        var token = new TokenDto("jwt", "Bearer", Agora.UtcDateTime.AddHours(1));
        _usuarioRepository.ObterPorEmailAsync("davi@exemplo.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Usuario?>(usuario));
        _senhaHasher.Verificar("hash-salvo", "senha-forte-123").Returns(true);
        _tokenService.Gerar(usuario).Returns(token);

        var resultado = await _service.LoginAsync(Credenciais(email: " Davi@Exemplo.com"));

        Assert.Same(token, resultado);
    }

    [Fact]
    public async Task LoginAsync_com_senha_errada_lanca_CredenciaisInvalidasException()
    {
        _usuarioRepository.ObterPorEmailAsync("davi@exemplo.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Usuario?>(new Usuario { Id = 3, SenhaHash = "hash-salvo" }));
        _senhaHasher.Verificar("hash-salvo", Arg.Any<string>()).Returns(false);

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => _service.LoginAsync(Credenciais()));

        _tokenService.DidNotReceiveWithAnyArgs().Gerar(default!);
    }

    [Fact]
    public async Task LoginAsync_com_email_inexistente_verifica_senha_mesmo_assim_e_lanca_CredenciaisInvalidasException()
    {
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(() => _service.LoginAsync(Credenciais()));

        // Verificação com hash nulo: gasta o mesmo tempo de quando o usuário existe
        _senhaHasher.Received(1).Verificar(null, "senha-forte-123");
        _tokenService.DidNotReceiveWithAnyArgs().Gerar(default!);
    }
}
