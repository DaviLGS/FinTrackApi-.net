using FinTrack.Application.Common;
using FinTrack.Application.DTOs;
using FluentValidation;

namespace FinTrack.Application.Validators;

public class TransacaoCreateValidator : AbstractValidator<TransacaoCreateDto>
{
    public const int DescricaoTamanhoMaximo = 200;

    public TransacaoCreateValidator(TimeProvider timeProvider)
    {
        RuleFor(t => t.Descricao)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(DescricaoTamanhoMaximo)
            .WithMessage($"A descrição deve ter no máximo {DescricaoTamanhoMaximo} caracteres.");

        RuleFor(t => t.Valor)
            .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");

        RuleFor(t => t.Data)
            .NotEmpty().WithMessage("A data é obrigatória.")
            .Must(data => data.ParaUtc() <= timeProvider.GetUtcNow().UtcDateTime)
            .WithMessage("A data não pode ser futura.");

        RuleFor(t => t.Tipo)
            .IsInEnum().WithMessage("O tipo deve ser 'Receita' ou 'Despesa'.");

        RuleFor(t => t.CategoriaId)
            .GreaterThan(0).WithMessage("A categoria é obrigatória.");

        RuleFor(t => t.ContaId)
            .GreaterThan(0).WithMessage("A conta é obrigatória.");
    }
}
