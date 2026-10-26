using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Interfaces;

public interface ITransacaoRepository
{
    /// <summary>Retorna a transação com Categoria e Conta carregadas.</summary>
    Task<Transacao?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Lista paginada, ordenada da mais recente para a mais antiga, com Categoria e Conta carregadas.</summary>
    Task<PagedResult<Transacao>> ListarAsync(TransacaoFiltroDto filtro, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Transacao transacao, CancellationToken cancellationToken = default);
}
