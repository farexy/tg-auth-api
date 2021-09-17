using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TG.Auth.Api.Constants;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Models.Response;
using TG.Auth.Api.Services;
using TG.Auth.Api.Services.Dto;
using TG.Core.App.Constants;
using TG.Core.App.Exceptions;
using TG.Core.App.OperationResults;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api.Application.Tokens
{
    public record CreateTokensByAppleAuthCommand(string Token) : IRequest<OperationResult<TokensResponse>>;
    
    public class CreateTokensByAppleAuthCommandHandler : BaseCreateAuthTokensCommandHandler<CreateTokensByAppleAuthCommand>
    {
        private readonly IGoogleApiClient _googleApiClient;
        private readonly ITokenService _tokenService;

        public CreateTokensByAppleAuthCommandHandler(ApplicationDbContext dbContext, IGoogleApiClient googleApiClient,
            ITokenService tokenService, IQueueProducer<NewUserAuthorizationMessage> queueProducer,
            IMapper mapper)
            : base(dbContext, queueProducer, mapper)
        {
            _googleApiClient = googleApiClient;
            _tokenService = tokenService;
        }

        public override async Task<OperationResult<TokensResponse>> Handle(CreateTokensByAppleAuthCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            var tokenPayload = await _googleApiClient.GetUserTokenPayloadAsync(command.Token, cancellationToken);
            if (tokenPayload is null)
            {
                throw new BusinessLogicException("Invalid token");
            }

            var googleAccount = await GetAccountAsync(tokenPayload.Subject, AuthType.Apple, cancellationToken);

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
                Roles = new [] {UserRoles.AppleUser}
            };

            var appleAccount = new ExternalAccount
            {
                Id = payload.Subject,
                Type = AuthType.Apple,
                Email = payload.Email,
                TgUser = user,
            };

            await AddUserAsync(appleAccount, cancellationToken);

            return appleAccount;
        }
    }
}