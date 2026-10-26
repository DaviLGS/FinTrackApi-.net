using FinTrack.Application.DTOs;
using FinTrack.Application.Validators;
using FinTrack.Domain.Enums;
using FluentValidation.TestHelper;

namespace FinTrack.Tests.Validators;

public class TransacaoFiltroValidatorTests
{
    private readonly TransacaoFiltroValidator _validator = new();

    [Fact]
    public void Filtro_padrao_nao_deve_ter_erros()
    {
        var resultado = _validator.TestValidate(new TransacaoFiltroDto());

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Page_zero_deve_ter_erro()
    {
        var resultado = _validator.TestValidate(new TransacaoFiltroDto { Page = 0 });

        resultado.ShouldHaveValidationErrorFor(f => f.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(TransacaoFiltroDto.PageSizeMaximo + 1)]
    public void PageSize_fora_do_intervalo_deve_ter_erro(int pageSize)
    {
        var resultado = _validator.TestValidate(new TransacaoFiltroDto { PageSize = pageSize });

        resultado.ShouldHaveValidationErrorFor(f => f.PageSize);
    }

    [Fact]
    public void DataFim_anterior_a_DataInicio_deve_ter_erro()
    {
        var filtro = new TransacaoFiltroDto
        {
            DataInicio = new DateTime(2026, 10, 10),
            DataFim = new DateTime(2026, 10, 1)
        };

        var resultado = _validator.TestValidate(filtro);

        resultado.ShouldHaveValidationErrorFor(f => f.DataFim);
    }

    [Fact]
    public void DataFim_igual_a_DataInicio_e_valida()
    {
        var data = new DateTime(2026, 10, 10);

        var resultado = _validator.TestValidate(new TransacaoFiltroDto { DataInicio = data, DataFim = data });

        resultado.ShouldNotHaveValidationErrorFor(f => f.DataFim);
    }

    [Fact]
    public void Tipo_fora_do_enum_deve_ter_erro()
    {
        var resultado = _validator.TestValidate(new TransacaoFiltroDto { Tipo = (TipoTransacao)99 });

        resultado.ShouldHaveValidationErrorFor(f => f.Tipo);
    }
}
