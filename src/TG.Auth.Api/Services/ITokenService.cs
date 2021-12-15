using System.Threading;
using System.Threading.Tasks;
using TG.Auth.Api.Constants;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Models.Response;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Services
{
    public interface ITokenService
    {
        Task<OperationResult<TokensResponse>> CreateTokenAsync(User user, string deviceId, AuthType authType,
            CancellationToken cancellationToken);
        Task<OperationResult<TokensResponse>> ValidateAndRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}