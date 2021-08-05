using TG.Auth.Api.Entities;

namespace TG.Auth.Api.Services
{
    public interface ITokenService
    {
        string GenerateTokenPair(User user);
    }
}