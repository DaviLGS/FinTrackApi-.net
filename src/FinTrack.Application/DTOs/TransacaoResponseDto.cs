using FinTrack.Domain.Enums;

namespace FinTrack.Application.DTOs;

public record TransacaoResponseDto(
    int Id,
    string Descricao,
    decimal Valor,
    DateTime Data,
    TipoTransacao Tipo,
    int CategoriaId,
    string CategoriaNome,
    int ContaId,
    string ContaNome,
    DateTime CriadoEm);
