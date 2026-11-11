using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Transacao : EntityBase
{
    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    public TipoTransacao Tipo { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int ContaId { get; set; }
    public Conta? Conta { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime CriadoEm { get; set; }
}
