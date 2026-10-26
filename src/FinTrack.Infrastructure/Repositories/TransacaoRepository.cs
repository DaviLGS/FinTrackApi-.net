using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Repositories;

public class TransacaoRepository(AppDbContext context) : ITransacaoRepository
{
    public Task<Transacao?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Transacoes
            .AsNoTracking()
            .Include(t => t.Categoria)
            .Include(t => t.Conta)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<PagedResult<Transacao>> ListarAsync(TransacaoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(filtro.Page, 1);
        var pageSize = Math.Clamp(filtro.PageSize, 1, TransacaoFiltroDto.PageSizeMaximo);

        var query = context.Transacoes.AsNoTracking();

        if (filtro.DataInicio is { } dataInicio)
        {
            var inicioUtc = ParaUtc(dataInicio);
            query = query.Where(t => t.Data >= inicioUtc);
        }
        if (filtro.DataFim is { } dataFim)
        {
            var fimUtc = ParaUtc(dataFim);
            query = query.Where(t => t.Data <= fimUtc);
        }
        if (filtro.CategoriaId is { } categoriaId)
            query = query.Where(t => t.CategoriaId == categoriaId);
        if (filtro.ContaId is { } contaId)
            query = query.Where(t => t.ContaId == contaId);
        if (filtro.Tipo is { } tipo)
            query = query.Where(t => t.Tipo == tipo);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(t => t.Categoria)
            .Include(t => t.Conta)
            .OrderByDescending(t => t.Data)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Transacao>(items, page, pageSize, total);
    }

    public async Task AdicionarAsync(Transacao transacao, CancellationToken cancellationToken = default)
    {
        context.Transacoes.Add(transacao);
        await context.SaveChangesAsync(cancellationToken);
    }

    // Npgsql exige DateTime com Kind = Utc para colunas timestamptz.
    private static DateTime ParaUtc(DateTime data) => data.Kind switch
    {
        DateTimeKind.Utc => data,
        DateTimeKind.Local => data.ToUniversalTime(),
        _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
    };
}
