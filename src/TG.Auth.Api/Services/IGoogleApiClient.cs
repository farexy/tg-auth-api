using System.Threading;
using System.Threading.Tasks;
using TG.Auth.Api.Services.Dto;

namespace TG.Auth.Api.Services
{
    public interface IGoogleApiClient
    {
        Task<GoogleTokenPayload?> GetUserTokenPayloadAsync(string idToken, CancellationToken cancellationToken);
    }
}