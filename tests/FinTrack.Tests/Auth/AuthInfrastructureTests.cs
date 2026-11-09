using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FinTrack.Tests.Auth;

public class AuthInfrastructureTests
{
    private static readonly JwtOptions Opcoes = new()
    {
        Issuer = "FinTrack.Api",
        Audience = "FinTrack.Api",
        Key = "chave-de-teste-com-mais-de-32-caracteres",
        ExpiracaoMinutos = 60
    };

    [Fact]
    public void SenhaHasher_gera_hash_diferente_da_senha_e_confere_somente_a_senha_certa()
    {
        var hasher = new SenhaHasher();

        var hash = hasher.Hash("senha-forte-123");

        Assert.NotEqual("senha-forte-123", hash);
        Assert.True(hasher.Verificar(hash, "senha-forte-123"));
        Assert.False(hasher.Verificar(hash, "senha-errada"));
        Assert.False(hasher.Verificar(null, "senha-forte-123"));
    }

    [Fact]
    public void SenhaHasher_usa_salt_e_gera_hashes_diferentes_para_a_mesma_senha()
    {
        var hasher = new SenhaHasher();

        Assert.NotEqual(hasher.Hash("senha-forte-123"), hasher.Hash("senha-forte-123"));
    }

    [Fact]
    public async Task JwtTokenService_gera_token_assinado_com_o_usuario_e_a_expiracao()
    {
        var agora = DateTimeOffset.UtcNow;
        var service = new JwtTokenService(Options.Create(Opcoes), new FakeTimeProvider(agora));

        var token = service.Gerar(new Usuario { Id = 42, Email = "davi@exemplo.com" });

        var validacao = await new JsonWebTokenHandler().ValidateTokenAsync(token.AccessToken, new TokenValidationParameters
        {
            ValidIssuer = Opcoes.Issuer,
            ValidAudience = Opcoes.Audience,
            IssuerSigningKey = Opcoes.ObterChave()
        });

        Assert.True(validacao.IsValid, validacao.Exception?.Message);
        Assert.Equal("42", validacao.Claims[JwtRegisteredClaimNames.Sub]);
        Assert.Equal("davi@exemplo.com", validacao.Claims[JwtRegisteredClaimNames.Email]);
        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal(agora.UtcDateTime.AddMinutes(60), token.ExpiraEm);
    }

    [Fact]
    public async Task JwtTokenService_token_nao_vale_com_outra_chave()
    {
        var service = new JwtTokenService(Options.Create(Opcoes), TimeProvider.System);
        var token = service.Gerar(new Usuario { Id = 42, Email = "davi@exemplo.com" });

        var validacao = await new JsonWebTokenHandler().ValidateTokenAsync(token.AccessToken, new TokenValidationParameters
        {
            ValidIssuer = Opcoes.Issuer,
            ValidAudience = Opcoes.Audience,
            IssuerSigningKey = new JwtOptions { Key = "outra-chave-qualquer-com-mais-de-32-caracteres" }.ObterChave()
        });

        Assert.False(validacao.IsValid);
    }
}
