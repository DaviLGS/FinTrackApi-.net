using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Mappings;
using FinTrack.Domain.Entities;
using FluentValidation;

namespace FinTrack.Application.Services;

public class TransacaoService(
    ITransacaoRepository transacaoRepository,
    ICategoriaRepository categoriaRepository,
    IContaRepository contaRepository,
    IValidator<TransacaoCreateDto> createValidator,
    IValidator<TransacaoFiltroDto> filtroValidator) : ITransacaoService
{
    public async Task<TransacaoResponseDto> CriarAsync(TransacaoCreateDto dto, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var categoria = await categoriaRepository.ObterPorIdAsync(dto.CategoriaId, cancellationToken)
            ?? throw new BusinessRuleException($"Categoria com id '{dto.CategoriaId}' não existe.");

        var conta = await contaRepository.ObterPorIdAsync(dto.ContaId, cancellationToken)
            ?? throw new BusinessRuleException($"Conta com id '{dto.ContaId}' não existe.");

        if (categoria.Tipo != dto.Tipo)
        {
            throw new BusinessRuleException(
                $"A categoria '{categoria.Nome}' é do tipo '{categoria.Tipo}' e não aceita transações do tipo '{dto.Tipo}'.");
        }

        var transacao = new Transacao
        {
            Descricao = dto.Descricao.Trim(),
            Valor = dto.Valor,
            Data = dto.Data.ParaUtc(),
            Tipo = dto.Tipo,
            CategoriaId = categoria.Id,
            ContaId = conta.Id
        };

        await transacaoRepository.AdicionarAsync(transacao, cancellationToken);

        return transacao.ToDto(categoria, conta);
    }

    public async Task<TransacaoResponseDto> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var transacao = await transacaoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Transação", id);

        return transacao.ToDto();
    }

    public async Task<PagedResult<TransacaoResponseDto>> ListarAsync(TransacaoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        await filtroValidator.ValidateAndThrowAsync(filtro, cancellationToken);

        var resultado = await transacaoRepository.ListarAsync(filtro, cancellationToken);

        return new PagedResult<TransacaoResponseDto>(
            resultado.Items.Select(t => t.ToDto()).ToList(),
            resultado.Page,
            resultado.PageSize,
            resultado.TotalItems);
    }
}
