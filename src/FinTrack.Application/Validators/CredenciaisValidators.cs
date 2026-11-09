using FinTrack.Application.DTOs;
using FluentValidation;

namespace FinTrack.Application.Validators;

/// <summary>Regras do cadastro: e-mail válido e senha com tamanho mínimo.</summary>
public class RegistroValidator : AbstractValidator<CredenciaisDto>
{
    public const int EmailTamanhoMaximo = 254;
    public const int SenhaTamanhoMinimo = 8;
    public const int SenhaTamanhoMaximo = 128;

    public RegistroValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .MaximumLength(EmailTamanhoMaximo).WithMessage($"O e-mail deve ter no máximo {EmailTamanhoMaximo} caracteres.")
            .EmailAddress().WithMessage("O e-mail é inválido.");

        RuleFor(c => c.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(SenhaTamanhoMinimo).WithMessage($"A senha deve ter pelo menos {SenhaTamanhoMinimo} caracteres.")
            .MaximumLength(SenhaTamanhoMaximo).WithMessage($"A senha deve ter no máximo {SenhaTamanhoMaximo} caracteres.");
    }
}

/// <summary>Regras do login: só exige os campos, sem revelar as regras de senha.</summary>
public class LoginValidator : AbstractValidator<CredenciaisDto>
{
    public LoginValidator()
    {
        RuleFor(c => c.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(c => c.Senha).NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
