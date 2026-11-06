using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Mappings;

namespace FinTrack.Application.Services;

public class ContaService(IContaRepository contaRepository, ITransacaoRepository transacaoRepository) : IContaService
{
    public async Task<IReadOnlyList<ContaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var contas = await contaRepository.ListarAsync(cancellationToken);
        return contas.Select(c => c.ToDto()).ToList();
    }

    public async Task<ContaSaldoDto> ObterSaldoAsync(int id, CancellationToken cancellationToken = default)
    {
        var conta = await contaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Conta", id);

        var (receitas, despesas) = await transacaoRepository.ObterTotaisPorContaAsync(id, cancellationToken);

        return new ContaSaldoDto(
            conta.Id,
            conta.Nome,
            conta.SaldoInicial,
            receitas,
            despesas,
            conta.SaldoInicial + receitas - despesas);
    }
}
