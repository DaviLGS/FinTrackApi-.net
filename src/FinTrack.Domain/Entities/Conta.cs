namespace FinTrack.Domain.Entities;

public class Conta : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }

    public ICollection<Transacao> Transacoes { get; set; } = [];
}
