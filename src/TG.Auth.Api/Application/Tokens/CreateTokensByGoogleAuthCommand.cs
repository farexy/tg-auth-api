using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Models.Response;
using TG.Auth.Api.Services;
using TG.Auth.Api.Services.Dto;

namespace TG.Auth.Api.Application.Tokens
{
    public record CreateTokensByGoogleAuthCommand(string IdToken) : IRequest<TokensResponse>;
    
    public class CreateTokenTestCommandHandler : IRequestHandler<CreateTokensByGoogleAuthCommand, TokensResponse>
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IGoogleApiClient _googleApiClient;

        public CreateTokenTestCommandHandler(ApplicationDbContext dbContext, IGoogleApiClient googleApiClient)
        {
            _dbContext = dbContext;
            _googleApiClient = googleApiClient;
        }

        public async Task<TokensResponse> Handle(CreateTokensByGoogleAuthCommand command, CancellationToken cancellationToken)
        {
            var tokenPayload = await _googleApiClient.ValidateAndParseTokenAsync(command.IdToken, cancellationToken);
            if (tokenPayload is null)
            {
                return null!;
            }
            var googleAccount = await _dbContext.GoogleAccounts
                .Include(g => g.TgUser)
                .FirstOrDefaultAsync(g => g.Id == tokenPayload.Subject, cancellationToken);

            googleAccount ??= await CreateUserAsync(tokenPayload, cancellationToken);
            
            var token = new Token
            {
                Id = Guid.NewGuid(),
                UserId = googleAccount.TgUserId,
                RefreshToken = ""
            };
            return new TokensResponse();
        }

        private async Task<GoogleAccount> CreateUserAsync(GoogleTokenPayload payload, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                GoogleAccountId = payload.Subject,
                Login = payload.Email,
            };

            var googleAccount = new GoogleAccount
            {
                Id = payload.Subject,
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