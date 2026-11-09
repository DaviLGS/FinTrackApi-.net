namespace FinTrack.Application.DTOs;

/// <summary>Parâmetros dos relatórios. <see cref="Mes"/> só é usado no relatório mensal.</summary>
public record RelatorioFiltroDto
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
