using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Interfaces;

public interface ITransacaoRepository
{
    Task<Transacao?> ObterPorIdAsync(int id, int usuarioId, CancellationToken cancellationToken = default);

    Task<PagedResult<Transacao>> ListarAsync(int usuarioId, TransacaoFiltroDto filtro, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Transacao transacao, CancellationToken cancellationToken = default);

    Task<bool> AtualizarAsync(Transacao transacao, CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(int id, int usuarioId, CancellationToken cancellationToken = default);

    Task<(decimal Receitas, decimal Despesas)> ObterTotaisPorContaAsync(int contaId, int usuarioId, CancellationToken cancellationToken = default);
}
