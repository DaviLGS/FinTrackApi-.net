using FinTrack.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinTrack.Api.Auth;

/// <summary>Lê o id do usuário da claim "sub" do token JWT da requisição.</summary>
public class UsuarioAtual(IHttpContextAccessor httpContextAccessor) : IUsuarioAtual
{
    public int Id
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return int.TryParse(sub, out var id)
                ? id
                : throw new InvalidOperationException("Não há usuário autenticado nesta requisição.");
        }
    }
}
