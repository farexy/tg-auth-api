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
    public record CreateTokensByGoogleAuthCommand(string IdToken) : IRequest<OperationResult<TokensResponse>>;
    
    public class CreateTokensByGoogleAuthCommandHandler : BaseCreateAuthTokensCommandHandler<CreateTokensByGoogleAuthCommand>
    {
        private readonly IGoogleApiClient _googleApiClient;
        private readonly ITokenService _tokenService;
        private readonly ILoginGenerator _loginGenerator;

        public CreateTokensByGoogleAuthCommandHandler(ApplicationDbContext dbContext, IGoogleApiClient googleApiClient,
            ITokenService tokenService, ILoginGenerator loginGenerator, IQueueProducer<NewUserAuthorizationMessage> queueProducer,
            IMapper mapper)
            : base(dbContext, queueProducer, mapper)
        {
            _googleApiClient = googleApiClient;
            _tokenService = tokenService;
            _loginGenerator = loginGenerator;
        }

        public override async Task<OperationResult<TokensResponse>> Handle(CreateTokensByGoogleAuthCommand command, CancellationToken cancellationToken)
        {
            var tokenPayload = await _googleApiClient.GetUserTokenPayloadAsync(command.IdToken, cancellationToken);
            if (tokenPayload is null)
            {
                throw new BusinessLogicException("Invalid token");
            }

            var googleAccount = await GetAccountAsync(tokenPayload.Subject, AuthType.Google, cancellationToken);

            googleAccount ??= await CreateUserAsync(tokenPayload, cancellationToken);

            var tokens = await _tokenService.CreateTokenAsync(googleAccount.TgUser!, AuthType.Google, cancellationToken);
            
            return tokens;
        }
        
        private async Task<ExternalAccount> CreateUserAsync(GoogleTokenPayload payload, CancellationToken cancellationToken)
        {
            var login = await _loginGenerator.GenerateLoginAsync(payload.Email);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = login,
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

            await AddUserAsync(googleAccount, cancellationToken);
            return googleAccount;
        }
    }
}