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
    public record CreateTokensByFacebookAuthCommand(string DeviceId, string AccessToken) : IRequest<OperationResult<TokensResponse>>;
    
    public class CreateTokensByFacebookAuthCommandHandler : BaseCreateAuthTokensCommandHandler<CreateTokensByFacebookAuthCommand>
    {
        private readonly IFbApiClient _fbApiClient;
        private readonly ITokenService _tokenService;
        private readonly ILoginGenerator _loginGenerator;

        public CreateTokensByFacebookAuthCommandHandler(ApplicationDbContext dbContext, ITokenService tokenService,
            IFbApiClient fbApiClient, ILoginGenerator loginGenerator, IQueueProducer<NewUserAuthorizationMessage> queueProducer,
            IMapper mapper)
            : base(dbContext, queueProducer, mapper)
        {
            _tokenService = tokenService;
            _fbApiClient = fbApiClient;
            _loginGenerator = loginGenerator;
        }

        public override async Task<OperationResult<TokensResponse>> Handle(CreateTokensByFacebookAuthCommand command, CancellationToken cancellationToken)
        {
            var tokenPayload = await _fbApiClient.GetUserTokenPayloadAsync(command.AccessToken, cancellationToken);
            if (tokenPayload is null)
            {
                throw new BusinessLogicException("Invalid token");
            }

            var fbAccount = await GetAccountAsync(tokenPayload.Data.UserId, AuthType.Facebook, cancellationToken);

            fbAccount ??= await CreateUserAsync(tokenPayload.Data, command.AccessToken, cancellationToken);

            var tokens = await _tokenService.CreateTokenAsync(fbAccount.TgUser!, command.DeviceId, AuthType.Facebook, cancellationToken);
            
            return tokens;
        }
        
        private async Task<ExternalAccount> CreateUserAsync(FbTokenData data, string accessToken, CancellationToken cancellationToken)
        {
            var fbUser = await _fbApiClient.GetUserDataAsync(data.UserId, accessToken, cancellationToken);
            var email = UnescapeEmailUnicode(fbUser.Email);
            var login = await _loginGenerator.GenerateLoginAsync(email);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = login,
                Email = email,
                FirstName = fbUser.FirstName,
                LastName = fbUser.LastName,
                Roles = new [] {UserRoles.FbUser}
            };

            var fbAccount = new ExternalAccount
            {
                Id = data.UserId,
                Type = AuthType.Facebook,
                TgUser = user,
                Email = email
            };

            await AddUserAsync(fbAccount, cancellationToken);

            return fbAccount;
        }

        private static string? UnescapeEmailUnicode(string? email)
        {
            const string atUnicode = "\\u0040";
            return email?.Replace(atUnicode, "@");
        }
    }
}