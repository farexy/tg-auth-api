using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Constants;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Models.Response;
using TG.Core.App.OperationResults;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api.Application.Tokens
{
    public abstract class BaseCreateAuthTokensCommandHandler<TCommand> : IRequestHandler<TCommand, OperationResult<TokensResponse>>
        where TCommand : IRequest<OperationResult<TokensResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IQueueProducer<NewUserAuthorizationMessage> _queueProducer;
        private readonly IMapper _mapper;

        protected BaseCreateAuthTokensCommandHandler(ApplicationDbContext dbContext, IQueueProducer<NewUserAuthorizationMessage> queueProducer, IMapper mapper)
        {
            _dbContext = dbContext;
            _queueProducer = queueProducer;
            _mapper = mapper;
        }

        protected async Task<ExternalAccount?> GetAccountAsync(string id, AuthType authType, CancellationToken cancellationToken) =>
            await _dbContext.ExternalAccounts
                .Include(a => a.TgUser)
                .FirstOrDefaultAsync(a => a.Id == id && a.Type == authType, cancellationToken);
        
        protected async Task AddUserAsync(ExternalAccount account, CancellationToken cancellationToken)
        {
            await _dbContext.AddAsync(account.TgUser!, cancellationToken);
            await _dbContext.AddAsync(account, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _queueProducer.SendMessageAsync(_mapper.Map<NewUserAuthorizationMessage>(account.TgUser));
        }

        public abstract Task<OperationResult<TokensResponse>> Handle(TCommand request, CancellationToken cancellationToken);
    }
}