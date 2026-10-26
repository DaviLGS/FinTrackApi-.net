using FinTrack.Domain.Enums;

namespace FinTrack.Application.DTOs;

public record CategoriaDto(int Id, string Nome, TipoTransacao Tipo);
