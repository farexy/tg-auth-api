using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TG.Auth.Api.Models.Response;
using TG.Auth.Api.Services;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Application.Tokens
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<OperationResult<TokensResponse>>;
    
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, OperationResult<TokensResponse>>
    {
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public Task<OperationResult<TokensResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) =>
            _tokenService.ValidateAndRefreshTokenAsync(request.RefreshToken, cancellationToken);
    }
}