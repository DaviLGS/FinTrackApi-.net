using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Categoria : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public TipoTransacao Tipo { get; set; }

    public ICollection<Transacao> Transacoes { get; set; } = [];
}
