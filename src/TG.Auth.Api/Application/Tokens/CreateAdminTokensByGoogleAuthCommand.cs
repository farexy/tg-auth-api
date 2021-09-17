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
    public record CreateAdminTokensByGoogleAuthCommand(string IdToken) : IRequest<OperationResult<TokensResponse>>;
    
    public class CreateAdminTokensByGoogleAuthCommandHandler : BaseCreateAuthTokensCommandHandler<CreateAdminTokensByGoogleAuthCommand>
    {
        private const string EmailDomain = "@somniumgame.com";

        private readonly IGoogleApiClient _googleApiClient;
        private readonly ITokenService _tokenService;
        private readonly ILoginGenerator _loginGenerator;

        public CreateAdminTokensByGoogleAuthCommandHandler(ApplicationDbContext dbContext, IGoogleApiClient googleApiClient,
            ITokenService tokenService, ILoginGenerator loginGenerator, IQueueProducer<NewUserAuthorizationMessage> queueProducer,
            IMapper mapper)
            : base(dbContext, queueProducer, mapper)
        {
            _googleApiClient = googleApiClient;
            _tokenService = tokenService;
            _loginGenerator = loginGenerator;
        }

        public override async Task<OperationResult<TokensResponse>> Handle(CreateAdminTokensByGoogleAuthCommand command, CancellationToken cancellationToken)
        {
            var tokenPayload = await _googleApiClient.GetUserTokenPayloadAsync(command.IdToken, cancellationToken);
            if (tokenPayload is null)
            {
                throw new BusinessLogicException("Invalid token");
            }

            if (!tokenPayload.Email.EndsWith(EmailDomain))
            {
                throw new ForbiddenAccessException("Invalid google account");
            }

            var googleAccount = await GetAccountAsync(tokenPayload.Subject, AuthType.GoogleAdmin, cancellationToken);

            googleAccount ??= await CreateUserAsync(tokenPayload, cancellationToken);

            var tokens = await _tokenService.CreateTokenAsync(googleAccount.TgUser!, AuthType.GoogleAdmin, cancellationToken);
            
            return tokens;
        }
        
        private async Task<ExternalAccount> CreateUserAsync(GoogleTokenPayload payload, CancellationToken cancellationToken)
        {
            var email = payload.Email;
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = await _loginGenerator.GenerateLoginAsync(email),
                Email = email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Roles = new [] {UserRoles.Admin}
            };

            var googleAccount = new ExternalAccount
            {
                Id = payload.Subject,
                Type = AuthType.GoogleAdmin,
                Email = payload.Email,
                TgUser = user,
            };

            await AddUserAsync(googleAccount, cancellationToken);

            return googleAccount;
        }
    }
}