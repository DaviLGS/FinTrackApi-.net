namespace FinTrack.Application.DTOs;

/// <summary>Parâmetros do relatório anual.</summary>
public record RelatorioAnualFiltroDto
{
    public int Ano { get; init; }
}

/// <summary>Parâmetros do relatório mensal: o ano e o mês (1 a 12).</summary>
public record RelatorioMensalFiltroDto
{
    public int Ano { get; init; }
    public int? Mes { get; init; }
}

/// <summary>Total de uma categoria no período e sua participação (%) no total do mesmo tipo.</summary>
public record RelatorioCategoriaDto(int CategoriaId, string Categoria, decimal Total, decimal Percentual);

public record RelatorioMesDto(int Mes, decimal Receitas, decimal Despesas, decimal Saldo);

public record RelatorioMensalDto(
    int Ano,
    int Mes,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo,
    IReadOnlyList<RelatorioCategoriaDto> DespesasPorCategoria,
    IReadOnlyList<RelatorioCategoriaDto> ReceitasPorCategoria);

public record RelatorioAnualDto(
    int Ano,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo,
    IReadOnlyList<RelatorioCategoriaDto> DespesasPorCategoria,
    IReadOnlyList<RelatorioCategoriaDto> ReceitasPorCategoria,
    IReadOnlyList<RelatorioMesDto> Meses);
