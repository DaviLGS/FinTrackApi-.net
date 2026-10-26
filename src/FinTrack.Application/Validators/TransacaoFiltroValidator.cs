using FinTrack.Application.DTOs;
using FluentValidation;

namespace FinTrack.Application.Validators;

public class TransacaoFiltroValidator : AbstractValidator<TransacaoFiltroDto>
{
    public TransacaoFiltroValidator()
    {
        RuleFor(f => f.Page)
            .GreaterThan(0).WithMessage("A página deve ser maior que zero.");

        RuleFor(f => f.PageSize)
            .InclusiveBetween(1, TransacaoFiltroDto.PageSizeMaximo)
            .WithMessage($"O tamanho da página deve estar entre 1 e {TransacaoFiltroDto.PageSizeMaximo}.");

        RuleFor(f => f.DataFim)
            .GreaterThanOrEqualTo(f => f.DataInicio)
            .When(f => f.DataInicio.HasValue && f.DataFim.HasValue)
            .WithMessage("A data final deve ser maior ou igual à data inicial.");

        RuleFor(f => f.Tipo)
            .IsInEnum().When(f => f.Tipo.HasValue)
            .WithMessage("O tipo deve ser 'Receita' ou 'Despesa'.");
    }
}
