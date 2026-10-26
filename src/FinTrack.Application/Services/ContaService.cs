using FinTrack.Application.DTOs;
using FinTrack.Application.Interfaces;
using FinTrack.Application.Mappings;

namespace FinTrack.Application.Services;

public class ContaService(IContaRepository contaRepository) : IContaService
{
    public async Task<IReadOnlyList<ContaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var contas = await contaRepository.ListarAsync(cancellationToken);
        return contas.Select(c => c.ToDto()).ToList();
    }
}
