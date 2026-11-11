using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinTrack.Application.DTOs;
using FinTrack.Domain.Enums;

namespace FinTrack.IntegrationTests;

public class RelatorioTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = await factory.CriarClienteAutenticadoAsync();

        await Criar("Mercado", 300m, TipoTransacao.Despesa, categoriaId: 3, new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc));
        await Criar("Restaurante", 100m, TipoTransacao.Despesa, categoriaId: 3, new DateTime(2026, 10, 31, 23, 59, 0, DateTimeKind.Utc));
        await Criar("Ônibus", 100m, TipoTransacao.Despesa, categoriaId: 4, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        await Criar("Salário", 5000m, TipoTransacao.Receita, categoriaId: 1, new DateTime(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc));

        await Criar("Aluguel", 1500m, TipoTransacao.Despesa, categoriaId: 5, new DateTime(2026, 9, 30, 23, 59, 0, DateTimeKind.Utc));
        await Criar("Freela antigo", 800m, TipoTransacao.Receita, categoriaId: 2, new DateTime(2025, 10, 10, 0, 0, 0, DateTimeKind.Utc));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task Criar(string descricao, decimal valor, TipoTransacao tipo, int categoriaId, DateTime data)
    {
        var resposta = await _client.PostAsJsonAsync("/api/transacoes", new TransacaoCreateDto
        {
            Descricao = descricao,
            Valor = valor,
            Data = data,
            Tipo = tipo,
            CategoriaId = categoriaId,
            ContaId = 1
        }, ApiFactory.Json);
        resposta.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Mensal_soma_so_o_mes_pedido_agrupando_gastos_por_categoria()
    {
        var relatorio = await _client.GetFromJsonAsync<RelatorioMensalDto>("/api/relatorios/mensal?ano=2026&mes=10", ApiFactory.Json);

        Assert.Equal(5000m, relatorio!.TotalReceitas);
        Assert.Equal(500m, relatorio.TotalDespesas);
        Assert.Equal(4500m, relatorio.Saldo);
        Assert.Collection(relatorio.DespesasPorCategoria,
            c => { Assert.Equal("Alimentação", c.Categoria); Assert.Equal(400m, c.Total); Assert.Equal(80m, c.Percentual); },
            c => { Assert.Equal("Transporte", c.Categoria); Assert.Equal(100m, c.Total); Assert.Equal(20m, c.Percentual); });
        var receita = Assert.Single(relatorio.ReceitasPorCategoria);
        Assert.Equal("Salário", receita.Categoria);
    }

    [Fact]
    public async Task Anual_soma_o_ano_e_detalha_cada_mes()
    {
        var relatorio = await _client.GetFromJsonAsync<RelatorioAnualDto>("/api/relatorios/anual?ano=2026", ApiFactory.Json);

        Assert.Equal(5000m, relatorio!.TotalReceitas);
        Assert.Equal(2000m, relatorio.TotalDespesas);
        Assert.Equal(12, relatorio.Meses.Count);
        Assert.Equal(new RelatorioMesDto(9, 0m, 1500m, -1500m), relatorio.Meses[8]);
        Assert.Equal(new RelatorioMesDto(10, 5000m, 500m, 4500m), relatorio.Meses[9]);
        Assert.Equal("Moradia", relatorio.DespesasPorCategoria[0].Categoria);

        var anterior = await _client.GetFromJsonAsync<RelatorioAnualDto>("/api/relatorios/anual?ano=2025", ApiFactory.Json);
        Assert.Equal(800m, anterior!.TotalReceitas);
    }

    [Theory]
    [InlineData("/api/relatorios/mensal?ano=2026")]
    [InlineData("/api/relatorios/mensal?ano=2026&mes=13")]
    [InlineData("/api/relatorios/anual")]
    public async Task Parametros_invalidos_retornam_400(string rota)
    {
        var resposta = await _client.GetAsync(rota);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Theory]
    [InlineData("/api/relatorios/mensal", new[] { "Ano", "Mes" })]
    [InlineData("/api/relatorios/anual", new[] { "Ano" })]
    public async Task Swagger_documenta_so_os_parametros_usados_por_cada_relatorio(string rota, string[] esperados)
    {
        using var swagger = JsonDocument.Parse(await _client.GetStringAsync("/swagger/v1/swagger.json"));

        var parametros = swagger.RootElement
            .GetProperty("paths").GetProperty(rota).GetProperty("get").GetProperty("parameters")
            .EnumerateArray().Select(p => p.GetProperty("name").GetString());

        Assert.Equal(esperados, parametros);
    }
}
