using FinTrack.Application.DTOs;
using FluentValidation;

namespace FinTrack.Application.Validators;

public class RelatorioAnualValidator : AbstractValidator<RelatorioAnualFiltroDto>
{
    public const int AnoMinimo = 2000;
    public const int AnoMaximo = 2100;

    public RelatorioAnualValidator()
    {
        RuleFor(f => f.Ano)
            .InclusiveBetween(AnoMinimo, AnoMaximo)
            .WithMessage($"Informe um ano entre {AnoMinimo} e {AnoMaximo}.");
    }
}

public class RelatorioMensalValidator : AbstractValidator<RelatorioMensalFiltroDto>
{
    public RelatorioMensalValidator()
    {
        RuleFor(f => f.Ano)
            .InclusiveBetween(RelatorioAnualValidator.AnoMinimo, RelatorioAnualValidator.AnoMaximo)
            .WithMessage($"Informe um ano entre {RelatorioAnualValidator.AnoMinimo} e {RelatorioAnualValidator.AnoMaximo}.");

        RuleFor(f => f.Mes)
            .NotNull().WithMessage("Informe o mês.")
            .InclusiveBetween(1, 12).WithMessage("O mês deve estar entre 1 e 12.");
    }
}
