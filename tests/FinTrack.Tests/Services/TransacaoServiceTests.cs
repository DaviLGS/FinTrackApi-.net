using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Services;
using FinTrack.Application.Validators;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace FinTrack.Tests.Services;

public class TransacaoServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    private static readonly Categoria Alimentacao = new() { Id = 3, Nome = "Alimentação", Tipo = TipoTransacao.Despesa };
    private static readonly Categoria Salario = new() { Id = 1, Nome = "Salário", Tipo = TipoTransacao.Receita };
    private static readonly Conta Carteira = new() { Id = 1, Nome = "Carteira" };

    private readonly ITransacaoRepository _transacaoRepository = Substitute.For<ITransacaoRepository>();
    private readonly ICategoriaRepository _categoriaRepository = Substitute.For<ICategoriaRepository>();
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly TransacaoService _service;

    public TransacaoServiceTests()
    {
        _categoriaRepository.ObterPorIdAsync(Alimentacao.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Categoria?>(Alimentacao));
        _categoriaRepository.ObterPorIdAsync(Salario.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Categoria?>(Salario));
        _contaRepository.ObterPorIdAsync(Carteira.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Conta?>(Carteira));

        _service = new TransacaoService(
            _transacaoRepository,
            _categoriaRepository,
            _contaRepository,
            new TransacaoCreateValidator(new FakeTimeProvider(Agora)),
            new TransacaoFiltroValidator());
    }

    private static TransacaoCreateDto DtoValido() => new()
    {
        Descricao = "  Almoço  ",
        Valor = 42.90m,
        Data = new DateTime(2026, 10, 20, 12, 30, 0, DateTimeKind.Unspecified),
        Tipo = TipoTransacao.Despesa,
        CategoriaId = Alimentacao.Id,
        ContaId = Carteira.Id
    };

    [Fact]
    public async Task CriarAsync_com_dados_validos_persiste_e_retorna_dto()
    {
        _transacaoRepository
            .When(r => r.AdicionarAsync(Arg.Any<Transacao>(), Arg.Any<CancellationToken>()))
            .Do(call => call.Arg<Transacao>().Id = 10);

        var resultado = await _service.CriarAsync(DtoValido());

        await _transacaoRepository.Received(1).AdicionarAsync(
            Arg.Is<Transacao>(t =>
                t.Descricao == "Almoço" &&
                t.Valor == 42.90m &&
                t.Tipo == TipoTransacao.Despesa &&
                t.CategoriaId == Alimentacao.Id &&
                t.ContaId == Carteira.Id),
            Arg.Any<CancellationToken>());

        Assert.Equal(10, resultado.Id);
        Assert.Equal("Almoço", resultado.Descricao);
        Assert.Equal("Alimentação", resultado.CategoriaNome);
        Assert.Equal("Carteira", resultado.ContaNome);
    }

    [Fact]
    public async Task CriarAsync_converte_data_sem_fuso_para_utc()
    {
        var resultado = await _service.CriarAsync(DtoValido());

        Assert.Equal(DateTimeKind.Utc, resultado.Data.Kind);
        Assert.Equal(new DateTime(2026, 10, 20, 12, 30, 0, DateTimeKind.Utc), resultado.Data);
    }

    [Fact]
    public async Task CriarAsync_com_dto_invalido_lanca_ValidationException_e_nao_persiste()
    {
        var dto = DtoValido() with { Valor = 0 };

        await Assert.ThrowsAsync<ValidationException>(() => _service.CriarAsync(dto));

        await _transacaoRepository.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, default);
    }

    [Fact]
    public async Task CriarAsync_com_categoria_inexistente_lanca_BusinessRuleException()
    {
        var dto = DtoValido() with { CategoriaId = 999 };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CriarAsync(dto));

        Assert.Contains("Categoria", ex.Message);
        await _transacaoRepository.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, default);
    }

    [Fact]
    public async Task CriarAsync_com_conta_inexistente_lanca_BusinessRuleException()
    {
        var dto = DtoValido() with { ContaId = 999 };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CriarAsync(dto));

        Assert.Contains("Conta", ex.Message);
        await _transacaoRepository.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, default);
    }

    [Fact]
    public async Task CriarAsync_com_tipo_incompativel_com_a_categoria_lanca_BusinessRuleException()
    {
        var dto = DtoValido() with { CategoriaId = Salario.Id, Tipo = TipoTransacao.Despesa };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CriarAsync(dto));

        await _transacaoRepository.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, default);
    }

    [Fact]
    public async Task ObterPorIdAsync_inexistente_lanca_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPorIdAsync(999));
    }

    [Fact]
    public async Task ObterPorIdAsync_existente_retorna_dto()
    {
        var transacao = new Transacao
        {
            Id = 5,
            Descricao = "Almoço",
            Valor = 10m,
            Data = Agora.UtcDateTime,
            Tipo = TipoTransacao.Despesa,
            CategoriaId = Alimentacao.Id,
            Categoria = Alimentacao,
            ContaId = Carteira.Id,
            Conta = Carteira
        };
        _transacaoRepository.ObterPorIdAsync(5, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Transacao?>(transacao));

        var resultado = await _service.ObterPorIdAsync(5);

        Assert.Equal(5, resultado.Id);
        Assert.Equal("Alimentação", resultado.CategoriaNome);
        Assert.Equal("Carteira", resultado.ContaNome);
    }

    [Fact]
    public async Task ListarAsync_com_filtro_invalido_lanca_ValidationException()
    {
        var filtro = new TransacaoFiltroDto { PageSize = 0 };

        await Assert.ThrowsAsync<ValidationException>(() => _service.ListarAsync(filtro));

        await _transacaoRepository.DidNotReceiveWithAnyArgs().ListarAsync(default!, default);
    }

    [Fact]
    public async Task ListarAsync_mapeia_resultado_paginado()
    {
        var filtro = new TransacaoFiltroDto { Page = 2, PageSize = 1 };
        var transacao = new Transacao
        {
            Id = 7,
            Descricao = "Uber",
            Valor = 25m,
            Data = Agora.UtcDateTime,
            Tipo = TipoTransacao.Despesa,
            CategoriaId = Alimentacao.Id,
            Categoria = Alimentacao,
            ContaId = Carteira.Id,
            Conta = Carteira
        };
        _transacaoRepository.ListarAsync(filtro, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Transacao>([transacao], 2, 1, 3));

        var resultado = await _service.ListarAsync(filtro);

        Assert.Equal(2, resultado.Page);
        Assert.Equal(3, resultado.TotalItems);
        Assert.Equal(3, resultado.TotalPages);
        var item = Assert.Single(resultado.Items);
        Assert.Equal(7, item.Id);
        Assert.Equal("Uber", item.Descricao);
    }
}
