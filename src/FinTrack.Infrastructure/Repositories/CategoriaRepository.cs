using FinTrack.Application.Interfaces;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Repositories;

public class CategoriaRepository(AppDbContext context) : ICategoriaRepository
{
    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);

    public Task<Categoria?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
