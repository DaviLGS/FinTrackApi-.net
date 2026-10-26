using FinTrack.Domain.Enums;

namespace FinTrack.Application.DTOs;

public record TransacaoCreateDto
{
    public string Descricao { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime Data { get; init; }
    public TipoTransacao Tipo { get; init; }
    public int CategoriaId { get; init; }
    public int ContaId { get; init; }
}
