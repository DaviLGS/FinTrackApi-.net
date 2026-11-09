using FinTrack.Application.Interfaces;
using FinTrack.Infrastructure.Auth;
using FinTrack.Infrastructure.Context;
using FinTrack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("postgres");

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.Secao))
            .Validate(o => o.Key.Length >= JwtOptions.ChaveTamanhoMinimo,
                $"Configure 'Jwt:Key' com pelo menos {JwtOptions.ChaveTamanhoMinimo} caracteres.")
            .ValidateOnStart();

        services.AddSingleton<ISenhaHasher, SenhaHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IContaRepository, ContaRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRelatorioRepository, RelatorioRepository>();

        return services;
    }
}
