using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Models.Response;

namespace TG.Auth.Api.Application.Tokens
{
    public record CreateTokenTestCommand(Guid UserId) : IRequest<TokensResponse>;
    
    public class CreateTokenTestCommandHandler : IRequestHandler<CreateTokenTestCommand, TokensResponse>
    {
        private readonly ApplicationDbContext _dbContext;
        // private readonly idatet

        public CreateTokenTestCommandHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TokensResponse> Handle(CreateTokenTestCommand request, CancellationToken cancellationToken)
        {
            var token = new Token
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                RefreshToken = ""
            };
            return new TokensResponse();
        }
    }
}