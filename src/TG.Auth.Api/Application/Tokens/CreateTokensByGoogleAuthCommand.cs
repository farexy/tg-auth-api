using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Constants;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Models.Response;
using TG.Auth.Api.Services;
using TG.Auth.Api.Services.Dto;
using TG.Core.App.Constants;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Application.Tokens
{
    public record CreateTokensByGoogleAuthCommand(string IdToken) : IRequest<OperationResult<TokensResponse>>;
    
    public class CreateTokensByGoogleAuthCommandHandler : IRequestHandler<CreateTokensByGoogleAuthCommand, OperationResult<TokensResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IGoogleApiClient _googleApiClient;
        private readonly ITokenService _tokenService;

        public CreateTokensByGoogleAuthCommandHandler(ApplicationDbContext dbContext, IGoogleApiClient googleApiClient,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _googleApiClient = googleApiClient;
            _tokenService = tokenService;
        }

        public async Task<OperationResult<TokensResponse>> Handle(CreateTokensByGoogleAuthCommand command, CancellationToken cancellationToken)
        {
            var tokenPayload = await _googleApiClient.GetUserTokenPayloadAsync(command.IdToken, cancellationToken);
            if (tokenPayload is null)
            {
                return null!;
            }
            var googleAccount = await _dbContext.ExternalAccounts
                .Include(a => a.TgUser)
                .FirstOrDefaultAsync(a => a.Id == tokenPayload.Subject && a.Type == AuthType.Google, cancellationToken);

            googleAccount ??= await CreateUserAsync(tokenPayload, cancellationToken);

            var tokens = await _tokenService.CreateTokenAsync(googleAccount.TgUser!, AuthType.Google, cancellationToken);
            
            return tokens;
        }
        
        private async Task<ExternalAccount> CreateUserAsync(GoogleTokenPayload payload, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = payload.Email,
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Roles = new [] {UserRoles.GoogleUser}
            };

            var googleAccount = new ExternalAccount
            {
                Id = payload.Subject,
                Type = AuthType.Google,
                Email = payload.Email,
                TgUser = user,
            };

            await _dbContext.AddAsync(user, cancellationToken);
            await _dbContext.AddAsync(googleAccount, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return googleAccount;
        }
    }
}