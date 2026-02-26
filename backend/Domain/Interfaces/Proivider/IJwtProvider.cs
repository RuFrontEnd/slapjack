using Domain.Entities;

namespace Domain.Interfaces.Proivider
{
    public interface IJwtProvider
    {
        (string Token, DateTime Expiration) GetJwtToken(Guid userId, string email);
    }
}
