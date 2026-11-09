namespace FinTrack.Application.Interfaces;

public interface ISenhaHasher
{
    string Hash(string senha);

    /// <summary>
    /// Confere a senha com o hash. Com <paramref name="hash"/> nulo (usuário inexistente), faz uma
    /// verificação equivalente e retorna false, para que o tempo de resposta não revele se o e-mail existe.
    /// </summary>
    bool Verificar(string? hash, string senha);
}
