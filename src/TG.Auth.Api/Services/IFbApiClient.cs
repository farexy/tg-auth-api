using System.Threading;
using System.Threading.Tasks;
using TG.Auth.Api.Services.Dto;

namespace TG.Auth.Api.Services
{
    public interface IFbApiClient
    {
        Task<FbTokenPayload?> GetUserTokenPayloadAsync(string accessToken, CancellationToken cancellationToken);
        Task<FbUserData> GetUserDataAsync(string id, string accessToken, CancellationToken cancellationToken);
    }
}