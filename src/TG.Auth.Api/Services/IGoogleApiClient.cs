using System.Threading;
using System.Threading.Tasks;
using TG.Auth.Api.Services.Dto;

namespace TG.Auth.Api.Services
{
    public interface IGoogleApiClient
    {
        Task<GoogleTokenPayload?> ValidateAndParseTokenAsync(string idToken, CancellationToken cancellationToken);
    }
}