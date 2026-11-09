using FinTrack.Domain.Enums;

namespace FinTrack.Application.Interfaces;

public record TotalPorCategoria(int CategoriaId, string Categoria, TipoTransacao Tipo, decimal Total);

public record TotalPorMes(int Mes, TipoTransacao Tipo, decimal Total);

/// <summary>Agregações das transações de um usuário. Períodos em UTC: início inclusivo, fim exclusivo.</summary>
public interface IRelatorioRepository
{
    Task<IReadOnlyList<TotalPorCategoria>> ObterTotaisPorCategoriaAsync(
        int usuarioId, DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TotalPorMes>> ObterTotaisPorMesAsync(
        int usuarioId, int ano, CancellationToken cancellationToken = default);
}
