using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Interfaces;

public interface ITokenService
{
    TokenDto Gerar(Usuario usuario);
}
