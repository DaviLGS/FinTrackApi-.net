using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Mappings;

namespace FinTrack.Application.Services;

public class CategoriaService(ICategoriaRepository categoriaRepository) : ICategoriaService
{
    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await categoriaRepository.ListarAsync(cancellationToken);
        return categorias.Select(c => c.ToDto()).ToList();
    }
}
