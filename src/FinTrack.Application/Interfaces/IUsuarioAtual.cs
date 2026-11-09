namespace FinTrack.Application.Interfaces;

/// <summary>Usuário autenticado na requisição atual (lido do token JWT).</summary>
public interface IUsuarioAtual
{
    int Id { get; }
}
