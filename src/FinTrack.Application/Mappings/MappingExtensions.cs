using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Mappings;

public static class MappingExtensions
{
    public static CategoriaDto ToDto(this Categoria categoria) =>
        new(categoria.Id, categoria.Nome, categoria.Tipo);

    public static ContaDto ToDto(this Conta conta) =>
        new(conta.Id, conta.Nome, conta.SaldoInicial);

    /// <summary>Requer <see cref="Transacao.Categoria"/> e <see cref="Transacao.Conta"/> carregadas.</summary>
    public static TransacaoResponseDto ToDto(this Transacao transacao) =>
        transacao.ToDto(transacao.Categoria!, transacao.Conta!);

    public static TransacaoResponseDto ToDto(this Transacao transacao, Categoria categoria, Conta conta) =>
        new(
            transacao.Id,
            transacao.Descricao,
            transacao.Valor,
            transacao.Data,
            transacao.Tipo,
            categoria.Id,
            categoria.Nome,
            conta.Id,
            conta.Nome,
            transacao.CriadoEm);
}
