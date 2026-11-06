using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Services;
using FinTrack.Domain.Entities;
using NSubstitute;

namespace FinTrack.Tests.Services;

public class ContaServiceTests
{
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly ITransacaoRepository _transacaoRepository = Substitute.For<ITransacaoRepository>();
    private readonly ContaService _service;

    public ContaServiceTests()
    {
        _service = new ContaService(_contaRepository, _transacaoRepository);
    }

    [Fact]
    public async Task ObterSaldoAsync_soma_saldo_inicial_e_receitas_e_subtrai_despesas()
    {
        _contaRepository.ObterPorIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Conta?>(new Conta { Id = 1, Nome = "Carteira", SaldoInicial = 100m }));
        _transacaoRepository.ObterTotaisPorContaAsync(1, Arg.Any<CancellationToken>())
            .Returns((50m, 30.5m));

        var saldo = await _service.ObterSaldoAsync(1);

        Assert.Equal(100m, saldo.SaldoInicial);
        Assert.Equal(50m, saldo.TotalReceitas);
        Assert.Equal(30.5m, saldo.TotalDespesas);
        Assert.Equal(119.5m, saldo.SaldoAtual);
    }

    [Fact]
    public async Task ObterSaldoAsync_de_conta_inexistente_lanca_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterSaldoAsync(999));
    }
}
