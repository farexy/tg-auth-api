using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TG.Auth.Api.Constants;
using TG.Auth.Api.Db;
using TG.Auth.Api.Errors;
using TG.Auth.Api.Models.Response;
using TG.Auth.Api.Services;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Application.Users
{
    public record AdminCreateUserTokensCommand(Guid UserId) : IRequest<OperationResult<TokensResponse>>;
    
    public class AdminCreateUserTokensCommandHandler : IRequestHandler<AdminCreateUserTokensCommand, OperationResult<TokensResponse>>
    {
        private const string DeviceId = "gbo_debug";
        private readonly ApplicationDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public AdminCreateUserTokensCommandHandler(ApplicationDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<OperationResult<TokensResponse>> Handle(AdminCreateUserTokensCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(request.UserId);
            if (user is null)
            {
                return AppErrors.NotFound;
            }

            var result = await _tokenService.CreateTokenAsync(user, DeviceId, AuthType.GoogleAdmin, cancellationToken);
            return result;
        }
    }
}