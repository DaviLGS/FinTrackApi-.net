using FinTrack.Application.Interfaces;
using FinTrack.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Repositories;

public class RelatorioRepository(AppDbContext context) : IRelatorioRepository
{
    public async Task<IReadOnlyList<TotalPorCategoria>> ObterTotaisPorCategoriaAsync(
        int usuarioId, DateTime inicio, DateTime fim, CancellationToken cancellationToken = default) =>
        await context.Transacoes
            .Where(t => t.UsuarioId == usuarioId && t.Data >= inicio && t.Data < fim)
            .GroupBy(t => new { t.CategoriaId, t.Categoria!.Nome, t.Tipo })
            .Select(g => new TotalPorCategoria(g.Key.CategoriaId, g.Key.Nome, g.Key.Tipo, g.Sum(t => t.Valor)))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TotalPorMes>> ObterTotaisPorMesAsync(
        int usuarioId, int ano, CancellationToken cancellationToken = default)
    {
        var inicio = new DateTime(ano, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddYears(1);

        return await context.Transacoes
            .Where(t => t.UsuarioId == usuarioId && t.Data >= inicio && t.Data < fim)
            .GroupBy(t => new { t.Data.Month, t.Tipo })
            .Select(g => new TotalPorMes(g.Key.Month, g.Key.Tipo, g.Sum(t => t.Valor)))
            .ToListAsync(cancellationToken);
    }
}
