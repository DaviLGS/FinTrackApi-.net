using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Infrastructure.Context;

public static class DatabaseExtensions
{
    /// <summary>Aplica as migrations pendentes (usado no startup em Development e no container).</summary>
    public static async Task AplicarMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
