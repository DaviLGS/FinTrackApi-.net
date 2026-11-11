using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Validators;
using FinTrack.Domain.Enums;
using FluentValidation;

namespace FinTrack.Application.Services;

public class RelatorioService(
    IRelatorioRepository relatorioRepository,
    IUsuarioAtual usuarioAtual,
    RelatorioMensalValidator mensalValidator,
    RelatorioAnualValidator anualValidator) : IRelatorioService
{
    public async Task<RelatorioMensalDto> GerarMensalAsync(RelatorioMensalFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        await mensalValidator.ValidateAndThrowAsync(filtro, cancellationToken);

        var mes = filtro.Mes!.Value;
        var inicio = new DateTime(filtro.Ano, mes, 1, 0, 0, 0, DateTimeKind.Utc);

        var totais = await relatorioRepository.ObterTotaisPorCategoriaAsync(
            usuarioAtual.Id, inicio, inicio.AddMonths(1), cancellationToken);

        var (receitas, despesas) = (Somar(totais, TipoTransacao.Receita), Somar(totais, TipoTransacao.Despesa));

        return new RelatorioMensalDto(
            filtro.Ano,
            mes,
            receitas,
            despesas,
            receitas - despesas,
            PorCategoria(totais, TipoTransacao.Despesa),
            PorCategoria(totais, TipoTransacao.Receita));
    }

    public async Task<RelatorioAnualDto> GerarAnualAsync(RelatorioAnualFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        await anualValidator.ValidateAndThrowAsync(filtro, cancellationToken);

        var inicio = new DateTime(filtro.Ano, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var totais = await relatorioRepository.ObterTotaisPorCategoriaAsync(
            usuarioAtual.Id, inicio, inicio.AddYears(1), cancellationToken);
        var totaisPorMes = await relatorioRepository.ObterTotaisPorMesAsync(usuarioAtual.Id, filtro.Ano, cancellationToken);

        var meses = Enumerable.Range(1, 12)
            .Select(mes =>
            {
                var receitas = totaisPorMes.Where(t => t.Mes == mes && t.Tipo == TipoTransacao.Receita).Sum(t => t.Total);
                var despesas = totaisPorMes.Where(t => t.Mes == mes && t.Tipo == TipoTransacao.Despesa).Sum(t => t.Total);
                return new RelatorioMesDto(mes, receitas, despesas, receitas - despesas);
            })
            .ToList();

        var (totalReceitas, totalDespesas) = (Somar(totais, TipoTransacao.Receita), Somar(totais, TipoTransacao.Despesa));

        return new RelatorioAnualDto(
            filtro.Ano,
            totalReceitas,
            totalDespesas,
            totalReceitas - totalDespesas,
            PorCategoria(totais, TipoTransacao.Despesa),
            PorCategoria(totais, TipoTransacao.Receita),
            meses);
    }

    private static decimal Somar(IEnumerable<TotalPorCategoria> totais, TipoTransacao tipo) =>
        totais.Where(t => t.Tipo == tipo).Sum(t => t.Total);

    private static List<RelatorioCategoriaDto> PorCategoria(IReadOnlyList<TotalPorCategoria> totais, TipoTransacao tipo)
    {
        var total = Somar(totais, tipo);

        return totais
            .Where(t => t.Tipo == tipo)
            .OrderByDescending(t => t.Total)
            .ThenBy(t => t.Categoria)
            .Select(t => new RelatorioCategoriaDto(
                t.CategoriaId,
                t.Categoria,
                t.Total,
                total == 0 ? 0 : Math.Round(t.Total / total * 100, 2, MidpointRounding.AwayFromZero)))
            .ToList();
    }
}
