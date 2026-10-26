using FinTrack.Application.DTOs;
using FinTrack.Application.Validators;
using FinTrack.Domain.Enums;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace FinTrack.Tests.Validators;

public class TransacaoCreateValidatorTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly TransacaoCreateValidator _validator = new(new FakeTimeProvider(Agora));

    private static TransacaoCreateDto DtoValido() => new()
    {
        Descricao = "Almoço",
        Valor = 42.90m,
        Data = Agora.UtcDateTime.AddDays(-1),
        Tipo = TipoTransacao.Despesa,
        CategoriaId = 3,
        ContaId = 1
    };

    [Fact]
    public void Dto_valido_nao_deve_ter_erros()
    {
        var resultado = _validator.TestValidate(DtoValido());

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Descricao_vazia_deve_ter_erro(string descricao)
    {
        var resultado = _validator.TestValidate(DtoValido() with { Descricao = descricao });

        resultado.ShouldHaveValidationErrorFor(t => t.Descricao)
            .WithErrorMessage("A descrição é obrigatória.");
    }

    [Fact]
    public void Descricao_com_mais_de_200_caracteres_deve_ter_erro()
    {
        var resultado = _validator.TestValidate(DtoValido() with { Descricao = new string('a', 201) });

        resultado.ShouldHaveValidationErrorFor(t => t.Descricao);
    }

    [Fact]
    public void Descricao_com_exatamente_200_caracteres_e_valida()
    {
        var resultado = _validator.TestValidate(DtoValido() with { Descricao = new string('a', 200) });

        resultado.ShouldNotHaveValidationErrorFor(t => t.Descricao);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Valor_menor_ou_igual_a_zero_deve_ter_erro(decimal valor)
    {
        var resultado = _validator.TestValidate(DtoValido() with { Valor = valor });

        resultado.ShouldHaveValidationErrorFor(t => t.Valor)
            .WithErrorMessage("O valor deve ser maior que zero.");
    }

    [Fact]
    public void Data_futura_deve_ter_erro()
    {
        var resultado = _validator.TestValidate(DtoValido() with { Data = Agora.UtcDateTime.AddMinutes(1) });

        resultado.ShouldHaveValidationErrorFor(t => t.Data)
            .WithErrorMessage("A data não pode ser futura.");
    }

    [Fact]
    public void Data_igual_ao_momento_atual_e_valida()
    {
        var resultado = _validator.TestValidate(DtoValido() with { Data = Agora.UtcDateTime });

        resultado.ShouldNotHaveValidationErrorFor(t => t.Data);
    }

    [Fact]
    public void Data_nao_informada_deve_ter_erro()
    {
        var resultado = _validator.TestValidate(DtoValido() with { Data = default });

        resultado.ShouldHaveValidationErrorFor(t => t.Data)
            .WithErrorMessage("A data é obrigatória.");
    }

    [Fact]
    public void Tipo_fora_do_enum_deve_ter_erro()
    {
        var resultado = _validator.TestValidate(DtoValido() with { Tipo = (TipoTransacao)99 });

        resultado.ShouldHaveValidationErrorFor(t => t.Tipo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CategoriaId_invalido_deve_ter_erro(int categoriaId)
    {
        var resultado = _validator.TestValidate(DtoValido() with { CategoriaId = categoriaId });

        resultado.ShouldHaveValidationErrorFor(t => t.CategoriaId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ContaId_invalido_deve_ter_erro(int contaId)
    {
        var resultado = _validator.TestValidate(DtoValido() with { ContaId = contaId });

        resultado.ShouldHaveValidationErrorFor(t => t.ContaId);
    }
}
