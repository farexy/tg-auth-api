using System.Threading.Tasks;

namespace TG.Auth.Api.Services
{
    public interface ITokenService
    {
        Task GenerateTokenPairAsync();
    }
}