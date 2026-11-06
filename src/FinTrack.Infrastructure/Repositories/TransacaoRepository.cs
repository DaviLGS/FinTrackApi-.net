using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
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
            var inicioUtc = dataInicio.ParaUtc();
            query = query.Where(t => t.Data >= inicioUtc);
        }
        if (filtro.DataFim is { } dataFim)
        {
            var fimUtc = dataFim.ParaUtc();
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

    public async Task<bool> AtualizarAsync(Transacao transacao, CancellationToken cancellationToken = default)
    {
        var linhas = await context.Transacoes
            .Where(t => t.Id == transacao.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Descricao, transacao.Descricao)
                .SetProperty(t => t.Valor, transacao.Valor)
                .SetProperty(t => t.Data, transacao.Data)
                .SetProperty(t => t.Tipo, transacao.Tipo)
                .SetProperty(t => t.CategoriaId, transacao.CategoriaId)
                .SetProperty(t => t.ContaId, transacao.ContaId),
                cancellationToken);

        return linhas > 0;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var linhas = await context.Transacoes
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return linhas > 0;
    }

    public async Task<(decimal Receitas, decimal Despesas)> ObterTotaisPorContaAsync(int contaId, CancellationToken cancellationToken = default)
    {
        var totais = await context.Transacoes
            .Where(t => t.ContaId == contaId)
            .GroupBy(t => t.Tipo)
            .Select(g => new { Tipo = g.Key, Total = g.Sum(t => t.Valor) })
            .ToListAsync(cancellationToken);

        return (
            totais.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Total),
            totais.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Total));
    }
}
