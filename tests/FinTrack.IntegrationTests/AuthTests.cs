using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinTrack.Application.DTOs;
using FinTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.IntegrationTests;

public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private Task<HttpResponseMessage> Registrar(string email, string senha = ApiFactory.Senha) =>
        _client.PostAsJsonAsync("/api/auth/registrar", new CredenciaisDto { Email = email, Senha = senha }, ApiFactory.Json);

    private Task<HttpResponseMessage> Login(string email, string senha = ApiFactory.Senha) =>
        _client.PostAsJsonAsync("/api/auth/login", new CredenciaisDto { Email = email, Senha = senha }, ApiFactory.Json);

    [Fact]
    public async Task Registrar_retorna_201_com_o_usuario_sem_expor_a_senha()
    {
        var email = ApiFactory.NovoEmail();

        var resposta = await Registrar(email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("senha", corpo, StringComparison.OrdinalIgnoreCase);
        var usuario = await resposta.Content.ReadFromJsonAsync<UsuarioDto>(ApiFactory.Json);
        Assert.Equal(email, usuario!.Email);
    }

    [Fact]
    public async Task Registrar_email_repetido_retorna_409_mesmo_com_maiusculas()
    {
        var email = ApiFactory.NovoEmail();
        await Registrar(email);

        var resposta = await Registrar($"  {email.ToUpperInvariant()} ");

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Theory]
    [InlineData("nao-e-email", ApiFactory.Senha)]
    [InlineData("valido@exemplo.com", "curta")]
    public async Task Registrar_invalido_retorna_400(string email, string senha)
    {
        var resposta = await Registrar(email, senha);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_com_credenciais_corretas_retorna_token_que_acessa_a_api()
    {
        var email = ApiFactory.NovoEmail();
        await Registrar(email);

        var resposta = await Login(email);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var token = await resposta.Content.ReadFromJsonAsync<TokenDto>(ApiFactory.Json);
        Assert.Equal("Bearer", token!.TokenType);
        Assert.True(token.ExpiraEm > DateTime.UtcNow);

        var autenticado = factory.CreateClient();
        autenticado.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await autenticado.GetAsync("/api/transacoes")).StatusCode);
    }

    [Fact]
    public async Task Login_com_senha_errada_ou_email_inexistente_retorna_401_com_a_mesma_mensagem()
    {
        var email = ApiFactory.NovoEmail();
        await Registrar(email);

        var senhaErrada = await Login(email, "senha-errada-123");
        var emailInexistente = await Login(ApiFactory.NovoEmail());

        Assert.Equal(HttpStatusCode.Unauthorized, senhaErrada.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, emailInexistente.StatusCode);
        var detalhe1 = (await senhaErrada.Content.ReadFromJsonAsync<ProblemDetails>(ApiFactory.Json))!.Detail;
        var detalhe2 = (await emailInexistente.Content.ReadFromJsonAsync<ProblemDetails>(ApiFactory.Json))!.Detail;
        Assert.Equal(detalhe1, detalhe2);
    }

    [Theory]
    [InlineData("/api/transacoes")]
    [InlineData("/api/categorias")]
    [InlineData("/api/contas")]
    [InlineData("/api/contas/1/saldo")]
    [InlineData("/api/relatorios/mensal?ano=2026&mes=10")]
    [InlineData("/api/relatorios/anual?ano=2026")]
    public async Task Rotas_protegidas_sem_token_retornam_401(string rota)
    {
        var resposta = await _client.GetAsync(rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_invalido_retorna_401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-falso");

        var resposta = await client.GetAsync("/api/transacoes");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Usuario_nao_enxerga_nem_altera_transacoes_de_outro_usuario()
    {
        var dono = await factory.CriarClienteAutenticadoAsync();
        var outro = await factory.CriarClienteAutenticadoAsync();
        var dto = new TransacaoCreateDto
        {
            Descricao = "Particular",
            Valor = 50m,
            Data = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc),
            Tipo = TipoTransacao.Despesa,
            CategoriaId = 3,
            ContaId = 1
        };
        var criada = await (await dono.PostAsJsonAsync("/api/transacoes", dto, ApiFactory.Json))
            .Content.ReadFromJsonAsync<TransacaoResponseDto>(ApiFactory.Json);
        var rota = $"/api/transacoes/{criada!.Id}";

        var lista = await outro.GetFromJsonAsync<PagedResultDto>("/api/transacoes", ApiFactory.Json);
        var saldo = await outro.GetFromJsonAsync<ContaSaldoDto>("/api/contas/1/saldo", ApiFactory.Json);

        Assert.Equal(0, lista!.TotalItems);
        Assert.Equal(0m, saldo!.TotalDespesas);
        Assert.Equal(HttpStatusCode.NotFound, (await outro.GetAsync(rota)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outro.PutAsJsonAsync(rota, dto with { Valor = 1m }, ApiFactory.Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outro.DeleteAsync(rota)).StatusCode);

        var doDono = await dono.GetFromJsonAsync<TransacaoResponseDto>(rota, ApiFactory.Json);
        Assert.Equal(50m, doDono!.Valor);
    }

    private record PagedResultDto(int TotalItems);
}
