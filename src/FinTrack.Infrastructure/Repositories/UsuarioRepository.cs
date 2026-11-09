using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Interfaces;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinTrack.Infrastructure.Repositories;

public class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Usuarios.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        context.Usuarios.Add(usuario);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        // Dois cadastros simultâneos com o mesmo e-mail: o índice único barra o segundo
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("Já existe um usuário com este e-mail.");
        }
    }
}
