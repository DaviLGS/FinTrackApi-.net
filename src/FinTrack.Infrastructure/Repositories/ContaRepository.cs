using FinTrack.Application.Interfaces;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Repositories;

public class ContaRepository(AppDbContext context) : IContaRepository
{
    public async Task<IReadOnlyList<Conta>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Contas
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);

    public Task<Conta?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Contas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
