using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Interfaces;

/// <summary>Todas as operações ficam restritas às transações do usuário informado.</summary>
public interface ITransacaoRepository
{
    /// <summary>Retorna a transação com Categoria e Conta carregadas.</summary>
    Task<Transacao?> ObterPorIdAsync(int id, int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>Lista paginada, ordenada da mais recente para a mais antiga, com Categoria e Conta carregadas.</summary>
    Task<PagedResult<Transacao>> ListarAsync(int usuarioId, TransacaoFiltroDto filtro, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Transacao transacao, CancellationToken cancellationToken = default);

    /// <summary>Atualiza os dados editáveis (não altera CriadoEm nem o dono). Retorna false se a transação não existir mais.</summary>
    Task<bool> AtualizarAsync(Transacao transacao, CancellationToken cancellationToken = default);

    /// <summary>Retorna false se a transação não existir.</summary>
    Task<bool> RemoverAsync(int id, int usuarioId, CancellationToken cancellationToken = default);

    Task<(decimal Receitas, decimal Despesas)> ObterTotaisPorContaAsync(int contaId, int usuarioId, CancellationToken cancellationToken = default);
}
