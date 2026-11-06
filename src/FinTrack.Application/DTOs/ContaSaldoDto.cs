namespace FinTrack.Application.DTOs;

public record ContaSaldoDto(
    int ContaId,
    string ContaNome,
    decimal SaldoInicial,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal SaldoAtual);
