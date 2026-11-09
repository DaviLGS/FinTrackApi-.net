using FinTrack.Application.Interfaces;
using FinTrack.Application.Services;
using FinTrack.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        // Registra cada validador pela interface e pela própria classe. Alguns DTOs têm mais de um
        // validador (cadastro x login, relatório mensal x anual), e os serviços recebem a classe concreta.
        services.AddValidatorsFromAssemblyContaining<TransacaoCreateValidator>();

        services.AddScoped<ITransacaoService, TransacaoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IContaService, ContaService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRelatorioService, RelatorioService>();

        return services;
    }
}
