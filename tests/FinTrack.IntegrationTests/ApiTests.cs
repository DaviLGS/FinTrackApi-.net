using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FinTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.IntegrationTests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private const int CategoriaSalario = 1;
    private const int CategoriaAlimentacao = 3;
    private const int CategoriaTransporte = 4;
    private const int ContaCarteira = 1;
    private const int ContaCorrente = 2;

    private static readonly JsonSerializerOptions Json = ApiFactory.Json;

    private HttpClient _client = null!;

    public async Task InitializeAsync() => _client = await factory.CriarClienteAutenticadoAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static TransacaoCreateDto Despesa(string descricao = "Almoço", decimal valor = 42.90m, int contaId = ContaCarteira) => new()
    {
        Descricao = descricao,
        Valor = valor,
        Data = new DateTime(2026, 10, 20, 12, 30, 0, DateTimeKind.Utc),
        Tipo = TipoTransacao.Despesa,
        CategoriaId = CategoriaAlimentacao,
        ContaId = contaId
    };

    private Task<HttpResponseMessage> Post(TransacaoCreateDto dto) =>
        _client.PostAsJsonAsync("/api/transacoes", dto, Json);

    private async Task<TransacaoResponseDto> Criar(TransacaoCreateDto dto)
    {
        var resposta = await Post(dto);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<TransacaoResponseDto>(Json))!;
    }

    [Fact]
    public async Task Migrations_criam_os_dados_iniciais()
    {
        var categorias = await _client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias", Json);
        var contas = await _client.GetFromJsonAsync<List<ContaDto>>("/api/contas", Json);

        Assert.Equal(6, categorias!.Count);
        Assert.Equal(2, contas!.Count);
    }

    [Fact]
    public async Task Health_responde_saudavel_com_o_banco_no_ar()
    {
        var resposta = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Healthy", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Post_valido_retorna_201_com_location_e_a_transacao_pode_ser_consultada()
    {
        var resposta = await Post(Despesa("Mercado"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var criada = await resposta.Content.ReadFromJsonAsync<TransacaoResponseDto>(Json);
        Assert.EndsWith($"/api/transacoes/{criada!.Id}", resposta.Headers.Location!.ToString());

        var consultada = await _client.GetFromJsonAsync<TransacaoResponseDto>(resposta.Headers.Location, Json);
        Assert.Equal("Mercado", consultada!.Descricao);
        Assert.Equal(42.90m, consultada.Valor);
        Assert.Equal("Alimentação", consultada.CategoriaNome);
        Assert.Equal(DateTimeKind.Utc, consultada.Data.Kind);
    }

    [Fact]
    public async Task Post_invalido_retorna_400_com_erros_por_campo()
    {
        var resposta = await Post(Despesa() with { Descricao = "", Valor = -1 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
        Assert.Contains("descricao", problema!.Errors.Keys);
        Assert.Contains("valor", problema.Errors.Keys);
    }

    [Theory]
    [InlineData(CategoriaSalario, ContaCarteira)]
    [InlineData(999, ContaCarteira)]
    [InlineData(CategoriaAlimentacao, 999)]
    public async Task Post_que_viola_regra_de_negocio_retorna_422(int categoriaId, int contaId)
    {
        var resposta = await Post(Despesa() with { CategoriaId = categoriaId, ContaId = contaId });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Get_inexistente_retorna_404()
    {
        var resposta = await _client.GetAsync("/api/transacoes/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Listagem_aplica_filtros_e_paginacao()
    {
        var alvo = await Criar(Despesa("Ônibus") with { CategoriaId = CategoriaTransporte });
        await Criar(Despesa("Outra"));

        var pagina = await _client.GetFromJsonAsync<PagedResult<TransacaoResponseDto>>(
            $"/api/transacoes?categoriaId={CategoriaTransporte}&tipo=Despesa&dataInicio=2026-10-01&dataFim=2026-10-31&pageSize=100", Json);

        Assert.Contains(pagina!.Items, t => t.Id == alvo.Id);
        Assert.All(pagina.Items, t => Assert.Equal(CategoriaTransporte, t.CategoriaId));

        var fora = await _client.GetFromJsonAsync<PagedResult<TransacaoResponseDto>>(
            $"/api/transacoes?categoriaId={CategoriaTransporte}&dataInicio=2020-01-01&dataFim=2020-12-31", Json);

        Assert.DoesNotContain(fora!.Items, t => t.Id == alvo.Id);
    }

    [Fact]
    public async Task Put_altera_os_dados_e_preserva_CriadoEm()
    {
        var criada = await Criar(Despesa("Antes"));
        var novosDados = Despesa("Depois", 10m) with { CategoriaId = CategoriaTransporte };

        var resposta = await _client.PutAsJsonAsync($"/api/transacoes/{criada.Id}", novosDados, Json);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var consultada = await _client.GetFromJsonAsync<TransacaoResponseDto>($"/api/transacoes/{criada.Id}", Json);
        Assert.Equal("Depois", consultada!.Descricao);
        Assert.Equal(10m, consultada.Valor);
        Assert.Equal(CategoriaTransporte, consultada.CategoriaId);
        Assert.Equal("Transporte", consultada.CategoriaNome);
        Assert.Equal(criada.CriadoEm, consultada.CriadoEm, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Put_inexistente_retorna_404()
    {
        var resposta = await _client.PutAsJsonAsync("/api/transacoes/999999", Despesa(), Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Delete_remove_a_transacao_e_repetir_retorna_404()
    {
        var criada = await Criar(Despesa("Para remover"));

        var primeira = await _client.DeleteAsync($"/api/transacoes/{criada.Id}");
        var consulta = await _client.GetAsync($"/api/transacoes/{criada.Id}");
        var segunda = await _client.DeleteAsync($"/api/transacoes/{criada.Id}");

        Assert.Equal(HttpStatusCode.NoContent, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, segunda.StatusCode);
    }

    [Fact]
    public async Task Saldo_considera_receitas_e_despesas_da_conta()
    {
        await Criar(Despesa("Conta de luz", 30.50m, ContaCorrente));
        await Criar(new TransacaoCreateDto
        {
            Descricao = "Salário",
            Valor = 100m,
            Data = new DateTime(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc),
            Tipo = TipoTransacao.Receita,
            CategoriaId = CategoriaSalario,
            ContaId = ContaCorrente
        });

        var saldo = await _client.GetFromJsonAsync<ContaSaldoDto>($"/api/contas/{ContaCorrente}/saldo", Json);

        Assert.Equal(100m, saldo!.TotalReceitas);
        Assert.Equal(30.50m, saldo.TotalDespesas);
        Assert.Equal(69.50m, saldo.SaldoAtual);
    }

    [Fact]
    public async Task Saldo_de_conta_inexistente_retorna_404()
    {
        var resposta = await _client.GetAsync("/api/contas/999/saldo");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}
