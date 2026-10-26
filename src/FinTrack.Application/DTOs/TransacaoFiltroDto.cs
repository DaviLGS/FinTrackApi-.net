using FinTrack.Domain.Enums;

namespace FinTrack.Application.DTOs;

public record TransacaoFiltroDto
{
    public const int PageSizeMaximo = 100;

    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public int? CategoriaId { get; init; }
    public int? ContaId { get; init; }
    public TipoTransacao? Tipo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
