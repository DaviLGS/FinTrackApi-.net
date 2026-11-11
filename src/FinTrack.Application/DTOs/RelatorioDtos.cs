namespace FinTrack.Application.DTOs;

public record RelatorioAnualFiltroDto
{
    public int Ano { get; init; }
}

public record RelatorioMensalFiltroDto
{
    public int Ano { get; init; }
    public int? Mes { get; init; }
}

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
