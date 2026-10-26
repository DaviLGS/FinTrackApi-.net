using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Transacao : EntityBase
{
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Sempre positivo; o sinal é dado pelo <see cref="Tipo"/>.</summary>
    public decimal Valor { get; set; }

    /// <summary>Data da transação em UTC.</summary>
    public DateTime Data { get; set; }

    public TipoTransacao Tipo { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int ContaId { get; set; }
    public Conta? Conta { get; set; }

    public DateTime CriadoEm { get; set; }
}
