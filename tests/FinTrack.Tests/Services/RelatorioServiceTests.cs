using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Services;
using FinTrack.Application.Validators;
using FinTrack.Domain.Enums;
using FluentValidation;
using NSubstitute;

namespace FinTrack.Tests.Services;

public class RelatorioServiceTests
{
    private const int UsuarioId = 7;

    private readonly IRelatorioRepository _repository = Substitute.For<IRelatorioRepository>();
    private readonly IUsuarioAtual _usuarioAtual = Substitute.For<IUsuarioAtual>();
    private readonly RelatorioService _service;

    public RelatorioServiceTests()
    {
        _usuarioAtual.Id.Returns(UsuarioId);
        _service = new RelatorioService(_repository, _usuarioAtual, new RelatorioMensalValidator(), new RelatorioAnualValidator());
    }

    private static readonly IReadOnlyList<TotalPorCategoria> Totais =
    [
        new(3, "Alimentação", TipoTransacao.Despesa, 300m),
        new(5, "Moradia", TipoTransacao.Despesa, 700m),
        new(1, "Salário", TipoTransacao.Receita, 5000m),
        new(2, "Freelance", TipoTransacao.Receita, 1000m)
    ];

    [Fact]
    public async Task Mensal_consulta_o_mes_em_utc_do_usuario_e_calcula_totais_e_percentuais()
    {
        var inicio = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        _repository.ObterTotaisPorCategoriaAsync(UsuarioId, inicio, inicio.AddMonths(1), Arg.Any<CancellationToken>())
            .Returns(Totais);

        var relatorio = await _service.GerarMensalAsync(new RelatorioFiltroDto { Ano = 2026, Mes = 10 });

        Assert.Equal(6000m, relatorio.TotalReceitas);
        Assert.Equal(1000m, relatorio.TotalDespesas);
        Assert.Equal(5000m, relatorio.Saldo);

        // Maior gasto primeiro, com a participação no total de despesas
        Assert.Collection(relatorio.DespesasPorCategoria,
            c => { Assert.Equal("Moradia", c.Categoria); Assert.Equal(70m, c.Percentual); },
            c => { Assert.Equal("Alimentação", c.Categoria); Assert.Equal(30m, c.Percentual); });
        Assert.Collection(relatorio.ReceitasPorCategoria,
            c => { Assert.Equal("Salário", c.Categoria); Assert.Equal(83.33m, c.Percentual); },
            c => { Assert.Equal("Freelance", c.Categoria); Assert.Equal(16.67m, c.Percentual); });
    }

    [Fact]
    public async Task Mensal_sem_movimentacao_retorna_zeros_e_listas_vazias()
    {
        _repository.ObterTotaisPorCategoriaAsync(default, default, default).ReturnsForAnyArgs([]);

        var relatorio = await _service.GerarMensalAsync(new RelatorioFiltroDto { Ano = 2026, Mes = 2 });

        Assert.Equal(0m, relatorio.Saldo);
        Assert.Empty(relatorio.DespesasPorCategoria);
        Assert.Empty(relatorio.ReceitasPorCategoria);
    }

    [Fact]
    public async Task Anual_retorna_os_12_meses_inclusive_os_sem_movimentacao()
    {
        var inicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _repository.ObterTotaisPorCategoriaAsync(UsuarioId, inicio, inicio.AddYears(1), Arg.Any<CancellationToken>())
            .Returns(Totais);
        _repository.ObterTotaisPorMesAsync(UsuarioId, 2026, Arg.Any<CancellationToken>())
            .Returns(
            [
                new TotalPorMes(1, TipoTransacao.Receita, 5000m),
                new TotalPorMes(1, TipoTransacao.Despesa, 700m),
                new TotalPorMes(3, TipoTransacao.Receita, 1000m),
                new TotalPorMes(3, TipoTransacao.Despesa, 300m)
            ]);

        var relatorio = await _service.GerarAnualAsync(new RelatorioFiltroDto { Ano = 2026 });

        Assert.Equal(5000m, relatorio.Saldo);
        Assert.Equal(Enumerable.Range(1, 12), relatorio.Meses.Select(m => m.Mes));
        Assert.Equal(new RelatorioMesDto(1, 5000m, 700m, 4300m), relatorio.Meses[0]);
        Assert.Equal(new RelatorioMesDto(2, 0m, 0m, 0m), relatorio.Meses[1]);
        Assert.Equal(new RelatorioMesDto(3, 1000m, 300m, 700m), relatorio.Meses[2]);
    }

    [Theory]
    [InlineData(2026, null)]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    [InlineData(1999, 5)]
    [InlineData(0, 5)]
    public async Task Mensal_com_ano_ou_mes_invalido_lanca_ValidationException(int ano, int? mes)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GerarMensalAsync(new RelatorioFiltroDto { Ano = ano, Mes = mes }));
    }

    [Fact]
    public async Task Anual_com_ano_invalido_lanca_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _service.GerarAnualAsync(new RelatorioFiltroDto { Ano = 2101 }));
    }
}
