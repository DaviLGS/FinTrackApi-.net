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

        services.AddValidatorsFromAssemblyContaining<TransacaoCreateValidator>();

        services.AddScoped<ITransacaoService, TransacaoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IContaService, ContaService>();

        return services;
    }
}
